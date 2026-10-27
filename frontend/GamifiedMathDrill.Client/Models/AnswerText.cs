namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 答えの形（API の AnswerFormat、013）に合わせた表示と、答えの欄の単位
/// </summary>
public static class AnswerText
{
    /// <summary>
    /// 「4時5分」「1時間40分」「80分」「1分30秒」「90秒」「1km250m」「3050g」「3 あまり 2」「6こ あまり 2こ」「42」。2つ目を答えていないときは「?」。
    /// units は欄の単位（答えの形が Units・UnitsRemainder のとき、015・017）
    /// </summary>
    public static string Format(string? answerFormat, int first, int? second, IReadOnlyList<string>? units = null)
    {
        var s = second?.ToString() ?? "?";
        return answerFormat switch
        {
            "Units" when units is { Count: 2 } => second == 0 ? $"{first}{units[0]}" : $"{first}{units[0]}{s}{units[1]}",
            "Units" when units is { Count: 1 } => $"{first}{units[0]}",
            "UnitsRemainder" when units is { Count: 2 } => $"{first}{units[0]} あまり {s}{units[1]}",
            "QuotientRemainder" => $"{first} あまり {s}",
            "ClockTime" => second == 0 ? $"{first}時" : $"{first}時{s}分",
            "HoursMinutes" => second == 0 ? $"{first}時間" : $"{first}時間{s}分",
            "Minutes" => $"{first}分",
            "MinutesSeconds" => second == 0 ? $"{first}分" : $"{first}分{s}秒",
            "Seconds" => $"{first}秒",
            // 古い記録（答えの形が無い）でも、あまりがあれば「3 あまり 2」にする
            _ => second.HasValue ? $"{first} あまり {s}" : first.ToString()
        };
    }

    /// <summary>
    /// 1つ目・2つ目の欄のうしろに出す単位（無ければ null）
    /// </summary>
    public static (string? First, string? Second) Units(string? answerFormat, IReadOnlyList<string>? units = null) => answerFormat switch
    {
        "Units" or "UnitsRemainder" => (units?.ElementAtOrDefault(0), units?.ElementAtOrDefault(1)),
        "ClockTime" => ("時", "分"),
        "HoursMinutes" => ("時間", "分"),
        "Minutes" => ("分", null),
        "MinutesSeconds" => ("分", "秒"),
        "Seconds" => ("秒", null),
        _ => (null, null)
    };
}
