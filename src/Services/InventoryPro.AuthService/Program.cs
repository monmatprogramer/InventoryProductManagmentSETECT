using Microsoft.AspNetCore.Authentication.JwtBearer;
using InventoryPro.AuthService.Data;
using InventoryPro.AuthService.Services;
using InventoryPro.AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// Ensure the "Secret" value is not null or empty before using it
var secret = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secret))
    {
    throw new InvalidOperationException("JWT Secret is not configured properly.");
    }

var key = Encoding.ASCII.GetBytes(secret);


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
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
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InventoryPro Auth Service", Version = "v1" });

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
//using (var scope = app.Services.CreateScope())
//    {
//    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
//    context.Database.Migrate();
//    }
using (var scope = app.Services.CreateScope())
    {
    var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    context.Database.Migrate();

    // Ensure admin user exists
    if (!context.Users.Any(u => u.Username == "admin"))
        {
        var adminUser = new User
            {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Email = "admin@inventorypro.com",
            FirstName = "System",
            LastName = "Administrator",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
            };

        context.Users.Add(adminUser);
        context.SaveChanges();

        Console.WriteLine("Admin user created successfully!");
        Console.WriteLine("Username: admin");
        Console.WriteLine("Password: admin123");
        }
    }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Auth Service started on port 5001");

app.Run();