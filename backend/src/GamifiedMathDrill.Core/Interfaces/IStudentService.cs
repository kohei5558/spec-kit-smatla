using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IStudentService
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<Student> UpdateLoginAsync(int id);
    Task<Student> UpdatePointsAsync(int id, int pointsToAdd);
    string GetEncouragementMessage(int consecutiveDays);
}
