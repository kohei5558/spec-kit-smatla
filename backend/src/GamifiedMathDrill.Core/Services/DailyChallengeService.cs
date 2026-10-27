using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// デイリーチャレンジ（010）。学習者ごと・日本時間の日ごとに、選んだ種類のレベルの最大難易度の問題を1問出す（014）
/// </summary>
public class DailyChallengeService : IDailyChallengeService
{
    /// <summary>
    /// チャレンジに出す種類（答えが1つの四則だけ）
    /// </summary>
    private static readonly CalculationType[] ChallengeTypes =
    {
        CalculationType.Addition, CalculationType.Subtraction, CalculationType.Multiplication, CalculationType.Division
    };

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

    private DateOnly Today => JapanTime.Today(_timeProvider);

    public async Task<DailyChallenge?> GetOrCreateTodaysChallengeAsync(int studentId)
    {
        var today = Today;
        var existing = await _challengeRepository.GetByStudentAndDateAsync(studentId, today);
        if (existing != null)
        {
            return existing;
        }

        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            return null;
        }

        // 答えが1つの4種類（あまりのあるわり算などは除く、009）から1つ選び、その種類のレベルで出る一番難しい難易度から選ぶ（014）。
        // 使わない問題は除く（011）。その種類に問題がなければ、ほかの種類を試す
        var problems = (await _problemRepository.GetAllAsync()).Where(p => p.IsActive).ToList();
        Problem? selected = null;
        foreach (var type in ChallengeTypes.OrderBy(_ => Random.Shared.Next()))
        {
            var levelId = student.Levels.FirstOrDefault(l => l.CalculationType == type)?.LevelId ?? 1;
            var level = await _levelRepository.GetByIdAsync(levelId);
            if (level == null)
            {
                continue;
            }

            var candidates = problems.Where(p => p.CalculationType == type && p.DifficultyLevel == level.MaxDifficulty).ToList();
            if (candidates.Count > 0)
            {
                selected = candidates[Random.Shared.Next(candidates.Count)];
                break;
            }
        }

        if (selected == null)
        {
            return null;
        }

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

        return new DailyChallengeAnswerResult(DailyChallengeAnswerStatus.Answered, isCorrect, bonusPoints, problem.CorrectAnswer,
            isCorrect ? null : ExplanationBuilder.Build(problem, answer, null));
    }
}
