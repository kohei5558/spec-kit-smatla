using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 交換申請承認リクエスト
/// </summary>
public class ApproveRequestRequest
{
    /// <summary>
    /// 保護者メモ（オプション）
    /// </summary>
    [MaxLength(1000, ErrorMessage = "保護者メモは1000文字以内で入力してください。")]
    public string? ParentNote { get; set; }
}
