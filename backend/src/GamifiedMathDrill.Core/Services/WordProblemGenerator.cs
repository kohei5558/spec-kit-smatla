using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 文章問題（017）。文章は保護者が確認したひな形（specs/017-word-problems/templates.md）に数を入れるだけで作る。
/// ひな形の番号は templates.md と同じ
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
                        14 => New($"{a}この あめを {b}人で 同じ数ずつ 分けます。1人 何こに なって、何こ あまりますか。", q, "こ こ", a % b),
                        15 => New($"{a}まいの 画用紙を 1人に {b}まいずつ くばります。何人に くばれて、何まい あまりますか。", q, "人 まい", a % b),
                        // あまりの分も1はこいる
                        16 => New($"ケーキが {a}こ あります。1はこに {b}こずつ 入れます。ぜんぶ 入れるには、はこは 何こ いりますか。", q + 1, "こ"),
                        // あまりのお金では買えない
                        _ => New($"{a}円で 1こ {b}円の あめを 買います。何こ 買えますか。", q, "こ")
                    };
                }
            case 9:
                {
                    var (a, b) = (r.Next(10, 100), Kuku());
                    return New($"1こ {a}円の ガムを {b}こ 買います。何円 ですか。", a * b, "円");
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
            1 => New($"赤い 花が {a}本、白い 花が {b}本 さいています。あわせて 何本 ですか。", answer, "本"),
            2 => New($"バスに {a}人 のっています。{b}人 のってきました。みんなで 何人 ですか。", answer, "人"),
            3 => New($"シールを {a}まい もっています。友だちに {b}まい あげました。のこりは 何まい ですか。", answer, "まい"),
            _ => New($"兄は {a}円、弟は {b}円 もっています。ちがいは 何円 ですか。", answer, "円")
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
            return c >= 100 ? null : New($"かごに みかんが 何こか あります。{b}こ 入れたら、ぜんぶで {c}こ に なりました。はじめに 何こ ありましたか。", start, "こ");
        }

        // はじめ - b = c なので、たし算
        var left = start - b;
        return left < 10 ? null : New($"公園で 子どもが あそんでいます。{b}人 帰ったので、{left}人 に なりました。はじめに 何人 いましたか。", start, "人");
    }

    private static Problem Multiply(int template, int a, int b) => template switch
    {
        7 => New($"1はこに {a}こずつ 入った クッキーが {b}はこ あります。ぜんぶで 何こ ですか。", a * b, "こ"),
        8 => New($"子どもが {a}人ずつ {b}れつに ならんでいます。みんなで 何人 ですか。", a * b, "人"),
        _ => New($"1日に {a}ページずつ 本を 読みます。{b}日間 では 何ページ 読めますか。", a * b, "ページ")
    };

    /// <summary>
    /// ひな形11〜13（九九の逆。b でわって q）
    /// </summary>
    private static Problem Divide(int template, int b, int q) => template switch
    {
        11 => New($"{b * q}まいの 色紙を {b}人で 同じ数ずつ 分けます。1人 何まい ですか。", q, "まい"),
        12 => New($"えんぴつが {b * q}本 あります。{b}本ずつ たばに すると、何たば できますか。", q, "たば"),
        _ => New($"{b * q}cmの テープを {b}cmずつ 切ります。何本 できますか。", q, "本")
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
                    return left <= 0 ? null : New($"{money}円 もって 買いものに 行きました。1こ {price}円の パンを {count}こ 買うと、のこりは 何円 ですか。", left, "円");
                }
            case 19:
                {
                    // 2人の合計を、九九の範囲でわり切れるように分ける
                    var (people, each) = (r.Next(2, 10), r.Next(2, 10));
                    var mine = r.Next(1, people * each);
                    return New($"どんぐりを、わたしは {mine}こ、妹は {people * each - mine}こ ひろいました。2人の どんぐりを {people}人で 同じ数ずつ 分けると、1人 何こ ですか。", each, "こ");
                }
            default:
                {
                    var (a, b) = (r.Next(2, 10), r.Next(2, 10));
                    var eaten = r.Next(1, a * b);
                    return New($"1ふくろ {a}こ入りの あめを {b}ふくろ 買いました。{eaten}こ 食べると、のこりは 何こ ですか。", a * b - eaten, "こ");
                }
        }
    }

    private static Problem New(string question, int answer, string units, int? remainder = null) =>
        new() { Question = question, CorrectAnswer = answer, CorrectRemainder = remainder, AnswerUnits = units };
}
