namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// パスワードリセット要求レスポンス
/// </summary>
public class ForgotPasswordResponse
{
    /// <summary>
    /// 処理成功フラグ（セキュリティのため常にtrue）
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = "メールアドレスが登録されている場合、パスワードリセットのメールをお送りしました。";
}
