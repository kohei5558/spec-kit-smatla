using System.Net;
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

    /// <summary>
    /// 学習者の今日のチャレンジ。用意できないときは null
    /// </summary>
    public async Task<DailyChallengeDto?> GetTodaysChallengeAsync(int studentId)
    {
        var response = await _httpClient.GetAsync($"api/DailyChallenges/today?studentId={studentId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DailyChallengeDto>();
    }

    /// <summary>
    /// チャレンジに答える。今日すでに答えていたときは null（1日1回）
    /// </summary>
    public async Task<DailyChallengeAnswerResponse?> SubmitAnswerAsync(int challengeId, DailyChallengeAnswerRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/DailyChallenges/{challengeId}/answer", request);
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DailyChallengeAnswerResponse>();
    }
}
