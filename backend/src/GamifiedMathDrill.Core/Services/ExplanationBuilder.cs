using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// まちがえたときの解説。Steps は考え方（最後の行で答えにたどりつく）、Hint はよくある まちがいの一言（当てはまらなければ null）
/// </summary>
public record Explanation(IReadOnlyList<string> Steps, string? Hint);

/// <summary>
/// 問題文を読み直して、まちがえたときの解説を作る（019、文は specs/019-explanations/explanations.md）。
/// 読めない問題文（古い問題など）は null
/// </summary>
public static partial class ExplanationBuilder
{
    // 解説の中のひき算の記号（画面のひっ算と同じ）
    private const string Minus = "−";

    public static Explanation? Build(Problem problem, int answer, int? remainder) => problem.CalculationType switch
    {
        CalculationType.Time => Time(problem.Question, answer, remainder),
        CalculationType.Measurement => Measurement(problem.Question, answer, remainder),
        CalculationType.WordProblem => WordProblem(problem.Question, answer, remainder),
        _ => Arithmetic(problem.CalculationType, problem.Question, answer, remainder)
    };

    private static readonly Regex Expression = new(@"^(\d+) ([+\-×÷]) (\d+) = \?$");

    private static Explanation? Arithmetic(CalculationType type, string question, int answer, int? remainder)
    {
        var m = Expression.Match(question);
        if (!m.Success)
        {
            return null;
        }

        var (a, b) = (int.Parse(m.Groups[1].Value), int.Parse(m.Groups[3].Value));
        return type switch
        {
            CalculationType.Addition => Add(a, b, answer),
            CalculationType.Subtraction => Subtract(a, b, answer),
            CalculationType.Multiplication => Multiply(a, b, answer),
            CalculationType.Division => Divide(a, b, answer),
            CalculationType.DivisionWithRemainder => DivideWithRemainder(a, b, answer, remainder),
            _ => null
        };
    }

    private static Explanation Add(int a, int b, int answer)
    {
        var sum = a + b;
        var steps = new List<string>();
        if (a < 10 && b < 10)
        {
            // 10 をこえるときは、10 のまとまりを作る（さくらんぼ計算）。こえないときは式だけ
            if (sum >= 10)
            {
                var need = 10 - a;
                steps.Add(sum == 10
                    ? $"{a} と {b} で ちょうど 10"
                    : $"{a} は あと {need} で 10。{b} を {need} と {b - need} に 分けて、10 と {b - need} で {sum}");
            }
        }
        else
        {
            steps.Add("一の位から じゅんに たそう。" + RegroupText(Carries(a, b), "くり上がり", "（上の位に 1 を たす）"));
        }
        steps.Add($"しき: {a} + {b} = {sum}");

        string? hint = null;
        if (answer != sum && answer == AddWithoutCarry(a, b))
        {
            hint = "くり上がりを わすれたかも？";
        }
        else if (answer == Math.Abs(a - b))
        {
            hint = "ひき算に なっているかも？ たし算だよ";
        }
        return new(steps, hint);
    }

    private static Explanation Subtract(int a, int b, int answer)
    {
        var diff = a - b;
        var steps = new List<string>();
        if (a >= 10 && a < 20 && b < 10 && a % 10 < b)
        {
            // 13 − 8 は 10 から ひく（くり下がりのある 1けたの答え）
            steps.Add($"{a} を 10 と {a - 10} に 分けて、10 {Minus} {b} = {10 - b}、{10 - b} と {a - 10} で {diff}");
        }
        else if (a >= 10)
        {
            steps.Add("一の位から じゅんに ひこう。ひけないときは 上の位から 1 かりて 10 に する。" + RegroupText(Borrows(a, b), "くり下がり", ""));
        }
        steps.Add($"しき: {a} {Minus} {b} = {diff}");

        string? hint = null;
        if (answer != diff && answer == SubtractWithoutBorrow(a, b))
        {
            hint = "くり下がりを わすれたかも？ ひけない 位は 上の位から 1 かりてね";
        }
        else if (answer == a + b)
        {
            hint = "たし算に なっているかも？ ひき算だよ";
        }
        return new(steps, hint);
    }

    private static Explanation Multiply(int a, int b, int answer)
    {
        var product = a * b;
        var steps = new List<string>();
        if (a < 10 && b < 10)
        {
            steps.Add($"{a} の だん。{a} が {b}こ で {product}");
        }
        else
        {
            steps.Add(MultiplySplit(a, b));
        }
        steps.Add($"しき: {a} × {b} = {product}");

        var neighbor = a < 10 && b < 10 && (answer == product - a || answer == product + a || answer == product - b || answer == product + b);
        return new(steps, neighbor ? "おしい！ 1つ となりの 九九かも？" : null);
    }

    /// <summary>
    /// 「23 を 20 と 3 に 分けて、20 × 4 = 80、3 × 4 = 12、あわせて 92」。かける数が2けたなら、かける数を分ける
    /// </summary>
    private static string MultiplySplit(int a, int b)
    {
        var product = a * b;
        if (b < 10)
        {
            var parts = PlaceParts(a);
            if (parts.Count == 1)
            {
                var unit = PlaceUnit(a);
                return $"{a / unit} × {b} = {a / unit * b} の {unit}こ分 で {product}";
            }
            return $"{a} を {string.Join(" と ", parts)} に 分けて、{string.Join("、", parts.Select(x => $"{x} × {b} = {x * b}"))}、あわせて {product}";
        }
        else
        {
            var parts = PlaceParts(b);
            if (parts.Count == 1)
            {
                var unit = PlaceUnit(b);
                return $"{a} × {b / unit} = {a * (b / unit)} の {unit}こ分 で {product}";
            }
            return $"{b} を {string.Join(" と ", parts)} に 分けて、{string.Join("、", parts.Select(x => $"{a} × {x} = {a * x}"))}、あわせて {product}";
        }
    }

    private static Explanation Divide(int a, int b, int answer)
    {
        var q = a / b;
        var steps = new List<string>();
        if (q < 10)
        {
            steps.Add($"{b} の だんで {a} に なる 数を さがそう。{b} × {q} = {a} だから {q}");
        }
        else
        {
            // 商を位ごとに分けて、わられる数もその分ずつに分ける
            var parts = PlaceParts(q);
            if (parts.Count == 1)
            {
                var unit = PlaceUnit(q);
                steps.Add($"{a / unit} ÷ {b} = {q / unit} の {unit}こ分 で {q}");
            }
            else
            {
                steps.Add($"{a} を {string.Join(" と ", parts.Select(x => x * b))} に 分けて、{string.Join("、", parts.Select(x => $"{x * b} ÷ {b} = {x}"))}、あわせて {q}");
            }
        }
        steps.Add($"しき: {a} ÷ {b} = {q}");
        return new(steps, answer == a * b ? "かけ算に なっているかも？ わり算だよ" : null);
    }

    private static Explanation DivideWithRemainder(int a, int b, int answer, int? remainder)
    {
        var (q, r) = (a / b, a % b);
        if (q < 10)
        {
            var steps = new[]
            {
                $"{b} の だんで {a} を こえない いちばん 大きい 数は {b} × {q} = {b * q}",
                $"{a} {Minus} {b * q} = {r} あまる。こたえは {q} あまり {r}"
            };
            return new(steps, RemainderHint(a, b, answer, remainder));
        }

        // 商が2けた以上: わり切れるわり算と同じく分けて、さいごの分から あまりが出る
        var parts = PlaceParts(q);
        var split = parts.Count == 1
            ? $"{a} を {b * q} と {r} に 分けて、{b * q} ÷ {b} = {q}。{r} は {b} で われないので あまり"
            : $"{a} を {string.Join(" と ", parts.Select((x, i) => x * b + (i == parts.Count - 1 ? r : 0)))} に 分けて、"
                + string.Join("、", parts.Select((x, i) => i == parts.Count - 1 ? $"{x * b + r} ÷ {b} = {x} あまり {r}" : $"{x * b} ÷ {b} = {x}"));
        return new(new[] { split, $"こたえは {q} あまり {r}" }, RemainderHint(a, b, answer, remainder));
    }

    private static string? RemainderHint(int a, int b, int answer, int? remainder)
    {
        if (remainder >= b)
        {
            return $"あまりは わる数の {b} より 小さく なるよ。もう 1つ ふやせるかも？";
        }
        if (answer == a / b + 1)
        {
            return $"{b} × {answer} = {b * answer} は {a} を こえちゃうよ";
        }
        return null;
    }

    // ---- 位の計算 ----

    private static readonly string[] PlaceNames = { "一の位", "十の位", "百の位", "千の位", "一万の位" };

    /// <summary>
    /// 「くり上がりは 一の位と 十の位で 1回ずつ」「くり上がりは ないよ」
    /// </summary>
    private static string RegroupText(List<int> places, string word, string suffix) => places.Count switch
    {
        0 => $"{word}は ないよ",
        1 => $"{word}は {PlaceNames[places[0]]}で 1回{suffix}",
        _ => $"{word}は {string.Join("と ", places.Select(p => PlaceNames[p]))}で 1回ずつ{suffix}"
    };

    // くり上がりが出た位（0 が一の位）
    private static List<int> Carries(int a, int b)
    {
        var places = new List<int>();
        var carry = 0;
        for (var i = 0; a > 0 || b > 0; i++, a /= 10, b /= 10)
        {
            carry = a % 10 + b % 10 + carry >= 10 ? 1 : 0;
            if (carry == 1)
            {
                places.Add(i);
            }
        }
        return places;
    }

    // くり下がりが出た位（上の位から かりた位）
    private static List<int> Borrows(int a, int b)
    {
        var places = new List<int>();
        var borrow = 0;
        for (var i = 0; a > 0 || b > 0; i++, a /= 10, b /= 10)
        {
            borrow = a % 10 - b % 10 - borrow < 0 ? 1 : 0;
            if (borrow == 1)
            {
                places.Add(i);
            }
        }
        return places;
    }

    // 位ごとにたして、くり上がりを足さなかった数（345 + 278 → 513）
    private static int AddWithoutCarry(int a, int b) => Digitwise(a, b, (x, y) => (x + y) % 10);

    // 位ごとに「大きい数字 − 小さい数字」をした数（623 − 278 → 455）
    private static int SubtractWithoutBorrow(int a, int b) => Digitwise(a, b, (x, y) => Math.Abs(x - y));

    private static int Digitwise(int a, int b, Func<int, int, int> digit)
    {
        int result = 0, unit = 1;
        for (; a > 0 || b > 0; a /= 10, b /= 10, unit *= 10)
        {
            result += digit(a % 10, b % 10) * unit;
        }
        return result;
    }

    // 0 でない位の数に分ける（123 → 100, 20, 3）
    private static List<int> PlaceParts(int n)
    {
        var parts = new List<int>();
        for (var unit = 1; n > 0; n /= 10, unit *= 10)
        {
            if (n % 10 != 0)
            {
                parts.Insert(0, n % 10 * unit);
            }
        }
        return parts;
    }

    // 0 でない位が1つだけの数（300 など）の、その位の大きさ（100）
    private static int PlaceUnit(int n)
    {
        var unit = 1;
        while (n % (unit * 10) == 0)
        {
            unit *= 10;
        }
        return unit;
    }
}
