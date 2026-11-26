using GamifiedMathDrill.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GamifiedMathDrill.Infrastructure.Data;

/// <summary>
/// EF Core のトランザクションで保存をまとめる（#31）。リポジトリは保存のたびに SaveChanges するので、その外側で囲む
/// </summary>
public class EfTransactionRunner : ITransactionRunner
{
    private readonly ApplicationDbContext _context;

    public EfTransactionRunner(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T> RunAsync<T>(Func<Task<T>> work)
    {
        // InMemory DB（結合テスト）はトランザクションが無い。すでに外側のトランザクションの中なら、そのまま使う
        if (!_context.Database.IsRelational() || _context.Database.CurrentTransaction != null)
        {
            return await work();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await work();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            // DB は元に戻ったので、途中まで変えた値を覚えたままにしない（同じリクエストで あとから 保存されないように）
            _context.ChangeTracker.Clear();
            throw;
        }
    }
}
