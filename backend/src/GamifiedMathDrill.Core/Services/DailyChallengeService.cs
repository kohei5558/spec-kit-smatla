using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// デイリーチャレンジ（010）。学習者ごと・日本時間の日ごとに、今のレベルの最大難易度の問題を1問出す
/// </summary>
public class DailyChallengeService : IDailyChallengeService
{
    // 日本は夏時間がないため固定の +9 時間で日付を決める（タイムゾーンのデータがない環境でも動くように）
    private static readonly TimeSpan JapanOffset = TimeSpan.FromHours(9);

    private readonly IDailyChallengeRepository _challengeRepository;
    private readonly IProblemRepository _problemRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILearningRecordRepository _learningRecordRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly TimeProvider _timeProvider;

    public DailyChallengeService(
        IDailyChallengeRepository challengeRepository,
        IProblemRepository problemRepository,
        IStudentRepository studentRepository,
        ILearningRecordRepository learningRecordRepository,
        ILevelRepository levelRepository,
        TimeProvider timeProvider)
    {
        _challengeRepository = challengeRepository;
        _problemRepository = problemRepository;
        _studentRepository = studentRepository;
        _learningRecordRepository = learningRecordRepository;
        _levelRepository = levelRepository;
        _timeProvider = timeProvider;
    }

    private DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetUtcNow().ToOffset(JapanOffset).DateTime);

    public async Task<DailyChallenge?> GetOrCreateTodaysChallengeAsync(int studentId)
    {
        var today = Today;
        var existing = await _challengeRepository.GetByStudentAndDateAsync(studentId, today);
        if (existing != null)
        {
            return existing;
        }

        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return null;
        }

        var level = await _levelRepository.GetByIdAsync(student.CurrentLevelId);
        if (level == null)
        {
            return null;
        }

        // 今のレベルで出る一番難しい難易度から選ぶ。答えが1つの問題だけ（あまりのあるわり算は除く、009）
        var problems = await _problemRepository.GetAllAsync();
        var candidates = problems
            .Where(p => p.DifficultyLevel == level.MaxDifficulty && p.CalculationType != CalculationType.DivisionWithRemainder)
            .ToList();
        if (candidates.Count == 0)
        {
            return null;
        }

        var selected = candidates[Random.Shared.Next(candidates.Count)];
        return await _challengeRepository.AddOrGetExistingAsync(new DailyChallenge
        {
            StudentId = studentId,
            ProblemId = selected.Id,
            TargetDate = today,
            BonusPoints = DailyChallenge.DefaultBonusPoints
        });
    }

    public async Task<DailyChallengeAnswerResult> SubmitChallengeAnswerAsync(int studentId, int challengeId, int answer)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge == null || challenge.StudentId != studentId || challenge.TargetDate != Today)
        {
            return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.NotFound);
        }

        if (challenge.AnsweredAt != null)
        {
            return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.AlreadyAnswered);
        }

        var problem = await _problemRepository.GetByIdAsync(challenge.ProblemId);
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (problem == null || student == null)
        {
            return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.NotFound);
        }

        var isCorrect = answer == problem.CorrectAnswer;
        var bonusPoints = isCorrect ? challenge.BonusPoints : 0;

        // 先に回答済みにする。同時に答えが送られた場合、後のものはここで弾かれポイントは増えない
        challenge.AnsweredAt = _timeProvider.GetUtcNow().UtcDateTime;
        challenge.IsCorrect = isCorrect;
        challenge.StudentAnswer = answer;
        if (!await _challengeRepository.TrySaveAnswerAsync(challenge))
        {
            return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.AlreadyAnswered);
        }

        // 学習記録に残す（正答率に入る）。連続正解数・レベルは通常の問題だけで決まるため変えない
        await _learningRecordRepository.AddAsync(new LearningRecord
        {
            StudentId = studentId,
            ProblemId = problem.Id,
            IsCorrect = isCorrect,
            StudentAnswer = answer,
            PointsEarned = bonusPoints,
            TimeTakenSeconds = 0,
            SolvedAt = challenge.AnsweredAt.Value
        });

        student.TotalProblems++;
        student.TotalPoints += bonusPoints;
        await _studentRepository.UpdateAsync(student);

        return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.Answered, isCorrect, bonusPoints, problem.CorrectAnswer);
    }
}
