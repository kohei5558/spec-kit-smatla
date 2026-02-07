using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class LearningRecordService : ILearningRecordService
{
    private readonly ILearningRecordRepository _recordRepository;
    private readonly IStudentRepository _studentRepository;

    public LearningRecordService(
        ILearningRecordRepository recordRepository,
        IStudentRepository studentRepository)
    {
        _recordRepository = recordRepository;
        _studentRepository = studentRepository;
    }

    public async Task<(List<LearningRecord> Records, int TotalCount)> GetRecordsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CalculationType? calculationType = null,
        int page = 1,
        int pageSize = 20)
    {
        // Verify student exists
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return (new List<LearningRecord>(), 0);
        }

        // Get all records for student
        IEnumerable<LearningRecord> records;
        if (startDate.HasValue && endDate.HasValue)
        {
            records = await _recordRepository.GetByStudentIdAndDateRangeAsync(
                studentId, startDate.Value, endDate.Value.AddDays(1).AddSeconds(-1));
        }
        else if (startDate.HasValue)
        {
            var allRecords = await _recordRepository.GetByStudentIdAsync(studentId);
            records = allRecords.Where(r => r.SolvedAt >= startDate.Value);
        }
        else if (endDate.HasValue)
        {
            var allRecords = await _recordRepository.GetByStudentIdAsync(studentId);
            records = allRecords.Where(r => r.SolvedAt <= endDate.Value.AddDays(1).AddSeconds(-1));
        }
        else
        {
            records = await _recordRepository.GetByStudentIdAsync(studentId);
        }

        // Filter by calculation type if specified
        if (calculationType.HasValue)
        {
            records = records.Where(r => r.Problem != null && r.Problem.CalculationType == calculationType.Value);
        }

        // Get total count
        var totalCount = records.Count();

        // Apply pagination and ordering
        var paginatedRecords = records
            .OrderByDescending(r => r.SolvedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (paginatedRecords, totalCount);
    }

    public async Task<LearningStatistics> GetStatisticsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CalculationType? calculationType = null)
    {
        // Verify student exists
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            return new LearningStatistics();
        }

        // Get all records for student
        IEnumerable<LearningRecord> records;
        if (startDate.HasValue && endDate.HasValue)
        {
            records = await _recordRepository.GetByStudentIdAndDateRangeAsync(
                studentId, startDate.Value, endDate.Value.AddDays(1).AddSeconds(-1));
        }
        else if (startDate.HasValue)
        {
            var allRecords = await _recordRepository.GetByStudentIdAsync(studentId);
            records = allRecords.Where(r => r.SolvedAt >= startDate.Value);
        }
        else if (endDate.HasValue)
        {
            var allRecords = await _recordRepository.GetByStudentIdAsync(studentId);
            records = allRecords.Where(r => r.SolvedAt <= endDate.Value.AddDays(1).AddSeconds(-1));
        }
        else
        {
            records = await _recordRepository.GetByStudentIdAsync(studentId);
        }

        var recordsList = records.ToList();

        // Apply calculation type filter if specified
        if (calculationType.HasValue)
        {
            recordsList = recordsList.Where(r => r.Problem.CalculationType == calculationType.Value).ToList();
        }

        if (!recordsList.Any())
        {
            return new LearningStatistics();
        }

        // Calculate overall statistics
        var totalProblems = recordsList.Count;
        var correctAnswers = recordsList.Count(r => r.IsCorrect);
        var incorrectAnswers = totalProblems - correctAnswers;
        var accuracyRate = totalProblems > 0 ? (double)correctAnswers / totalProblems * 100 : 0;
        var totalPoints = recordsList.Sum(r => r.PointsEarned);

        // Calculate statistics by calculation type
        var problemsByType = recordsList
            .Where(r => r.Problem != null)
            .GroupBy(r => r.Problem!.CalculationType)
            .ToDictionary(g => g.Key, g => g.Count());

        var accuracyByType = recordsList
            .Where(r => r.Problem != null)
            .GroupBy(r => r.Problem!.CalculationType)
            .ToDictionary(
                g => g.Key,
                g => g.Count() > 0 ? (double)g.Count(r => r.IsCorrect) / g.Count() * 100 : 0);

        // Calculate daily statistics
        var dailyProblems = recordsList
            .GroupBy(r => r.SolvedAt.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        var dailyAccuracy = recordsList
            .GroupBy(r => r.SolvedAt.Date.ToString("yyyy-MM-dd"))
            .ToDictionary(
                g => g.Key,
                g => g.Count() > 0 ? (double)g.Count(r => r.IsCorrect) / g.Count() * 100 : 0);

        return new LearningStatistics
        {
            TotalProblems = totalProblems,
            CorrectAnswers = correctAnswers,
            IncorrectAnswers = incorrectAnswers,
            AccuracyRate = Math.Round(accuracyRate, 1),
            TotalPoints = totalPoints,
            ProblemsByType = problemsByType,
            AccuracyByType = accuracyByType.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value, 1)),
            DailyProblems = dailyProblems,
            DailyAccuracy = dailyAccuracy.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Round(kvp.Value, 1))
        };
    }
}
