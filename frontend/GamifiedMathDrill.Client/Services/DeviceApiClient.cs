using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 子供用端末の登録APIクライアント。端末トークンはこの端末の LocalStorage に保存する
/// </summary>
public class DeviceApiClient
{
    /// <summary>
    /// 端末トークンを送るHTTPヘッダー名
    /// </summary>
    public const string DeviceTokenHeader = "X-Device-Token";

    private const string DeviceTokenKey = "deviceToken";
    private const string DeviceIdKey = "deviceId";

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public DeviceApiClient(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    /// <summary>
    /// この端末に保存された端末トークン（未登録なら null）
    /// </summary>
    public async Task<string?> GetDeviceTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(DeviceTokenKey);
    }

    /// <summary>
    /// この端末の登録ID（未登録なら null）
    /// </summary>
    public async Task<int?> GetDeviceIdAsync()
    {
        return await _localStorage.ContainKeyAsync(DeviceIdKey)
            ? await _localStorage.GetItemAsync<int>(DeviceIdKey)
            : null;
    }

    /// <summary>
    /// この端末を子供用に登録する（保護者ログイン中のみ）。失敗時はエラーメッセージを返す
    /// </summary>
    public async Task<(bool success, string? errorMessage)> RegisterThisDeviceAsync(string name)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/devices", new RegisterDeviceRequest { Name = name });
            if (!response.IsSuccessStatusCode)
            {
                var error = await ReadMessageAsync(response);
                return (false, error ?? "端末の登録に失敗しました");
            }

            var result = await response.Content.ReadFromJsonAsync<RegisterDeviceResponse>();
            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                return (false, "端末の登録に失敗しました");
            }

            await _localStorage.SetItemAsync(DeviceTokenKey, result.Token);
            await _localStorage.SetItemAsync(DeviceIdKey, result.Device.Id);
            return (true, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering device: {ex.Message}");
            return (false, "端末の登録中にエラーが発生しました");
        }
    }

    /// <summary>
    /// 保護者が登録した端末一覧を取得
    /// </summary>
    public async Task<List<RegisteredDeviceViewModel>?> GetDevicesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RegisteredDeviceViewModel>>("api/devices");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting devices: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 端末の登録を解除する。この端末自身を解除した場合は保存済みトークンも削除する
    /// </summary>
    public async Task<bool> RevokeDeviceAsync(int deviceId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/devices/{deviceId}");
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            if (await GetDeviceIdAsync() == deviceId)
            {
                await ClearDeviceAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error revoking device: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// この端末を登録した家庭の、利用中の子供一覧を取得する（子供ログイン画面用）。
    /// トークンが無効と分かったら端末側のトークンを削除する
    /// </summary>
    public async Task<DeviceChildrenResult> GetChildrenForThisDeviceAsync()
    {
        var token = await GetDeviceTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return new DeviceChildrenResult(false, new());
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/devices/current/children");
            request.Headers.Add(DeviceTokenHeader, token);
            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await ClearDeviceAsync();
                return new DeviceChildrenResult(false, new());
            }

            response.EnsureSuccessStatusCode();
            var children = await response.Content.ReadFromJsonAsync<List<ChildAccountViewModel>>();
            return new DeviceChildrenResult(true, children ?? new());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting children for device: {ex.Message}");
            return new DeviceChildrenResult(true, new(), "よみこみに しっぱいしました。もういちど ためしてね");
        }
    }

    /// <summary>
    /// この端末に保存された登録情報を削除する
    /// </summary>
    public async Task ClearDeviceAsync()
    {
        await _localStorage.RemoveItemAsync(DeviceTokenKey);
        await _localStorage.RemoveItemAsync(DeviceIdKey);
    }

    private static async Task<string?> ReadMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            return body != null && body.TryGetValue("message", out var message) ? message?.ToString() : null;
        }
        catch
        {
            return null;
        }
    }
}
