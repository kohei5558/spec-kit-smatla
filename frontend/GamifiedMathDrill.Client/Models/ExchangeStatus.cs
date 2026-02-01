namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 交換申請のステータス
/// </summary>
public enum ExchangeStatus
{
    /// <summary>
    /// 申請中（承認待ち）
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 承認済み
    /// </summary>
    Approved = 1,

    /// <summary>
    /// 却下
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// キャンセル
    /// </summary>
    Cancelled = 3
}
