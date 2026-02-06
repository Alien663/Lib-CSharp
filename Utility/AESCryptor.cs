using System.Text;
using System.Security.Cryptography;

namespace Alien.Common.Utility;

public class AesGcmCrypto
{
    private const int NonceSize = 12; // AEC-GCM recommand Nonce size
    private const int TagSize = 16; // AES-GCM recommand Tag size

    public EncryptedData Encrypt(string plaintext, byte[]? key  = null, byte[]? iv = null, byte[]? aad = null)
    {
        if(key is null)
            key = RandomNumberGenerator.GetBytes(32); // create a 256-bit key
        if(iv is null)
            iv = RandomNumberGenerator.GetBytes(NonceSize); // create a random nonce

        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize); // create a random nonce
        byte[] cipherText = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];
        byte[] plainTextByte = Encoding.UTF8.GetBytes(plaintext);
        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plainTextByte, cipherText, tag, aad);
            CryptographicOperations.ZeroMemory(key); // clear the key from memory
            return new EncryptedData(cipherText, key, nonce, tag);
        }
    }

    public string Decrypt(EncryptedData encryptedData, byte[]? aad = null)
    {
        var plaintextBytes = DecryptByte(encryptedData, aad);
        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public byte[] DecryptByte(EncryptedData encryptedData, byte[]? aad = null)
    {
        using (var aes = new AesGcm(encryptedData.Key, TagSize))
        {
            byte[] plaintext = new byte[encryptedData.CipherText.Length];
            aes.Decrypt(encryptedData.Nonce, encryptedData.CipherText, encryptedData.Tag, plaintext, aad);
            CryptographicOperations.ZeroMemory(encryptedData.Key); // clear the key from memory
            return plaintext;
        }
    }

    public record EncryptedData(
        byte[] CipherText,
        byte[] Key,
        byte[] Nonce,
        byte[] Tag
    );
}
