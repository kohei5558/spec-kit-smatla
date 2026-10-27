using System.Text.RegularExpressions;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 時こくと時間の解説（問題文の形は TimeProblemGenerator）
/// </summary>
public static partial class ExplanationBuilder
{
    private static readonly Regex HoursToMinutes = new(@"^(\d+)時間は 何分？$");
    private static readonly Regex HoursMinutesToMinutes = new(@"^(\d+)時間(\d+)分は 何分？$");
    private static readonly Regex MinutesToHoursMinutes = new(@"^(\d+)分は 何時間何分？$");
    private static readonly Regex MinutesSecondsToSeconds = new(@"^(\d+)分(\d+)秒は 何秒？$");
    private static readonly Regex SecondsToMinutesSeconds = new(@"^(\d+)秒は 何分何秒？$");
    private static readonly Regex Shift = new(@"^(\d+)時(?:(\d+)分)?の (?:(\d+)時間)?(\d+)分(後|前)は 何時何分？$");
    private static readonly Regex Elapsed = new(@"^(\d+)時(?:(\d+)分)?から (\d+)時(?:(\d+)分)?までは 何分？$");

    private static Explanation? Time(string question, int answer, int? remainder)
    {
        int N(Match m, int group) => m.Groups[group].Success ? int.Parse(m.Groups[group].Value) : 0;

        if (HoursToMinutes.Match(question) is { Success: true } h)
        {
            var hours = N(h, 1);
            return new(new[] { hours == 1 ? "1時間 = 60分" : $"1時間 = 60分。{hours}時間は 60 × {hours} = {hours * 60}分" }, null);
        }
        if (HoursMinutesToMinutes.Match(question) is { Success: true } hm)
        {
            return ToSmall(N(hm, 1), N(hm, 2), "時間", "分", answer);
        }
        if (MinutesToHoursMinutes.Match(question) is { Success: true } mh)
        {
            return ToPair(N(mh, 1), "時間", "分", answer, remainder);
        }
        if (MinutesSecondsToSeconds.Match(question) is { Success: true } ms)
        {
            return ToSmall(N(ms, 1), N(ms, 2), "分", "秒", answer);
        }
        if (SecondsToMinutesSeconds.Match(question) is { Success: true } sm)
        {
            return ToPair(N(sm, 1), "分", "秒", answer, remainder);
        }
        if (Shift.Match(question) is { Success: true } s)
        {
            var start = N(s, 1) * 60 + N(s, 2);
            var (hours, minutes, later) = (N(s, 3), N(s, 4), s.Groups[5].Value == "後");
            var steps = new List<string>();
            if (hours > 0)
            {
                // まず 時間 だけ動かしてから、分を動かす
                start += (later ? 60 : -60) * hours;
                steps.Add($"まず {hours}時間 {(later ? "すすめて" : "もどして")} {ClockText(start)}");
            }
            steps.Add(later ? MinutesLater(start, minutes) : MinutesEarlier(start, minutes));
            return new(steps, MinuteHint("分", remainder));
        }
        if (Elapsed.Match(question) is { Success: true } e)
        {
            var (start, end) = (N(e, 1) * 60 + N(e, 2), N(e, 3) * 60 + N(e, 4));
            var minutes = end - start;
            var next = start / 60 + 1;
            var step = start / 60 == end / 60
                ? $"{ZeroMinutes(start)}{end % 60} {Minus} {start % 60} = {minutes}分"
                : end % 60 == 0
                    ? $"{next}時まで {minutes}分"
                    : $"{next}時まで {60 - start % 60}分、{next}時から {ClockText(end)}まで {end % 60}分、あわせて {minutes}分";
            return new(new[] { step }, null);
        }
        return null;
    }

    /// <summary>
    /// 「1時間20分は 何分？」「1分30秒は 何秒？」
    /// </summary>
    private static Explanation ToSmall(int big, int small, string bigUnit, string smallUnit, int answer)
    {
        var total = big * 60 + small;
        var step = big == 1
            ? $"1{bigUnit} = 60{smallUnit}。60 + {small} = {total}{smallUnit}"
            : $"1{bigUnit} = 60{smallUnit} だから {big}{bigUnit} = {big * 60}{smallUnit}。{big * 60} + {small} = {total}{smallUnit}";
        var hundred = answer != total && answer == big * 100 + small;
        return new(new[] { step }, hundred ? HundredHint(bigUnit, smallUnit) : null);
    }

    /// <summary>
    /// 「100分は 何時間何分？」「90秒は 何分何秒？」
    /// </summary>
    private static Explanation ToPair(int total, string bigUnit, string smallUnit, int answer, int? remainder)
    {
        var (big, small) = (total / 60, total % 60);
        var step = small == 0
            ? $"60{smallUnit} = 1{bigUnit}。{total}{smallUnit}は ちょうど {big}{bigUnit}"
            : $"60{smallUnit} = 1{bigUnit}。{total}{smallUnit}は {big * 60}{smallUnit} と {small}{smallUnit} だから {big}{bigUnit}{small}{smallUnit}";
        var hint = MinuteHint(smallUnit, remainder)
            ?? ((answer, remainder) == (total / 100, total % 100) && (answer, remainder) != (big, small) ? HundredHint(bigUnit, smallUnit) : null);
        return new(new[] { step }, hint);
    }

    private static string HundredHint(string bigUnit, string smallUnit) => $"1{bigUnit}は 100{smallUnit} じゃなくて 60{smallUnit} だよ";

    // 2つ目の欄（分・秒）に 60 以上を書いた
    private static string? MinuteHint(string unit, int? remainder) => remainder >= 60
        ? unit == "秒" ? "秒は 60 より 小さく なるよ。60秒で 1分だね" : "分は 60 より 小さく なるよ。60分で 1時間だね"
        : null;

    private static string MinutesLater(int start, int minutes)
    {
        var m = start % 60;
        if (m + minutes < 60)
        {
            return $"{ZeroMinutes(start)}{m} + {minutes} = {m + minutes} で、{ClockText(start + minutes)}";
        }
        var toNext = 60 - m;
        var next = start / 60 + 1;
        return minutes == toNext
            ? $"{next}時まで {toNext}分 で ちょうど {next}時"
            : $"{next}時まで {toNext}分。のこり {minutes - toNext}分 すすめて {ClockText(start + minutes)}";
    }

    private static string MinutesEarlier(int start, int minutes)
    {
        var m = start % 60;
        if (minutes <= m)
        {
            return $"{m} {Minus} {minutes} = {m - minutes} で、{ClockText(start - minutes)}";
        }
        if (m == 0)
        {
            return $"1時間 = 60分。60 {Minus} {minutes} = {60 - minutes} で、{ClockText(start - minutes)}";
        }
        return $"{start / 60}時まで {m}分 もどる。のこり {minutes - m}分 もどって {ClockText(start - minutes)}";
    }

    // ちょうどの時刻の「0分」を先に言っておく（「11時は 11時0分。0 + 30 = 30 で…」）
    private static string ZeroMinutes(int time) => time % 60 == 0 ? $"{time / 60}時は {time / 60}時0分。" : "";

    // 「4時」「4時5分」（0時0分からの分で）
    private static string ClockText(int time) => time % 60 == 0 ? $"{time / 60}時" : $"{time / 60}時{time % 60}分";
}
