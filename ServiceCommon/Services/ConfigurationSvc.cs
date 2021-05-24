using Microsoft.Extensions.Configuration;

namespace Monitor.ServiceCommon.Services
{
    public class ConfigurationSvc
    {
        public ConfigurationSvc(CommonServiceConfig cfg)
        {
            ConfigurationBuilder cb = new ConfigurationBuilder();
            cb.AddJsonFile("conf/config.json")
              .AddEnvironmentVariables("MON_");

            cfg.SetUpCommandLine?.Invoke(cb);

            Config = cb.Build();
        }

        public IConfiguration Config { get; }
    }
}