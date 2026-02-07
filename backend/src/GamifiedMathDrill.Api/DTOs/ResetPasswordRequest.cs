using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// パスワードリセット実行リクエスト
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// パスワードリセットトークン
    /// </summary>
    [Required(ErrorMessage = "トークンは必須です")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 新しいパスワード（8文字以上、大文字・小文字・数字・記号を各1文字以上含む）
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードは必須です")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// パスワード確認（newPasswordと一致する必要あり）
    /// </summary>
    [Required(ErrorMessage = "パスワード確認は必須です")]
    [Compare(nameof(NewPassword), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
