using System.Text.RegularExpressions;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 長さ・重さ・かさの解説（問題文の形は MeasurementProblemGenerator）
/// </summary>
public static partial class ExplanationBuilder
{
    // （大きい単位, 小さい単位）→ 倍率
    private static readonly Dictionary<(string Big, string Small), int> UnitFactors = new()
    {
        [("cm", "mm")] = 10, [("m", "cm")] = 100, [("L", "dL")] = 10,
        [("km", "m")] = 1000, [("kg", "g")] = 1000, [("t", "kg")] = 1000, [("L", "mL")] = 1000
    };

    // 長い単位を先に（「mL」を「m」と「L」に分けて読まないように）
    private const string Unit = "mm|cm|km|mL|dL|m|kg|g|t|L";
    private static readonly Regex Amount = new($@"(\d+)({Unit})");
    private static readonly Regex ToSingleUnit = new($@"^((?:\d+(?:{Unit}))+)は 何({Unit})？$");
    private static readonly Regex ToTwoUnits = new($@"^((?:\d+(?:{Unit}))+)は 何({Unit})何({Unit})？$");
    private static readonly Regex AddSubtractUnits = new($@"^((?:\d+(?:{Unit}))+) ([+-]) ((?:\d+(?:{Unit}))+)は 何({Unit})何({Unit})？$");

    private static Explanation? Measurement(string question, int answer, int? remainder)
    {
        if (AddSubtractUnits.Match(question) is { Success: true } a)
        {
            var (big, small) = (a.Groups[4].Value, a.Groups[5].Value);
            if (!UnitFactors.TryGetValue((big, small), out var f))
            {
                return null;
            }
            var x = Parts(a.Groups[1].Value, big, small);
            var y = Parts(a.Groups[3].Value, big, small);
            var steps = a.Groups[2].Value == "+" ? AddUnits(x, y, big, small, f) : SubtractUnits(x, y, big, small, f);
            return new(steps, SmallPartHint(small, f, remainder));
        }

        if (ToTwoUnits.Match(question) is { Success: true } t)
        {
            var (big, small) = (t.Groups[2].Value, t.Groups[3].Value);
            if (!UnitFactors.TryGetValue((big, small), out var f))
            {
                return null;
            }
            var (b, s) = Parts(t.Groups[1].Value, big, small);
            var value = b * f + s;
            var step = $"{f}{small} = 1{big}。{t.Groups[1].Value}は {value / f * f}{small} と {value % f}{small} だから {UnitText(value, big, small, f)}";
            var hint = SmallPartHint(small, f, remainder)
                ?? (OtherFactors(f).Any(o => (answer, remainder) == (value / o, value % o)) ? FactorHint(big, small, f) : null);
            return new(new[] { step }, hint);
        }

        if (ToSingleUnit.Match(question) is { Success: true } o)
        {
            var small = o.Groups[2].Value;
            var big = Amount.Match(o.Groups[1].Value).Groups[2].Value;
            if (!UnitFactors.TryGetValue((big, small), out var f))
            {
                return null;
            }
            var (b, s) = Parts(o.Groups[1].Value, big, small);
            var step = s == 0
                ? $"1{big} = {f}{small}。{b}{big}は {f} × {b} = {b * f}{small}"
                : $"1{big} = {f}{small}。{b}{big}は {b * f}{small}、{s}{small} と あわせて {b * f + s}{small}";
            var wrongFactor = OtherFactors(f).Any(other => answer == b * other + s);
            return new(new[] { step }, wrongFactor ? FactorHint(big, small, f) : null);
        }
        return null;
    }

    private static List<string> AddUnits((int Big, int Small) x, (int Big, int Small) y, string big, string small, int f)
    {
        var steps = new List<string> { $"{big} は {big}、{small} は {small} どうしで たそう" };
        var smallSum = x.Small + y.Small;
        var carry = smallSum >= f;
        if (x.Small > 0 && y.Small > 0)
        {
            steps.Add($"{small} どうし: {x.Small}{small} + {y.Small}{small} = {smallSum}{small}" + (carry ? $" = {UnitText(smallSum, big, small, f)}" : ""));
        }
        var bigs = new List<int> { x.Big };
        if (y.Big > 0)
        {
            bigs.Add(y.Big);
        }
        if (carry)
        {
            bigs.Add(1);
        }
        if (bigs.Count > 1)
        {
            steps.Add($"{big} どうし: {string.Join(" + ", bigs)} = {bigs.Sum()}{big}" + (carry ? "（くり上がりの 1 も たす）" : ""));
        }
        steps.Add($"こたえは {UnitText(x.Big * f + x.Small + y.Big * f + y.Small, big, small, f)}");
        return steps;
    }

    private static List<string> SubtractUnits((int Big, int Small) x, (int Big, int Small) y, string big, string small, int f)
    {
        var steps = new List<string> { $"{big} は {big}、{small} は {small} どうしで ひこう" };
        var xBig = x.Big;
        if (x.Small < y.Small)
        {
            // 小さい単位が ひけないときは、大きい単位から 1 かりる
            var borrowed = x.Small + f;
            var from = UnitText(x.Big * f + x.Small, big, small, f);
            var split = x.Big > 1 ? $"{x.Big - 1}{big} と {borrowed}{small} に 分けて" : $"{borrowed}{small} に して";
            steps.Add($"{small} が ひけないので、{from} を {split}、{borrowed}{small} {Minus} {y.Small}{small} = {borrowed - y.Small}{small}");
            xBig--;
        }
        else if (y.Small > 0)
        {
            steps.Add($"{small} どうし: {x.Small}{small} {Minus} {y.Small}{small} = {x.Small - y.Small}{small}");
        }
        if (y.Big > 0)
        {
            steps.Add($"{big} どうし: {xBig} {Minus} {y.Big} = {xBig - y.Big}{big}");
        }
        steps.Add($"こたえは {UnitText(x.Big * f + x.Small - y.Big * f - y.Small, big, small, f)}");
        return steps;
    }

    // 「1km300m」→ (1, 300)
    private static (int Big, int Small) Parts(string amount, string big, string small)
    {
        int b = 0, s = 0;
        foreach (Match m in Amount.Matches(amount))
        {
            var n = int.Parse(m.Groups[1].Value);
            if (m.Groups[2].Value == big)
            {
                b = n;
            }
            else if (m.Groups[2].Value == small)
            {
                s = n;
            }
        }
        return (b, s);
    }

    // 「1km250m」「2km」「400m」（0 の部分は書かない）
    private static string UnitText(int value, string big, string small, int f) =>
        (value / f > 0 ? $"{value / f}{big}" : "") + (value % f > 0 || value == 0 ? $"{value % f}{small}" : "");

    private static IEnumerable<int> OtherFactors(int f) => new[] { 10, 100, 1000 }.Where(o => o != f);

    private static string FactorHint(string big, string small, int f) => $"1{big} は {f}{small} だよ";

    private static string? SmallPartHint(string small, int f, int? remainder) =>
        remainder >= f ? $"{small} の ところは {f} より 小さく なるよ" : null;
}
