using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 時こくと時間の問題が難易度表（specs/013-time-and-clock/spec.md）どおりか。
/// 問題文を読み直して答えを計算し、作られた正解・答えの形・時計の時刻と比べる
/// </summary>
public class TimeProblemGeneratorTests
{
    private static readonly List<Problem> TimeProblems =
        ProblemGenerator.GenerateAll().Where(p => p.CalculationType == CalculationType.Time).ToList();

    private static readonly Regex Later = new(@"^(\d+)時(?:(\d+)分)?の (?:(\d+)時間)?(\d+)分(後|前)は 何時何分？$");
    private static readonly Regex Elapsed = new(@"^(\d+)時(?:(\d+)分)?から (\d+)時(?:(\d+)分)?までは 何分？$");
    private static readonly Regex HoursToMinutes = new(@"^(\d+)時間(?:(\d+)分)?は 何分？$");
    private static readonly Regex MinutesToHours = new(@"^(\d+)分は 何時間何分？$");
    private static readonly Regex MinutesToSeconds = new(@"^(\d+)分(?:(\d+)秒)?は 何秒？$");
    private static readonly Regex SecondsToMinutes = new(@"^(\d+)秒は 何分何秒？$");

    [Fact]
    public void EachDifficulty_HasExpectedCount_AndUniqueQuestions()
    {
        for (var d = 1; d <= 10; d++)
        {
            var set = TimeProblems.Where(p => p.DifficultyLevel == d).ToList();
            // 難易度1（1〜3時間は何分？）はありうる問題が3つだけ
            Assert.Equal(d == 1 ? 3 : ProblemGenerator.ProblemsPerSet, set.Count);
            Assert.Equal(set.Count, set.Select(p => p.Question).Distinct().Count());
        }
    }

    [Fact]
    public void ExistingProblemSet_IsUnchanged()
    {
        // 時こくは種類リストの末尾に足したので、ほかの種類の問題は今までと同じ（011 の同期で入れ替わらない）
        var others = ProblemGenerator.GenerateAll().Where(p => p.CalculationType != CalculationType.Time).ToList();
        Assert.Equal(1419, others.Count);
        Assert.All(others.Where(p => p.CalculationType == CalculationType.DivisionWithRemainder), p => Assert.Equal(AnswerFormat.QuotientRemainder, p.AnswerFormat));
        Assert.All(others.Where(p => p.CalculationType != CalculationType.DivisionWithRemainder), p => Assert.Equal(AnswerFormat.Number, p.AnswerFormat));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void Problems_MatchDifficultyTable(int difficulty)
    {
        foreach (var p in TimeProblems.Where(p => p.DifficultyLevel == difficulty))
        {
            Check(p);
        }
    }

    private static void Check(Problem p)
    {
        var d = p.DifficultyLevel;
        var q = p.Question;

        if (Later.Match(q) is { Success: true } m)
        {
            var start = Time(m.Groups[1], m.Groups[2]);
            var hours = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
            var minutes = int.Parse(m.Groups[4].Value);
            var sign = m.Groups[5].Value == "後" ? 1 : -1;
            var answer = start + sign * (hours * 60 + minutes);

            Assert.True(d is 2 or 3 or 5 or 8 or 10, $"難易度{d}に何分後・前の問題: {q}");
            Assert.Equal(AnswerFormat.ClockTime, p.AnswerFormat);
            Assert.Equal((answer / 60, answer % 60), (p.CorrectAnswer, p.CorrectRemainder));
            Assert.Equal(Clock(start), p.ClockTimes);
            AssertInRange(start, answer, q);
            var crossesHour = start / 60 != answer / 60;
            switch (d)
            {
                case 2: Assert.Equal(1, sign); Assert.False(crossesHour, q); AssertStep5(start, minutes); Assert.Equal(0, hours); break;
                case 3: Assert.Equal(-1, sign); Assert.False(crossesHour, q); AssertStep5(start, minutes); Assert.Equal(0, hours); break;
                case 5: Assert.True(crossesHour, q); AssertStep5(start, minutes); Assert.Equal(0, hours); break;
                case 8: Assert.True(crossesHour, q); Assert.Equal(0, hours); break;
                case 10: Assert.InRange(hours, 1, 2); AssertStep5(start, minutes); break;
            }
            return;
        }

        if (Elapsed.Match(q) is { Success: true } e)
        {
            var start = Time(e.Groups[1], e.Groups[2]);
            var end = Time(e.Groups[3], e.Groups[4]);
            Assert.True(d is 4 or 6 or 8, $"難易度{d}にかかった時間の問題: {q}");
            Assert.Equal(AnswerFormat.Minutes, p.AnswerFormat);
            Assert.Equal(end - start, p.CorrectAnswer);
            Assert.Null(p.CorrectRemainder);
            Assert.Equal($"{Clock(start)} {Clock(end)}", p.ClockTimes);
            AssertInRange(start, end, q);
            Assert.InRange(end - start, 1, 59);
            var crossesHour = start / 60 != end / 60;
            if (d == 4) { Assert.False(crossesHour, q); AssertStep5(start, end); }
            else { Assert.True(crossesHour, q); }
            if (d == 6) AssertStep5(start, end);
            return;
        }

        Assert.Null(p.ClockTimes);
        if (HoursToMinutes.Match(q) is { Success: true } h)
        {
            var hours = int.Parse(h.Groups[1].Value);
            var minutes = h.Groups[2].Success ? int.Parse(h.Groups[2].Value) : 0;
            Assert.True(d is 1 or 7, q);
            Assert.Equal(AnswerFormat.Minutes, p.AnswerFormat);
            Assert.Equal(hours * 60 + minutes, p.CorrectAnswer);
            Assert.Null(p.CorrectRemainder);
            if (d == 1) { Assert.InRange(hours, 1, 3); Assert.False(h.Groups[2].Success, q); }
            else { Assert.InRange(hours * 60 + minutes, 61, 179); Assert.True(h.Groups[2].Success, q); }
            return;
        }

        if (MinutesToHours.Match(q) is { Success: true } mh)
        {
            var minutes = int.Parse(mh.Groups[1].Value);
            Assert.Equal(7, d);
            Assert.Equal(AnswerFormat.HoursMinutes, p.AnswerFormat);
            Assert.Equal((minutes / 60, minutes % 60), (p.CorrectAnswer, p.CorrectRemainder));
            Assert.InRange(minutes, 61, 179);
            Assert.NotEqual(0, minutes % 60);
            return;
        }

        if (MinutesToSeconds.Match(q) is { Success: true } ms)
        {
            var minutes = int.Parse(ms.Groups[1].Value);
            var seconds = ms.Groups[2].Success ? int.Parse(ms.Groups[2].Value) : 0;
            Assert.Equal(9, d);
            Assert.Equal(AnswerFormat.Seconds, p.AnswerFormat);
            Assert.Equal(minutes * 60 + seconds, p.CorrectAnswer);
            Assert.InRange(minutes * 60 + seconds, 61, 179);
            Assert.True(ms.Groups[2].Success, q);
            return;
        }

        if (SecondsToMinutes.Match(q) is { Success: true } sm)
        {
            var seconds = int.Parse(sm.Groups[1].Value);
            Assert.Equal(9, d);
            Assert.Equal(AnswerFormat.MinutesSeconds, p.AnswerFormat);
            Assert.Equal((seconds / 60, seconds % 60), (p.CorrectAnswer, p.CorrectRemainder));
            Assert.InRange(seconds, 61, 179);
            Assert.NotEqual(0, seconds % 60);
            return;
        }

        Assert.Fail($"問題文の形が想定外: {q}");
    }

    // 「3時」は 3:00、「3時40分」は 3:40（分で数える）。「3時0分」とは書かない
    private static int Time(Group hour, Group minute)
    {
        if (minute.Success)
        {
            Assert.NotEqual("0", minute.Value);
        }
        return int.Parse(hour.Value) * 60 + (minute.Success ? int.Parse(minute.Value) : 0);
    }

    private static string Clock(int minutes) => $"{minutes / 60}:{minutes % 60:00}";

    // 時刻は 1時〜11時台。12時をまたがない
    private static void AssertInRange(int a, int b, string q)
    {
        Assert.True(a >= 60 && a < 12 * 60, q);
        Assert.True(b >= 60 && b < 12 * 60, q);
    }

    private static void AssertStep5(params int[] minutes) =>
        Assert.All(minutes, m => Assert.Equal(0, m % 5));
}
