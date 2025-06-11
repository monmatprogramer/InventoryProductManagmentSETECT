using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Serilog.Settings.Configuration; // Add this using directive
using InventoryPro.SalesService.Data;
using InventoryPro.SalesService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Ensure the Serilog.Settings.Configuration package is installed
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/sales-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
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

// Register services
builder.Services.AddScoped<ISalesService, SalesService>();

// Add HttpClient for calling Product Service
builder.Services.AddHttpClient<ISalesService, SalesService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5089/"); // Product Service URL
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InventoryPro Sales Service", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new()
        {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
        });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    {
    app.UseSwagger();
    app.UseSwaggerUI();
    }

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
    {
    var context = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    context.Database.Migrate();
    }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Sales Service started on port 5282");

app.Run();