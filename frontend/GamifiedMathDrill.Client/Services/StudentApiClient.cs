using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Client.Models;

namespace GamifiedMathDrill.Client.Services;

public class StudentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public StudentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<StudentDto?> GetStudentAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/students/{id}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(_jsonOptions);
            return result?.Data;
        }

        return null;
    }

    public async Task<StudentDto?> CreateStudentAsync(string name)
    {
        var createDto = new CreateStudentDto { Name = name };
        var response = await _httpClient.PostAsJsonAsync("api/students", createDto, _jsonOptions);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(_jsonOptions);
            return result?.Data;
        }

        return null;
    }

    public async Task<StudentDto?> UpdateLoginAsync(int id)
    {
        var response = await _httpClient.PatchAsync($"api/students/{id}/login", null);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<StudentDto>>(_jsonOptions);
            return result?.Data;
        }

        return null;
    }
}
