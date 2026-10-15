using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 問題の生成が難易度表（specs/008-grade3-difficulty/spec.md）に従うことのテスト
/// </summary>
public class ProblemGeneratorTests
{
    private static readonly List<Problem> AllProblems = ProblemGenerator.GenerateAll();

    private static readonly Regex QuestionPattern = new(@"^(\d+) ([+\-×÷]) (\d+) = \?$");

    public static IEnumerable<object[]> AllSets()
    {
        foreach (var type in Enum.GetValues<CalculationType>())
        {
            for (var difficulty = 1; difficulty <= 10; difficulty++)
            {
                yield return new object[] { type, difficulty };
            }
        }
    }

    [Theory]
    [MemberData(nameof(AllSets))]
    public void EverySet_HasValidUniqueProblems(CalculationType type, int difficulty)
    {
        var problems = Set(type, difficulty);

        // 組み合わせが30通り以上ある難易度は30問、それ未満はありうる問題すべて
        Assert.Equal(ExpectedCount(type, difficulty), problems.Count);
        Assert.Equal(problems.Count, problems.Select(p => p.Question).Distinct().Count());

        foreach (var p in problems)
        {
            var (a, op, b) = Parse(p.Question);
            Assert.Equal(type switch
            {
                CalculationType.Addition => "+",
                CalculationType.Subtraction => "-",
                CalculationType.Multiplication => "×",
                _ => "÷" // 割り算・あまりのあるわり算
            }, op);
            Assert.True(p.CorrectAnswer >= 0, p.Question);
            Assert.Equal(type switch
            {
                CalculationType.Addition => a + b,
                CalculationType.Subtraction => a - b,
                CalculationType.Multiplication => a * b,
                _ => a / b
            }, p.CorrectAnswer);
            Assert.Equal(type == CalculationType.DivisionWithRemainder ? a % b : null, p.CorrectRemainder);
            Assert.True(MatchesRule(type, difficulty, a, b), $"難易度{difficulty}の条件を満たさない: {p.Question}");
        }
    }

    [Fact]
    public void Division_IsAlwaysExact()
    {
        foreach (var p in AllProblems.Where(p => p.CalculationType == CalculationType.Division))
        {
            var (a, _, b) = Parse(p.Question);
            Assert.Equal(0, a % b);
        }
    }

    [Fact]
    public void DivisionWithRemainder_AlwaysHasRemainderSmallerThanDivisor()
    {
        var problems = AllProblems.Where(p => p.CalculationType == CalculationType.DivisionWithRemainder).ToList();
        Assert.NotEmpty(problems);
        foreach (var p in problems)
        {
            var (_, _, b) = Parse(p.Question);
            Assert.InRange(p.CorrectRemainder!.Value, 1, b - 1);
        }
    }

    [Fact]
    public void Generation_IsDeterministic()
    {
        var again = ProblemGenerator.GenerateAll();

        Assert.Equal(AllProblems.Select(p => p.Question), again.Select(p => p.Question));
    }

    [Fact]
    public void Grade3Content_IsIncluded()
    {
        // 小3で習う計算（3桁の筆算、何十×1桁、2桁×1桁、2桁÷1桁）が難易度6〜8に含まれる
        Assert.Contains(Set(CalculationType.Addition, 7), p => Parse(p.Question).A >= 100);
        Assert.Contains(Set(CalculationType.Subtraction, 8), p => Parse(p.Question).A >= 100);
        Assert.Contains(Set(CalculationType.Multiplication, 6), p => Parse(p.Question).A % 10 == 0);
        Assert.Contains(Set(CalculationType.Multiplication, 8), p => Parse(p.Question).A >= 10);
        Assert.Contains(Set(CalculationType.Division, 8), p => p.CorrectAnswer >= 10);
    }

    #region 難易度表

    private static bool MatchesRule(CalculationType type, int d, int a, int b) => type switch
    {
        CalculationType.Addition => d switch
        {
            1 => Digits(a) == 1 && Digits(b) == 1 && AddCarries(a, b) == 0,
            2 => Digits(a) == 1 && Digits(b) == 1 && AddCarries(a, b) >= 1,
            3 => Digits(a) == 2 && Digits(b) == 1,
            4 => Digits(a) == 2 && Digits(b) == 2 && AddCarries(a, b) == 0,
            5 => Digits(a) == 2 && Digits(b) == 2 && AddCarries(a, b) >= 1,
            6 => Digits(a) == 3 && Digits(b) == 2,
            7 => Digits(a) == 3 && Digits(b) == 3 && AddCarries(a, b) <= 1,
            8 => Digits(a) == 3 && Digits(b) == 3 && AddCarries(a, b) >= 2,
            9 => Digits(a) == 4 && Digits(b) == 3,
            10 => Digits(a) == 4 && Digits(b) == 4,
            _ => false
        },
        CalculationType.Subtraction => a >= b && d switch
        {
            1 => Digits(a) == 1 && Digits(b) == 1,
            2 => Digits(a) == 2 && Digits(b) == 1 && a - b < 10 && SubBorrows(a, b) >= 1,
            3 => Digits(a) == 2 && Digits(b) == 1,
            4 => Digits(a) == 2 && Digits(b) == 2 && SubBorrows(a, b) == 0,
            5 => Digits(a) == 2 && Digits(b) == 2 && SubBorrows(a, b) >= 1,
            6 => Digits(a) == 3 && Digits(b) == 2,
            7 => Digits(a) == 3 && Digits(b) == 3 && SubBorrows(a, b) <= 1,
            8 => Digits(a) == 3 && Digits(b) == 3 && SubBorrows(a, b) >= 2,
            9 => Digits(a) == 4 && Digits(b) == 3,
            10 => Digits(a) == 4 && Digits(b) == 4,
            _ => false
        },
        CalculationType.Multiplication => d switch
        {
            1 => new[] { 2, 5 }.Contains(a) && b is >= 1 and <= 9,
            2 => new[] { 3, 4 }.Contains(a) && b is >= 1 and <= 9,
            3 => new[] { 6, 7 }.Contains(a) && b is >= 1 and <= 9,
            4 => new[] { 8, 9, 1 }.Contains(a) && b is >= 1 and <= 9,
            5 => a is >= 1 and <= 9 && b is >= 1 and <= 9,
            6 => Digits(a) == 2 && a % 10 == 0 && b is >= 2 and <= 9,
            7 => Digits(a) == 2 && a % 10 != 0 && b is >= 2 and <= 9 && MulCarries(a, b) == 0,
            8 => Digits(a) == 2 && b is >= 2 and <= 9 && MulCarries(a, b) >= 1,
            9 => Digits(a) == 3 && b is >= 2 and <= 9,
            10 => Digits(a) == 2 && Digits(b) == 2,
            _ => false
        },
        CalculationType.DivisionWithRemainder => a % b >= 1 && d switch
        {
            1 => new[] { 2, 3 }.Contains(b) && a / b is >= 1 and <= 9,
            2 => new[] { 4, 5 }.Contains(b) && a / b is >= 1 and <= 9,
            3 => new[] { 6, 7 }.Contains(b) && a / b is >= 1 and <= 9,
            4 => new[] { 8, 9 }.Contains(b) && a / b is >= 1 and <= 9,
            5 => b is >= 2 and <= 9 && a / b is >= 1 and <= 9,
            6 => Digits(a) == 2 && b is >= 2 and <= 9 && Digits(a / b) == 2 && a / 10 % b == 0,
            7 => Digits(a) == 2 && b is >= 2 and <= 9 && Digits(a / b) == 2 && a / 10 % b != 0,
            8 => Digits(a) == 3 && b is >= 2 and <= 9 && Digits(a / b) == 2,
            9 => Digits(a) == 3 && b is >= 2 and <= 5 && Digits(a / b) == 3,
            10 => Digits(a) == 3 && b is >= 6 and <= 9 && Digits(a / b) == 3,
            _ => false
        },
        _ => a % b == 0 && d switch
        {
            1 => new[] { 2, 5 }.Contains(b) && a / b is >= 1 and <= 9,
            2 => new[] { 3, 4 }.Contains(b) && a / b is >= 1 and <= 9,
            3 => new[] { 6, 7 }.Contains(b) && a / b is >= 1 and <= 9,
            4 => new[] { 8, 9, 1 }.Contains(b) && a / b is >= 1 and <= 9,
            5 => b is >= 1 and <= 9 && a / b is >= 1 and <= 9,
            6 => Digits(a) == 2 && a % 10 == 0 && b is >= 2 and <= 9,
            7 => Digits(a) == 2 && b is >= 2 and <= 9 && Digits(a / b) == 2 && a / 10 % b == 0 && a % 10 % b == 0,
            8 => Digits(a) == 2 && b is >= 2 and <= 9 && Digits(a / b) == 2 && !(a / 10 % b == 0 && a % 10 % b == 0),
            9 => Digits(a) == 3 && b is >= 2 and <= 9 && Digits(a / b) == 2,
            10 => Digits(a) == 3 && b is >= 2 and <= 9 && Digits(a / b) == 3,
            _ => false
        }
    };

    /// <summary>
    /// 組み合わせが30通り未満の難易度（段が限られる九九とその逆の割り算）は、ありうる問題の数
    /// </summary>
    private static int ExpectedCount(CalculationType type, int difficulty) => (type, difficulty) switch
    {
        (CalculationType.Multiplication or CalculationType.Division, 1 or 2 or 3) => 18,
        (CalculationType.Multiplication or CalculationType.Division, 4) => 27,
        (CalculationType.DivisionWithRemainder, 1) => 27,
        _ => ProblemGenerator.ProblemsPerSet
    };

    #endregion

    #region Helpers

    private static List<Problem> Set(CalculationType type, int difficulty) =>
        AllProblems.Where(p => p.CalculationType == type && p.DifficultyLevel == difficulty).ToList();

    private static (int A, string Op, int B) Parse(string question)
    {
        var m = QuestionPattern.Match(question);
        Assert.True(m.Success, $"問題文の形式が違う: {question}");
        return (int.Parse(m.Groups[1].Value), m.Groups[2].Value, int.Parse(m.Groups[3].Value));
    }

    private static int Digits(int n) => n.ToString().Length;

    private static int AddCarries(int a, int b)
    {
        int carries = 0, carry = 0;
        while (a > 0 || b > 0)
        {
            carry = (a % 10 + b % 10 + carry) >= 10 ? 1 : 0;
            carries += carry;
            a /= 10;
            b /= 10;
        }
        return carries;
    }

    private static int SubBorrows(int a, int b)
    {
        int borrows = 0, borrow = 0;
        while (a > 0 || b > 0)
        {
            borrow = (a % 10 - borrow) < b % 10 ? 1 : 0;
            borrows += borrow;
            a /= 10;
            b /= 10;
        }
        return borrows;
    }

    private static int MulCarries(int a, int b)
    {
        int carries = 0, carry = 0;
        while (a > 0)
        {
            var value = a % 10 * b + carry;
            carry = value / 10;
            if (carry > 0)
            {
                carries++;
            }
            a /= 10;
        }
        return carries;
    }

    #endregion
}
