using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 長さ・重さ・かさの問題（015、難易度表は specs/015-measurement-units/spec.md）。
/// 量は小さい単位で数える（1km300m = 1300）
/// </summary>
public static class MeasurementProblemGenerator
{
    private const int MaxAttempts = 100_000;

    private record UnitPair(string Big, string Small, int Factor);

    // 小2で習う 10倍・100倍の組
    private static readonly UnitPair[] Tens =
    {
        new("cm", "mm", 10), new("m", "cm", 100), new("L", "dL", 10), new("dL", "mL", 100)
    };

    // 小3で習う 1000倍の組（小さい単位は10きざみにする）
    private static readonly UnitPair[] Thousands =
    {
        new("km", "m", 1000), new("kg", "g", 1000), new("t", "kg", 1000), new("L", "mL", 1000)
    };

    private static readonly UnitPair[] All = Tens.Concat(Thousands).ToArray();

    public static List<Problem> Generate(int difficulty, Random random)
    {
        var problems = new List<Problem>();
        var seen = new HashSet<string>();
        for (var attempt = 0; attempt < MaxAttempts && problems.Count < ProblemGenerator.ProblemsPerSet; attempt++)
        {
            var problem = Sample(difficulty, random);
            if (problem != null && seen.Add(problem.Question))
            {
                problem.CalculationType = CalculationType.Measurement;
                problem.DifficultyLevel = difficulty;
                problem.AnswerFormat = AnswerFormat.Units;
                problems.Add(problem);
            }
        }
        return problems;
    }

    private static Problem? Sample(int d, Random r)
    {
        UnitPair Pick(UnitPair[] pairs) => pairs[r.Next(pairs.Length)];
        // 小さい単位の部分（1〜倍率-1。1000倍の組は10きざみ）
        int SmallPart(UnitPair u) => u.Factor == 1000 ? r.Next(1, 100) * 10 : r.Next(1, u.Factor);

        switch (d)
        {
            case 1:
            case 4:
                {
                    // ○cm は 何mm？（ちょうど）
                    var u = Pick(d == 1 ? Tens : Thousands);
                    var big = r.Next(1, 10);
                    return Single($"{Text(u, big * u.Factor)}は 何{u.Small}？", big * u.Factor, u);
                }
            case 2:
            case 5:
                {
                    // ○cm○mm は 何mm？
                    var u = Pick(d == 2 ? Tens : Thousands);
                    var value = r.Next(1, 10) * u.Factor + SmallPart(u);
                    return Single($"{Text(u, value)}は 何{u.Small}？", value, u);
                }
            case 3:
            case 6:
                {
                    // ○mm は 何cm何mm？
                    var u = Pick(d == 3 ? Tens : Thousands);
                    var value = r.Next(1, 10) * u.Factor + SmallPart(u);
                    return Pair($"{value}{u.Small}は 何{u.Big}何{u.Small}？", value, u);
                }
            case 7:
            case 8:
            case 9:
            case 10:
                return AddOrSubtract(d, Pick(All), r);
            default:
                return null;
        }
    }

    /// <summary>
    /// 1km300m + 400m は 何km何m？ など。7: たし算（くり上がりなし）、8: たし算（くり上がりあり）、9: ひき算（くり下がりなし）、10: ひき算（くり下がりあり）
    /// </summary>
    private static Problem? AddOrSubtract(int d, UnitPair u, Random r)
    {
        int Small() => u.Factor == 1000 ? r.Next(0, 100) * 10 : r.Next(0, u.Factor);
        var x = r.Next(1, 10) * u.Factor + Small();
        var y = r.Next(0, 3) * u.Factor + Small();
        if (y == 0)
        {
            return null;
        }

        var plus = d is 7 or 8;
        var answer = plus ? x + y : x - y;
        var carry = x % u.Factor + y % u.Factor >= u.Factor;
        var borrow = x % u.Factor < y % u.Factor;
        var ok = d switch
        {
            7 => !carry,
            8 => carry,
            9 => !borrow,
            _ => borrow
        };
        if (!ok || answer / u.Factor < 1 || answer / u.Factor > 9)
        {
            return null;
        }

        return Pair($"{Text(u, x)} {(plus ? "+" : "-")} {Text(u, y)}は 何{u.Big}何{u.Small}？", answer, u);
    }

    // 「1km300m」「2km」「400m」（0 の部分は書かない）
    private static string Text(UnitPair u, int value)
    {
        var big = value / u.Factor;
        var small = value % u.Factor;
        return (big > 0 ? $"{big}{u.Big}" : "") + (small > 0 ? $"{small}{u.Small}" : "");
    }

    private static Problem Single(string question, int answer, UnitPair u) =>
        new() { Question = question, CorrectAnswer = answer, AnswerUnits = u.Small };

    private static Problem Pair(string question, int value, UnitPair u) =>
        new() { Question = question, CorrectAnswer = value / u.Factor, CorrectRemainder = value % u.Factor, AnswerUnits = $"{u.Big} {u.Small}" };
}
