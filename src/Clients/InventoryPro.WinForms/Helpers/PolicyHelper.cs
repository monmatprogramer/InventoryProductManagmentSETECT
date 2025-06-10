using InventoryPro.WinForms.Services;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace InventoryPro.WinForms.Helpers
    {
    /// <summary>
    /// Helper class for creating HTTP client policies for resilience
    /// Provides retry and circuit breaker patterns for API communication
    /// </summary>
    public static class PolicyHelper
        {
        /// <summary>
        /// Creates a retry policy for HTTP requests
        /// Retries failed requests with exponential backoff
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
            {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException and 5XX, 408 status codes
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var logger = Program.GetService<Microsoft.Extensions.Logging.ILogger<ApiService>>();
                        logger?.LogWarning(
                            "Retry {RetryCount} for {Operation} in {Delay}ms",
                            retryCount,
                            context.OperationKey ?? "Unknown",
                            timespan.TotalMilliseconds);
                    });
            }

        /// <summary>
        /// Creates a circuit breaker policy for HTTP requests
        /// Prevents continuous calls to failing services
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
            {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, // Open circuit after 5 consecutive failures
                    durationOfBreak: TimeSpan.FromSeconds(30), // Keep circuit open for 30 seconds
                    onBreak: (delegateResult, timespan) =>
                    {
                        var logger = Program.GetService<Microsoft.Extensions.Logging.ILogger<ApiService>>();
                        logger?.LogWarning("Circuit breaker opened for {Duration}ms", timespan.TotalMilliseconds);
                    },
                    onReset: () =>
                    {
                        var logger = Program.GetService<Microsoft.Extensions.Logging.ILogger<ApiService>>();
                        logger?.LogInformation("Circuit breaker reset");
                    });
            }
        }
    }