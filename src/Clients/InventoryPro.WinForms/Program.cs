using InventoryPro.WinForms.Forms;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Net.Http.Headers;

namespace InventoryPro.WinForms;

static class Program
    {
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
        {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Configure services
        var host = CreateHostBuilder().Build();
        _serviceProvider = host.Services;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(GetConfiguration())
            .CreateLogger();

        try
            {
            Log.Information("Starting InventoryPro Windows Forms Application");

            // Run the application with the login form
            Application.Run(_serviceProvider.GetRequiredService<LoginForm>());
            }
        catch (Exception ex)
            {
            Log.Fatal(ex, "Application terminated unexpectedly");
            MessageBox.Show($"Fatal error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        finally
            {
            Log.CloseAndFlush();
            }
        }

    /// <summary>
    /// Creates the host builder with dependency injection
    /// </summary>
    static IHostBuilder CreateHostBuilder()
        {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton<IConfiguration>(GetConfiguration());

                // HTTP Client for API communication
                services.AddHttpClient<IApiService, ApiService>(client =>
                {
                    var config = GetConfiguration();
                    client.BaseAddress = new Uri(config["ApiSettings:BaseUrl"] ?? "https://localhost:5000");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(config.GetValue<int>("ApiSettings:Timeout", 30));
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());

                // Services
                services.AddSingleton<IAuthService, AuthService>();
                services.AddScoped<IApiService, ApiService>();

                // Forms
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();
                services.AddTransient<ProductForm>();
                services.AddTransient<CustomerForm>();
                services.AddTransient<SalesForm>();
                services.AddTransient<ReportForm>();

                // Logging
                services.AddLogging(configure => configure.AddSerilog());
            })
            .UseSerilog();
        }

    /// <summary>
    /// Gets the application configuration
    /// </summary>
    private static IConfiguration GetConfiguration()
        {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                optional: true, reloadOnChange: true);

        return builder.Build();
        }

    /// <summary>
    /// HTTP retry policy
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Log.Warning("Delaying for {delay}ms, then making retry {retry}.",
                        timespan.TotalMilliseconds, retryCount);
                });
        }

    /// <summary>
    /// Circuit breaker policy
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    Log.Warning("Circuit breaker opened for {duration}s", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                });
        }

    /// <summary>
    /// Gets a required service from the DI container
    /// </summary>
    public static T GetRequiredService<T>() where T : notnull
        {
        return _serviceProvider?.GetRequiredService<T>()
            ?? throw new InvalidOperationException("Service provider not initialized");
        }

    /// <summary>
    /// Gets a service from the DI container
    /// </summary>
    public static T? GetService<T>() where T : notnull
        {
        return _serviceProvider?.GetService<T>();
        }
    }