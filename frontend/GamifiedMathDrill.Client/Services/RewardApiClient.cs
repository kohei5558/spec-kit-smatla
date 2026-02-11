using System.Net.Http.Json;
using System.Net.Http.Headers;
using GamifiedMathDrill.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace GamifiedMathDrill.Client.Services;

public class RewardApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public RewardApiClient(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<List<RewardDto>> GetRewardsAsync(string? category = null, int? maxPoints = null)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
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
            await SetAuthorizationHeaderAsync();
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
            await SetAuthorizationHeaderAsync();
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

    /// <summary>
    /// 景品を新規作成します（保護者専用）
    /// </summary>
    public async Task<RewardDto?> CreateRewardAsync(string name, string description, int requiredPoints, 
        RewardCategory category, int? stock, bool isPhysical, IBrowserFile? imageFile)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(name), "Name");
            content.Add(new StringContent(description), "Description");
            content.Add(new StringContent(requiredPoints.ToString()), "RequiredPoints");
            content.Add(new StringContent(((int)category).ToString()), "Category");
            
            if (stock.HasValue)
            {
                content.Add(new StringContent(stock.Value.ToString()), "Stock");
            }
            
            content.Add(new StringContent(isPhysical.ToString()), "IsPhysical");
            
            if (imageFile != null)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024)); // 5MB max
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(fileContent, "Image", imageFile.Name);
            }
            
            var response = await _httpClient.PostAsync("api/Rewards", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RewardDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating reward: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 景品を更新します（保護者専用）
    /// </summary>
    public async Task<RewardDto?> UpdateRewardAsync(int id, string name, string description, int requiredPoints, 
        RewardCategory category, int? stock, bool isPhysical, bool isActive, byte[]? rowVersion, IBrowserFile? imageFile)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(name), "Name");
            content.Add(new StringContent(description), "Description");
            content.Add(new StringContent(requiredPoints.ToString()), "RequiredPoints");
            content.Add(new StringContent(((int)category).ToString()), "Category");
            
            if (stock.HasValue)
            {
                content.Add(new StringContent(stock.Value.ToString()), "Stock");
            }
            
            content.Add(new StringContent(isPhysical.ToString()), "IsPhysical");
            content.Add(new StringContent(isActive.ToString()), "IsActive");
            
            if (rowVersion != null)
            {
                content.Add(new StringContent(Convert.ToBase64String(rowVersion)), "RowVersion");
            }
            
            if (imageFile != null)
            {
                var fileContent = new StreamContent(imageFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024)); // 5MB max
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(fileContent, "Image", imageFile.Name);
            }
            
            var response = await _httpClient.PutAsync($"api/Rewards/{id}", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RewardDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating reward: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 景品を削除します（保護者専用）
    /// </summary>
    public async Task<bool> DeleteRewardAsync(int id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/Rewards/{id}");
            response.EnsureSuccessStatusCode();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting reward: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 景品の詳細を取得します
    /// </summary>
    public async Task<RewardDto?> GetRewardByIdAsync(int id)
    {
        try
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync($"api/Rewards/{id}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<RewardDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching reward: {ex.Message}");
            throw;
        }
    }
    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
