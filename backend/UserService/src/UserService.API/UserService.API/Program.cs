using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using UserService.Application.Validators;
using UserService.Application.Commands;
using UserService.Application.Interfaces;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Configuration;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using UserService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON property names
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Handle circular references
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

// MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly);
    // Add behaviors if needed (validation, logging, etc.)
});

// Database
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(3)
    );
});

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Auth0
builder.Services.Configure<Auth0Settings>(
    builder.Configuration.GetSection("Auth0"));
builder.Services.AddHttpClient<IAuth0Service, Auth0Service>();

// JWT Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
    options.Audience = builder.Configuration["Auth0:Audience"];
    
    // Add token validation parameters if needed
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = ClaimTypes.NameIdentifier
        
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Log all claims for debugging
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated successfully");
            
            foreach (var claim in context.Principal.Claims)
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            
            // Ensure the Auth0 user ID is added as a "sub" claim if it doesn't exist
            var auth0UserId = context.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(auth0UserId) && context.Principal.FindFirst("sub") == null)
            {
                var identity = context.Principal.Identity as ClaimsIdentity;
                identity?.AddClaim(new Claim("sub", auth0UserId));
                logger.LogInformation("Added 'sub' claim with value: {Value}", auth0UserId);
            }
            
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Authentication failed");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication challenge issued: {Error}", context.Error);
            return Task.CompletedTask;
        }
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "User Service API", 
        Version = "v1",
        Description = "API for managing user authentication and profiles"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

// Health checks
// builder.Services.AddHealthChecks()
//     .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"));

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

var app = builder.Build();

// Apply database migrations at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("Attempting to apply database migrations...");
    
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        
        // Wait strategy with retry logic
        const int maxRetries = 3;
        var retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            try
            {
                logger.LogInformation($"Migration attempt {retryCount + 1}/{maxRetries}...");
                
                // This will create the database if it doesn't exist and apply any pending migrations
                context.Database.Migrate();
                
                logger.LogInformation("Database migrations applied successfully");
                break; // Success - exit the retry loop
            }
            catch (Exception ex)
            {
                retryCount++;
                
                if (retryCount >= maxRetries)
                {
                    logger.LogError(ex, $"Failed to apply migrations after {maxRetries} attempts");
                    throw; // Re-throw if we've exhausted retries
                }
                
                logger.LogWarning($"Database not ready, retrying in 5 seconds... (Attempt {retryCount}/{maxRetries})");
                logger.LogDebug(ex, "Migration failure details");
                
                // Wait before retrying
                Thread.Sleep(5000);
            }
        }
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
    });
    app.UseDeveloperExceptionPage();
}
else
{
    // Add production exception handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Add global exception handling middleware if needed
// app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();
// app.MapHealthChecks("/health");

app.Run();