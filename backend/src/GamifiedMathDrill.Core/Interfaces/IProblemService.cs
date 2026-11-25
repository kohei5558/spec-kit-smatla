using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IProblemService
{
    /// <summary>今日（日本時間）答えた問題の数</summary>
    Task<int> CountTodaysAnswersAsync(int studentId);
    /// <summary>次の問題。マスターした問題は出さず、まちがえたままの問題を ときどき 復習で出す（030）</summary>
    /// <param name="subject">種類の指定が無いとき（すべて）に選ぶ教科。無ければ算数（020）</param>
    Task<NextProblem> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null, Subject? subject = null);
    /// <param name="text">文字の答え（漢字の問題、020）</param>
    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel, CalculationType CalculationType)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer,
        int? remainder = null,
        string? text = null);
}
