using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// 問題のシードデータ生成
/// </summary>
public static class ProblemSeeder
{
    private static readonly Random Random = new();

    public static List<Problem> GetProblems()
    {
        var problems = new List<Problem>();
        int id = 1;

        // 足し算問題 (200問)
        problems.AddRange(GenerateAdditionProblems(ref id, 200));

        // 引き算問題 (200問)
        problems.AddRange(GenerateSubtractionProblems(ref id, 200));

        // 掛け算問題 (200問)
        problems.AddRange(GenerateMultiplicationProblems(ref id, 200));

        // 割り算問題 (100問)
        problems.AddRange(GenerateDivisionProblems(ref id, 100));

        return problems;
    }

    private static List<Problem> GenerateAdditionProblems(ref int startId, int count)
    {
        var problems = new List<Problem>();
        for (int i = 0; i < count; i++)
        {
            int difficulty = (i % 10) + 1; // 1-10の難易度
            int maxValue = difficulty * 10;
            int a = Random.Next(1, maxValue);
            int b = Random.Next(1, maxValue);

            problems.Add(new Problem
            {
                Id = startId++,
                Question = $"{a} + {b} = ?",
                CorrectAnswer = a + b,
                CalculationType = CalculationType.Addition,
                DifficultyLevel = difficulty
            });
        }
        return problems;
    }

    private static List<Problem> GenerateSubtractionProblems(ref int startId, int count)
    {
        var problems = new List<Problem>();
        for (int i = 0; i < count; i++)
        {
            int difficulty = (i % 10) + 1;
            int maxValue = difficulty * 10;
            int a = Random.Next(1, maxValue);
            int b = Random.Next(1, a + 1); // 負の数を避けるため

            problems.Add(new Problem
            {
                Id = startId++,
                Question = $"{a} - {b} = ?",
                CorrectAnswer = a - b,
                CalculationType = CalculationType.Subtraction,
                DifficultyLevel = difficulty
            });
        }
        return problems;
    }

    private static List<Problem> GenerateMultiplicationProblems(ref int startId, int count)
    {
        var problems = new List<Problem>();
        for (int i = 0; i < count; i++)
        {
            int difficulty = (i % 10) + 1;
            int maxValue = Math.Min(difficulty * 2, 12); // 九九の範囲内
            int a = Random.Next(1, maxValue + 1);
            int b = Random.Next(1, maxValue + 1);

            problems.Add(new Problem
            {
                Id = startId++,
                Question = $"{a} × {b} = ?",
                CorrectAnswer = a * b,
                CalculationType = CalculationType.Multiplication,
                DifficultyLevel = difficulty
            });
        }
        return problems;
    }

    private static List<Problem> GenerateDivisionProblems(ref int startId, int count)
    {
        var problems = new List<Problem>();
        for (int i = 0; i < count; i++)
        {
            int difficulty = (i % 10) + 1;
            int divisor = Random.Next(2, 11); // 2-10
            int quotient = Random.Next(1, difficulty * 5);
            int dividend = divisor * quotient;

            problems.Add(new Problem
            {
                Id = startId++,
                Question = $"{dividend} ÷ {divisor} = ?",
                CorrectAnswer = quotient,
                CalculationType = CalculationType.Division,
                DifficultyLevel = difficulty
            });
        }
        return problems;
    }
}
