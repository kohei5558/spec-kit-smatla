using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 連続日数（1問以上解いた日を日本時間で数える）の計算のテスト
/// </summary>
public class StudyStreakTests
{
    private static readonly DateOnly Today = new(2026, 10, 11);

    [Fact]
    public void Streak_UsesJapanDate()
    {
        // UTC 10/10 15:30 は日本時間 10/11 0:30（今日）、UTC 10/10 14:30 は日本時間 10/10 23:30（昨日）
        var solved = new[] { Utc("2026-10-10T15:30:00Z"), Utc("2026-10-10T14:30:00Z") };

        Assert.Equal(2, StudyStreak.ConsecutiveDays(solved, Today));
    }

    [Fact]
    public void Streak_IsZero_WithoutRecords()
    {
        Assert.Equal(0, StudyStreak.ConsecutiveDays(Array.Empty<DateTime>(), Today));
    }

    [Fact]
    public void Streak_CountsSameDayOnce()
    {
        var solved = new[] { Utc("2026-10-11T00:00:00Z"), Utc("2026-10-11T01:00:00Z"), Utc("2026-10-10T03:00:00Z") };

        Assert.Equal(2, StudyStreak.ConsecutiveDays(solved, Today));
    }

    private static DateTime Utc(string value) => DateTimeOffset.Parse(value).UtcDateTime;
}
