namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// エラーレスポンス
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// エラーコード
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// エラー詳細（デバッグ用）
    /// </summary>
    public string? Details { get; set; }
}
