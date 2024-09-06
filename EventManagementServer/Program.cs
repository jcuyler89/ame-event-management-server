using EventManagementServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file for local development (optional)
DotNetEnv.Env.Load();

// Add services to the container, including Authorization
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Retrieve JWT settings from environment variables
var jwtIssuerSecret = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudienceSecret = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrEmpty(jwtIssuerSecret) || string.IsNullOrEmpty(jwtAudienceSecret))
{
    throw new InvalidOperationException("JWT configuration environment variables are not set. Please ensure JWT_ISSUER and JWT_AUDIENCE are set.");
}

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Use JwtBearerDefaults here
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("AUTHORITY_URL"); // Set authority from environment variable
        options.Audience = Environment.GetEnvironmentVariable("AUDIENCE_ID");    // Set audience from environment variable
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuerSecret,
            ValidAudience = jwtAudienceSecret,
        };
    });

// Retrieve AmeEventManagementServerAllowedOrigins from environment variables
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
if (string.IsNullOrEmpty(allowedOrigins))
{
    throw new InvalidOperationException("AmeEventManagementServerAllowedOrigins environment variable is not set.");
}

// Add CORS policy using allowed origins from environment variables
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins(allowedOrigins.Split(','))
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Get the default connection string from environment variables
var defaultConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(defaultConnectionString))
{
    throw new InvalidOperationException("Database connection string is not set. Please ensure the DB_CONNECTION_STRING environment variable is set.");
}

// Configure the DbContext for the default connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin"); // Use the CORS policy

app.UseAuthentication(); // Add this before Authorization middleware
app.UseAuthorization();  // Ensure authorization middleware is configured after authentication

app.MapControllers();

app.Run();
