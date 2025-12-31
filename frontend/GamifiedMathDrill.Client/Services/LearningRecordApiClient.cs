using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

public class LearningRecordApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public LearningRecordApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<LearningRecordsResponseDto?> GetRecordsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? calculationType = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = $"?studentId={studentId}&page={page}&pageSize={pageSize}";
        
        if (startDate.HasValue)
        {
            query += $"&startDate={startDate.Value:yyyy-MM-dd}";
        }
        
        if (endDate.HasValue)
        {
            query += $"&endDate={endDate.Value:yyyy-MM-dd}";
        }
        
        if (!string.IsNullOrEmpty(calculationType))
        {
            query += $"&calculationType={calculationType}";
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/learning-records{query}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LearningRecordsResponseDto>>(_jsonOptions);
                return result?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching learning records: {ex.Message}");
            return null;
        }
    }

    public async Task<StatisticsDto?> GetStatisticsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = $"?studentId={studentId}";
        
        if (startDate.HasValue)
        {
            query += $"&startDate={startDate.Value:yyyy-MM-dd}";
        }
        
        if (endDate.HasValue)
        {
            query += $"&endDate={endDate.Value:yyyy-MM-dd}";
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/learning-records/statistics{query}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<StatisticsDto>>(_jsonOptions);
                return result?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching statistics: {ex.Message}");
            return null;
        }
    }
}
