using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IDailyChallengeService
{
    /// <summary>
    /// 学習者の今日（日本時間）のチャレンジを返す。まだなければレベルに合わせて作る。出せる問題がなければ null
    /// </summary>
    Task<DailyChallenge?> GetOrCreateTodaysChallengeAsync(int studentId);

    Task<DailyChallengeAnswerResult> SubmitChallengeAnswerAsync(int studentId, int challengeId, int answer);
}

public enum DailyChallengeAnswerStatus
{
    Answered,
    /// <summary>チャレンジがない、他の学習者のもの、または今日のものではない</summary>
    NotFound,
    AlreadyAnswered
}

/// <param name="Explanation">まちがえたときの解説（正解のときは null、019）</param>
public record DailyChallengeAnswerResult(DailyChallengeAnswerStatus Status, bool IsCorrect = false, int BonusPoints = 0, int CorrectAnswer = 0, Explanation? Explanation = null);
