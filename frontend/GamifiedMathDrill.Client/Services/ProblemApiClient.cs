using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

public class ProblemApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProblemApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ProblemDto?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, string? category = null)
    {
        var queryParams = new List<string> { $"studentId={studentId}" };
        
        if (excludeRecentIds != null && excludeRecentIds.Any())
        {
            foreach (var id in excludeRecentIds)
            {
                queryParams.Add($"excludeRecentIds={id}");
            }
        }

        if (!string.IsNullOrEmpty(category))
        {
            queryParams.Add($"category={category}");
        }

        var query = string.Join("&", queryParams);
        var response = await _httpClient.GetAsync($"api/problems/next?{query}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProblemDto>>(_jsonOptions);
            return result?.Data;
        }

        return null;
    }

    public async Task<AnswerResultDto?> SubmitAnswerAsync(int problemId, int studentId, int answer, int? remainder = null)
    {
        var submitDto = new SubmitAnswerDto { Answer = answer, Remainder = remainder };
        var response = await _httpClient.PostAsJsonAsync(
            $"api/problems/{problemId}/answer?studentId={studentId}", 
            submitDto,
            _jsonOptions);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AnswerResultDto>>(_jsonOptions);
            return result?.Data;
        }

        return null;
    }
}
