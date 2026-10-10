namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 日本時間での「今日」。日本は夏時間がないため固定の +9 時間で計算する（タイムゾーンのデータがない環境でも動くように）
/// </summary>
public static class JapanTime
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(9);

    public static DateOnly Today(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetUtcNow().ToOffset(Offset).DateTime);

    /// <summary>
    /// 日本時間の今日の0時（UTC）。DB の日時は UTC で保存している
    /// </summary>
    public static DateTime StartOfTodayUtc(TimeProvider timeProvider) =>
        Today(timeProvider).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - Offset;
}
