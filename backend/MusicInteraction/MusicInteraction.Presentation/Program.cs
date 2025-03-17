using MediatR;
using MusicInteraction.Infrastructure;
using MusicInteraction.Application;
using MusicInteraction.Infrastructure.Migration;
using MusicInteraction.Infrastructure.MongoDB;
using MusicInteraction.Infrastructure.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure connection strings
// MongoDB connection configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    {"MongoDB:ConnectionString", "mongodb+srv://mpozdniakov:kElQkUOLQzhRHtzl@gradingmethods.dm47r.mongodb.net/?retryWrites=true&w=majority&appName=GradingMethods"},
    {"MongoDB:DatabaseName", "MusicEvaluationPlatform"},
    {"ConnectionStrings:PostgreSQL", "Host=localhost;Database=MusicInteraction;Username=qualiaaa;Password=password"}
});

// Keep LocalDBTemplate available during migration
builder.Services.AddSingleton<LocalDBTemplate>();

// Register MongoDB services for grading methods
builder.Services.AddMongoDbServices();

// Register PostgreSQL services for interactions
builder.Services.AddPostgreSQLServices();

// Register migration service for MongoDB grading methods
builder.Services.AddGradingMethodMigration();

// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostInteractionCommand).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Always map controllers, even if initialization fails
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

try
{
    // Try to initialize the databases, but don't fail the app if it doesn't work
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetService<MusicInteractionDbContext>();
        if (dbContext != null)
        {
            // Just check if we can connect, don't do migrations here
            await dbContext.Database.CanConnectAsync();
        }
    }
}
catch (Exception ex)
{
    // Log the error but continue - we'll still have the in-memory DB as fallback
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database.");
}

// Run the app regardless of database initialization success
app.Run();