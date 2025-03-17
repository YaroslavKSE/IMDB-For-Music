using MediatR;
using MusicInteraction.Infrastructure;
using MusicInteraction.Application;
using MusicInteraction.Infrastructure.Migration;
using MusicInteraction.Infrastructure.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB connection configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
{
    {"MongoDB:ConnectionString", "mongodb+srv://mpozdniakov:kElQkUOLQzhRHtzl@gradingmethods.dm47r.mongodb.net/?retryWrites=true&w=majority&appName=GradingMethods"},
    {"MongoDB:DatabaseName", "MusicEvaluationPlatform"}
});

// Register Infrastructure services
builder.Services.AddSingleton<LocalDBTemplate>();

// Register local storage for interactions
builder.Services.AddSingleton<MusicInteraction.Application.Interfaces.IInteractionStorage, MusicInteraction.Infrastructure.LocalStorages.InteractionStorage>();

// Register MongoDB services for grading methods
builder.Services.AddMongoDbServices();

// Register migration service
builder.Services.AddGradingMethodMigration();

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
app.Run();