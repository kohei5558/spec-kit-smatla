namespace GamifiedMathDrill.Client.Models;

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
}
