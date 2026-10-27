using System.Text.RegularExpressions;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 文章問題のひな形（017。保護者が確認した specs/017-word-problems/templates.md と同じ文、番号も同じ）。
/// 問題づくり（WordProblemGenerator）と、解説のための読み直し（ExplanationBuilder）で使う
/// </summary>
public static class WordProblemTemplates
{
    private static readonly Dictionary<int, string> Texts = new()
    {
        [1] = "赤い 花が {a}本、白い 花が {b}本 さいています。あわせて 何本 ですか。",
        [2] = "バスに {a}人 のっています。{b}人 のってきました。みんなで 何人 ですか。",
        [3] = "シールを {a}まい もっています。友だちに {b}まい あげました。のこりは 何まい ですか。",
        [4] = "兄は {a}円、弟は {b}円 もっています。ちがいは 何円 ですか。",
        [5] = "かごに みかんが 何こか あります。{b}こ 入れたら、ぜんぶで {c}こ に なりました。はじめに 何こ ありましたか。",
        [6] = "公園で 子どもが あそんでいます。{b}人 帰ったので、{c}人 に なりました。はじめに 何人 いましたか。",
        [7] = "1はこに {a}こずつ 入った クッキーが {b}はこ あります。ぜんぶで 何こ ですか。",
        [8] = "子どもが {a}人ずつ {b}れつに ならんでいます。みんなで 何人 ですか。",
        [9] = "1こ {a}円の ガムを {b}こ 買います。何円 ですか。",
        [10] = "1日に {a}ページずつ 本を 読みます。{b}日間 では 何ページ 読めますか。",
        [11] = "{a}まいの 色紙を {b}人で 同じ数ずつ 分けます。1人 何まい ですか。",
        [12] = "えんぴつが {a}本 あります。{b}本ずつ たばに すると、何たば できますか。",
        [13] = "{a}cmの テープを {b}cmずつ 切ります。何本 できますか。",
        [14] = "{a}この あめを {b}人で 同じ数ずつ 分けます。1人 何こに なって、何こ あまりますか。",
        [15] = "{a}まいの 画用紙を 1人に {b}まいずつ くばります。何人に くばれて、何まい あまりますか。",
        [16] = "ケーキが {a}こ あります。1はこに {b}こずつ 入れます。ぜんぶ 入れるには、はこは 何こ いりますか。",
        [17] = "{a}円で 1こ {b}円の あめを 買います。何こ 買えますか。",
        [18] = "{a}円 もって 買いものに 行きました。1こ {b}円の パンを {c}こ 買うと、のこりは 何円 ですか。",
        [19] = "どんぐりを、わたしは {a}こ、妹は {b}こ ひろいました。2人の どんぐりを {c}人で 同じ数ずつ 分けると、1人 何こ ですか。",
        [20] = "1ふくろ {a}こ入りの あめを {b}ふくろ 買いました。{c}こ 食べると、のこりは 何こ ですか。"
    };

    // ひな形の文を、{a}{b}{c} に数が入る正規表現にしたもの
    private static readonly List<(int Id, Regex Pattern)> Patterns = Texts
        .Select(t => (t.Key, new Regex("^" + Regex.Replace(Regex.Escape(t.Value), @"\\\{([abc])}", "(?<$1>\\d+)") + "$")))
        .ToList();

    /// <summary>
    /// ひな形 id の {a}{b}{c} に数を入れた問題文
    /// </summary>
    public static string Format(int id, int a = 0, int b = 0, int c = 0) =>
        Texts[id].Replace("{a}", a.ToString()).Replace("{b}", b.ToString()).Replace("{c}", c.ToString());

    /// <summary>
    /// 問題文がどのひな形かを調べ、入っている数を取り出す。どのひな形でもなければ null
    /// </summary>
    public static (int Id, int A, int B, int C)? Parse(string question)
    {
        foreach (var (id, pattern) in Patterns)
        {
            var m = pattern.Match(question);
            if (m.Success)
            {
                int Get(string name) => m.Groups[name].Success ? int.Parse(m.Groups[name].Value) : 0;
                return (id, Get("a"), Get("b"), Get("c"));
            }
        }
        return null;
    }
}
