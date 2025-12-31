using System.Net.Http.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

public class RewardApiClient
{
    private readonly HttpClient _httpClient;

    public RewardApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RewardDto>> GetRewardsAsync(string? category = null, int? maxPoints = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(category))
            {
                queryParams.Add($"category={Uri.EscapeDataString(category)}");
            }
            if (maxPoints.HasValue)
            {
                queryParams.Add($"maxPoints={maxPoints.Value}");
            }

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"api/Rewards{query}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<RewardDto>>() ?? new List<RewardDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching rewards: {ex.Message}");
            throw;
        }
    }

    public async Task<ExchangeRewardResponse?> ExchangeRewardAsync(int rewardId, int studentId)
    {
        try
        {
            var request = new ExchangeRewardRequest { StudentId = studentId };
            var response = await _httpClient.PostAsJsonAsync($"api/Rewards/{rewardId}/exchange", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Exchange failed: {errorContent}");
                
                // Try to parse error message
                try
                {
                    var errorObj = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                    if (errorObj != null && errorObj.TryGetValue("message", out var message))
                    {
                        return new ExchangeRewardResponse { Success = false, Message = message };
                    }
                }
                catch { }
                
                return new ExchangeRewardResponse { Success = false, Message = "景品の交換に失敗しました。" };
            }

            return await response.Content.ReadFromJsonAsync<ExchangeRewardResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error exchanging reward: {ex.Message}");
            throw;
        }
    }

    public async Task<List<AcquiredRewardDto>> GetAcquiredRewardsAsync(int studentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/Rewards/acquired?studentId={studentId}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<AcquiredRewardDto>>() ?? new List<AcquiredRewardDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching acquired rewards: {ex.Message}");
            throw;
        }
    }
}
