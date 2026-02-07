using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// 子供アカウント作成リクエスト
/// </summary>
public class ChildAccountCreateDto
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
    /// PINコード（4桁の数字）
    /// </summary>
    [Required(ErrorMessage = "PINは必須です")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "PINは4桁で入力してください")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "PINは4桁の数字で入力してください")]
    public string PIN { get; set; } = string.Empty;
}
