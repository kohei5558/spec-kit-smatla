using System.Collections.Concurrent;

namespace GamifiedMathDrill.Api.Middleware;

/// <summary>
/// IPベースのレート制限ミドルウェア
/// ログイン試行を監視し、短時間に複数回失敗した場合に一時的にブロックする
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;

    // IPアドレスごとの失敗試行記録（メモリ内キャッシュ）
    private static readonly ConcurrentDictionary<string, List<DateTime>> _loginAttempts = new();

    // レート制限設定
    private const int MaxAttemptsPerWindow = 5; // ウィンドウ内の最大試行回数
    private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(15); // 監視ウィンドウ
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15); // ブロック期間

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 認証エンドポイントのみチェック
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var isAuthEndpoint = path.Contains("/api/auth/login") ||
                            path.Contains("/api/auth/register") ||
                            path.Contains("/api/auth/forgot-password");

        if (!isAuthEndpoint)
        {
            await _next(context);
            return;
        }

        var ipAddress = GetClientIpAddress(context);

        // レート制限チェック
        if (_loginAttempts.TryGetValue(ipAddress, out var attempts))
        {
            // 古い試行記録を削除（ウィンドウ外）
            attempts.RemoveAll(t => t < DateTime.UtcNow - WindowDuration);

            // ブロック期間内に複数回失敗している場合
            if (attempts.Count >= MaxAttemptsPerWindow)
            {
                var oldestAttempt = attempts.Min();
                var timeSinceOldest = DateTime.UtcNow - oldestAttempt;

                if (timeSinceOldest < BlockDuration)
                {
                    var remainingTime = BlockDuration - timeSinceOldest;
                    _logger.LogWarning(
                        "Rate limit exceeded for IP {IpAddress}. Blocked for {RemainingMinutes} more minutes.",
                        ipAddress,
                        remainingTime.TotalMinutes);

                    context.Response.StatusCode = 429; // Too Many Requests
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "しばらくしてから再度お試しください",
                        retryAfterSeconds = (int)remainingTime.TotalSeconds
                    });
                    return;
                }
            }
        }

        // レスポンスをキャプチャするための準備
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // 次のミドルウェアを実行
        await _next(context);

        // 認証失敗（401 Unauthorized）の場合、試行回数を記録
        if (context.Response.StatusCode == 401 && context.Request.Method == "POST")
        {
            _loginAttempts.AddOrUpdate(ipAddress,
                new List<DateTime> { DateTime.UtcNow },
                (key, existing) =>
                {
                    existing.Add(DateTime.UtcNow);
                    return existing;
                });

            _logger.LogInformation(
                "Failed authentication attempt from IP {IpAddress}. Total attempts in window: {AttemptCount}",
                ipAddress,
                _loginAttempts[ipAddress].Count);
        }

        // 認証成功の場合、記録をクリア
        if (context.Response.StatusCode == 200 && context.Request.Method == "POST")
        {
            _loginAttempts.TryRemove(ipAddress, out _);
        }

        // レスポンスを元のストリームにコピー
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }

    /// <summary>
    /// クライアントのIPアドレスを取得（プロキシ対応）
    /// </summary>
    private string GetClientIpAddress(HttpContext context)
    {
        // X-Forwarded-For ヘッダーをチェック（プロキシ経由の場合）
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // 複数のIPがある場合、最初のものを使用
            return forwardedFor.Split(',')[0].Trim();
        }

        // X-Real-IP ヘッダーをチェック
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 直接接続の場合
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// 定期的に古い記録をクリーンアップ（バックグラウンドタスクから呼び出す）
    /// </summary>
    public static void CleanupOldAttempts()
    {
        var cutoffTime = DateTime.UtcNow - WindowDuration;
        var keysToRemove = new List<string>();

        foreach (var kvp in _loginAttempts)
        {
            kvp.Value.RemoveAll(t => t < cutoffTime);
            if (kvp.Value.Count == 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _loginAttempts.TryRemove(key, out _);
        }
    }
}
