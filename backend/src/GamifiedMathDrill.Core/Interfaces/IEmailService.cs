namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// メール送信サービスのインターフェース
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// パスワードリセットメールを送信
    /// </summary>
    /// <param name="toEmail">送信先メールアドレス</param>
    /// <param name="resetLink">パスワードリセットリンク</param>
    /// <returns>送信成功したかどうか</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink);

    /// <summary>
    /// ウェルカムメールを送信
    /// </summary>
    /// <param name="toEmail">送信先メールアドレス</param>
    /// <param name="displayName">表示名</param>
    /// <returns>送信成功したかどうか</returns>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string displayName);
}
