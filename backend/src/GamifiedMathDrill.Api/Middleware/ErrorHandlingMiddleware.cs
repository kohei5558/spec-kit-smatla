using System.Net;
using System.Text.Json;

namespace GamifiedMathDrill.Api.Middleware;

/// <summary>
/// グローバルエラーハンドリングミドルウェア
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        if (exception is ArgumentException or ArgumentNullException)
        {
            code = HttpStatusCode.BadRequest;
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
        }
        else if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var response = new
        {
            error = new
            {
                message = exception.Message,
                statusCode = (int)code
            }
        };

        result = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(result);
    }
}

/// <summary>
/// ミドルウェア拡張メソッド
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
