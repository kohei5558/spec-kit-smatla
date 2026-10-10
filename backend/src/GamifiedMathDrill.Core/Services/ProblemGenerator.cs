using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 難易度表（specs/008-grade3-difficulty/spec.md）に従って問題を作る。DB に依存しない。
/// 乱数の種を固定しているため、どの環境でも同じ問題セットになる
/// </summary>
public static class ProblemGenerator
{
    /// <summary>
    /// 計算の種類・難易度ごとの問題数（組み合わせがこれ未満の難易度は、ありうる問題すべて）
    /// </summary>
    public const int ProblemsPerSet = 30;

    public const int DefaultSeed = 2026;

    // 組み合わせが少ない難易度でも、ありうる問題を取りこぼさない程度の試行回数
    private const int MaxAttemptsPerSet = 100_000;

    /// <summary>
    /// すべての計算の種類・難易度（1〜10）の問題を作る
    /// </summary>
    public static List<Problem> GenerateAll(int seed = DefaultSeed)
    {
        var random = new Random(seed);
        var problems = new List<Problem>();
        foreach (var type in new[] { CalculationType.Addition, CalculationType.Subtraction, CalculationType.Multiplication, CalculationType.Division })
        {
            for (var difficulty = 1; difficulty <= 10; difficulty++)
            {
                problems.AddRange(GenerateSet(type, difficulty, random));
            }
        }
        return problems;
    }

    private static List<Problem> GenerateSet(CalculationType type, int difficulty, Random random)
    {
        var problems = new List<Problem>();
        var seen = new HashSet<(int, int)>();
        for (var attempt = 0; attempt < MaxAttemptsPerSet && problems.Count < ProblemsPerSet; attempt++)
        {
            var (a, b) = Sample(type, difficulty, random);
            if (!IsValid(type, difficulty, a, b) || !seen.Add((a, b)))
            {
                continue;
            }

            problems.Add(new Problem
            {
                Question = $"{a} {Symbol(type)} {b} = ?",
                CorrectAnswer = type switch
                {
                    CalculationType.Addition => a + b,
                    CalculationType.Subtraction => a - b,
                    CalculationType.Multiplication => a * b,
                    _ => a / b
                },
                CalculationType = type,
                DifficultyLevel = difficulty
            });
        }
        return problems;
    }

    /// <summary>
    /// 難易度ごとの数の範囲から候補を作る（条件の判定は IsValid）
    /// </summary>
    private static (int A, int B) Sample(CalculationType type, int d, Random r)
    {
        int N(int digits) => r.Next((int)Math.Pow(10, digits - 1), (int)Math.Pow(10, digits));
        int Pick(params int[] values) => values[r.Next(values.Length)];

        switch (type)
        {
            case CalculationType.Addition:
                return d switch
                {
                    1 or 2 => (N(1), N(1)),
                    3 => (N(2), N(1)),
                    4 or 5 => (N(2), N(2)),
                    6 => (N(3), N(2)),
                    7 or 8 => (N(3), N(3)),
                    9 => (N(4), N(3)),
                    _ => (N(4), N(4))
                };
            case CalculationType.Subtraction:
                return d switch
                {
                    1 => (N(1), N(1)),
                    2 => (r.Next(10, 19), N(1)),
                    3 => (N(2), N(1)),
                    4 or 5 => (N(2), N(2)),
                    6 => (N(3), N(2)),
                    7 or 8 => (N(3), N(3)),
                    9 => (N(4), N(3)),
                    _ => (N(4), N(4))
                };
            case CalculationType.Multiplication:
                return d switch
                {
                    1 => (Pick(2, 5), N(1)),
                    2 => (Pick(3, 4), N(1)),
                    3 => (Pick(6, 7), N(1)),
                    4 => (Pick(8, 9, 1), N(1)),
                    5 => (N(1), N(1)),
                    6 => (N(1) * 10, r.Next(2, 10)),
                    7 or 8 => (N(2), r.Next(2, 10)),
                    9 => (N(3), r.Next(2, 10)),
                    _ => (N(2), N(2))
                };
            default:
            {
                // 割り算は「割る数 × 商」で割られる数を作り、割り切れる問題にする
                var (divisor, quotient) = d switch
                {
                    1 => (Pick(2, 5), N(1)),
                    2 => (Pick(3, 4), N(1)),
                    3 => (Pick(6, 7), N(1)),
                    4 => (Pick(8, 9, 1), N(1)),
                    5 => (N(1), N(1)),
                    6 => (r.Next(2, 10), r.Next(1, 46)),
                    7 or 8 => (r.Next(2, 10), r.Next(10, 50)),
                    9 => (r.Next(2, 10), N(2)),
                    _ => (r.Next(2, 10), r.Next(100, 500))
                };
                return (divisor * quotient, divisor);
            }
        }
    }

    /// <summary>
    /// 難易度表の条件を満たすか
    /// </summary>
    private static bool IsValid(CalculationType type, int d, int a, int b) => type switch
    {
        CalculationType.Addition => d switch
        {
            1 => AddCarries(a, b) == 0,
            2 => AddCarries(a, b) >= 1,
            4 => AddCarries(a, b) == 0,
            5 => AddCarries(a, b) >= 1,
            7 => AddCarries(a, b) <= 1,
            8 => AddCarries(a, b) >= 2,
            _ => true
        },
        CalculationType.Subtraction => a >= b && d switch
        {
            2 => a - b < 10 && SubBorrows(a, b) >= 1,
            4 => SubBorrows(a, b) == 0,
            5 => SubBorrows(a, b) >= 1,
            7 => SubBorrows(a, b) <= 1,
            8 => SubBorrows(a, b) >= 2,
            _ => true
        },
        CalculationType.Multiplication => d switch
        {
            7 => a % 10 != 0 && MulCarries(a, b) == 0,
            8 => MulCarries(a, b) >= 1,
            _ => true
        },
        _ => d switch
        {
            // 何十÷1桁
            6 => Digits(a) == 2 && a % 10 == 0,
            // 2桁÷1桁（商が2桁）。7 は各位がそのまま割れる（くり下がりなし）、8 はそれ以外
            7 => Digits(a) == 2 && Digits(a / b) == 2 && a / 10 % b == 0 && a % 10 % b == 0,
            8 => Digits(a) == 2 && Digits(a / b) == 2 && !(a / 10 % b == 0 && a % 10 % b == 0),
            9 => Digits(a) == 3 && Digits(a / b) == 2,
            10 => Digits(a) == 3 && Digits(a / b) == 3,
            _ => true
        }
    };

    private static string Symbol(CalculationType type) => type switch
    {
        CalculationType.Addition => "+",
        CalculationType.Subtraction => "-",
        CalculationType.Multiplication => "×",
        _ => "÷"
    };

    private static int Digits(int n) => n.ToString().Length;

    /// <summary>筆算の足し算でくり上がる回数</summary>
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

    /// <summary>筆算の引き算でくり下がる回数</summary>
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

    /// <summary>筆算の（多桁）×（1桁）でくり上がる回数</summary>
    private static int MulCarries(int a, int b)
    {
        int carries = 0, carry = 0;
        while (a > 0)
        {
            carry = (a % 10 * b + carry) / 10;
            if (carry > 0)
            {
                carries++;
            }
            a /= 10;
        }
        return carries;
    }
}
