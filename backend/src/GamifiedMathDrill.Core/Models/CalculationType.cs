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
    DivisionWithRemainder = 4,

    /// <summary>
    /// 時こくと時間（013。答えの形は問題ごとに AnswerFormat で決まる）
    /// </summary>
    Time = 5,

    /// <summary>
    /// 長さ・重さ・かさ（015。答えの欄の単位は Problem.AnswerUnits）
    /// </summary>
    Measurement = 6,

    /// <summary>
    /// 文章問題（017。文章は specs/017-word-problems/templates.md のひな形だけ）
    /// </summary>
    WordProblem = 7,

    /// <summary>
    /// 漢字の読み（020、国語。答えは ひらがな）
    /// </summary>
    KanjiReading = 8,

    /// <summary>
    /// 漢字の書き（020、国語。4つから選ぶ）
    /// </summary>
    KanjiWriting = 9,

    /// <summary>
    /// 漢字のことば（熟語、020、国語。4つから選ぶ）
    /// </summary>
    KanjiWords = 10
}
