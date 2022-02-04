using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using Monitor.ServiceCommon;
using Monitor.ServiceCommon.Services.InitStage.PostActivated;
using Ninject;
using Ninject.Modules;
using SingleInstanceHelper;

namespace WpfAppCommon;

public interface IWpfAppEntryPoint
{
    public string[] Args { get; }
    public bool SingleInstance { get; }
    public bool SignalInitFinish { get; }
    public bool ShowConsole { get; }
}

public class WpfAppEntryPoint<TApp, TMainForm> : IWpfAppEntryPoint
    where TApp : Application
    where TMainForm : Window
{
    private readonly List<INinjectModule> additionalModules;
    private readonly string[] args;
    private readonly bool singleInstance;
    private readonly bool signalInitFinish;
    private readonly bool showConsole;

    private WpfAppService<TApp, TMainForm>? app;
    private WpfAppStartupSequence<TApp, TMainForm>? startupSequenceOverride;

    public WpfAppEntryPoint(List<INinjectModule> additionalModules, string[] args, bool singleInstance = true, bool signalInitFinish = true, bool showConsole = true)
    {
        this.additionalModules = additionalModules;
        this.args = args;
        this.singleInstance = singleInstance;
        this.signalInitFinish = signalInitFinish;
        this.showConsole = showConsole;
    }

    public string[] Args => args;

    public bool SingleInstance => singleInstance;

    public bool SignalInitFinish => signalInitFinish;

    public bool ShowConsole => showConsole;

    public void OverrideStartupSequence(WpfAppStartupSequence<TApp, TMainForm> newSequence) => startupSequenceOverride = newSequence;

    public void Start()
    {
        try
        {
            if (singleInstance)
            {
                bool isFirstInstance = ApplicationActivator.LaunchOrReturn(OtherInstanceCallback, args);
                if (!isFirstInstance)
                {
                    return;
                }
            }

            if (showConsole)
            {
                WpfAppEntryPointHelper.AllocateConsole();
            }

            IKernel kernel = ServiceCommon.StartCommonService(
                new CommonServiceConfig
                {
                    AdditionalModules = additionalModules.ToArray()
                }
            );

            kernel.Bind<IWpfAppEntryPoint>().ToConstant(this);
            kernel.Bind<WpfAppService<TApp, TMainForm>>().ToSelf().InSingletonScope();

            app = kernel.Get<WpfAppService<TApp, TMainForm>>();
            if (startupSequenceOverride != null)
            {
                app.ReplaceStartupSequence(startupSequenceOverride);
            }

            if (signalInitFinish)
            {
                PaInitSvc initSvc = kernel.Get<PaInitSvc>();
                initSvc.AllDone();
            }

            app.Run();
        }
        catch (Exception e)
        {
            if (showConsole)
            {
                Console.WriteLine(e);

                Console.ReadLine();
            }
        }
    }

    private void OtherInstanceCallback(string[] otherArgs)
    {
        if (otherArgs.Length == 2 && otherArgs[1] == "close")
        {
            app?.Close();
        }
    }
}

public class WpfAppEntryPointHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    public static bool AllocateConsole() => AllocConsole();
}