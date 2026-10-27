using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Services;
using Xunit;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 漢字の問題（020）が、保護者が確認した一覧（specs/020-kanji/kanji-batch1.md）とまったく同じ中身か。
/// 一覧のファイルを読み、作られた問題と1つずつ比べる
/// </summary>
public class KanjiProblemGeneratorTests
{
    private static readonly List<Problem> All = ProblemGenerator.GenerateAll();

    private record Row(int Index, string Kanji, string ReadingQuestion, string ReadingAnswer, string WritingQuestion, string[] WritingChoices,
        string? WordReading, string[]? WordChoices);

    private static readonly List<Row> Rows = ReadBatch();

    [Fact]
    public void Batch_Has30Kanji()
    {
        Assert.Equal(30, Rows.Count);
        Assert.Equal(30, Rows.Select(r => r.Kanji).Distinct().Count());
    }

    [Fact]
    public void ReadingProblems_MatchTheList()
    {
        var problems = Of(CalculationType.KanjiReading);
        Assert.Equal(Rows.Count, problems.Count);
        foreach (var row in Rows)
        {
            var p = Assert.Single(problems, p => p.Question == row.ReadingQuestion);
            Assert.Equal(row.ReadingAnswer, p.CorrectText);
            Assert.Equal(AnswerFormat.Text, p.AnswerFormat);
            Assert.Null(p.Choices);
            Assert.Matches("^[ぁ-ゖー]+$", p.CorrectText!);
            Assert.Equal(Difficulty(row), p.DifficultyLevel);
        }
    }

    [Fact]
    public void WritingProblems_MatchTheList()
    {
        var problems = Of(CalculationType.KanjiWriting);
        Assert.Equal(Rows.Count, problems.Count);
        foreach (var row in Rows)
        {
            var p = Assert.Single(problems, p => p.Question == row.WritingQuestion);
            AssertChoices(p, row.WritingChoices);
            Assert.Equal(row.Kanji, p.CorrectText);
            Assert.Equal(Difficulty(row), p.DifficultyLevel);
        }
    }

    [Fact]
    public void WordProblems_MatchTheList()
    {
        var problems = Of(CalculationType.KanjiWords);
        var rows = Rows.Where(r => r.WordReading != null).ToList();
        Assert.Equal(rows.Count, problems.Count);
        foreach (var row in rows)
        {
            var p = Assert.Single(problems, p => p.Question == $"[{row.WordReading}]");
            AssertChoices(p, row.WordChoices!);
            Assert.Contains(row.Kanji, p.CorrectText);
            Assert.Equal(Difficulty(row), p.DifficultyLevel);
        }
    }

    [Fact]
    public void EarlierProblemSets_AreUnchanged()
    {
        // 末尾に足したので、算数の問題は今までと同じ
        Assert.Equal(1419, All.Count(p => p.CalculationType <= CalculationType.DivisionWithRemainder));
        Assert.Equal(273, All.Count(p => p.CalculationType == CalculationType.Time));
        Assert.Equal(297, All.Count(p => p.CalculationType == CalculationType.Measurement));
        Assert.Equal(300, All.Count(p => p.CalculationType == CalculationType.WordProblem));
        Assert.All(All.Where(p => Subjects.Of(p.CalculationType) == Subject.Math), p => Assert.Null(p.CorrectText));
    }

    [Fact]
    public void Subjects_SplitTheTypes()
    {
        Assert.Equal(new[] { CalculationType.KanjiReading, CalculationType.KanjiWriting, CalculationType.KanjiWords }, Subjects.Types(Subject.Japanese));
        Assert.DoesNotContain(CalculationType.KanjiReading, Subjects.Types(Subject.Math));
        Assert.Contains(CalculationType.WordProblem, Subjects.Types(Subject.Math));
    }

    // 30字を並びの順に 3字ずつ むずかしさ 1〜10 に分ける
    private static int Difficulty(Row row) => row.Index / 3 + 1;

    private static void AssertChoices(Problem p, string[] expected)
    {
        Assert.Equal(AnswerFormat.Choice, p.AnswerFormat);
        Assert.Equal(expected[0], p.CorrectText); // 一覧はいちばん左が正解
        var choices = p.Choices!.Split(' ');
        Assert.Equal(expected.OrderBy(c => c), choices.OrderBy(c => c));
        Assert.Equal(4, choices.Distinct().Count());
    }

    private static List<Problem> Of(CalculationType type) => All.Where(p => p.CalculationType == type).ToList();

    private static List<Row> ReadBatch()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "specs")))
        {
            dir = dir.Parent;
        }
        var lines = File.ReadAllLines(Path.Combine(dir!.FullName, "specs", "020-kanji", "kanji-batch1.md"));
        var rows = new List<Row>();
        foreach (var line in lines.Where(l => Regex.IsMatch(l, @"^\| \d+ \|")))
        {
            var cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
            var reading = Regex.Match(cells[2], @"^(.+)（(.+)）$");
            var writing = Regex.Match(cells[3], @"^(.+) (\S+)$");
            var word = Regex.Match(cells[4], @"^(\S+): (\S+)$");
            rows.Add(new Row(
                rows.Count,
                cells[1],
                reading.Groups[1].Value,
                reading.Groups[2].Value,
                Regex.Replace(writing.Groups[1].Value, @"\*\*(.+?)\*\*", "[$1]"),
                writing.Groups[2].Value.Split('・'),
                word.Success ? word.Groups[1].Value : null,
                word.Success ? word.Groups[2].Value.Split('・') : null));
        }
        return rows;
    }
}
