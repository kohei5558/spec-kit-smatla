namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 新規アカウント作成リクエスト
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 表示名（保護者の名前）
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// パスワード（8文字以上、大文字・小文字・数字・記号を各1文字以上含む）
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// パスワード確認（passwordと一致する必要あり）
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
