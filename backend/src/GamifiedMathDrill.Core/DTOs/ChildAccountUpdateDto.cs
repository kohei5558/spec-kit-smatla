using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// 子供アカウント更新リクエスト
/// </summary>
public class ChildAccountUpdateDto
{
    /// <summary>
    /// 子供の名前
    /// </summary>
    [Required(ErrorMessage = "名前は必須です")]
    [MaxLength(50, ErrorMessage = "名前は50文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 学年（1〜6年生）
    /// </summary>
    [Required(ErrorMessage = "学年は必須です")]
    [Range(1, 6, ErrorMessage = "学年は1〜6の範囲で入力してください")]
    public int GradeLevel { get; set; }

    /// <summary>
    /// プリセットアバターID
    /// </summary>
    [Required(ErrorMessage = "アバターの選択は必須です")]
    public int PresetAvatarId { get; set; }

    /// <summary>
    /// 新しいPINコード（変更時のみ入力）
    /// </summary>
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PINは4桁で入力してください")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PINは4桁の数字で入力してください")]
    public string? NewPIN { get; set; }
}
