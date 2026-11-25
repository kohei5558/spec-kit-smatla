namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// マスター・復習の判定に使う 学習記録の一部（030）
/// </summary>
public record AnswerHistory(int Id, int ProblemId, bool IsCorrect, DateTime SolvedAt);

/// <summary>
/// 次の問題（030）。IsReview は まちがえたままの問題を 復習で出したとき、AllMastered は 出せる問題を 全部マスターしていたとき
/// </summary>
public record NextProblem(Problem? Problem, bool IsReview, bool AllMastered);
