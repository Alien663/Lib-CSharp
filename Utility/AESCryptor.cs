using System.Text;
using System.Security.Cryptography;

namespace Alien.Common.Utility;

public class AesGcmCrypto
{
    private const int NonceSize = 12; // AEC-GCM recommand Nonce size
    private const int TagSize = 16; // AES-GCM recommand Tag size

    public byte[] Encrypt(string plaintext, byte[] key, byte[]? add = null)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] cipherText = new byte[plainTextBytes.Length];
        byte[] tag = new byte[TagSize];

        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plainTextBytes, cipherText, tag, add);
        }

        byte[] result = new byte[NonceSize + TagSize + cipherText.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize);
        Buffer.BlockCopy(cipherText, 0, result, NonceSize, cipherText.Length);
        Buffer.BlockCopy(tag, 0, result, NonceSize + cipherText.Length, TagSize);

        return result;
    }

    public string Decrypt(byte[] ciphertext, byte[] key, byte[]? add = null)
    {
        byte[] nonce = new byte[NonceSize];
        Buffer.BlockCopy(ciphertext, 0, nonce, 0, NonceSize);

        int cipherTextLength = ciphertext.Length - NonceSize - TagSize;
        byte[] cipherText = new byte[cipherTextLength];
        Buffer.BlockCopy(ciphertext, NonceSize, cipherText, 0, cipherTextLength);

        byte[] tag = new byte[TagSize];
        Buffer.BlockCopy(ciphertext, NonceSize + cipherTextLength, tag, 0, TagSize);

        byte[] plainText = new byte[cipherTextLength];
        using(var aes = new AesGcm(key, TagSize))
        {
            aes.Decrypt(nonce, cipherText, tag, plainText, add);
        }

        return Encoding.UTF8.GetString(plainText);
    }
}
