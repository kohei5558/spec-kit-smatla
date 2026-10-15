using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IDailyChallengeRepository : IRepository<DailyChallenge>
{
    Task<DailyChallenge?> GetByStudentAndDateAsync(int studentId, DateOnly date);

    /// <summary>
    /// チャレンジを追加する。同じ学習者・日のチャレンジが同時に作られていた場合は、そちらを返す
    /// </summary>
    Task<DailyChallenge> AddOrGetExistingAsync(DailyChallenge challenge);

    /// <summary>
    /// 回答結果を保存する。他の回答が先に保存されていた場合は false
    /// </summary>
    Task<bool> TrySaveAnswerAsync(DailyChallenge challenge);
}
