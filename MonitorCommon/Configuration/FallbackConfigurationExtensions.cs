using Microsoft.Extensions.Configuration;

namespace MonitorCommon.Configuration
{
    public static class FallbackConfigurationExtensions
    {
        public static IConfiguration WithFallback(this IConfiguration parent, IConfiguration fallback) => new FallbackConfiguration(parent, fallback);
        
        public static IConfigurationSection WithFallback(this IConfigurationSection parent, IConfigurationSection fallback) => new FallbackConfigurationSection(parent, fallback);
    }
}