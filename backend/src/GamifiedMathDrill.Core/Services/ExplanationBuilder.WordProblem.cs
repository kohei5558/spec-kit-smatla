namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 文章問題の解説（ひな形の番号は WordProblemTemplates・templates.md と同じ）
/// </summary>
public static partial class ExplanationBuilder
{
    private const string ForgotSecondStep = "2つめの 計算を わすれたかも？";
    private const string Reversed = "たし算と ひき算が 逆かも？";

    private static Explanation? WordProblem(string question, int answer, int? remainder)
    {
        if (WordProblemTemplates.Parse(question) is not { } t)
        {
            return null;
        }

        var (a, b, c) = (t.A, t.B, t.C);
        return t.Id switch
        {
            1 => new(new[] { "「あわせて」は たし算", $"しき: {a} + {b} = {a + b}" }, answer == Math.Abs(a - b) ? Reversed : null),
            2 => new(new[] { "「のってきた」から ふえる。たし算", $"しき: {a} + {b} = {a + b}" }, answer == Math.Abs(a - b) ? Reversed : null),
            3 => new(new[] { "「あげた」から へる。ひき算", $"しき: {a} {Minus} {b} = {a - b}" }, answer == a + b ? Reversed : null),
            4 => new(new[] { $"「ちがい」は 大きい 数 {Minus} 小さい 数", $"しき: {a} {Minus} {b} = {a - b}" }, answer == a + b ? Reversed : null),
            5 => new(new[] { $"はじめの 数に {b}こ 入れて {c}こ。はじめの 数は {c} から {b} を ひく", $"しき: {c} {Minus} {b} = {c - b}" },
                answer == b + c ? "「入れた」でも たし算とは かぎらないよ。はじめの 数を さがそう" : null),
            6 => new(new[] { $"はじめの 数から {b}人 帰って {c}人。はじめの 数は {c} に {b} を たす", $"しき: {c} + {b} = {c + b}" },
                answer == c - b ? "「帰った」でも ひき算とは かぎらないよ。はじめの 数を さがそう" : null),
            7 or 8 or 10 => new(new[] { "「1つ分の 数 × いくつ分」で かけ算", $"しき: {a} × {b} = {a * b}" }, null),
            9 => new(new[] { $"「1こ {a}円 × {b}こ」で かけ算", MultiplySplit(a, b) }, null),
            11 => new(new[] { "「同じ数ずつ 分ける」は わり算", $"しき: {a} ÷ {b} = {a / b}" }, null),
            12 => new(new[] { $"「{b}本ずつ」で いくつ分 できるかは わり算", $"しき: {a} ÷ {b} = {a / b}" }, null),
            13 => new(new[] { $"「{b}cmずつ」で いくつ分 できるかは わり算", $"しき: {a} ÷ {b} = {a / b}" }, null),
            14 => new(new[] { "同じ数ずつ 分けるので わり算", $"しき: {a} ÷ {b} = {a / b} あまり {a % b}" }, RemainderHint(a, b, answer, remainder)),
            15 => new(new[] { $"「{b}まいずつ」くばるので わり算", $"しき: {a} ÷ {b} = {a / b} あまり {a % b}" }, RemainderHint(a, b, answer, remainder)),
            16 => new(new[] { $"{a} ÷ {b} = {a / b} あまり {a % b}", $"あまった {a % b}こも 入れる はこが いるので、{a / b} + 1 = {a / b + 1}こ" },
                answer == a / b ? $"あまった {a % b}こも はこに 入れなきゃ いけないよ" : null),
            17 => new(new[] { $"{a} ÷ {b} = {a / b} あまり {a % b}", $"あまりの {a % b}円 では 買えないので {a / b}こ" },
                answer == a / b + 1 ? $"あまりの {a % b}円 では 1こ 買えないよ" : null),
            18 => new(new[] { $"まず パンの 代金: {b} × {c} = {b * c}", $"つぎに のこり: {a} {Minus} {b * c} = {a - b * c}" },
                answer == b * c ? ForgotSecondStep : null),
            19 => new(new[] { $"まず 2人の どんぐりを あわせる: {a} + {b} = {a + b}", $"つぎに 分ける: {a + b} ÷ {c} = {(a + b) / c}" },
                answer == a + b ? ForgotSecondStep : null),
            20 => new(new[] { $"まず あめの 数: {a} × {b} = {a * b}", $"つぎに 食べた 分を ひく: {a * b} {Minus} {c} = {a * b - c}" },
                answer == a * b ? ForgotSecondStep : null),
            _ => null
        };
    }
}
