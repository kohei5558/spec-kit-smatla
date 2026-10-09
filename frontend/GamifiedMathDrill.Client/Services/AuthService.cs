using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 認証サービス
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    private readonly Func<AuthenticationStateProvider> _authStateProviderFunc;

    public AuthService(HttpClient httpClient, TokenService tokenService, Func<AuthenticationStateProvider> authStateProviderFunc)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _authStateProviderFunc = authStateProviderFunc;
    }

    /// <summary>
    /// 保護者ログイン
    /// </summary>
    public async Task<(bool success, string? errorMessage)> LoginAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null && loginResponse.Success)
                {
                    await _tokenService.SaveTokenAsync(
                        loginResponse.Token!,
                        loginResponse.UserId!,
                        loginResponse.DisplayName!,
                        loginResponse.Role!,
                        loginResponse.ParentId,
                        null,  // 保護者はStudentIdなし
                        loginResponse.ExpiresAt!.Value,
                        rememberMe
                    );
                    NotifyAuthenticationStateChanged();
                    return (true, null);
                }
                
                return (false, loginResponse?.ErrorMessage ?? "ログインに失敗しました");
            }

            // 429 Too Many Requests のチェック
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var rateLimitContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var rateLimitError = JsonSerializer.Deserialize<RateLimitError>(rateLimitContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var retryMinutes = (rateLimitError?.RetryAfterSeconds ?? 900) / 60;
                    return (false, $"試行回数が多すぎます。{retryMinutes}分後に再度お試しください。");
                }
                catch
                {
                    return (false, "試行回数が多すぎます。しばらくしてから再度お試しください。");
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
    public async Task<(bool success, string? errorMessage)> ChildLoginAsync(string childId, string pin, string deviceToken)
    {
        try
        {
            var request = new ChildLoginRequest
            {
                ChildAccountId = childId,
                PIN = pin
            };

            // 子供ログインは子供用に登録された端末からのみ受け付けられる
            using var message = new HttpRequestMessage(HttpMethod.Post, "/api/auth/child/login")
            {
                Content = JsonContent.Create(request)
            };
            message.Headers.Add(DeviceApiClient.DeviceTokenHeader, deviceToken);
            var response = await _httpClient.SendAsync(message);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null && loginResponse.Success)
                {
                    await _tokenService.SaveTokenAsync(
                        loginResponse.Token!,
                        loginResponse.UserId!,
                        loginResponse.DisplayName!,
                        loginResponse.Role!,
                        loginResponse.ParentId,
                        loginResponse.StudentId,  // 学生ID
                        loginResponse.ExpiresAt!.Value,
                        false  // 子供ログインはセッションストレージのみ
                    );
                    NotifyAuthenticationStateChanged();
                    return (true, null);
                }
                
                return (false, loginResponse?.ErrorMessage ?? "ログインに失敗しました");
            }

            // 子供ログインAPIはエラー時も LoginResponse（errorMessage）を返す
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<LoginResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (false, errorResponse?.ErrorMessage ?? "ログインに失敗しました");
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
        NotifyAuthenticationStateChanged();
    }

    /// <summary>
    /// 認証状態の変更を通知
    /// </summary>
    private void NotifyAuthenticationStateChanged()
    {
        if (_authStateProviderFunc() is CustomAuthenticationStateProvider provider)
        {
            provider.NotifyAuthenticationStateChanged();
        }
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

    /// <summary>
    /// パスワードリセットメールを送信
    /// </summary>
    public async Task<(bool success, string? message)> ForgotPasswordAsync(string email)
    {
        try
        {
            var request = new ForgotPasswordRequest
            {
                Email = email
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/forgot-password", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
                return (result?.Success ?? false, result?.Message);
            }

            // 429 Too Many Requests のチェック
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var rateLimitContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var rateLimitError = JsonSerializer.Deserialize<RateLimitError>(rateLimitContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var retryMinutes = (rateLimitError?.RetryAfterSeconds ?? 900) / 60;
                    return (false, $"試行回数が多すぎます。{retryMinutes}分後に再度お試しください。");
                }
                catch
                {
                    return (false, "試行回数が多すぎます。しばらくしてから再度お試しください。");
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent);
        }
        catch (Exception ex)
        {
            return (false, $"エラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// パスワードリセットトークンを検証
    /// </summary>
    public async Task<bool> ValidateResetTokenAsync(string token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/auth/validate-reset-token?token={Uri.EscapeDataString(token)}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result?.Data ?? false;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// パスワードをリセット
    /// </summary>
    public async Task<(bool success, string? message)> ResetPasswordAsync(string token, string newPassword, string confirmPassword)
    {
        try
        {
            var request = new ResetPasswordRequest
            {
                Token = token,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/reset-password", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
                return (result?.Success ?? false, result?.Message);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ResetPasswordResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (false, errorResponse?.Message ?? "パスワードリセットに失敗しました");
            }
            catch
            {
                return (false, "パスワードリセットに失敗しました");
            }
        }
        catch (Exception ex)
        {
            return (false, $"エラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// 新規アカウントを作成（自動ログイン）
    /// </summary>
    public async Task<(bool success, string? message, string? token)> RegisterAsync(
        string email, 
        string displayName, 
        string password, 
        string confirmPassword)
    {
        try
        {
            var request = new RegisterRequest
            {
                Email = email,
                DisplayName = displayName,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    // 自動ログイン: トークンをセッションストレージに保存（rememberMe=false）
                    await _tokenService.SetTokenAsync(result.Token, rememberMe: false);
                    return (true, result.Message, result.Token);
                }
                return (false, result?.Message ?? "登録に失敗しました", null);
            }

            // 429 Too Many Requests のチェック
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var rateLimitContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var rateLimitError = JsonSerializer.Deserialize<RateLimitError>(rateLimitContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var retryMinutes = (rateLimitError?.RetryAfterSeconds ?? 900) / 60;
                    return (false, $"試行回数が多すぎます。{retryMinutes}分後に再度お試しください。", null);
                }
                catch
                {
                    return (false, "試行回数が多すぎます。しばらくしてから再度お試しください。", null);
                }
            }

            // エラーレスポンスを解析
            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = JsonSerializer.Deserialize<RegisterResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (false, errorResponse?.Message ?? "登録に失敗しました", null);
            }
            catch
            {
                return (false, "登録に失敗しました", null);
            }
        }
        catch (Exception ex)
        {
            return (false, $"エラーが発生しました: {ex.Message}", null);
        }
    }

    /// <summary>
    /// レート制限エラーレスポンスDTO
    /// </summary>
    private class RateLimitError
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int RetryAfterSeconds { get; set; }
    }
}
