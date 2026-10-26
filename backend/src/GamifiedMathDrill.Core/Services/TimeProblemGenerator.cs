using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 時こくと時間の問題（013、難易度表は specs/013-time-and-clock/spec.md）。
/// 時刻は 1時〜11時台で、12時をまたがない（午前・午後をつけないため）。時刻は「分」で数える（3時40分 = 220）
/// </summary>
public static class TimeProblemGenerator
{
    private const int MaxAttempts = 100_000;

    public static List<Problem> Generate(int difficulty, Random random)
    {
        var problems = new List<Problem>();
        var seen = new HashSet<string>();
        for (var attempt = 0; attempt < MaxAttempts && problems.Count < ProblemGenerator.ProblemsPerSet; attempt++)
        {
            var problem = Sample(difficulty, random);
            if (problem != null && seen.Add(problem.Question))
            {
                problem.CalculationType = CalculationType.Time;
                problem.DifficultyLevel = difficulty;
                problems.Add(problem);
            }
        }
        return problems;
    }

    private static Problem? Sample(int d, Random r)
    {
        int Step5(int min, int max) => r.Next(min / 5, max / 5 + 1) * 5;
        // 1時〜11時台の時刻（5分きざみ、または1分きざみ）
        int Clock5() => r.Next(1, 12) * 60 + Step5(0, 55);
        int Clock1() => r.Next(1, 12) * 60 + r.Next(0, 60);

        switch (d)
        {
            case 1:
                // ○時間は何分？（1〜3時間）
                var hours = r.Next(1, 4);
                return Single($"{hours}時間は 何分？", hours * 60, AnswerFormat.Minutes);
            case 2:
                return LaterOrEarlier(Clock5(), 0, Step5(5, 55), later: true, crossHour: false);
            case 3:
                return LaterOrEarlier(Clock5(), 0, Step5(5, 55), later: false, crossHour: false);
            case 4:
                return Elapsed(Clock5(), Step5(5, 55), crossHour: false);
            case 5:
                return LaterOrEarlier(Clock5(), 0, Step5(5, 55), later: r.Next(2) == 0, crossHour: true);
            case 6:
                return Elapsed(Clock5(), Step5(5, 55), crossHour: true);
            case 7:
                {
                    var total = r.Next(61, 180);
                    if (total % 60 == 0)
                    {
                        return null;
                    }
                    return r.Next(2) == 0
                        ? Single($"{total / 60}時間{total % 60}分は 何分？", total, AnswerFormat.Minutes)
                        : Pair($"{total}分は 何時間何分？", total / 60, total % 60, AnswerFormat.HoursMinutes);
                }
            case 8:
                return r.Next(3) switch
                {
                    0 => LaterOrEarlier(Clock1(), 0, r.Next(1, 60), later: true, crossHour: true),
                    1 => LaterOrEarlier(Clock1(), 0, r.Next(1, 60), later: false, crossHour: true),
                    _ => Elapsed(Clock1(), r.Next(1, 60), crossHour: true)
                };
            case 9:
                {
                    var total = r.Next(61, 180);
                    if (total % 60 == 0)
                    {
                        return null;
                    }
                    return r.Next(2) == 0
                        ? Single($"{total / 60}分{total % 60}秒は 何秒？", total, AnswerFormat.Seconds)
                        : Pair($"{total}秒は 何分何秒？", total / 60, total % 60, AnswerFormat.MinutesSeconds);
                }
            case 10:
                return LaterOrEarlier(Clock5(), r.Next(1, 3), Step5(5, 55), later: r.Next(2) == 0, crossHour: null);
            default:
                return null;
        }
    }

    /// <summary>
    /// ○時○分の（○時間）○分後・前は何時何分？ crossHour が null なら時をまたぐかどうかは問わない
    /// </summary>
    private static Problem? LaterOrEarlier(int start, int hours, int minutes, bool later, bool? crossHour)
    {
        var answer = start + (later ? 1 : -1) * (hours * 60 + minutes);
        if (!InRange(answer) || (crossHour.HasValue && crossHour.Value != (start / 60 != answer / 60)))
        {
            return null;
        }

        var span = hours > 0 ? $"{hours}時間{minutes}分" : $"{minutes}分";
        var problem = Pair($"{Text(start)}の {span}{(later ? "後" : "前")}は 何時何分？", answer / 60, answer % 60, AnswerFormat.ClockTime);
        problem.ClockTimes = Clock(start);
        return problem;
    }

    /// <summary>
    /// ○時○分から ○時○分までは何分？（60分未満）
    /// </summary>
    private static Problem? Elapsed(int start, int minutes, bool crossHour)
    {
        var end = start + minutes;
        if (!InRange(end) || crossHour != (start / 60 != end / 60))
        {
            return null;
        }

        var problem = Single($"{Text(start)}から {Text(end)}までは 何分？", minutes, AnswerFormat.Minutes);
        problem.ClockTimes = $"{Clock(start)} {Clock(end)}";
        return problem;
    }

    private static bool InRange(int time) => time >= 60 && time < 12 * 60;

    // 「3時」「3時40分」（ちょうどのときは「3時0分」と書かない）
    private static string Text(int time) => time % 60 == 0 ? $"{time / 60}時" : $"{time / 60}時{time % 60}分";

    private static string Clock(int time) => $"{time / 60}:{time % 60:00}";

    private static Problem Single(string question, int answer, AnswerFormat format) =>
        new() { Question = question, CorrectAnswer = answer, AnswerFormat = format };

    private static Problem Pair(string question, int first, int second, AnswerFormat format) =>
        new() { Question = question, CorrectAnswer = first, CorrectRemainder = second, AnswerFormat = format };
}
