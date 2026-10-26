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
    /// 子供アカウントを作成
    /// </summary>
    public async Task<ChildAccountSaveResult> CreateChildAccountAsync(ChildAccountCreateRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/child-accounts", request);
            response.EnsureSuccessStatusCode();
            return new ChildAccountSaveResult(await response.Content.ReadFromJsonAsync<ChildAccountViewModel>(), HasDuplicatePinWarning(response));
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
    public async Task<ChildAccountSaveResult> UpdateChildAccountAsync(string id, ChildAccountUpdateRequest request)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/child-accounts/{id}", request);
            response.EnsureSuccessStatusCode();
            return new ChildAccountSaveResult(await response.Content.ReadFromJsonAsync<ChildAccountViewModel>(), HasDuplicatePinWarning(response));
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

    /// <summary>
    /// 兄弟と同じ PIN のとき、API は X-Pin-Warning: duplicate-pin を付けて返す（005 T064）
    /// </summary>
    private static bool HasDuplicatePinWarning(HttpResponseMessage response) =>
        response.Headers.TryGetValues("X-Pin-Warning", out var values) && values.Contains("duplicate-pin");
}

/// <summary>
/// 子供アカウントの作成・更新の結果。HasDuplicatePin は兄弟と同じ PIN にしたとき true（保存はできている）
/// </summary>
public record ChildAccountSaveResult(ChildAccountViewModel? Account, bool HasDuplicatePin)
{
    public const string DuplicatePinMessage = "ほかの子供と同じ PIN です。保存はしましたが、ちがう PIN にすることをおすすめします。";
}
