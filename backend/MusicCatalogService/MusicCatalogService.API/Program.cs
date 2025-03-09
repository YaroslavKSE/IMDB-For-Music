using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Services;
using MusicCatalogService.Infrastructure.Clients;
using MusicCatalogService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DB Context
builder.Services.AddDbContext<MusicCatalogDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)
    )
);

// Configure Spotify settings
builder.Services.Configure<SpotifySettings>(
    builder.Configuration.GetSection("Spotify"));

// Add distributed caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MusicCatalog:";
});

// Register HTTP clients
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();

// Register repositories
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();

// Register services
builder.Services.AddScoped<IMusicCatalogService, MusicCatalogService.Core.Services.MusicCatalogService>();

// Configure rate limiting
var spotifySettings = builder.Configuration.GetSection("Spotify").Get<SpotifySettings>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("global", _ =>
            new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = spotifySettings?.RateLimitPerMinute ?? 80,
                QueueLimit = 10,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"error": "Too many requests. Please try again later."}""", 
            cancellationToken: token);
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MusicAppPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply database migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Attempting to apply database migrations...");
    
    try
    {
        var context = services.GetRequiredService<MusicCatalogDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying migrations");
        
        // In production, you might want to continue application startup even if migrations fail
        if (app.Environment.IsProduction())
        {
            logger.LogWarning("Application continuing despite migration failure - manual intervention may be required");
        }
        else
        {
            throw; // In development, fail fast to make issues obvious
        }
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("MusicAppPolicy");
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();