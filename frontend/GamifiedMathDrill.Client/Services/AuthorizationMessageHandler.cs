using System.Net.Http.Headers;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// HTTP リクエストに JWT トークンを自動的に追加するハンドラー
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // TokenServiceを取得してトークンを取得
        var tokenService = _serviceProvider.GetRequiredService<TokenService>();
        var token = await tokenService.GetTokenAsync();

        // トークンが存在する場合、Authorization ヘッダーに追加
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
