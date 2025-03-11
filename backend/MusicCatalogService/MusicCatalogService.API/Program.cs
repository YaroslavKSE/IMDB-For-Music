using System.Threading.RateLimiting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models.Spotify;
using MusicCatalogService.Core.Services;
using MusicCatalogService.Infrastructure.Clients;
using MusicCatalogService.Infrastructure.Configuration;
using MusicCatalogService.Infrastructure.Repositories;
using MusicCatalogService.Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

// Register repositories
builder.Services.AddSingleton<ICatalogRepository, MongoCatalogRepository>();

// Configure Spotify settings
builder.Services.Configure<SpotifySettings>(
    builder.Configuration.GetSection("Spotify"));

// Configure MongoDB serialization before registering services
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Register services
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// Register Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MusicCatalog:";
});
builder.Services.AddScoped<ICacheService, DistributedCacheService>();

// Register HTTP clients
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();

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
                PermitLimit = spotifySettings?.RateLimitPerMinute ?? 160,
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