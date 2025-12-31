namespace GamifiedMathDrill.Core.Models.Responses;

/// <summary>
/// API成功レスポンスモデル
/// </summary>
public class SuccessResponse<T>
{
    public bool Success => true;
    public T Data { get; set; }
    public string? Message { get; set; }

    public SuccessResponse(T data, string? message = null)
    {
        Data = data;
        Message = message;
    }
}

/// <summary>
/// データを含まない成功レスポンス
/// </summary>
public class SuccessResponse : SuccessResponse<object>
{
    public SuccessResponse(string? message = null) : base(new { }, message)
    {
    }
}
