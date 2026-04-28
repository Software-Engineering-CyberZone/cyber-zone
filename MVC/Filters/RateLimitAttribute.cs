using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MVC.Filters;

/// <summary>
/// Throttles requests per client IP using a sliding 1-minute window.
/// Apply as [RateLimit(10)] on controllers/actions. Requires IMemoryCache in DI.
/// </summary>
/// <example>
///     [RateLimit(5)]          // max 5 requests per minute per IP per route
///     public IActionResult Index() { ... }
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute, IAsyncActionFilter
{
    private const string CacheKeyPrefix = "rl:";
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    /// <summary>Maximum requests per window (per IP + bucket).</summary>
    public int Limit { get; }

    /// <summary>Optional bucket suffix — lets you share the counter across actions.</summary>
    public string? Bucket { get; set; }

    public RateLimitAttribute(int limit)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be >= 1");
        Limit = limit;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var cache = services.GetRequiredService<IMemoryCache>();
        var logger = services.GetRequiredService<ILogger<RateLimitAttribute>>();

        var ip = ResolveClientIp(context.HttpContext);
        var bucket = Bucket ?? $"{context.RouteData.Values["controller"]}/{context.RouteData.Values["action"]}";
        var key = $"{CacheKeyPrefix}{ip}:{bucket}";

        var now = DateTimeOffset.UtcNow;

        // Stored value: queue of timestamps for the last minute. ConcurrentQueue for thread safety.
        var timestamps = cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Window * 2;
            return new ConcurrentQueue<DateTimeOffset>();
        })!;

        // Evict old entries outside the window.
        while (timestamps.TryPeek(out var oldest) && now - oldest > Window)
            timestamps.TryDequeue(out _);

        if (timestamps.Count >= Limit)
        {
            logger.LogWarning(
                "Rate limit exceeded: IP {Ip}, bucket {Bucket}, limit {Limit}/min",
                ip, bucket, Limit);

            var retryAfter = timestamps.TryPeek(out var earliest)
                ? Math.Max(1, (int)(Window - (now - earliest)).TotalSeconds)
                : 60;
            context.HttpContext.Response.Headers["Retry-After"] = retryAfter.ToString();

            if (IsApiRequest(context.HttpContext))
            {
                context.Result = new ObjectResult(new
                {
                    error = "Too many requests",
                    retryAfterSeconds = retryAfter
                })
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("RateLimited", "Home", new { retryAfter });
            }
            return;
        }

        timestamps.Enqueue(now);
        await next();
    }

    private static string ResolveClientIp(HttpContext ctx)
    {
        // Honour X-Forwarded-For only if the app is explicitly behind a proxy.
        // For local/dev, use the direct connection.
        var forwarded = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static bool IsApiRequest(HttpContext ctx)
    {
        return ctx.Request.Path.StartsWithSegments("/api")
            || ctx.Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
