using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 交換申請却下リクエスト
/// </summary>
public class RejectRequestRequest
{
    /// <summary>
    /// 却下理由（必須）
    /// </summary>
    [Required(ErrorMessage = "却下理由を入力してください。")]
    [MaxLength(500, ErrorMessage = "却下理由は500文字以内で入力してください。")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 保護者メモ（オプション）
    /// </summary>
    [MaxLength(1000, ErrorMessage = "保護者メモは1000文字以内で入力してください。")]
    public string? ParentNote { get; set; }
}
