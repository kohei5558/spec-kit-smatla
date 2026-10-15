namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 「◯日連続」。1問以上解いた日（日本時間）が今日または昨日から何日続いているか。学習記録から計算する
/// </summary>
public static class StudyStreak
{
    public static int ConsecutiveDays(IEnumerable<DateTime> solvedAtUtc, DateOnly today)
    {
        var days = solvedAtUtc.Select(JapanTime.ToJapanDate).ToHashSet();

        // 今日まだ解いていなくても、昨日まで続いていれば途切れていない
        var day = days.Contains(today) ? today : today.AddDays(-1);
        var count = 0;
        while (days.Contains(day))
        {
            count++;
            day = day.AddDays(-1);
        }
        return count;
    }
}
