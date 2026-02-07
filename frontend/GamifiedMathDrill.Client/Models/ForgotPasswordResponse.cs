namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// パスワードリセットレスポンス
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// メッセージ
    /// </summary>
    public string? Message { get; set; }
}
