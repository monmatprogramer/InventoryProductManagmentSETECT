using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure port from environment or use default
var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT") ?? "5000";
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? $"http://0.0.0.0:{port}";

builder.WebHost.UseUrls(urls);

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
            {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            };
    });

// Add Ocelot
builder.Services.AddOcelot();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

try
{
    // Get logger after app is built but before running
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    
    // Configure the HTTP request pipeline
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    // Add Ocelot middleware
    await app.UseOcelot();

    logger.LogInformation($"Gateway starting on {urls}");
    
    app.Run();
}
catch (IOException ex) when (ex.Message.Contains("address already in use") || ex.Message.Contains("Address already in use"))
{
    Console.WriteLine($"Port binding failed: {ex.Message}");
    Console.WriteLine("Try setting a different port using: ASPNETCORE_PORT=5001 dotnet run");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"Gateway startup failed: {ex.Message}");
    Environment.Exit(1);
}