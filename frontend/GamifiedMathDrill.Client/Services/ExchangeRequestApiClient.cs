using GamifiedMathDrill.Client.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// 交換申請APIクライアント
/// </summary>
public class ExchangeRequestApiClient : ApiClientBase
{
    private readonly TokenService _tokenService;

    public ExchangeRequestApiClient(HttpClient httpClient, TokenService tokenService) 
        : base(httpClient)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// 交換申請を作成
    /// </summary>
    public async Task<ApiResponse<ExchangeRequestDto>> CreateRequestAsync(int rewardId)
    {
        await SetAuthorizationHeaderAsync();
        
        var request = new CreateExchangeRequestRequest { RewardId = rewardId };
        var response = await _httpClient.PostAsJsonAsync("api/exchangerequests", request);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ExchangeRequestDto>();
            return new ApiResponse<ExchangeRequestDto>
            {
                Success = true,
                Data = data
            };
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return new ApiResponse<ExchangeRequestDto>
        {
            Success = false,
            Message = $"交換申請の作成に失敗しました: {errorContent}"
        };
    }

    /// <summary>
    /// 自分の交換申請一覧を取得
    /// </summary>
    public async Task<ApiResponse<List<ExchangeRequestDto>>> GetMyRequestsAsync()
    {
        await SetAuthorizationHeaderAsync();

        var response = await _httpClient.GetAsync("api/exchangerequests/my");

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<List<ExchangeRequestDto>>();
            return new ApiResponse<List<ExchangeRequestDto>>
            {
                Success = true,
                Data = data
            };
        }

        return new ApiResponse<List<ExchangeRequestDto>>
        {
            Success = false,
            Message = "交換申請の取得に失敗しました。"
        };
    }

    /// <summary>
    /// 交換申請をキャンセル
    /// </summary>
    public async Task<ApiResponse<object>> CancelRequestAsync(int requestId)
    {
        await SetAuthorizationHeaderAsync();

        var response = await _httpClient.PutAsync($"api/exchangerequests/{requestId}/cancel", null);

        if (response.IsSuccessStatusCode)
        {
            return new ApiResponse<object>
            {
                Success = true,
                Message = "交換申請をキャンセルしました。"
            };
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return new ApiResponse<object>
        {
            Success = false,
            Message = $"キャンセルに失敗しました: {errorContent}"
        };
    }

    /// <summary>
    /// 全交換申請を取得（保護者用）
    /// </summary>
    public async Task<ApiResponse<List<ExchangeRequestDto>>> GetAllRequestsAsync(string? status = null)
    {
        await SetAuthorizationHeaderAsync();

        var url = "api/exchangerequests";
        if (!string.IsNullOrEmpty(status))
        {
            url += $"?status={status}";
        }

        var response = await _httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<List<ExchangeRequestDto>>();
            return new ApiResponse<List<ExchangeRequestDto>>
            {
                Success = true,
                Data = data
            };
        }

        return new ApiResponse<List<ExchangeRequestDto>>
        {
            Success = false,
            Message = "交換申請の取得に失敗しました。"
        };
    }

    /// <summary>
    /// 交換申請の詳細を取得（保護者用）
    /// </summary>
    public async Task<ApiResponse<ExchangeRequestDto>> GetRequestByIdAsync(int requestId)
    {
        await SetAuthorizationHeaderAsync();

        var response = await _httpClient.GetAsync($"api/exchangerequests/{requestId}");

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ExchangeRequestDto>();
            return new ApiResponse<ExchangeRequestDto>
            {
                Success = true,
                Data = data
            };
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new ApiResponse<ExchangeRequestDto>
            {
                Success = false,
                Message = "交換申請が見つかりません。"
            };
        }

        return new ApiResponse<ExchangeRequestDto>
        {
            Success = false,
            Message = "交換申請の取得に失敗しました。"
        };
    }

    /// <summary>
    /// 交換申請を承認（保護者用）
    /// </summary>
    public async Task<ApiResponse<ExchangeRequestDto>> ApproveRequestAsync(int requestId, string? parentNote = null)
    {
        await SetAuthorizationHeaderAsync();

        var requestBody = new { ParentNote = parentNote };
        var response = await _httpClient.PutAsJsonAsync($"api/exchangerequests/{requestId}/approve", requestBody);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ExchangeRequestDto>();
            return new ApiResponse<ExchangeRequestDto>
            {
                Success = true,
                Data = data,
                Message = "交換申請を承認しました。"
            };
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return new ApiResponse<ExchangeRequestDto>
        {
            Success = false,
            Message = $"承認に失敗しました: {errorContent}"
        };
    }

    /// <summary>
    /// 交換申請を却下（保護者用）
    /// </summary>
    public async Task<ApiResponse<ExchangeRequestDto>> RejectRequestAsync(int requestId, string reason, string? parentNote = null)
    {
        await SetAuthorizationHeaderAsync();

        var requestBody = new { Reason = reason, ParentNote = parentNote };
        var response = await _httpClient.PutAsJsonAsync($"api/exchangerequests/{requestId}/reject", requestBody);

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<ExchangeRequestDto>();
            return new ApiResponse<ExchangeRequestDto>
            {
                Success = true,
                Data = data,
                Message = "交換申請を却下しました。"
            };
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        return new ApiResponse<ExchangeRequestDto>
        {
            Success = false,
            Message = $"却下に失敗しました: {errorContent}"
        };
    }

    private async Task SetAuthorizationHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
