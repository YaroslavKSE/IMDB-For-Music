using System.Threading.RateLimiting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MusicCatalogService.Core.Interfaces;
using MusicCatalogService.Core.Models;
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

// Register serializers for special types
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// MongoDB configuration
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

// Register repositories
builder.Services.AddSingleton<ICatalogRepository, MongoCatalogRepository>();

// Configure Spotify settings
builder.Services.Configure<SpotifySettings>(
    builder.Configuration.GetSection("Spotify"));

// Configure MongoDB serialization before registering services
BsonClassMap.RegisterClassMap<CatalogItemBase>(cm =>
{
    cm.AutoMap();
    cm.SetIsRootClass(true);
});

BsonClassMap.RegisterClassMap<Album>();
BsonClassMap.RegisterClassMap<Track>();
BsonClassMap.RegisterClassMap<Artist>();
BsonClassMap.RegisterClassMap<SimplifiedArtist>();

// Register services
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// Register Redis Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "MusicCatalog:";
});
builder.Services.AddScoped<ICacheService, DistributedCacheService>();

// Register HTTP clients
builder.Services.AddSingleton<ISpotifyTokenService, SpotifyTokenService>();
builder.Services.AddHttpClient<ISpotifyApiClient, SpotifyApiClient>();

// Configure rate limiting
var spotifySettings = builder.Configuration.GetSection("Spotify").Get<SpotifySettings>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
    {
        return RateLimitPartition.GetFixedWindowLimiter("global", _ =>
            new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = spotifySettings?.RateLimitPerMinute ?? 1000,
                QueueLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"error": "Too many requests. Please try again later."}""",
            token);
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MusicAppPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"]?.Split(',') ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Basic health checks
builder.Services.AddHealthChecks();

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

app.MapHealthChecks("/health");

app.Run();