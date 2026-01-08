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

    public async Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null)
    {
        var student = await _studentRepository.GetByIdWithDetailsAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {studentId} not found.");
        }

        // 生徒の現在のレベルに基づいて問題の難易度を選択
        var difficultyLevel = (student.CurrentLevel?.MinDifficulty + student.CurrentLevel?.MaxDifficulty) / 2 ?? 1;

        return await _problemRepository.GetRandomProblemAsync(
            difficultyLevel,
            null,
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
