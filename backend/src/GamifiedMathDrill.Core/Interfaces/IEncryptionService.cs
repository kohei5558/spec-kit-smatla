namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 暗号化サービスのインターフェース
/// PII（個人識別情報）の暗号化と復号化を提供
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// 文字列を暗号化します
    /// </summary>
    /// <param name="plainText">暗号化する平文</param>
    /// <returns>Base64エンコードされた暗号化文字列</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// 暗号化された文字列を復号化します
    /// </summary>
    /// <param name="cipherText">Base64エンコードされた暗号化文字列</param>
    /// <returns>復号化された平文</returns>
    string Decrypt(string cipherText);

    /// <summary>
    /// バイト配列を暗号化します
    /// </summary>
    byte[] EncryptBytes(byte[] plainBytes);

    /// <summary>
    /// 暗号化されたバイト配列を復号化します
    /// </summary>
    byte[] DecryptBytes(byte[] cipherBytes);
}
