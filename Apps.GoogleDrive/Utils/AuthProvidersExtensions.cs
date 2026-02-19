using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Exceptions;

namespace Apps.GoogleDrive.Utils
{
    public static class AuthProvidersExtensions
    {
        public static string GetRequiredValue(
         this IEnumerable<AuthenticationCredentialsProvider> providers,
         string keyName)
        {
            if (providers == null)
                throw new PluginMisconfigurationException("Connection is missing. Please reconnect it.");

            var provider = providers.FirstOrDefault(p =>
                string.Equals(p.KeyName, keyName, StringComparison.OrdinalIgnoreCase));

            if (provider == null || string.IsNullOrWhiteSpace(provider.Value))
                throw new PluginMisconfigurationException("Connection is not connected or is broken. Please reconnect it.");

            return provider.Value;
        }
    }
}
