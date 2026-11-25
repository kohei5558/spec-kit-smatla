namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// いくつかの保存を1つのトランザクションでまとめる。途中で失敗したら、それまでの保存も全部取り消す（#31）
/// </summary>
public interface ITransactionRunner
{
    Task<T> RunAsync<T>(Func<Task<T>> work);
}
