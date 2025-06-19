using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using InventoryPro.ReportService.Services;
using InventoryPro.ReportService.Data;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/report-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Authentication
ConfigureJwtAuthentication(builder.Services, builder.Configuration);

// Register services
builder.Services.AddScoped<IReportService, ReportService>();

// Add HttpClient for calling other services
builder.Services.AddHttpClient<IReportService, ReportService>(client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => ConfigureSwagger(options));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReportDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Report database created/verified successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to create/verify report database - continuing without database functionality");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add a test endpoint
app.MapGet("/", () => "Report Service is running!");

Log.Information("Report Service started on port 5179");

app.Run();

// --- Local functions for clean code separation ---

void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("JwtSettings");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"] ?? "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!");

    services.AddAuthentication(options =>
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
}

void ConfigureSwagger(SwaggerGenOptions c)
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InventoryPro Report Service", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
}