using MusicInteraction.Infrastructure.MongoDB;
using MusicInteraction.Infrastructure.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure connection strings
// MongoDB connection configuration

// Register MongoDB services for grading methods
builder.Services.AddMongoDbServices();

// Register PostgreSQL services for interactions
builder.Services.AddPostgreSQLServices();

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