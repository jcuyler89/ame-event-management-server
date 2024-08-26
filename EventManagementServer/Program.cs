using EventManagementServer.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault to the configuration
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
var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Get the default connection string from environment variable or fallback to appsettings.json
var defaultConnectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION_STRING") ??
                              builder.Configuration.GetConnectionString("DefaultConnection");

// Get the second connection string from environment variable or fallback to appsettings.json
var secondConnectionString = Environment.GetEnvironmentVariable("SECOND_CONNECTION_STRING") ??
                             builder.Configuration.GetConnectionString("SecondConnection");

// Configure the DbContext for the default connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(defaultConnectionString));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
