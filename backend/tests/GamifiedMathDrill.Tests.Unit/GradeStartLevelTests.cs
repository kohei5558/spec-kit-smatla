using GamifiedMathDrill.Core.Models;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 学年に応じて始めるレベル（specs/008-grade3-difficulty/spec.md）のテスト
/// </summary>
public class GradeStartLevelTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    [InlineData(3, 5)]
    [InlineData(4, 7)]
    [InlineData(5, 7)]
    [InlineData(6, 7)]
    public void ForGrade_ReturnsStartLevel(int grade, int expectedLevel)
    {
        Assert.Equal(expectedLevel, GradeStartLevel.ForGrade(grade));
    }
}
