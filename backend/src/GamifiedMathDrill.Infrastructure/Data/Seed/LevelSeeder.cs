using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// レベルのシードデータ
/// </summary>
public static class LevelSeeder
{
    public static List<Level> GetLevels()
    {
        return new List<Level>
        {
            new Level { Id = 1, LevelNumber = 1, RequiredCorrectAnswers = 5, MinDifficulty = 1, MaxDifficulty = 2 },
            new Level { Id = 2, LevelNumber = 2, RequiredCorrectAnswers = 8, MinDifficulty = 2, MaxDifficulty = 3 },
            new Level { Id = 3, LevelNumber = 3, RequiredCorrectAnswers = 10, MinDifficulty = 3, MaxDifficulty = 4 },
            new Level { Id = 4, LevelNumber = 4, RequiredCorrectAnswers = 12, MinDifficulty = 4, MaxDifficulty = 5 },
            new Level { Id = 5, LevelNumber = 5, RequiredCorrectAnswers = 15, MinDifficulty = 5, MaxDifficulty = 6 },
            new Level { Id = 6, LevelNumber = 6, RequiredCorrectAnswers = 18, MinDifficulty = 6, MaxDifficulty = 7 },
            new Level { Id = 7, LevelNumber = 7, RequiredCorrectAnswers = 20, MinDifficulty = 7, MaxDifficulty = 8 },
            new Level { Id = 8, LevelNumber = 8, RequiredCorrectAnswers = 25, MinDifficulty = 8, MaxDifficulty = 9 },
            new Level { Id = 9, LevelNumber = 9, RequiredCorrectAnswers = 30, MinDifficulty = 9, MaxDifficulty = 10 },
            new Level { Id = 10, LevelNumber = 10, RequiredCorrectAnswers = 40, MinDifficulty = 10, MaxDifficulty = 10 }
        };
    }
}
