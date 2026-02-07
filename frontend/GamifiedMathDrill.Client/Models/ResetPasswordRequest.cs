using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// パスワードリセット実行リクエスト
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// リセットトークン
    /// </summary>
    [Required(ErrorMessage = "トークンは必須です")]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// 新しいパスワード
    /// </summary>
    [Required(ErrorMessage = "新しいパスワードを入力してください")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "パスワードは8文字以上100文字以内で入力してください")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "パスワードは大文字、小文字、数字、記号を各1文字以上含む必要があります")]
    public string NewPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// パスワード確認
    /// </summary>
    [Required(ErrorMessage = "パスワード確認を入力してください")]
    [Compare(nameof(NewPassword), ErrorMessage = "パスワードが一致しません")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
