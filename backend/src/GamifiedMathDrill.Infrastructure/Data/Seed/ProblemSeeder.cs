using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;

namespace GamifiedMathDrill.Infrastructure.Data.Seed;

/// <summary>
/// 問題のシードデータ（難易度表に従った問題を ProblemGenerator で作る）
/// </summary>
public static class ProblemSeeder
{
    public static List<Problem> GetProblems() => ProblemGenerator.GenerateAll();
}
