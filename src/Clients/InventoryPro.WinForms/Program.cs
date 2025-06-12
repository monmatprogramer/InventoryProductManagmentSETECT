using InventoryPro.WinForms.Forms;
using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Net.Http.Headers;


namespace InventoryPro.WinForms;

static class Program
    {
    private static IServiceProvider? _serviceProvider;
    private static MainForm? _mainForm;

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

            // Start with the main application flow
            RunApplication();
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
    /// Main application flow - handles login and main form
    /// </summary>
    private static void RunApplication()
        {
        while (true)
            {
            // Show login form
            var loginForm = GetRequiredService<LoginForm>();

            // Subscribe to login success event
            bool loginSuccessful = false;
            loginForm.FormClosed += (sender, e) =>
            {
                if (loginForm.DialogResult == DialogResult.OK)
                    {
                    loginSuccessful = true;
                    }
            };

            var loginResult = loginForm.ShowDialog();
            loginForm.Dispose();

            if (loginResult == DialogResult.OK && loginSuccessful)
                {
                // Login successful, show main form
                ShowMainForm();
                break; // Exit after main form is closed
                }
            else
                {
                // Login cancelled or failed, exit application
                break;
                }
            }
        }

    /// <summary>
    /// Shows the main form and handles its lifecycle
    /// </summary>
    private static void ShowMainForm()
        {
        try
            {
            _mainForm = GetRequiredService<MainForm>();

            // Handle main form closing to potentially show login again
            _mainForm.FormClosed += MainForm_FormClosed;

            // Show the main form
            Application.Run(_mainForm);
            }
        catch (Exception ex)
            {
            Log.Error(ex, "Error showing main form");
            MessageBox.Show($"Error loading main application: {ex.Message}",
                "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    /// <summary>
    /// Handles main form closed event
    /// </summary>
    private static void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
        try
            {
            // Clean up
            if (_mainForm != null)
                {
                _mainForm.FormClosed -= MainForm_FormClosed;
                _mainForm.Dispose();
                _mainForm = null;
                }

            Log.Information("Main form closed, application shutting down");
            }
        catch (Exception ex)
            {
            Log.Error(ex, "Error during main form cleanup");
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

                // Forms - Register as Transient so we get new instances each time
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