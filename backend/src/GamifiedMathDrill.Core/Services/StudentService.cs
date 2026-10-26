using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ILevelRepository _levelRepository;

    public StudentService(IStudentRepository studentRepository, ILevelRepository levelRepository)
    {
        _studentRepository = studentRepository;
        _levelRepository = levelRepository;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _studentRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<Student> CreateAsync(Student student)
    {
        // デフォルト値の設定
        student.TotalPoints = 0;
        student.ConsecutiveDays = 0;
        student.TotalProblems = 0;
        student.LastLoginAt = DateTime.UtcNow;
        student.CreatedAt = DateTime.UtcNow;

        // 学年が分からないため、種類ごとのレベルはすべてレベル1から（014）。
        // CreateForGrade(1) が作る行はすべてレベル ID 1 を指すので、そのレベルが DB にあることを先に確かめる
        var level1 = await _levelRepository.GetByIdAsync(1);
        if (level1 == null)
        {
            throw new InvalidOperationException("Level 1 not found in database. Please run database seeding.");
        }
        student.Levels = StudentLevelRules.CreateForGrade(1);

        return await _studentRepository.AddAsync(student);
    }

    public async Task<Student> UpdateLoginAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {id} not found.");
        }

        // 「◯日連続」はログインではなく学習記録（1問以上解いた日）から計算するため、ここでは変えない
        student.LastLoginAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(student);
        return student;
    }

    public async Task<Student> UpdatePointsAsync(int id, int pointsToAdd)
    {
        if (pointsToAdd < 0)
        {
            throw new ArgumentException("Points to add cannot be negative.", nameof(pointsToAdd));
        }

        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            throw new KeyNotFoundException($"Student with ID {id} not found.");
        }

        student.TotalPoints += pointsToAdd;

        await _studentRepository.UpdateAsync(student);
        return student;
    }

    public string GetEncouragementMessage(int consecutiveDays)
    {
        return consecutiveDays switch
        {
            >= 30 => "🎉 すごい！30日連続達成！あなたは計算マスターです！",
            >= 14 => "🌟 14日連続！素晴らしい継続力です！この調子で続けよう！",
            >= 7 => "💪 1週間連続達成！がんばっているね！",
            >= 3 => "😊 3日連続！いい感じだよ！毎日続けよう！",
            >= 1 => "👍 今日もがんばろう！",
            _ => "🎯 さあ、始めよう！"
        };
    }
}
