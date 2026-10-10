namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 計算種類
/// </summary>
public enum CalculationType
{
    /// <summary>
    /// 足し算
    /// </summary>
    Addition = 0,

    /// <summary>
    /// 引き算
    /// </summary>
    Subtraction = 1,

    /// <summary>
    /// 掛け算
    /// </summary>
    Multiplication = 2,

    /// <summary>
    /// 割り算（割り切れる）
    /// </summary>
    Division = 3,

    /// <summary>
    /// あまりのあるわり算（答えは商とあまり）
    /// </summary>
    DivisionWithRemainder = 4
}
