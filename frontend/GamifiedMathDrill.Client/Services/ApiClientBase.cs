using System.Net.Http.Json;
using System.Text.Json;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// APIクライアントの基底クラス
/// 共通のHTTP通信ロジックを提供
/// </summary>
public abstract class ApiClientBase
{
    protected readonly HttpClient _httpClient;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected ApiClientBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// GETリクエストを送信してデータを取得
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GET request failed: {endpoint}, Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// POSTリクエストを送信してデータを取得
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POST request failed: {endpoint}, Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// PATCHリクエストを送信
    /// </summary>
    protected async Task<TResponse?> PatchAsync<TResponse>(string endpoint)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PATCH request failed: {endpoint}, Error: {ex.Message}");
            throw;
        }
    }
}
