using Microsoft.Extensions.DependencyInjection;

namespace Alien.Common.Config;

public static class KeyVaultServiceCollectionExtensions
{
    public static IServiceCollection AddKeyVaultSecretProvider(this IServiceCollection services, Action<KeyVaultOptions> configure)
    {
        var options = new KeyVaultOptions();
        configure(options);

        services.AddSingleton(options);
        services.AddSingleton<ISecretProvider, KeyVaultSecretProvider>();
        return services;
    }
}
