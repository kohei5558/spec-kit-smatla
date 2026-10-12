using System.Net;
using GamifiedMathDrill.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// ログインの回数制限は接続元 IP で数える。X-Forwarded-For を偽っても逃れられない
/// （転送ヘッダーは ASPNETCORE_FORWARDEDHEADERS_ENABLED のときだけ ASP.NET Core が RemoteIpAddress に反映する）
/// </summary>
public class RateLimitMiddlewareTests
{
    [Fact]
    public async Task SpoofedForwardedFor_DoesNotBypassLimit()
    {
        // 他のテストと記録が混ざらないよう、毎回違う接続元にする
        var remoteIp = new IPAddress(new byte[] { 10, 250, (byte)Random.Shared.Next(256), (byte)Random.Shared.Next(1, 255) });
        var middleware = new RateLimitMiddleware(context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }, NullLogger<RateLimitMiddleware>.Instance);

        var statuses = new List<int>();
        for (var i = 1; i <= 6; i++)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.Path = "/api/auth/login";
            context.Connection.RemoteIpAddress = remoteIp;
            context.Request.Headers["X-Forwarded-For"] = $"203.0.113.{i}";
            context.Request.Headers["X-Real-IP"] = $"198.51.100.{i}";
            context.Response.Body = new MemoryStream();
            await middleware.InvokeAsync(context);
            statuses.Add(context.Response.StatusCode);
        }

        Assert.Equal(new[] { 401, 401, 401, 401, 401, 429 }, statuses);
    }
}
