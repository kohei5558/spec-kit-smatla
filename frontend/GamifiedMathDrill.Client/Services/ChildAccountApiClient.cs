using GamifiedMathDrill.Client.Models;
using System.Net.Http.Json;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 子供アカウント管理APIクライアント
/// </summary>
public class ChildAccountApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public ChildAccountApiClient(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    /// <summary>
    /// 子供アカウント一覧を取得（認証済み保護者用）
    /// </summary>
    public async Task<List<ChildAccountViewModel>?> GetChildAccountsAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<ChildAccountViewModel>>("api/child-accounts");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting child accounts: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// すべての有効な子供アカウント一覧を取得（認証不要、子供ログイン画面用）
    /// </summary>
    public async Task<List<ChildAccountViewModel>?> GetPublicChildAccountsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ChildAccountViewModel>>("api/child-accounts/public");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting public child accounts: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 子供アカウントを作成
    /// </summary>
    public async Task<ChildAccountViewModel?> CreateChildAccountAsync(ChildAccountCreateRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/child-accounts", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChildAccountViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating child account: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 子供アカウント情報を取得
    /// </summary>
    public async Task<ChildAccountViewModel?> GetChildAccountAsync(string id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ChildAccountViewModel>($"api/child-accounts/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting child account: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 子供アカウント詳細情報を取得（学習統計含む）
    /// </summary>
    public async Task<ChildAccountViewModel?> GetChildAccountDetailAsync(string id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ChildAccountViewModel>($"api/child-accounts/{id}/detail");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting child account detail: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 子供アカウントを更新
    /// </summary>
    public async Task<ChildAccountViewModel?> UpdateChildAccountAsync(string id, ChildAccountUpdateRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/child-accounts/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChildAccountViewModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating child account: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 子供アカウントを削除
    /// </summary>
    public async Task<bool> DeleteChildAccountAsync(string id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/child-accounts/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting child account: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 子供アカウントを一時停止
    /// </summary>
    public async Task<bool> SuspendChildAccountAsync(string id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync($"api/child-accounts/{id}/suspend", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error suspending child account: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 子供アカウントを再開
    /// </summary>
    public async Task<bool> ActivateChildAccountAsync(string id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync($"api/child-accounts/{id}/activate", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error activating child account: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// プリセットアバター一覧を取得
    /// </summary>
    public async Task<List<PresetAvatarViewModel>?> GetPresetAvatarsAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<PresetAvatarViewModel>>("api/preset-avatars");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting preset avatars: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 子供ログイン
    /// </summary>
    public async Task<LoginResponse?> ChildLoginAsync(ChildLoginRequestModel request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/child/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during child login: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Authorization ヘッダーを設定
    /// </summary>
    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
