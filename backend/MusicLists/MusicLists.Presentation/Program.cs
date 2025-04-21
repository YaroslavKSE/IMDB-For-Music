using MusicLists.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register PostgreSQL services
builder.Services.AddPostgreSQLServices();

// Register Application services including MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(MusicLists.Application.IMusicListsStorage).Assembly);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        // Get allowed origins from configuration based on environment
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
        logger.LogInformation("CORS configured with allowed origins: {Origins}", 
            allowedOrigins.Length > 0 ? string.Join(", ", allowedOrigins) : "none");
        
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Basic health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.UseCors("CorsPolicy");

app.MapHealthChecks("/health");

// Sample API endpoints
app.MapGet("/api/hello", () => "Hello from MusicLists API!");

app.Run();