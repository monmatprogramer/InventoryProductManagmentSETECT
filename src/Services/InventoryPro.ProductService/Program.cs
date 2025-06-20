using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/product-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure Entity Framework - Use InMemory for development if SQL Server is not available
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseInMemoryDatabase("ProductDB")
               .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}
else
{
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
               .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
}

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
        {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Temporarily disable for testing
        ValidateAudience = false, // Temporarily disable for testing
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        RequireExpirationTime = false
        };
    
    options.Events = new JwtBearerEvents
        {
        OnAuthenticationFailed = context =>
            {
            Log.Error("Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
            },
        OnTokenValidated = context =>
            {
            Log.Information("Token validated successfully for user: {User}", 
                context.Principal?.Identity?.Name ?? "Unknown");
            return Task.CompletedTask;
            }
        };
});

// Register services
builder.Services.AddScoped<IProductService, ProductService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InventoryPro Product Service", Version = "v1" });

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

// Ensure database is created and migrations are applied (only for relational databases)
using (var scope = app.Services.CreateScope())
    {
    var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    // Only migrate if using a relational database (not in-memory)
    if (!context.Database.IsInMemory())
        {
        context.Database.Migrate();
        }
    else
        {
        // Ensure in-memory database is created
        context.Database.EnsureCreated();
        }
    }

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("Product Service started on port 5089");

app.Run();