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
    private static async void RunApplication()

        {
        while (true)
            {
            try
                {
                Log.Information("=== STARTING LOGIN PROCESS ===");

                // Show login form
                using var loginForm = GetRequiredService<LoginForm>();
                Log.Information("About to show login dialog");
                var loginResult = loginForm.ShowDialog();

                Log.Information("Login form closed with result: {Result}", loginResult);
                //message box
               
                if (loginResult == DialogResult.OK)
                    {
                    Log.Information("=== LOGIN SUCCESSFUL - ATTEMPTING TO SHOW MAIN FORM ===");

                    // Login successful, show main form
                    //ShowMainForm();
                    var authService = GetRequiredService<IAuthService>();
                    var currentUser = await authService.GetCurrentUserAsync();
                    var token = await authService.GetTokenAsync();

                    Log.Information("Auth verification - User: {HasUser}, Token: {HasToken}",
                        currentUser != null, !string.IsNullOrEmpty(token));

                    if (currentUser != null && !string.IsNullOrEmpty(token))
                        {
                        Log.Information("Authentication verified, showing main form");
                        ShowMainForm();
                        }
                    else
                        {
                        Log.Error("Authentication verification failed after successful login");
                        MessageBox.Show("Authentication error. Please try logging in again.",
                            "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue; // Go back to login
                        }

                    // After main form closes, check if user wants to login again
                    Log.Information("Main form closed, returning to login screen");
                    continue; // Go back to login screen
                    }
                else
                    {
                    Log.Information("Login cancelled or failed, exiting application");
                    // Login cancelled or failed, exit application
                    break;
                    }
                }
            catch (Exception ex)
                {
                Log.Error(ex, "Error in application flow");

                var result = MessageBox.Show(
                    $"An error occurred: {ex.Message}\n\nWould you like to try again?",
                    "Application Error",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error);

                if (result != DialogResult.Retry)
                    {
                    break;
                    }
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
            /*
            Log.Information("=== CREATING AND SHOWING MAIN FORM ===");
            // First, let's create a simple test form to verify basic functionality
            var testMainForm = new Form();
            testMainForm.Text = "InventoryPro - Test Main Form";
            testMainForm.Size = new Size(800, 600);
            testMainForm.StartPosition = FormStartPosition.CenterScreen;
            testMainForm.WindowState = FormWindowState.Maximized;

            var label = new Label();
            label.Text = "Main form loaded successfully!\nThis is a test version.";
            label.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label.AutoSize = true;
            label.Location = new Point(50, 50);
            testMainForm.Controls.Add(label);

            var closeButton = new Button();
            closeButton.Text = "Close Application";
            closeButton.Size = new Size(150, 40);
            closeButton.Location = new Point(50, 150);
            closeButton.Click += (s, e) => testMainForm.Close();
            testMainForm.Controls.Add(closeButton);

            Log.Information("Test main form created, about to show with Application.Run");

            // Show the test form first to verify routing works
            Application.Run(testMainForm);

            Log.Information("Test main form closed");
            */
            
            Log.Information("=== CREATING MAIN FORM ===");
            _mainForm = GetRequiredService<MainForm>();

            Log.Information("Main form created successfully");

            // Handle main form closing to potentially show login again
            //_mainForm.FormClosed += MainForm_FormClosed;
            _mainForm.FormClosed += (sender, e) => {
                Log.Information("Main form closed event fired");
                MainForm_FormClosed(sender, e);
            };
            // Handle main form load event for debugging
            _mainForm.Load += (sender, e) => {
                Log.Information("Main form Load event fired");
            };
            // Handle main form shown event for debugging  
            _mainForm.Shown += (sender, e) => {
                Log.Information("Main form Shown event fired");
            };

            Log.Information("About to run main form with Application.Run");
            // Show the main form as the main application window
            Application.Run(_mainForm);
            Log.Information("Application.Run completed");
            
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
            Log.Information("Main form closed, cleaning up");

            // Clean up
            if (_mainForm != null)
                {
                _mainForm.FormClosed -= MainForm_FormClosed;
                _mainForm.Dispose();
                _mainForm = null;
                }

            Log.Information("Main form cleanup completed");
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
                services.AddTransient<SalesDetailsForm>();
                services.AddTransient<SalesHistoryForm>();
                services.AddTransient<ReportForm>();
                services.AddTransient<InvoiceForm>();

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