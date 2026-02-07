using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// パスワードリセットリクエスト
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスを入力してください")]
    [EmailAddress(ErrorMessage = "正しいメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;
}
