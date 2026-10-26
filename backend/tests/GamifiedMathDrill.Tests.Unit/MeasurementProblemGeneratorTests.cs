using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 長さ・重さ・かさの問題が難易度表（specs/015-measurement-units/spec.md）どおりか。
/// 問題文を読み直し、単位の関係（下の表）から答えを計算して、作られた正解・答えの形・単位と比べる
/// </summary>
public class MeasurementProblemGeneratorTests
{
    private static readonly List<Problem> Problems =
        ProblemGenerator.GenerateAll().Where(p => p.CalculationType == CalculationType.Measurement).ToList();

    // （大きい単位, 小さい単位）→ 倍率
    // dL と mL の組は使わない（大人になってもほぼ使わないため）。L と dL はなおし方（難易度1〜3）だけ
    private static readonly Dictionary<(string Big, string Small), int> Tens = new()
    {
        [("cm", "mm")] = 10, [("m", "cm")] = 100, [("L", "dL")] = 10
    };
    private static readonly Dictionary<(string Big, string Small), int> Thousands = new()
    {
        [("km", "m")] = 1000, [("kg", "g")] = 1000, [("t", "kg")] = 1000, [("L", "mL")] = 1000
    };

    // 長い単位を先に（「mL」を「m」と「L」に分けて読まないように）
    private const string Unit = "mm|cm|km|mL|dL|m|kg|g|t|L";
    private static readonly Regex Amount = new($@"(\d+)({Unit})");
    private static readonly Regex ToSingle = new($@"^((?:\d+(?:{Unit}))+)は 何({Unit})？$");
    private static readonly Regex ToPair = new($@"^((?:\d+(?:{Unit}))+)は 何({Unit})何({Unit})？$");
    private static readonly Regex AddSub = new($@"^((?:\d+(?:{Unit}))+) ([+-]) ((?:\d+(?:{Unit}))+)は 何({Unit})何({Unit})？$");

    [Fact]
    public void EachDifficulty_Has30UniqueProblems()
    {
        for (var d = 1; d <= 10; d++)
        {
            var set = Problems.Where(p => p.DifficultyLevel == d).ToList();
            // 難易度1（3つの組 × 1〜9）は、ありうる問題が27問だけ
            Assert.Equal(d == 1 ? 27 : ProblemGenerator.ProblemsPerSet, set.Count);
            Assert.Equal(set.Count, set.Select(p => p.Question).Distinct().Count());
        }
    }

    [Fact]
    public void EarlierProblemSets_AreUnchanged()
    {
        // 末尾に足したので、四則・あまりあり（1419問）と時こく（273問）は今までと同じ
        var all = ProblemGenerator.GenerateAll();
        Assert.Equal(1419, all.Count(p => p.CalculationType <= CalculationType.DivisionWithRemainder));
        Assert.Equal(273, all.Count(p => p.CalculationType == CalculationType.Time));
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
        foreach (var p in Problems.Where(p => p.DifficultyLevel == difficulty))
        {
            Check(p);
        }
    }

    private static void Check(Problem p)
    {
        var d = p.DifficultyLevel;
        var q = p.Question;
        Assert.Equal(AnswerFormat.Units, p.AnswerFormat);
        Assert.Null(p.ClockTimes);
        Assert.DoesNotMatch(@"(^|[^\d])0(" + Unit + ")", q); // 「2km0m」のように 0 の部分は書かない

        if (AddSub.Match(q) is { Success: true } a)
        {
            Assert.InRange(d, 7, 10);
            var (big, small) = (a.Groups[4].Value, a.Groups[5].Value);
            var f = Factor(big, small, d);
            Assert.NotEqual(("L", "dL"), (big, small)); // たし算・ひき算のかさは L と mL だけ
            var (x1, x2) = Parts(a.Groups[1].Value, big, small);
            var (y1, y2) = Parts(a.Groups[3].Value, big, small);
            var plus = a.Groups[2].Value == "+";
            var answer = plus ? (x1 * f + x2) + (y1 * f + y2) : (x1 * f + x2) - (y1 * f + y2);
            AssertPair(p, big, small, f, answer);
            AssertSmallStep(f, x2, y2);
            switch (d)
            {
                case 7: Assert.True(plus, q); Assert.True(x2 + y2 < f, q); break;
                case 8: Assert.True(plus, q); Assert.True(x2 + y2 >= f, q); break;
                case 9: Assert.False(plus, q); Assert.True(x2 >= y2, q); break;
                case 10: Assert.False(plus, q); Assert.True(x2 < y2, q); break;
            }
            return;
        }

        if (ToPair.Match(q) is { Success: true } t)
        {
            Assert.True(d is 3 or 6, q);
            var (big, small) = (t.Groups[2].Value, t.Groups[3].Value);
            var f = Factor(big, small, d);
            var (b, s) = Parts(t.Groups[1].Value, big, small);
            Assert.Equal(0, b); // 小さい単位だけで書いてある
            Assert.NotEqual(0, s % f);
            AssertPair(p, big, small, f, s);
            AssertSmallStep(f, s % f);
            return;
        }

        if (ToSingle.Match(q) is { Success: true } o)
        {
            Assert.True(d is 1 or 2 or 4 or 5, q);
            var small = o.Groups[2].Value;
            var big = Amount.Match(o.Groups[1].Value).Groups[2].Value;
            var f = Factor(big, small, d);
            var (b, s) = Parts(o.Groups[1].Value, big, small);
            Assert.InRange(b, 1, 9);
            if (d is 1 or 4) Assert.Equal(0, s); else Assert.InRange(s, 1, f - 1);
            AssertSmallStep(f, s);
            Assert.Equal(small, p.AnswerUnits);
            Assert.Equal(b * f + s, p.CorrectAnswer);
            Assert.Null(p.CorrectRemainder);
            return;
        }

        Assert.Fail($"問題文の形が想定外: {q}");
    }

    // 難易度1〜3は 10倍・100倍の組、4〜6は 1000倍の組、7〜10はどちらも
    private static int Factor(string big, string small, int d)
    {
        var inTens = Tens.TryGetValue((big, small), out var t);
        var inThousands = Thousands.TryGetValue((big, small), out var k);
        Assert.True(inTens || inThousands, $"知らない単位の組: {big} {small}");
        if (d <= 3) Assert.True(inTens, $"難易度{d}に 1000倍の組: {big} {small}");
        if (d is >= 4 and <= 6) Assert.True(inThousands, $"難易度{d}に 10倍・100倍の組: {big} {small}");
        return inTens ? t : k;
    }

    // 「1km300m」→ (1, 300)。大きい単位・小さい単位以外は出てこない
    private static (int Big, int Small) Parts(string amount, string big, string small)
    {
        int b = 0, s = 0;
        foreach (Match m in Amount.Matches(amount))
        {
            var n = int.Parse(m.Groups[1].Value);
            if (m.Groups[2].Value == big) b = n;
            else if (m.Groups[2].Value == small) s = n;
            else Assert.Fail($"ほかの単位が混ざっている: {amount}");
        }
        return (b, s);
    }

    private static void AssertPair(Problem p, string big, string small, int f, int valueInSmall)
    {
        Assert.Equal($"{big} {small}", p.AnswerUnits);
        Assert.Equal((valueInSmall / f, valueInSmall % f), (p.CorrectAnswer, p.CorrectRemainder));
        Assert.InRange(p.CorrectAnswer, 1, 9);
    }

    // 1000倍の組の小さい単位は10きざみ（1250m、3050g など）
    private static void AssertSmallStep(int f, params int[] smalls)
    {
        if (f == 1000)
        {
            Assert.All(smalls, s => Assert.Equal(0, s % 10));
        }
    }
}
