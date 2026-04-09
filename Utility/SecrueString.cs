using System.Text;
using System.Security.Cryptography;

namespace Alien.Common.Utility;

public delegate void UseDecryptedDataDelegate(ReadOnlySpan<byte> decryptedData);

public class SecrueString : IDisposable
{
    private byte[] _buffer;
    private bool _disposed;
    private byte[] _key;

    public int Length => _buffer.Length;

    public SecrueString(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        try
        {
            var cryptor = new AesGcmCrypto();
            _key = RandomNumberGenerator.GetBytes(32);
            _buffer = cryptor.Encrypt(input, _key);
        }
        catch
        {
            // Ensure cleanup on failure
            Dispose();
            throw;
        }
    }

    public ReadOnlySpan<byte> AsSpan() => _buffer;

    /// <summary>
    /// 將解密後的數據複製到指定的緩衝區中。使用完後請立即清除目標緩衝區。
    /// </summary>
    /// <param name="destination">目標緩衝區</param>
    /// <returns>實際複製的字節數</returns>
    public int CopyDecryptedTo(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cryptor = new AesGcmCrypto();
        var decrypted = Encoding.UTF8.GetBytes(cryptor.Decrypt(_buffer, _key));
        try
        {
            if (decrypted.Length > destination.Length)
                throw new ArgumentException("Destination buffer is too small.", nameof(destination));
            decrypted.CopyTo(destination);
            return decrypted.Length;
        }
        finally
        {
            // 立即清除解密後的數據
            Array.Clear(decrypted, 0, decrypted.Length);
        }
    }

    /// <summary>
    /// 執行指定的操作並自動清理解密後的數據。這是推薦的安全使用方式。
    /// </summary>
    /// <param name="action">要執行的操作</param>
    public void UseDecryptedData(UseDecryptedDataDelegate action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cryptor = new AesGcmCrypto();
        var decrypted = Encoding.UTF8.GetBytes(cryptor.Decrypt(_buffer, _key));
        try
        {
            action(decrypted);
        }
        finally
        {
            // 確保解密後的數據被清除
            Array.Clear(decrypted, 0, decrypted.Length);
        }
    }

    /// <summary>
    /// 獲取解密後的字符串。注意：字符串無法安全清除，請謹慎使用。
    /// </summary>
    public string GetDecryptedString()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var cryptor = new AesGcmCrypto();
        var decrypted = Encoding.UTF8.GetBytes(cryptor.Decrypt(_buffer, _key));
        try
        {
            return Encoding.UTF8.GetString(decrypted);
        }
        finally
        {
            Array.Clear(decrypted, 0, decrypted.Length);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Clear encrypted buffer
        if (_buffer != null)
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _buffer = null!;
        }

        // Clear AES key and IV
        if (_key != null)
        {
            Array.Clear(_key, 0, _key.Length);
        }

        _disposed = true;
    }

    ~SecrueString()
    {
        Dispose(false);
    }
}
