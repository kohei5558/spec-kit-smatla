namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 答えの形（013）。2つの欄がある形は、2つ目の答えを Problem.CorrectRemainder・LearningRecord.StudentRemainder に持つ
/// </summary>
public enum AnswerFormat
{
    /// <summary>
    /// 数1つ（四則）
    /// </summary>
    Number = 0,

    /// <summary>
    /// [ ] あまり [ ]（あまりのあるわり算）
    /// </summary>
    QuotientRemainder = 1,

    /// <summary>
    /// [ ]時[ ]分（時刻）
    /// </summary>
    ClockTime = 2,

    /// <summary>
    /// [ ]時間[ ]分
    /// </summary>
    HoursMinutes = 3,

    /// <summary>
    /// [ ]分
    /// </summary>
    Minutes = 4,

    /// <summary>
    /// [ ]分[ ]秒
    /// </summary>
    MinutesSeconds = 5,

    /// <summary>
    /// [ ]秒
    /// </summary>
    Seconds = 6
}
