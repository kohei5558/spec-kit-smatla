using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 文章問題が、保護者が確認したひな形（specs/017-word-problems/templates.md）と難易度表どおりか。
/// 問題文をひな形として読み直し、数から答えを計算して、作られた正解・答えの形・欄の単位と比べる
/// </summary>
public class WordProblemGeneratorTests
{
    private static readonly List<Problem> Problems =
        ProblemGenerator.GenerateAll().Where(p => p.CalculationType == CalculationType.WordProblem).ToList();

    // templates.md のひな形（文章は一字一句同じ）、欄の単位、答え（2つ目はあまり）
    private record Template(int Id, string Text, string Units, Func<int[], (int Answer, int? Remainder)> Solve);

    private static readonly Template[] Templates =
    {
        new(1, "赤い 花が {a}本、白い 花が {b}本 さいています。あわせて 何本 ですか。", "本", n => (n[0] + n[1], null)),
        new(2, "バスに {a}人 のっています。{b}人 のってきました。みんなで 何人 ですか。", "人", n => (n[0] + n[1], null)),
        new(3, "シールを {a}まい もっています。友だちに {b}まい あげました。のこりは 何まい ですか。", "まい", n => (n[0] - n[1], null)),
        new(4, "兄は {a}円、弟は {b}円 もっています。ちがいは 何円 ですか。", "円", n => (n[0] - n[1], null)),
        new(5, "かごに みかんが 何こか あります。{b}こ 入れたら、ぜんぶで {c}こ に なりました。はじめに 何こ ありましたか。", "こ", n => (n[1] - n[0], null)),
        new(6, "公園で 子どもが あそんでいます。{b}人 帰ったので、{c}人 に なりました。はじめに 何人 いましたか。", "人", n => (n[1] + n[0], null)),
        new(7, "1はこに {a}こずつ 入った クッキーが {b}はこ あります。ぜんぶで 何こ ですか。", "こ", n => (n[0] * n[1], null)),
        new(8, "子どもが {a}人ずつ {b}れつに ならんでいます。みんなで 何人 ですか。", "人", n => (n[0] * n[1], null)),
        new(9, "1こ {a}円の ガムを {b}こ 買います。何円 ですか。", "円", n => (n[0] * n[1], null)),
        new(10, "1日に {a}ページずつ 本を 読みます。{b}日間 では 何ページ 読めますか。", "ページ", n => (n[0] * n[1], null)),
        new(11, "{a}まいの 色紙を {b}人で 同じ数ずつ 分けます。1人 何まい ですか。", "まい", n => (n[0] / n[1], null)),
        new(12, "えんぴつが {a}本 あります。{b}本ずつ たばに すると、何たば できますか。", "たば", n => (n[0] / n[1], null)),
        new(13, "{a}cmの テープを {b}cmずつ 切ります。何本 できますか。", "本", n => (n[0] / n[1], null)),
        new(14, "{a}この あめを {b}人で 同じ数ずつ 分けます。1人 何こに なって、何こ あまりますか。", "こ こ", n => (n[0] / n[1], n[0] % n[1])),
        new(15, "{a}まいの 画用紙を 1人に {b}まいずつ くばります。何人に くばれて、何まい あまりますか。", "人 まい", n => (n[0] / n[1], n[0] % n[1])),
        new(16, "ケーキが {a}こ あります。1はこに {b}こずつ 入れます。ぜんぶ 入れるには、はこは 何こ いりますか。", "こ", n => (n[0] / n[1] + 1, null)),
        new(17, "{a}円で 1こ {b}円の あめを 買います。何こ 買えますか。", "こ", n => (n[0] / n[1], null)),
        new(18, "{a}円 もって 買いものに 行きました。1こ {b}円の パンを {c}こ 買うと、のこりは 何円 ですか。", "円", n => (n[0] - n[1] * n[2], null)),
        new(19, "どんぐりを、わたしは {a}こ、妹は {b}こ ひろいました。2人の どんぐりを {c}人で 同じ数ずつ 分けると、1人 何こ ですか。", "こ", n => ((n[0] + n[1]) / n[2], null)),
        new(20, "1ふくろ {a}こ入りの あめを {b}ふくろ 買いました。{c}こ 食べると、のこりは 何こ ですか。", "こ", n => (n[0] * n[1] - n[2], null))
    };

    // 難易度ごとに使うひな形（templates.md の難易度表。3けたの数は バス（2）では不自然なので 1・3・4 だけ）
    private static readonly Dictionary<int, int[]> TemplatesByDifficulty = new()
    {
        [1] = new[] { 1, 2, 3, 4 },
        [2] = new[] { 1, 2, 3, 4 },
        [3] = new[] { 1, 3, 4 },
        [4] = new[] { 7, 8, 10 },
        [5] = new[] { 11, 12, 13 },
        [6] = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13 },
        [7] = new[] { 14, 15 },
        [8] = new[] { 16, 17 },
        [9] = new[] { 9 },
        [10] = new[] { 18, 19, 20 }
    };

    [Fact]
    public void EachDifficulty_Has30UniqueProblems()
    {
        for (var d = 1; d <= 10; d++)
        {
            var set = Problems.Where(p => p.DifficultyLevel == d).ToList();
            Assert.Equal(ProblemGenerator.ProblemsPerSet, set.Count);
            Assert.Equal(set.Count, set.Select(p => p.Question).Distinct().Count());
        }
    }

    [Fact]
    public void EarlierProblemSets_AreUnchanged()
    {
        // 末尾に足したので、四則・あまりあり（1419問）・時こく（273問）・長さ重さかさ（297問）は今までと同じ
        var all = ProblemGenerator.GenerateAll();
        Assert.Equal(1419, all.Count(p => p.CalculationType <= CalculationType.DivisionWithRemainder));
        Assert.Equal(273, all.Count(p => p.CalculationType == CalculationType.Time));
        Assert.Equal(297, all.Count(p => p.CalculationType == CalculationType.Measurement));
    }

    [Fact]
    public void Questions_FitInDatabaseColumn()
    {
        // Problem.Question は 100 文字まで
        Assert.All(Problems, p => Assert.InRange(p.Question.Length, 1, 100));
    }

    [Fact]
    public void Difficulty6_MixesCarefulReadingProblems()
    {
        // 「入れた → たし算」と言葉だけで決めると間違える問題（5・6）を、ほかの問題と混ぜる
        var ids = Problems.Where(p => p.DifficultyLevel == 6).Select(p => Parse(p).Template.Id).ToList();
        Assert.InRange(ids.Count(id => id is 5 or 6), 10, 20);
        Assert.Contains(ids, id => id is not (5 or 6));
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
    public void Problems_MatchTemplatesAndDifficultyTable(int difficulty)
    {
        foreach (var p in Problems.Where(p => p.DifficultyLevel == difficulty))
        {
            var (t, n) = Parse(p);
            var q = p.Question;
            Assert.Contains(t.Id, TemplatesByDifficulty[difficulty]);

            var (answer, remainder) = t.Solve(n);
            Assert.Equal(answer, p.CorrectAnswer);
            Assert.Equal(remainder, p.CorrectRemainder);
            Assert.Equal(t.Units, p.AnswerUnits);
            Assert.Equal(remainder.HasValue ? AnswerFormat.UnitsRemainder : AnswerFormat.Units, p.AnswerFormat);
            Assert.Null(p.ClockTimes);
            Assert.True(answer > 0, q);

            switch (t.Id)
            {
                case 1 or 2 or 3 or 4:
                    var (x, y) = (n[0], n[1]);
                    var plus = t.Id <= 2;
                    Assert.True(plus || x > y, q);
                    var regroup = plus ? x % 10 + y % 10 >= 10 : x % 10 < y % 10;
                    switch (difficulty)
                    {
                        case 1: AssertDigits(2, x, y); Assert.False(regroup, q); Assert.True(answer <= 99, q); break;
                        case 2: AssertDigits(2, x, y); Assert.True(regroup, q); Assert.True(answer <= 99, q); break;
                        case 3: AssertDigits(3, x, y); Assert.True(answer <= 999, q); break;
                        default: AssertDigits(2, x, y); Assert.True(answer <= 99, q); break;
                    }
                    break;
                case 5 or 6:
                    Assert.InRange(n[0], 2, 99);
                    AssertDigits(2, n[1], answer);
                    Assert.True(answer <= 99, q);
                    break;
                case 7 or 8 or 10:
                    AssertKuku(n[0], n[1]);
                    break;
                case 11 or 12 or 13:
                    AssertKuku(n[1], answer);
                    Assert.Equal(0, n[0] % n[1]);
                    break;
                case 14 or 15 or 16 or 17:
                    // 九九の範囲（わる数・商が 2〜9）で、あまりがある
                    AssertKuku(n[1], n[0] / n[1]);
                    Assert.NotEqual(0, n[0] % n[1]);
                    break;
                case 9:
                    AssertDigits(2, n[0]);
                    Assert.InRange(n[1], 2, 9);
                    break;
                case 18:
                    Assert.Equal(0, n[0] % 100);
                    Assert.Equal(0, n[1] % 10);
                    Assert.InRange(n[2], 2, 5);
                    break;
                case 19:
                    Assert.Equal(0, (n[0] + n[1]) % n[2]);
                    AssertKuku(n[2], answer);
                    break;
                case 20:
                    AssertKuku(n[0], n[1]);
                    Assert.InRange(n[2], 1, n[0] * n[1] - 1);
                    break;
            }
        }
    }

    // 問題文がどのひな形か調べ、入っている数を取り出す（{a}{b}{c} の順）
    private static (Template Template, int[] Numbers) Parse(Problem p)
    {
        foreach (var t in Templates)
        {
            var pattern = "^" + Regex.Replace(Regex.Escape(t.Text), @"\\\{[abc]}|\{[abc]}", @"(\d+)") + "$";
            var m = Regex.Match(p.Question, pattern);
            if (m.Success)
            {
                return (t, m.Groups.Cast<Group>().Skip(1).Select(g => int.Parse(g.Value)).ToArray());
            }
        }
        Assert.Fail($"ひな形にない文章: {p.Question}");
        throw new InvalidOperationException();
    }

    private static void AssertDigits(int digits, params int[] numbers) =>
        Assert.All(numbers, n => Assert.Equal(digits, n.ToString().Length));

    private static void AssertKuku(params int[] numbers) =>
        Assert.All(numbers, n => Assert.InRange(n, 2, 9));
}
