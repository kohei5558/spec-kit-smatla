using System.ComponentModel.DataAnnotations;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 景品更新リクエスト
/// </summary>
public class UpdateRewardRequest
{
    [Required(ErrorMessage = "景品名は必須です")]
    [StringLength(100, ErrorMessage = "景品名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "説明は必須です")]
    [StringLength(500, ErrorMessage = "説明は500文字以内で入力してください")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "必要ポイントは必須です")]
    [Range(1, 10000, ErrorMessage = "必要ポイントは1〜10000の範囲で入力してください")]
    public int RequiredPoints { get; set; }

    [Required(ErrorMessage = "カテゴリは必須です")]
    public RewardCategory Category { get; set; }

    [Required(ErrorMessage = "実物景品フラグは必須です")]
    public bool IsPhysical { get; set; }

    [Range(0, 1000, ErrorMessage = "在庫数は0〜1000の範囲で入力してください")]
    public int? Stock { get; set; }

    [Required(ErrorMessage = "有効フラグは必須です")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 画像ファイル（5MB以下、image/jpeg, image/png, image/gif のみ）
    /// </summary>
    public IFormFile? Image { get; set; }

    /// <summary>
    /// 楽観的同時実行制御用のバージョン（Base64文字列で受け取ります）
    /// </summary>
    [Required(ErrorMessage = "RowVersionは必須です")]
    public string? RowVersion { get; set; }
}
