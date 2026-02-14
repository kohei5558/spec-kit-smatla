using GamifiedMathDrill.Client.Models;
using System.Net.Http.Json;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 保護者ダッシュボードAPIクライアント
/// </summary>
public class ParentDashboardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public ParentDashboardApiClient(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    /// <summary>
    /// ダッシュボードサマリーを取得
    /// </summary>
    public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            Console.WriteLine("Calling API: api/parent/dashboard");
            var result = await _httpClient.GetFromJsonAsync<DashboardSummaryDto>("api/parent/dashboard");
            Console.WriteLine($"API Response: {(result != null ? "Success" : "Null")}");
            if (result != null)
            {
                Console.WriteLine($"  PendingRequestsCount: {result.PendingRequestsCount}");
                Console.WriteLine($"  Children.Count: {result.Children?.Count ?? 0}");
                Console.WriteLine($"  RecentRequests.Count: {result.RecentRequests?.Count ?? 0}");
            }
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting dashboard summary: {ex.Message}");
            Console.WriteLine($"Exception type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return null;
        }
    }

    /// <summary>
    /// 未承認の交換申請数を取得
    /// </summary>
    public async Task<int> GetPendingRequestsCountAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<int>("api/parent/pending-requests");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting pending requests count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// 保護者の子供一覧を取得
    /// </summary>
    public async Task<List<ChildDto>?> GetChildrenAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<List<ChildDto>>("api/parent/children");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting children: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 保護者の統計情報を取得
    /// </summary>
    public async Task<ParentStatisticsDto?> GetStatisticsAsync()
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ParentStatisticsDto>("api/parent/statistics");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting statistics: {ex.Message}");
            return null;
        }
    }

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
