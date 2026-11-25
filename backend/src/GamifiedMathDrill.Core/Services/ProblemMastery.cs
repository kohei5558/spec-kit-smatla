using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 学習記録から、マスターした問題（3回つづけて正解）と 復習する問題（いちばん新しい答えが不正解）を 見分ける（030）
/// </summary>
public static class ProblemMastery
{
    /// <summary>つづけて この回数 正解したら マスター</summary>
    public const int CorrectInARow = 3;

    /// <summary>まちがえたままの問題を 復習で出す わりあい（だいたい 3問に1問）</summary>
    public const double ReviewRate = 1.0 / 3;

    public record Status(HashSet<int> Mastered, HashSet<int> NeedsReview);

    public static Status Analyze(IEnumerable<AnswerHistory> history)
    {
        var mastered = new HashSet<int>();
        var needsReview = new HashSet<int>();
        foreach (var answers in history.GroupBy(h => h.ProblemId))
        {
            // 新しい順（同じ時刻なら あとから 入れた記録を 新しいとする）
            var latest = answers.OrderByDescending(h => h.SolvedAt).ThenByDescending(h => h.Id).Take(CorrectInARow).ToList();
            if (!latest[0].IsCorrect)
            {
                needsReview.Add(answers.Key);
            }
            else if (latest.Count == CorrectInARow && latest.All(h => h.IsCorrect))
            {
                mastered.Add(answers.Key);
            }
        }
        return new(mastered, needsReview);
    }
}
