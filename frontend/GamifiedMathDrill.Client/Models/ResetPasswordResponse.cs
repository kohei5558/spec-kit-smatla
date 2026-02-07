namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// パスワードリセット実行レスポンス
/// </summary>
public class ResetPasswordResponse
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
