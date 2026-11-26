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
    private readonly ITransactionRunner _transaction;

    public ProblemService(
        IProblemRepository problemRepository,
        IStudentRepository studentRepository,
        ILearningRecordRepository learningRecordRepository,
        ILevelRepository levelRepository,
        TimeProvider timeProvider,
        ITransactionRunner transaction)
    {
        _transaction = transaction;
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

    public async Task<NextProblem> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null, Subject? subject = null)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        // 種類の指定があればその種類、「すべて」なら その教科（無ければ算数、020）の種類をランダムな順に試す（014: 難易度は種類ごとのレベルで決める）
        var types = category.HasValue
            ? new[] { category.Value }
            : Subjects.Types(subject ?? Subject.Math).OrderBy(_ => Random.Shared.Next()).ToArray();

        // マスターした問題（3回つづけて正解）は出さない。まちがえたままの問題は ときどき 復習で出す（030）
        var status = ProblemMastery.Analyze(await _learningRecordRepository.GetAnswerHistoryAsync(studentId) ?? []);
        var recent = excludeRecentIds ?? [];
        var reviewIds = status.NeedsReview.Except(recent).ToList();
        if (reviewIds.Count > 0 && Random.Shared.NextDouble() < ProblemMastery.ReviewRate)
        {
            var reviews = await _problemRepository.GetActiveByIdsAsync(reviewIds, types);
            if (reviews.Count > 0)
            {
                return new NextProblem(reviews[Random.Shared.Next(reviews.Count)], true, false);
            }
        }

        // 出さない問題: マスターした問題（と、できれば 最近出た問題）
        var mastered = status.Mastered.ToList();
        var masteredOrRecent = status.Mastered.Union(recent).ToList();
        foreach (var type in types)
        {
            var studentLevel = student.Levels.FirstOrDefault(l => l.CalculationType == type);
            var level = studentLevel?.Level ?? await _levelRepository.GetByIdAsync(studentLevel?.LevelId ?? 1);
            var difficultyLevel = ChooseDifficulty(level, studentLevel?.CorrectAnswers ?? 0);

            // 選んだ むずかしさ → 下の むずかしさ（漢字は字を入れた分だけ下から うまる、025。マスターした分も ここで 下に さがる）→ 上の むずかしさ（030）の順に さがす。
            // 最近出た問題は除く。ただし問題が少ない難易度（時こくの難易度1は3問）で除くと何も無くなるときは、最近の問題でも出す
            foreach (var difficulty in DifficultyOrder(difficultyLevel))
            {
                var problem = await _problemRepository.GetRandomProblemAsync(difficulty, type, masteredOrRecent)
                    ?? await _problemRepository.GetRandomProblemAsync(difficulty, type, mastered);
                if (problem != null)
                {
                    return new NextProblem(problem, status.NeedsReview.Contains(problem.Id), false);
                }
            }
        }

        // 問題は あるのに 出せない ＝ 全部マスターした
        return new NextProblem(null, false, await _problemRepository.AnyActiveAsync(types));
    }

    // 問題の むずかしさは 1〜10（008）
    private const int MaxDifficulty = 10;

    private static IEnumerable<int> DifficultyOrder(int chosen)
    {
        for (var d = chosen; d >= 1; d--)
        {
            yield return d;
        }
        for (var d = chosen + 1; d <= MaxDifficulty; d++)
        {
            yield return d;
        }
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
        int? remainder = null,
        string? text = null)
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

        // 文字の答えの問題（漢字、020）は、前後の空白を除いた文字が正解と同じときだけ正解。数の欄は見ない
        string? studentText = null;
        if (problem.CorrectText != null)
        {
            studentText = text?.Trim() ?? string.Empty;
            if (studentText.Length > 20)
            {
                studentText = studentText[..20];
            }
            isCorrect = studentText == problem.CorrectText;
        }

        var pointsEarned = isCorrect ? 10 : 0; // 基本ポイント10、不正解は0

        // 学習記録の作成
        var learningRecord = new LearningRecord
        {
            StudentId = studentId,
            ProblemId = problemId,
            StudentAnswer = answer,
            StudentRemainder = studentRemainder,
            StudentText = studentText,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            TimeTakenSeconds = 0, // TODO: 実際の所要時間を計測
            SolvedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        // 学習記録と、ポイント・連続正解数・レベルは まとめて保存する（途中で失敗したら 学習記録も残さない、#31 と同じ形）
        return await _transaction.RunAsync(async () =>
        {
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
        });
    }
}
