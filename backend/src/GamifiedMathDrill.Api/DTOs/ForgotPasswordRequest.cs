using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// パスワードリセット要求リクエスト
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = string.Empty;
}
