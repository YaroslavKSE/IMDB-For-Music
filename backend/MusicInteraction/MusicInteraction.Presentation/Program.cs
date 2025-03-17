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

// Local storage is no longer used

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

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

// Run the app
app.Run();