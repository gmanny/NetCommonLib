using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Monitor.ServiceCommon.Services;
using Monitor.ServiceCommon.Services.DiEager;
using Monitor.ServiceCommon.Services.InitStage;
using Monitor.ServiceCommon.Services.InitStage.PostActivated;
using Newtonsoft.Json;
using Ninject;
using Ninject.Modules;

namespace Monitor.ServiceCommon;

public class ServiceCommonModule : NinjectModule
{
    private readonly CommonServiceConfig config;

    public ServiceCommonModule(CommonServiceConfig config)
    {
        this.config = config;
    }

    public override void Load()
    {
        Bind<CommonServiceConfig>().ToConstant(config).InSingletonScope();

        Bind<UnhandledExceptionHandlerSvc>().ToSelf().AsEagerSingleton();
        Bind<ProcessLifetimeSvc>().ToSelf().AsEagerSingleton();
        Bind<ConfigurationSvc>().ToSelf().InSingletonScope();
        Bind<IConfiguration>().ToMethod(c => c.Kernel.Get<ConfigurationSvc>().Config).InSingletonScope();
        Bind<TaskScheduler>().ToConstant(TaskScheduler.Default).InSingletonScope();

        Bind<HttpClientSvc>().ToSelf().InSingletonScope();
        Bind<HttpClient>().ToProvider<HttpClientSvc>().InSingletonScope();
            
        Bind<JsonSerializerSvc>().ToSelf().InSingletonScope();
        Bind<JsonSerializer>().ToProvider<JsonSerializerSvc>().InSingletonScope();

        Bind<LoggingSvc>().ToSelf().InSingletonScope();
        Bind<ILogger>().ToMethod(c => c.Kernel.CreateLogger(c.Request.Target?.Member.DeclaringType ?? typeof(UnrecognizedServiceType)));

        Bind<TimePrecisionSvc>().ToSelf().AsEagerSingleton();
        Bind<PauseDetectorSvc>().ToSelf().AsEagerSingleton();
        Bind<GcSettingsSvc>().ToSelf().AsEagerSingleton();
        Bind<ConsoleCommandService>().ToSelf().AsEagerSingleton();

        Bind<SequenceMgrSvc>().ToSelf().InSingletonScope();
        Bind<GlobalNotificationRunnerSvc>().ToSelf().InSingletonScope();

        InitSignal<Unit>.Bind(this);

        Bind<EagerSingletonSvc>().ToSelf().InSingletonScope();

        Bind<PaInitSvc>().ToSelf().InSingletonScope();
    }
}

public class UnrecognizedServiceType {}