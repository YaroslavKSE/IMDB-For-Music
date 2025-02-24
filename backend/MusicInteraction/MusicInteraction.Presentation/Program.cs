using MediatR;
using MusicInteraction.Infrastructure;
using MusicInteraction.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Infrastructure services
builder.Services.AddSingleton<LocalDBTemplate>();
builder.Services.AddInfrastructureServices();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(WriteReviewCommand).Assembly));

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