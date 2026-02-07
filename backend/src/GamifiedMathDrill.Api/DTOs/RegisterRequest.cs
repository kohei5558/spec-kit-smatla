using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 新規アカウント作成リクエスト
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 表示名（保護者の名前）
    /// </summary>
    [Required(ErrorMessage = "表示名は必須です")]
    [StringLength(50, ErrorMessage = "表示名は50文字以内で入力してください")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// パスワード（8文字以上、大文字・小文字・数字・記号を各1文字以上含む）
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// パスワード確認（passwordと一致する必要あり）
    /// </summary>
    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(Password), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
