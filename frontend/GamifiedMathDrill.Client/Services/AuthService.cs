using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 認証サービス
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public AuthService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    /// <summary>
    /// 保護者ログイン
    /// </summary>
    public async Task<(bool success, string? errorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await _tokenService.SaveTokenAsync(
                        loginResponse.Token,
                        loginResponse.UserId,
                        loginResponse.DisplayName,
                        loginResponse.Role.ToString(),
                        loginResponse.ParentId,
                        loginResponse.ExpiresAt
                    );
                    return (true, null);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (false, errorResponse?.Message ?? "ログインに失敗しました");
            }
            catch
            {
                return (false, "ログインに失敗しました");
            }
        }
        catch (Exception ex)
        {
            return (false, $"エラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 子供ログイン
    /// </summary>
    public async Task<(bool success, string? errorMessage)> ChildLoginAsync(string childId, string pin)
    {
        try
        {
            var request = new ChildLoginRequest
            {
                ChildId = childId,
                PIN = pin
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/child-login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await _tokenService.SaveTokenAsync(
                        loginResponse.Token,
                        loginResponse.UserId,
                        loginResponse.DisplayName,
                        loginResponse.Role.ToString(),
                        loginResponse.ParentId,
                        loginResponse.ExpiresAt
                    );
                    return (true, null);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (false, errorResponse?.Message ?? "ログインに失敗しました");
            }
            catch
            {
                return (false, "ログインに失敗しました");
            }
        }
        catch (Exception ex)
        {
            return (false, $"エラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ログアウト
    /// </summary>
    public async Task LogoutAsync()
    {
        await _tokenService.RemoveTokenAsync();
    }

    /// <summary>
    /// 認証状態を確認
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync()
    {
        return await _tokenService.IsTokenValidAsync();
    }

    /// <summary>
    /// 現在のユーザーロールを取得
    /// </summary>
    public async Task<string?> GetCurrentUserRoleAsync()
    {
        return await _tokenService.GetUserRoleAsync();
    }

    /// <summary>
    /// 現在のユーザー名を取得
    /// </summary>
    public async Task<string?> GetCurrentUserNameAsync()
    {
        return await _tokenService.GetUserNameAsync();
    }
}
