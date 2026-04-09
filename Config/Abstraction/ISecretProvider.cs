namespace Alien.Common.Config;

public interface ISecretProvider
{
    Task<string?> GetSecretAsync(string name, CancellationToken cancellationToken = default);

    Task<string?> GetSecretAsync(string name, byte[] key , CancellationToken cancellationToken = default);
}
