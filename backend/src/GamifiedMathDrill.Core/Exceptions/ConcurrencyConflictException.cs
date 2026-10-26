namespace GamifiedMathDrill.Core.Exceptions;

/// <summary>
/// 読み込んだあとに別の操作で更新されていたため、保存できなかった（楽観的同時実行制御）。
/// InvalidOperationException を継承しているため、これまでどおり 409 / 400 として扱う箇所はそのまま動く
/// </summary>
public class ConcurrencyConflictException(string message) : InvalidOperationException(message);
