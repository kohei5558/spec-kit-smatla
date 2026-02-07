using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class ProblemService : IProblemService
{
    private readonly IProblemRepository _problemRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILearningRecordRepository _learningRecordRepository;
    private readonly ILevelRepository _levelRepository;

    public ProblemService(
        IProblemRepository problemRepository,
        IStudentRepository studentRepository,
        ILearningRecordRepository learningRecordRepository,
        ILevelRepository levelRepository)
    {
        _problemRepository = problemRepository;
        _studentRepository = studentRepository;
        _learningRecordRepository = learningRecordRepository;
        _levelRepository = levelRepository;
    }

    public async Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        // 生徒の現在のレベルに基づいて問題の難易度を選択（ハイブリッド方式）
        int difficultyLevel;

        if (student.CurrentLevel != null)
        {
            // レベル内の進捗率を計算（0.0～1.0）
            var progress = student.CurrentLevel.RequiredCorrectAnswers > 0
                ? Math.Min(1.0, (double)student.CorrectAnswers / student.CurrentLevel.RequiredCorrectAnswers)
                : 0.0;

            // 進捗に応じた基準難易度を計算
            var difficultyRange = student.CurrentLevel.MaxDifficulty - student.CurrentLevel.MinDifficulty;
            var baseDifficulty = student.CurrentLevel.MinDifficulty + (int)(difficultyRange * progress);

            // ランダム要素を追加（基準難易度の±1、ただしレベル範囲内）
            var random = new Random();
            var minDiff = Math.Max(student.CurrentLevel.MinDifficulty, baseDifficulty - 1);
            var maxDiff = Math.Min(student.CurrentLevel.MaxDifficulty, baseDifficulty + 1);

            difficultyLevel = minDiff == maxDiff ? minDiff : random.Next(minDiff, maxDiff + 1);
        }
        else
        {
            // レベル情報がない場合はデフォルト難易度
            difficultyLevel = 1;
        }

        // カテゴリパラメータを渡す（nullの場合はすべてのカテゴリから選択）
        return await _problemRepository.GetRandomProblemAsync(
            difficultyLevel,
            category,
            excludeRecentIds);
    }

    public async Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer)
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

        // 回答の正誤を判定
        var isCorrect = answer == problem.CorrectAnswer;

        // デバッグログ: 実際の比較内容を出力
        Console.WriteLine($"DEBUG: ProblemId={problemId}, UserAnswer={answer}, CorrectAnswer={problem.CorrectAnswer}, IsCorrect={isCorrect}");

        var pointsEarned = isCorrect ? 10 : 0; // 基本ポイント10、不正解は0

        // 学習記録の作成
        var learningRecord = new LearningRecord
        {
            StudentId = studentId,
            ProblemId = problemId,
            StudentAnswer = answer,
            IsCorrect = isCorrect,
            PointsEarned = pointsEarned,
            TimeTakenSeconds = 0, // TODO: 実際の所要時間を計測
            SolvedAt = DateTime.UtcNow
        };

        await _learningRecordRepository.AddAsync(learningRecord);

        // 生徒の統計を更新
        student.TotalProblems++;
        if (isCorrect)
        {
            student.CorrectAnswers++;
            student.TotalPoints += pointsEarned;
        }
        else
        {
            // 不正解の場合は連続正解数をリセット
            student.CorrectAnswers = 0;
        }

        await _studentRepository.UpdateAsync(student);

        // レベルアップのチェック
        var leveledUp = false;
        Level? newLevel = null;

        if (isCorrect && student.CurrentLevel != null)
        {
            // 現在のレベルの必要正解数を達成したかチェック
            if (student.CorrectAnswers >= student.CurrentLevel.RequiredCorrectAnswers)
            {
                // 次のレベルを取得
                var nextLevel = await _levelRepository.GetByIdAsync(student.CurrentLevelId + 1);
                if (nextLevel != null)
                {
                    student.CurrentLevelId = nextLevel.Id;
                    student.CorrectAnswers = 0; // 連続正解数をリセット
                    leveledUp = true;
                    newLevel = nextLevel;
                    await _studentRepository.UpdateAsync(student);
                }
            }
        }

        return (isCorrect, pointsEarned, leveledUp, newLevel);
    }
}
