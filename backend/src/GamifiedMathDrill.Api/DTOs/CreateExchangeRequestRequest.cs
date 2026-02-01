using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 交換申請作成リクエスト
/// </summary>
public class CreateExchangeRequestRequest
{
    [Required(ErrorMessage = "景品IDを指定してください。")]
    [Range(1, int.MaxValue, ErrorMessage = "有効な景品IDを指定してください。")]
    public int RewardId { get; set; }
}
