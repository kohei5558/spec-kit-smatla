namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 保護者ログインリクエスト
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// パスワード
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// ログイン状態を保持するフラグ
    /// false: セッションストレージ、トークン有効期限60分
    /// true: ローカルストレージ、トークン有効期限30日
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
