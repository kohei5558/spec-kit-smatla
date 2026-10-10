using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 日本時間の「今日」の計算のテスト
/// </summary>
public class JapanTimeTests
{
    [Theory]
    [InlineData("2026-10-10T14:59:00Z", "2026-10-10", "2026-10-09T15:00:00Z")] // 日本時間 10/10 23:59
    [InlineData("2026-10-10T15:00:00Z", "2026-10-11", "2026-10-10T15:00:00Z")] // 日本時間 10/11 0:00
    [InlineData("2026-10-10T23:00:00Z", "2026-10-11", "2026-10-10T15:00:00Z")] // 日本時間 10/11 8:00（UTC ではまだ 10/10）
    public void Today_AndItsStartInUtc(string utcNow, string expectedDate, string expectedStartUtc)
    {
        var time = new FixedTimeProvider(DateTimeOffset.Parse(utcNow));

        Assert.Equal(DateOnly.Parse(expectedDate), JapanTime.Today(time));
        Assert.Equal(DateTimeOffset.Parse(expectedStartUtc).UtcDateTime, JapanTime.StartOfTodayUtc(time));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
