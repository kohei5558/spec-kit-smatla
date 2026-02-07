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
    
    /// <summary>
    /// ログイン状態を保持する（true=ローカルストレージ30日、false=セッションストレージ60分）
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
