namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 交換申請承認リクエスト
/// </summary>
public class ApproveRequestRequest
{
    /// <summary>
    /// 保護者メモ（オプション）
    /// </summary>
    public string? ParentNote { get; set; }
}
