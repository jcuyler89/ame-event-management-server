using EventManagementServer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Corrected the using directive
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container, including Authorization
builder.Services.AddControllers();
builder.Services.AddAuthorization();

// Retrieve Key Vault name from environment variables
var keyVaultName = Environment.GetEnvironmentVariable("KeyVaultName");
if (string.IsNullOrEmpty(keyVaultName))
{
    throw new InvalidOperationException("KeyVaultName environment variable is not set. Please ensure you have set it correctly.\n\n" +
        "To set the environment variable, follow the instructions below based on your environment:\n\n" +
        "1. **For Local Development**:\n" +
        "   - **Windows Command Prompt**: Use the command `set KeyVaultName=your-key-vault-name`\n" +
        "   - **Windows PowerShell**: Use the command `$env:KeyVaultName=\"your-key-vault-name\"`\n" +
        "   - **Linux/Mac (bash shell)**: Use the command `export KeyVaultName=your-key-vault-name`\n\n" +
        "Please restart your application after setting the environment variable.");
}

// Set up Key Vault URI
var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

// Add Azure Key Vault to the configuration
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());

// Retrieve JWT settings from Azure Key Vault
var secretClient = new SecretClient(keyVaultUri, new DefaultAzureCredential());
var jwtIssuerSecret = secretClient.GetSecret("AmeEventManagementServer-JwtIssuer").Value?.Value;
var jwtAudienceSecret = secretClient.GetSecret("AmeEventMAnagementServer-JwtAudience").Value?.Value;

if (string.IsNullOrEmpty(jwtIssuerSecret) || string.IsNullOrEmpty(jwtAudienceSecret))
{
    throw new InvalidOperationException("JWT configuration secrets are not set in Azure Key Vault.");
}

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Use JwtBearerDefaults here
    .AddJwtBearer(options =>
    {
        options.Authority = "https://dev-kwk94vpz.us.auth0.com";
        options.Audience = "S9tJSEfSIwmLDTuoanUOpuUn4QTXo0Ti";
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

// Retrieve AmeEventManagementServerAllowedOrigins from Key Vault
var allowedOriginsSecret = secretClient.GetSecret("AmeEventManagementServerAllowedOrigins").Value?.Value;

if (string.IsNullOrEmpty(allowedOriginsSecret))
{
    throw new InvalidOperationException("AmeEventManagementServerAllowedOrigins secret is not set in Azure Key Vault.");
}

var allowedOrigins = allowedOriginsSecret.Split(',');

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Get the default connection string from Azure Key Vault or fallback to environment variables
var defaultConnectionStringSecret = secretClient.GetSecret("AmeEventManagementServerDefaultConnection").Value?.Value;
if (string.IsNullOrEmpty(defaultConnectionStringSecret))
{
    throw new InvalidOperationException("AmeEventManagementServerDefaultConnection secret is not set in Azure Key Vault.");
}

// Configure the DbContext for the default connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnectionStringSecret));

// Optionally, configure another DbContext for the second connection if needed
// If you're not using a second DbContext, you can skip this part
// builder.Services.AddDbContext<SecondDbContext>(options =>
//     options.UseNpgsql(secondConnectionString));

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
