using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// まちがえたときの解説（019、文は specs/019-explanations/explanations.md）
/// </summary>
public class ExplanationBuilderTests
{
    // 解説は算数の問題だけ（漢字の解説は、文の一覧を確認してから足す、020）
    private static readonly List<Problem> AllProblems = ProblemGenerator.GenerateAll().Where(p => Subjects.Of(p.CalculationType) == Subject.Math).ToList();

    [Fact]
    public void EveryProblem_HasExplanation_EndingWithTheCorrectAnswer()
    {
        foreach (var p in AllProblems)
        {
            var e = ExplanationBuilder.Build(p, p.CorrectAnswer + 1, p.CorrectRemainder);
            Assert.True(e != null, $"解説が無い: {p.Question}");
            Assert.InRange(e!.Steps.Count, 1, 4);
            Assert.All(e.Steps, s => Assert.False(string.IsNullOrWhiteSpace(s)));

            // 最後の行の終わりの数が正解（2つ目の答えがあるときは、その前の数と2つ）
            var numbers = Regex.Matches(e.Steps[^1], @"\d+").Select(m => int.Parse(m.Value)).ToList();
            var r = p.CorrectRemainder;
            var endsWithAnswer = r is null or 0
                ? numbers[^1] == p.CorrectAnswer || (r == 0 && numbers.Count >= 2 && (numbers[^2], numbers[^1]) == (p.CorrectAnswer, 0))
                : numbers.Count >= 2 && (numbers[^2], numbers[^1]) == (p.CorrectAnswer, r.Value);
            Assert.True(endsWithAnswer, $"{p.Question} → {string.Join(" / ", e.Steps)}（正解 {p.CorrectAnswer} {r}）");
        }
    }

    // 漢字の解説（022、型は specs/022-kanji-explanations/explanations.md）。中身は確認済みの KanjiBatches から作る
    [Fact]
    public void EveryKanjiProblem_HasExplanation_FromTheBatch()
    {
        var problems = ProblemGenerator.GenerateAll().Where(p => Subjects.Of(p.CalculationType) == Subject.Japanese).ToList();
        Assert.Equal(88 + 90 + 90, problems.Count); // 1回目88問（ことば28）＋2回目90問＋3回目90問
        foreach (var p in problems)
        {
            var e = ExplanationBuilder.Build(p, 0, null, "ん");
            Assert.True(e != null, p.Question);
            Assert.Contains(p.CorrectText!, e!.Steps[0]);
            Assert.Null(e.Hint);
        }
    }

    [Theory]
    [InlineData(CalculationType.KanjiReading, "[駅]まで あるく。", "えき", "「駅」は「えき」と 読むよ|ことば: 駅員（えきいん）")]
    [InlineData(CalculationType.KanjiReading, "[野球]を する。", "やきゅう", "「野球」は「やきゅう」と 読むよ|ことば: 地球（ちきゅう）")]
    [InlineData(CalculationType.KanjiReading, "[寒]い 日が つづく。", "さむ", "「寒」は「さむ」と 読むよ")]
    [InlineData(CalculationType.KanjiWriting, "[えき]の まえで まつ。", "駅", "「えき」は「駅」と 書くよ|ことば: 駅員（えきいん）")]
    [InlineData(CalculationType.KanjiWords, "[えきいん]", "駅員", "「えきいん」は「駅員」と 書くよ|「駅」を つかう ことばだよ")]
    public void KanjiSteps_MatchTheTemplates(CalculationType type, string question, string correct, string expected)
    {
        var e = ExplanationBuilder.Build(new Problem { Question = question, CalculationType = type, CorrectText = correct }, 0, null, "ん");
        Assert.Equal(expected.Split('|'), e!.Steps);
    }

    [Theory]
    [InlineData(CalculationType.KanjiReading, "なつは [暑]い。", "あつ", "あつい", "ひらがなの ところ（い）は 入れないよ")]
    [InlineData(CalculationType.KanjiReading, "夏[祭]りに 行く。", "まつ", "まつり", "ひらがなの ところ（り）は 入れないよ")]
    [InlineData(CalculationType.KanjiReading, "なつは [暑]い。", "あつ", "あたた", null)]
    [InlineData(CalculationType.KanjiWords, "[えきいん]", "駅員", "駅貝", "「駅」は あってるよ。2つめの 字を よく 見てね")]
    [InlineData(CalculationType.KanjiWords, "[えきいん]", "駅員", "馬員", "1つめの 字が ちがうよ。「駅」だよ")]
    [InlineData(CalculationType.KanjiWords, "[のみもの]", "飲み物", "食み物", "1つめの 字が ちがうよ。「飲」だよ")]
    [InlineData(CalculationType.KanjiWords, "[ゆうえんち]", "遊園地", "遊園池", "「遊」は あってるよ。3つめの 字を よく 見てね")]
    [InlineData(CalculationType.KanjiWriting, "[えき]の まえで まつ。", "駅", "馬", null)]
    public void KanjiHint_OnlyForCommonMistakes(CalculationType type, string question, string correct, string text, string? hint)
    {
        var e = ExplanationBuilder.Build(new Problem { Question = question, CalculationType = type, CorrectText = correct }, 0, null, text);
        Assert.Equal(hint, e!.Hint);
    }

    [Fact]
    public void UnknownQuestion_HasNoExplanation()
    {
        Assert.Null(ExplanationBuilder.Build(Problem("ふるい 問題", CalculationType.WordProblem), 1, null));
        Assert.Null(ExplanationBuilder.Build(Problem("1 + 1", CalculationType.Addition), 1, null));
    }

    // explanations.md の例
    [Theory]
    [InlineData(CalculationType.Addition, "3 + 4 = ?", "しき: 3 + 4 = 7")]
    [InlineData(CalculationType.Addition, "8 + 5 = ?", "8 は あと 2 で 10。5 を 2 と 3 に 分けて、10 と 3 で 13|しき: 8 + 5 = 13")]
    [InlineData(CalculationType.Addition, "345 + 278 = ?", "一の位から じゅんに たそう。くり上がりは 一の位と 十の位で 1回ずつ（上の位に 1 を たす）|しき: 345 + 278 = 623")]
    [InlineData(CalculationType.Addition, "23 + 45 = ?", "一の位から じゅんに たそう。くり上がりは ないよ|しき: 23 + 45 = 68")]
    [InlineData(CalculationType.Subtraction, "7 - 3 = ?", "しき: 7 − 3 = 4")]
    [InlineData(CalculationType.Subtraction, "13 - 8 = ?", "13 を 10 と 3 に 分けて、10 − 8 = 2、2 と 3 で 5|しき: 13 − 8 = 5")]
    [InlineData(CalculationType.Subtraction, "623 - 278 = ?", "一の位から じゅんに ひこう。ひけないときは 上の位から 1 かりて 10 に する。くり下がりは 一の位と 十の位で 1回ずつ|しき: 623 − 278 = 345")]
    [InlineData(CalculationType.Multiplication, "7 × 8 = ?", "7 の だん。7 が 8こ で 56|しき: 7 × 8 = 56")]
    [InlineData(CalculationType.Multiplication, "30 × 4 = ?", "3 × 4 = 12 の 10こ分 で 120|しき: 30 × 4 = 120")]
    [InlineData(CalculationType.Multiplication, "23 × 4 = ?", "23 を 20 と 3 に 分けて、20 × 4 = 80、3 × 4 = 12、あわせて 92|しき: 23 × 4 = 92")]
    [InlineData(CalculationType.Multiplication, "23 × 45 = ?", "45 を 40 と 5 に 分けて、23 × 40 = 920、23 × 5 = 115、あわせて 1035|しき: 23 × 45 = 1035")]
    [InlineData(CalculationType.Division, "56 ÷ 7 = ?", "7 の だんで 56 に なる 数を さがそう。7 × 8 = 56 だから 8|しき: 56 ÷ 7 = 8")]
    [InlineData(CalculationType.Division, "72 ÷ 3 = ?", "72 を 60 と 12 に 分けて、60 ÷ 3 = 20、12 ÷ 3 = 4、あわせて 24|しき: 72 ÷ 3 = 24")]
    [InlineData(CalculationType.Division, "80 ÷ 4 = ?", "8 ÷ 4 = 2 の 10こ分 で 20|しき: 80 ÷ 4 = 20")]
    [InlineData(CalculationType.DivisionWithRemainder, "26 ÷ 4 = ?", "4 の だんで 26 を こえない いちばん 大きい 数は 4 × 6 = 24|26 − 24 = 2 あまる。こたえは 6 あまり 2")]
    [InlineData(CalculationType.Time, "2時間は 何分？", "1時間 = 60分。2時間は 60 × 2 = 120分")]
    [InlineData(CalculationType.Time, "1時間20分は 何分？", "1時間 = 60分。60 + 20 = 80分")]
    [InlineData(CalculationType.Time, "100分は 何時間何分？", "60分 = 1時間。100分は 60分 と 40分 だから 1時間40分")]
    [InlineData(CalculationType.Time, "90秒は 何分何秒？", "60秒 = 1分。90秒は 60秒 と 30秒 だから 1分30秒")]
    [InlineData(CalculationType.Time, "3時10分の 20分後は 何時何分？", "10 + 20 = 30 で、3時30分")]
    [InlineData(CalculationType.Time, "3時40分の 25分後は 何時何分？", "4時まで 20分。のこり 5分 すすめて 4時5分")]
    [InlineData(CalculationType.Time, "4時5分の 15分前は 何時何分？", "4時まで 5分 もどる。のこり 10分 もどって 3時50分")]
    [InlineData(CalculationType.Time, "2時40分の 1時間35分後は 何時何分？", "まず 1時間 すすめて 3時40分|4時まで 20分。のこり 15分 すすめて 4時15分")]
    [InlineData(CalculationType.Time, "2時10分から 2時45分までは 何分？", "45 − 10 = 35分")]
    [InlineData(CalculationType.Time, "9時50分から 10時30分までは 何分？", "10時まで 10分、10時から 10時30分まで 30分、あわせて 40分")]
    [InlineData(CalculationType.Measurement, "3cmは 何mm？", "1cm = 10mm。3cmは 10 × 3 = 30mm")]
    [InlineData(CalculationType.Measurement, "4cm5mmは 何mm？", "1cm = 10mm。4cmは 40mm、5mm と あわせて 45mm")]
    [InlineData(CalculationType.Measurement, "45mmは 何cm何mm？", "10mm = 1cm。45mmは 40mm と 5mm だから 4cm5mm")]
    [InlineData(CalculationType.Measurement, "1km300m + 800mは 何km何m？", "km は km、m は m どうしで たそう|m どうし: 300m + 800m = 1100m = 1km100m|km どうし: 1 + 1 = 2km（くり上がりの 1 も たす）|こたえは 2km100m")]
    [InlineData(CalculationType.Measurement, "2km - 400mは 何km何m？", "km は km、m は m どうしで ひこう|m が ひけないので、2km を 1km と 1000m に 分けて、1000m − 400m = 600m|こたえは 1km600m")]
    public void Steps_MatchExamples(CalculationType type, string question, string expected)
    {
        var e = ExplanationBuilder.Build(Problem(question, type), -1, null);
        Assert.Equal(expected.Split('|'), e!.Steps);
    }

    // 文章問題（ひな形の番号は templates.md と同じ）
    [Theory]
    [InlineData(1, 24, 13, 0, "「あわせて」は たし算|しき: 24 + 13 = 37")]
    [InlineData(4, 350, 180, 0, "「ちがい」は 大きい 数 − 小さい 数|しき: 350 − 180 = 170")]
    [InlineData(5, 0, 8, 23, "はじめの 数に 8こ 入れて 23こ。はじめの 数は 23 から 8 を ひく|しき: 23 − 8 = 15")]
    [InlineData(6, 0, 6, 14, "はじめの 数から 6人 帰って 14人。はじめの 数は 14 に 6 を たす|しき: 14 + 6 = 20")]
    [InlineData(9, 45, 3, 0, "「1こ 45円 × 3こ」で かけ算|45 を 40 と 5 に 分けて、40 × 3 = 120、5 × 3 = 15、あわせて 135")]
    [InlineData(14, 26, 4, 0, "同じ数ずつ 分けるので わり算|しき: 26 ÷ 4 = 6 あまり 2")]
    [InlineData(16, 22, 4, 0, "22 ÷ 4 = 5 あまり 2|あまった 2こも 入れる はこが いるので、5 + 1 = 6こ")]
    [InlineData(17, 50, 8, 0, "50 ÷ 8 = 6 あまり 2|あまりの 2円 では 買えないので 6こ")]
    [InlineData(18, 500, 120, 3, "まず パンの 代金: 120 × 3 = 360|つぎに のこり: 500 − 360 = 140")]
    [InlineData(19, 17, 13, 5, "まず 2人の どんぐりを あわせる: 17 + 13 = 30|つぎに 分ける: 30 ÷ 5 = 6")]
    [InlineData(20, 8, 3, 5, "まず あめの 数: 8 × 3 = 24|つぎに 食べた 分を ひく: 24 − 5 = 19")]
    public void WordProblemSteps_MatchExamples(int template, int a, int b, int c, string expected)
    {
        var e = ExplanationBuilder.Build(Problem(WordProblemTemplates.Format(template, a, b, c), CalculationType.WordProblem), -1, null);
        Assert.Equal(expected.Split('|'), e!.Steps);
    }

    // よくある まちがい: その答えのときだけ出る
    [Theory]
    [InlineData(CalculationType.Addition, "345 + 278 = ?", 513, null, "くり上がりを わすれたかも？")]
    [InlineData(CalculationType.Addition, "45 + 23 = ?", 22, null, "ひき算に なっているかも？ たし算だよ")]
    [InlineData(CalculationType.Subtraction, "623 - 278 = ?", 455, null, "くり下がりを わすれたかも？ ひけない 位は 上の位から 1 かりてね")]
    [InlineData(CalculationType.Subtraction, "45 - 23 = ?", 68, null, "たし算に なっているかも？ ひき算だよ")]
    [InlineData(CalculationType.Multiplication, "7 × 8 = ?", 49, null, "おしい！ 1つ となりの 九九かも？")]
    [InlineData(CalculationType.Multiplication, "7 × 8 = ?", 64, null, "おしい！ 1つ となりの 九九かも？")]
    [InlineData(CalculationType.Division, "56 ÷ 7 = ?", 392, null, "かけ算に なっているかも？ わり算だよ")]
    [InlineData(CalculationType.DivisionWithRemainder, "26 ÷ 4 = ?", 5, 6, "あまりは わる数の 4 より 小さく なるよ。もう 1つ ふやせるかも？")]
    [InlineData(CalculationType.DivisionWithRemainder, "26 ÷ 4 = ?", 7, 2, "4 × 7 = 28 は 26 を こえちゃうよ")]
    [InlineData(CalculationType.Time, "3時40分の 25分後は 何時何分？", 3, 65, "分は 60 より 小さく なるよ。60分で 1時間だね")]
    [InlineData(CalculationType.Time, "1時間20分は 何分？", 120, null, "1時間は 100分 じゃなくて 60分 だよ")]
    [InlineData(CalculationType.Time, "100分は 何時間何分？", 1, 0, "1時間は 100分 じゃなくて 60分 だよ")]
    [InlineData(CalculationType.Time, "90秒は 何分何秒？", 0, 90, "秒は 60 より 小さく なるよ。60秒で 1分だね")]
    [InlineData(CalculationType.Measurement, "1km300mは 何m？", 400, null, "1km は 1000m だよ")]
    [InlineData(CalculationType.Measurement, "1250mは 何km何m？", 12, 50, "1km は 1000m だよ")]
    [InlineData(CalculationType.Measurement, "1250mは 何km何m？", 0, 1250, "m の ところは 1000 より 小さく なるよ")]
    public void Hint_ShownForCommonMistake(CalculationType type, string question, int answer, int? remainder, string hint)
    {
        Assert.Equal(hint, ExplanationBuilder.Build(Problem(question, type), answer, remainder)!.Hint);
    }

    [Theory]
    [InlineData(5, 0, 8, 23, 31, "「入れた」でも たし算とは かぎらないよ。はじめの 数を さがそう")]
    [InlineData(6, 0, 6, 14, 8, "「帰った」でも ひき算とは かぎらないよ。はじめの 数を さがそう")]
    [InlineData(1, 24, 13, 0, 11, "たし算と ひき算が 逆かも？")]
    [InlineData(3, 42, 15, 0, 57, "たし算と ひき算が 逆かも？")]
    [InlineData(16, 22, 4, 0, 5, "あまった 2こも はこに 入れなきゃ いけないよ")]
    [InlineData(17, 50, 8, 0, 7, "あまりの 2円 では 1こ 買えないよ")]
    [InlineData(18, 500, 120, 3, 360, "2つめの 計算を わすれたかも？")]
    [InlineData(19, 17, 13, 5, 30, "2つめの 計算を わすれたかも？")]
    [InlineData(20, 8, 3, 5, 24, "2つめの 計算を わすれたかも？")]
    public void WordProblemHint_ShownForCommonMistake(int template, int a, int b, int c, int answer, string hint)
    {
        var p = Problem(WordProblemTemplates.Format(template, a, b, c), CalculationType.WordProblem);
        Assert.Equal(hint, ExplanationBuilder.Build(p, answer, null)!.Hint);
    }

    [Theory]
    [InlineData(CalculationType.Addition, "345 + 278 = ?", 600, null)]
    [InlineData(CalculationType.Multiplication, "7 × 8 = ?", 54, null)]
    [InlineData(CalculationType.DivisionWithRemainder, "26 ÷ 4 = ?", 6, 1)]
    [InlineData(CalculationType.Time, "3時40分の 25分後は 何時何分？", 4, 10)]
    [InlineData(CalculationType.Measurement, "1250mは 何km何m？", 2, 250)]
    public void Hint_NotShownForOtherMistakes(CalculationType type, string question, int answer, int? remainder)
    {
        Assert.Null(ExplanationBuilder.Build(Problem(question, type), answer, remainder)!.Hint);
    }

    [Fact]
    public void WordProblemTemplates_ParseEveryGeneratedProblem()
    {
        foreach (var p in AllProblems.Where(p => p.CalculationType == CalculationType.WordProblem))
        {
            var t = WordProblemTemplates.Parse(p.Question);
            Assert.NotNull(t);
            Assert.Equal(p.Question, WordProblemTemplates.Format(t!.Value.Id, t.Value.A, t.Value.B, t.Value.C));
        }
    }

    private static Problem Problem(string question, CalculationType type) => new() { Question = question, CalculationType = type };
}
