using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 文章問題（017）。文章は保護者が確認したひな形（specs/017-word-problems/templates.md）に数を入れるだけで作る。
/// ひな形の文と番号は WordProblemTemplates（templates.md と同じ）
/// </summary>
public static class WordProblemGenerator
{
    private const int MaxAttempts = 100_000;

    public static List<Problem> Generate(int difficulty, Random random)
    {
        var problems = new List<Problem>();
        var seen = new HashSet<string>();
        for (var attempt = 0; attempt < MaxAttempts && problems.Count < ProblemGenerator.ProblemsPerSet; attempt++)
        {
            var problem = Sample(difficulty, random);
            if (problem != null && seen.Add(problem.Question))
            {
                problem.CalculationType = CalculationType.WordProblem;
                problem.DifficultyLevel = difficulty;
                problem.AnswerFormat = problem.CorrectRemainder.HasValue ? AnswerFormat.UnitsRemainder : AnswerFormat.Units;
                problems.Add(problem);
            }
        }
        return problems;
    }

    private static Problem? Sample(int d, Random r)
    {
        int Kuku() => r.Next(2, 10);
        T Pick<T>(params T[] items) => items[r.Next(items.Length)];

        switch (d)
        {
            case 1:
            case 2:
                return AddOrSubtract(Pick(1, 2, 3, 4), 2, regroup: d == 2, r);
            case 3:
                // 3けたの人数（バス）は不自然なので、花・シール・お金だけ
                return AddOrSubtract(Pick(1, 3, 4), 3, regroup: null, r);
            case 4:
                return Multiply(Pick(7, 8, 10), Kuku(), Kuku());
            case 5:
                return Divide(Pick(11, 12, 13), Kuku(), Kuku());
            case 6:
                // 「入れた → たし算」と言葉だけで決めると間違える問題（5・6）を半分くらい、ほかの問題と混ぜる
                if (r.Next(2) == 0)
                {
                    return Reverse(Pick(5, 6), r);
                }
                return Pick(1, 2, 3, 4, 7, 8, 10, 11, 12, 13) switch
                {
                    var t and <= 4 => AddOrSubtract(t, 2, regroup: null, r),
                    var t and (7 or 8 or 10) => Multiply(t, Kuku(), Kuku()),
                    var t => Divide(t, Kuku(), Kuku())
                };
            case 7:
            case 8:
                {
                    // 九九の範囲で、あまりがある（わる数・商が 2〜9）
                    var (b, q) = (Kuku(), Kuku());
                    var a = b * q + r.Next(1, b);
                    return Pick(d == 7 ? new[] { 14, 15 } : new[] { 16, 17 }) switch
                    {
                        14 => New(WordProblemTemplates.Format(14, a, b), q, "こ こ", a % b),
                        15 => New(WordProblemTemplates.Format(15, a, b), q, "人 まい", a % b),
                        // あまりの分も1はこいる
                        16 => New(WordProblemTemplates.Format(16, a, b), q + 1, "こ"),
                        // あまりのお金では買えない
                        _ => New(WordProblemTemplates.Format(17, a, b), q, "こ")
                    };
                }
            case 9:
                {
                    var (a, b) = (r.Next(10, 100), Kuku());
                    return New(WordProblemTemplates.Format(9, a, b), a * b, "円");
                }
            case 10:
                return TwoSteps(Pick(18, 19, 20), r);
            default:
                return null;
        }
    }

    /// <summary>
    /// ひな形1〜4。regroup は くり上がり・くり下がりがあるか（null はどちらでも）
    /// </summary>
    private static Problem? AddOrSubtract(int template, int digits, bool? regroup, Random r)
    {
        var (min, max) = digits == 2 ? (10, 100) : (100, 1000);
        var (a, b) = (r.Next(min, max), r.Next(min, max));
        var plus = template <= 2;
        var answer = plus ? a + b : a - b;
        var hasRegroup = plus ? a % 10 + b % 10 >= 10 : a % 10 < b % 10;
        if (answer <= 0 || answer >= max || (regroup.HasValue && regroup != hasRegroup))
        {
            return null;
        }

        return template switch
        {
            1 => New(WordProblemTemplates.Format(1, a, b), answer, "本"),
            2 => New(WordProblemTemplates.Format(2, a, b), answer, "人"),
            3 => New(WordProblemTemplates.Format(3, a, b), answer, "まい"),
            _ => New(WordProblemTemplates.Format(4, a, b), answer, "円")
        };
    }

    /// <summary>
    /// ひな形5・6（はじめの数をもとめる）。答えも、文章の数（{c}）も2けた
    /// </summary>
    private static Problem? Reverse(int template, Random r)
    {
        var start = r.Next(10, 100);
        var b = r.Next(2, 100);
        if (template == 5)
        {
            // はじめ + b = c なので、ひき算
            var c = start + b;
            return c >= 100 ? null : New(WordProblemTemplates.Format(5, b: b, c: c), start, "こ");
        }

        // はじめ - b = c なので、たし算
        var left = start - b;
        return left < 10 ? null : New(WordProblemTemplates.Format(6, b: b, c: left), start, "人");
    }

    private static Problem Multiply(int template, int a, int b) => template switch
    {
        7 => New(WordProblemTemplates.Format(7, a, b), a * b, "こ"),
        8 => New(WordProblemTemplates.Format(8, a, b), a * b, "人"),
        _ => New(WordProblemTemplates.Format(10, a, b), a * b, "ページ")
    };

    /// <summary>
    /// ひな形11〜13（九九の逆。b でわって q）
    /// </summary>
    private static Problem Divide(int template, int b, int q) => template switch
    {
        11 => New(WordProblemTemplates.Format(11, a: b * q, b: b), q, "まい"),
        12 => New(WordProblemTemplates.Format(12, a: b * q, b: b), q, "たば"),
        _ => New(WordProblemTemplates.Format(13, a: b * q, b: b), q, "本")
    };

    /// <summary>
    /// ひな形18〜20（2つの計算）
    /// </summary>
    private static Problem? TwoSteps(int template, Random r)
    {
        switch (template)
        {
            case 18:
                {
                    // 300〜1000円をもって、1こ 50〜200円のパンを 2〜5こ
                    var (money, price, count) = (r.Next(3, 11) * 100, r.Next(5, 21) * 10, r.Next(2, 6));
                    var left = money - price * count;
                    return left <= 0 ? null : New(WordProblemTemplates.Format(18, a: money, b: price, c: count), left, "円");
                }
            case 19:
                {
                    // 2人の合計を、九九の範囲でわり切れるように分ける
                    var (people, each) = (r.Next(2, 10), r.Next(2, 10));
                    var mine = r.Next(1, people * each);
                    return New(WordProblemTemplates.Format(19, a: mine, b: people * each - mine, c: people), each, "こ");
                }
            default:
                {
                    var (a, b) = (r.Next(2, 10), r.Next(2, 10));
                    var eaten = r.Next(1, a * b);
                    return New(WordProblemTemplates.Format(20, a, b, eaten), a * b - eaten, "こ");
                }
        }
    }

    private static Problem New(string question, int answer, string units, int? remainder = null) =>
        new() { Question = question, CorrectAnswer = answer, CorrectRemainder = remainder, AnswerUnits = units };
}
