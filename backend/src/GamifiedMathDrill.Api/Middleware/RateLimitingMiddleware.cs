using System.Collections.Concurrent;
using System.Net;

namespace GamifiedMathDrill.Api.Middleware;

/// <summary>
/// APIレート制限ミドルウェア
/// 児童の学習体験を保護し、APIの乱用を防止
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // エンドポイントごとのレート制限設定
    private static readonly Dictionary<string, RateLimitConfig> _rateLimits = new()
    {
        { "/api/problems/next", new RateLimitConfig(5, TimeSpan.FromSeconds(1)) }, // 1秒あたり5リクエスト
        { "/api/problems/", new RateLimitConfig(3, TimeSpan.FromSeconds(1)) }, // 回答送信: 1秒あたり3リクエスト
        { "/api/rewards/", new RateLimitConfig(10, TimeSpan.FromMinutes(1)) } // 景品交換: 1分あたり10リクエスト
    };

    // IPアドレスごとのリクエスト履歴
    private static readonly ConcurrentDictionary<string, RequestHistory> _requestHistories = new();

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // レート制限対象のエンドポイントを特定
        var rateLimitConfig = GetRateLimitConfig(path);
        if (rateLimitConfig == null)
        {
            await _next(context);
            return;
        }

        // リクエスト履歴を取得または作成
        var history = _requestHistories.GetOrAdd($"{clientIp}:{path}", _ => new RequestHistory());

        lock (history)
        {
            // 古いリクエストを削除
            var now = DateTime.UtcNow;
            history.Requests.RemoveAll(r => now - r > rateLimitConfig.TimeWindow);

            // レート制限チェック
            if (history.Requests.Count >= rateLimitConfig.MaxRequests)
            {
                _logger.LogWarning("レート制限超過: IP={ClientIp}, Path={Path}, Count={Count}", 
                    clientIp, path, history.Requests.Count);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";

                var retryAfter = (int)rateLimitConfig.TimeWindow.TotalSeconds;
                context.Response.Headers["Retry-After"] = retryAfter.ToString();

                var errorResponse = System.Text.Json.JsonSerializer.Serialize(new
                {
                    error = "RateLimitExceeded",
                    message = "リクエストが多すぎます。少し待ってからもう一度お試しください。",
                    retryAfter
                });

                await context.Response.WriteAsync(errorResponse);
                return;
            }

            // リクエストを記録
            history.Requests.Add(now);
        }

        await _next(context);
    }

    private static RateLimitConfig? GetRateLimitConfig(string path)
    {
        foreach (var kvp in _rateLimits)
        {
            if (path.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }
        return null;
    }

    private class RateLimitConfig
    {
        public int MaxRequests { get; }
        public TimeSpan TimeWindow { get; }

        public RateLimitConfig(int maxRequests, TimeSpan timeWindow)
        {
            MaxRequests = maxRequests;
            TimeWindow = timeWindow;
        }
    }

    private class RequestHistory
    {
        public List<DateTime> Requests { get; } = new();
    }
}

/// <summary>
/// RateLimitingMiddlewareの拡張メソッド
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
