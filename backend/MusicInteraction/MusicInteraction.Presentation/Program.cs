using MusicInteraction.Infrastructure.MongoDB;
using MusicInteraction.Infrastructure.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure connection strings
var configuration = builder.Configuration;

// Configure specific settings for Docker environment
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    Console.WriteLine("Running in Docker container");

    // Override connection strings if provided as environment variables
    var postgresConnection = Environment.GetEnvironmentVariable("ConnectionStrings__PostgreSQL");
    var mongoConnection = Environment.GetEnvironmentVariable("MongoDB__ConnectionString");
    var mongoDbName = Environment.GetEnvironmentVariable("MongoDB__DatabaseName");

    if (!string.IsNullOrEmpty(postgresConnection))
    {
        configuration["ConnectionStrings:PostgreSQL"] = postgresConnection;
    }

    if (!string.IsNullOrEmpty(mongoConnection))
    {
        configuration["MongoDB:ConnectionString"] = mongoConnection;
    }

    if (!string.IsNullOrEmpty(mongoDbName))
    {
        configuration["MongoDB:DatabaseName"] = mongoDbName;
    }
}

// Register MongoDB services for grading methods
builder.Services.AddMongoDbServices();

// Register PostgreSQL services for interactions
builder.Services.AddPostgreSQLServices();

// Register MediatR services
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PostInteractionCommand).Assembly));

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

app.Run();