using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 汎用リポジトリインターフェース
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
}
