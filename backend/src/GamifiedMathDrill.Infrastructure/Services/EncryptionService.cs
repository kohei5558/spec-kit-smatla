using GamifiedMathDrill.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace GamifiedMathDrill.Infrastructure.Services;

/// <summary>
/// AES-256を使用した暗号化サービスの実装
/// 児童の個人情報（名前など）を保護
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IConfiguration configuration)
    {
        // 本番環境では環境変数またはAzure Key Vaultから取得
        var keyString = configuration["Encryption:Key"] ?? throw new InvalidOperationException("暗号化キーが設定されていません");
        var ivString = configuration["Encryption:IV"] ?? throw new InvalidOperationException("暗号化IVが設定されていません");

        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException("暗号化キーは32バイト（256ビット）である必要があります");
        }

        if (_iv.Length != 16)
        {
            throw new InvalidOperationException("暗号化IVは16バイト（128ビット）である必要があります");
        }
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = EncryptBytes(plainBytes);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        var cipherBytes = Convert.FromBase64String(cipherText);
        var decryptedBytes = DecryptBytes(cipherBytes);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public byte[] EncryptBytes(byte[] plainBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    public byte[] DecryptBytes(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
    }
}
