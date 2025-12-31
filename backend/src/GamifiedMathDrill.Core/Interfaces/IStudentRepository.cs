using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 児童リポジトリインターフェース
/// </summary>
public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByIdWithDetailsAsync(int id);
}
