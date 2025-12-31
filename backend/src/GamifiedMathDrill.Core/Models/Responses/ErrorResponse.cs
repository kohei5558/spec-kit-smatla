namespace GamifiedMathDrill.Core.Models.Responses;

/// <summary>
/// APIエラーレスポンスモデル
/// </summary>
public class ErrorResponse
{
    public bool Success => false;
    public string Error { get; set; }
    public string Message { get; set; }
    public Dictionary<string, string>? Details { get; set; }

    public ErrorResponse(string error, string message, Dictionary<string, string>? details = null)
    {
        Error = error;
        Message = message;
        Details = details;
    }
}
