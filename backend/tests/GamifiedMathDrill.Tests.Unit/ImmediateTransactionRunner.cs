using GamifiedMathDrill.Core.Interfaces;

namespace GamifiedMathDrill.Tests.Unit;

/// <summary>
/// 単体テスト用: トランザクションを使わず、そのまま実行する（本物の DB での取り消しは 結合テストの ExchangeTransactionTests で確かめる）
/// </summary>
public class ImmediateTransactionRunner : ITransactionRunner
{
    public Task<T> RunAsync<T>(Func<Task<T>> work) => work();
}
