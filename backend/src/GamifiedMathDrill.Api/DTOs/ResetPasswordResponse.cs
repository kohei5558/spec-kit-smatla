namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// パスワードリセット実行レスポンス
/// </summary>
public class ResetPasswordResponse
{
    /// <summary>
    /// リセット成功フラグ
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 成功またはエラーメッセージ
    /// </summary>
    public string? Message { get; set; }
}
