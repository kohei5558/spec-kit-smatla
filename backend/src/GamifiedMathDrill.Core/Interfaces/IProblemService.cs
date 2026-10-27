using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IProblemService
{
    /// <summary>今日（日本時間）答えた問題の数</summary>
    Task<int> CountTodaysAnswersAsync(int studentId);
    /// <param name="subject">種類の指定が無いとき（すべて）に選ぶ教科。無ければ算数（020）</param>
    Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null, Subject? subject = null);
    /// <param name="text">文字の答え（漢字の問題、020）</param>
    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel, CalculationType CalculationType)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer,
        int? remainder = null,
        string? text = null);
}
