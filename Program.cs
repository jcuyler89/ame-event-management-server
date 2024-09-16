using EventManagementServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file for local development (optional)
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Helper function to retrieve environment variables
string GetEnvironmentVariableOrThrow(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrEmpty(value))
    {
        throw new InvalidOperationException($"{name} environment variable is not set.");
    }
    return value;
}

// Retrieve JWT settings
var jwtIssuer = GetEnvironmentVariableOrThrow("JWT_ISSUER");
var jwtAudience = GetEnvironmentVariableOrThrow("JWT_AUDIENCE");
var jwtAuthority = GetEnvironmentVariableOrThrow("AUTHORITY_URL");

// Add services to the container, including Authorization
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = jwtAuthority;
        options.Audience = jwtAudience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
        };
    });

// Retrieve CORS settings
var allowedOrigins = GetEnvironmentVariableOrThrow("ALLOWED_ORIGINS");

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

// Get the default connection string
var defaultConnectionString = GetEnvironmentVariableOrThrow("DB_CONNECTION_STRING");

// Configure the DbContext for the default connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run("http://0.0.0.0:5050");
