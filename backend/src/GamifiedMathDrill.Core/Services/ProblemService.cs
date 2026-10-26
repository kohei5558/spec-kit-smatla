using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILearningRecordRepository _learningRecordRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly TimeProvider _timeProvider;

    public ProblemService(
        IProblemRepository problemRepository,
        IStudentRepository studentRepository,
        ILearningRecordRepository learningRecordRepository,
        ILevelRepository levelRepository,
        TimeProvider timeProvider)
    {
        _problemRepository = problemRepository;
        _studentRepository = studentRepository;
        _learningRecordRepository = learningRecordRepository;
        _levelRepository = levelRepository;
        _timeProvider = timeProvider;
    }

    public async Task<int> CountTodaysAnswersAsync(int studentId)
    {
        // 今日（日本時間）答えた数。問題画面の「今日の◯問め」に使う
        var start = JapanTime.StartOfTodayUtc(_timeProvider);
        var records = await _learningRecordRepository.GetByStudentIdAndDateRangeAsync(studentId, start, start.AddDays(1).AddTicks(-1));
        return records.Count();
    }

    public async Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        // 種類の指定があればその種類、「すべて」なら問題がある種類をランダムな順に試す（014: 難易度は種類ごとのレベルで決める）
        var types = category.HasValue
            ? new[] { category.Value }
            : Enum.GetValues<CalculationType>().OrderBy(_ => Random.Shared.Next()).ToArray();

        foreach (var type in types)
        {
            var studentLevel = student.Levels.FirstOrDefault(l => l.CalculationType == type);
            var level = studentLevel?.Level ?? await _levelRepository.GetByIdAsync(studentLevel?.LevelId ?? 1);
            var difficultyLevel = ChooseDifficulty(level, studentLevel?.CorrectAnswers ?? 0);

            var problem = await _problemRepository.GetRandomProblemAsync(difficultyLevel, type, excludeRecentIds);
            if (problem != null)
            {
                return problem;
            }
        }

        return null;
    }

    /// <summary>
    /// レベルの中の進み具合（連続正解数 / 必要数）に応じて難易度を上げていき、前後1のゆらぎを入れる（ハイブリッド方式）
    /// </summary>
    private static int ChooseDifficulty(Level? level, int correctAnswers)
    {
        if (level == null)
        {
            return 1;
        }

        var progress = level.RequiredCorrectAnswers > 0
            ? Math.Min(1.0, (double)correctAnswers / level.RequiredCorrectAnswers)
            : 0.0;
        var baseDifficulty = level.MinDifficulty + (int)((level.MaxDifficulty - level.MinDifficulty) * progress);
        var minDiff = Math.Max(level.MinDifficulty, baseDifficulty - 1);
        var maxDiff = Math.Min(level.MaxDifficulty, baseDifficulty + 1);
        return minDiff == maxDiff ? minDiff : Random.Shared.Next(minDiff, maxDiff + 1);
    }

    public async Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel, CalculationType CalculationType)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer,
        int? remainder = null)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        var problem = await _problemRepository.GetByIdAsync(problemId);
        if (problem == null)
        {
            throw new KeyNotFoundException($"Problem with ID {problemId} not found.");
        }

        // 回答の正誤を判定。2つ目の答えがある問題（あまりのあるわり算、時こくの「時・分」など）は両方が合うときだけ正解。
        // 答えが1つの問題では2つ目を無視する（013）
        var hasRemainder = problem.CorrectRemainder != null;
        var studentRemainder = hasRemainder ? remainder : null;
        var isCorrect = answer == problem.CorrectAnswer
            && (!hasRemainder || studentRemainder == problem.CorrectRemainder);

        var pointsEarned = isCorrect ? 10 : 0; // 基本ポイント10、不正解は0

        // 学習記録の作成
        var learningRecord = new LearningRecord
        {
            StudentId = studentId,
            ProblemId = problemId,
            StudentAnswer = answer,
            StudentRemainder = studentRemainder,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            TimeTakenSeconds = 0, // TODO: 実際の所要時間を計測
            SolvedAt = DateTime.UtcNow
        };

        await _learningRecordRepository.AddAsync(learningRecord);

        // 生徒の統計を更新。連続正解数とレベルは、この問題の種類のものだけを変える（014）
        var studentLevel = StudentLevelRules.GetOrAdd(student, problem.CalculationType);
        student.TotalProblems++;
        if (isCorrect)
        {
            studentLevel.CorrectAnswers++;
            student.TotalPoints += pointsEarned;
        }
        else
        {
            // 不正解の場合は連続正解数をリセット
            studentLevel.CorrectAnswers = 0;
        }

        // レベルアップのチェック
        var leveledUp = false;
        Level? newLevel = null;

        var currentLevel = studentLevel.Level ?? await _levelRepository.GetByIdAsync(studentLevel.LevelId);
        if (isCorrect && currentLevel != null && studentLevel.CorrectAnswers >= currentLevel.RequiredCorrectAnswers)
        {
            var nextLevel = await _levelRepository.GetByIdAsync(studentLevel.LevelId + 1);
            if (nextLevel != null)
            {
                studentLevel.LevelId = nextLevel.Id;
                studentLevel.Level = nextLevel;
                studentLevel.CorrectAnswers = 0; // 連続正解数をリセット
                leveledUp = true;
                newLevel = nextLevel;
            }
        }

        await _studentRepository.UpdateAsync(student);

        return (isCorrect, pointsEarned, leveledUp, newLevel, problem.CalculationType);
    }
}
