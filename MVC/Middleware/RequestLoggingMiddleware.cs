using System.Diagnostics;
using System.Security.Claims;
using Serilog.Context;

namespace MVC.Middleware;

/// <summary>
/// Logs each HTTP request with user identity, correlation id, and timing.
/// Pushes CorrelationId + UserName into Serilog LogContext so downstream logs
/// (controllers, services) are enriched automatically.
/// </summary>
public class RequestLoggingMiddleware
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString("N");

        context.Response.Headers[CorrelationHeader] = correlationId;

        var userName = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirstValue(ClaimTypes.Name) ?? "unknown"
            : "anonymous";

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserName", userName))
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP {Method} {Path}{QueryString} by {UserName} from {RemoteIp} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    userName,
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
