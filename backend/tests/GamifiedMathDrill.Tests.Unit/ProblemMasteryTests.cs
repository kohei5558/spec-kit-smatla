using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// マスター（3回つづけて正解）と 復習（まちがえたまま）の判定（030）
/// </summary>
public class ProblemMasteryTests
{
    private static readonly DateTime Start = new(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

    // 問題IDと正誤を 答えた順に並べる
    private static List<AnswerHistory> History(params (int ProblemId, bool IsCorrect)[] answers) =>
        answers.Select((a, i) => new AnswerHistory(i + 1, a.ProblemId, a.IsCorrect, Start.AddMinutes(i))).ToList();

    [Fact]
    public void ThreeCorrectInARow_IsMastered()
    {
        var status = ProblemMastery.Analyze(History((1, true), (1, true), (1, true)));
        Assert.Contains(1, status.Mastered);
        Assert.DoesNotContain(1, status.NeedsReview);
    }

    [Fact]
    public void TwoCorrect_IsNotMasteredYet()
    {
        var status = ProblemMastery.Analyze(History((1, true), (1, true)));
        Assert.Empty(status.Mastered);
        Assert.Empty(status.NeedsReview);
    }

    [Fact]
    public void WrongAnswer_StartsTheCountAgain()
    {
        // 正解・正解・不正解・正解・正解 は、新しい方から3つが 全部 正解ではない
        var status = ProblemMastery.Analyze(History((1, true), (1, true), (1, false), (1, true), (1, true)));
        Assert.Empty(status.Mastered);
        Assert.Empty(status.NeedsReview);

        status = ProblemMastery.Analyze(History((1, true), (1, true), (1, false), (1, true), (1, true), (1, true)));
        Assert.Contains(1, status.Mastered);
    }

    [Fact]
    public void LatestAnswerWrong_NeedsReview()
    {
        var status = ProblemMastery.Analyze(History((1, true), (1, true), (1, true), (1, false), (2, false), (2, true), (3, false)));
        Assert.Equal(new[] { 1, 3 }, status.NeedsReview.OrderBy(id => id));
        // 一度マスターしても、まちがえたら マスターでは なくなる
        Assert.Empty(status.Mastered);
    }

    [Fact]
    public void OrderIsDecidedByTime_NotByListOrder()
    {
        // 学習記録が 新しい順に 並んでいても、答えた時刻で 判定する
        var history = History((1, false), (1, true), (1, true), (1, true));
        history.Reverse();
        Assert.Contains(1, ProblemMastery.Analyze(history).Mastered);
    }
}
