using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// カスタム認証状態プロバイダー
/// トークンベースの認証状態を管理
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokenService;

    public CustomAuthenticationStateProvider(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        
        if (string.IsNullOrEmpty(token))
        {
            // 未認証状態
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // トークンの有効性を確認
        var isValid = await _tokenService.IsTokenValidAsync();
        if (!isValid)
        {
            // トークンが無効な場合、削除して未認証状態を返す
            await _tokenService.RemoveTokenAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // ユーザー情報を取得
        var userId = await _tokenService.GetUserIdAsync();
        var userName = await _tokenService.GetUserNameAsync();
        var userRole = await _tokenService.GetUserRoleAsync();

        // Claimsを作成
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId ?? string.Empty),
            new Claim(ClaimTypes.Name, userName ?? string.Empty),
            new Claim(ClaimTypes.Role, userRole ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    /// <summary>
    /// 認証状態の変更を通知
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
