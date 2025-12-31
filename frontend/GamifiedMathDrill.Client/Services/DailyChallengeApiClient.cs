using System.Net.Http.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

public class DailyChallengeApiClient
{
    private readonly HttpClient _httpClient;

    public DailyChallengeApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DailyChallengeDto?> GetTodaysChallengeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/DailyChallenges/today");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DailyChallengeDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching today's challenge: {ex.Message}");
            throw;
        }
    }

    public async Task<DailyChallengeAnswerResponse?> SubmitAnswerAsync(int challengeId, DailyChallengeAnswerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/DailyChallenges/{challengeId}/answer", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DailyChallengeAnswerResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error submitting challenge answer: {ex.Message}");
            throw;
        }
    }
}
