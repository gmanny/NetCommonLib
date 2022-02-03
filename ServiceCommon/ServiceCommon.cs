using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monitor.ServiceCommon.Services;
using Monitor.ServiceCommon.Services.DiEager;
using Monitor.ServiceCommon.Services.InitStage.PostActivated;
using MonitorCommon.Tasks;
using Ninject;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace Monitor.ServiceCommon;

public static class ServiceCommon
{
    public static IKernel CreateCommonService(CommonServiceConfig cfg)
    {
        INinjectModule[] mods = new INinjectModule[cfg.AdditionalModules.Length + 1];
        mods[0] = new ServiceCommonModule(cfg);
        cfg.AdditionalModules.CopyTo(mods, 1);

        StandardKernel kernel = new(mods);
        kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

        return kernel;
    }

    public static IKernel StartCommonService(CommonServiceConfig cfg)
    {
        Thread.CurrentThread.Name = "Main";

        IKernel kernel = CreateCommonService(cfg);

        // eager singletons here
        kernel.Get<EagerSingletonSvc>();
            
        return kernel;
    }

    public static async Task<IKernel> RunCommonService(Type program, CommonServiceConfig cfg, params Type[] services)
    {
        try
        {
            IKernel container = StartCommonService(cfg);

            ILogger logger = container.CreateLogger(program);

            Task<string[]> complete = container.StartServices(services);
                
            PaInitSvc initSvc = container.Get<PaInitSvc>();
            initSvc.AllDone();

            string[] svcNames = await complete;

            logger.LogInformation($"All services finished work: {String.Join(", ", svcNames)}");

            return container;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error running service: {e}");

            throw;
        }
    }

    public static ILogger CreateLogger(this IKernel kernel, Type type)
    {
        return kernel.Get<LoggingSvc>().Factory.CreateLogger(type);
    }

    public static async Task<string[]> StartServices(this IKernel kernel, params Type[] services)
    {
        foreach (Type service in services)
        {
            kernel.Bind(service).ToSelf().InSingletonScope();

            if (typeof(IRunningService).IsAssignableFrom(service))
            {
                kernel.Bind<IRunningService>().To(service);
            }
        }

        return await Task.WhenAll(
            kernel.GetAll<IRunningService>().Select(s => s.Finished.Map(() => s.GetType().Name)).ToArray()
        );
    }
}