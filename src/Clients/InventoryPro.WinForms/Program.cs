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
using Serilog.Extensions.Hosting;

namespace InventoryPro.WinForms;

static class Program
    {
    private static IServiceProvider? _serviceProvider;

    [STAThread]
    static void Main()
        {
        ApplicationConfiguration.Initialize();

        var host = CreateHostBuilder().Build();
        _serviceProvider = host.Services;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(GetConfiguration())
            .CreateLogger();

        try
            {
            Log.Information("Starting InventoryPro Windows Forms Application");
            Application.Run(GetRequiredService<LoginForm>());
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

    static IHostBuilder CreateHostBuilder()
        {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var config = GetConfiguration();
                services.AddSingleton<IConfiguration>(config);

                // HTTP Client for API communication with explicit BaseAddress
                var baseUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5000";

                services.AddHttpClient<IApiService, ApiService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
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

    private static IConfiguration GetConfiguration()
        {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                optional: true, reloadOnChange: true);

        return builder.Build();
        }

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

    public static T GetRequiredService<T>() where T : notnull
        {
        if (_serviceProvider == null)
            {
            throw new InvalidOperationException("Service provider not initialized.");
            }
        return _serviceProvider.GetRequiredService<T>();
        }

    public static T? GetService<T>() where T : class
        {
        return _serviceProvider?.GetService<T>();
        }
    }