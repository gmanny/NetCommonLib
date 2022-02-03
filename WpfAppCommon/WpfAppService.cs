using System.Windows;
using System.Windows.Threading;
using Monitor.ServiceCommon.Services;
using Ninject;

namespace WpfAppCommon;

public delegate void WpfAppStartupSequence<TApp, TMainForm>(IKernel k, TApp a, WpfAppService<TApp, TMainForm> service)
    where TApp : Application
    where TMainForm : Window;

public class WpfAppService<TApp, TMainForm> : IService
    where TApp : Application
    where TMainForm : Window
{
    private readonly TApp app;
    private readonly IKernel kernel;
        
    private TMainForm mainForm;
    private WpfAppStartupSequence<TApp, TMainForm> startupSequence;

    // gets app and form from the DI engine
    public WpfAppService(TApp app, IKernel kernel)
    {
        this.app = app;
        this.kernel = kernel;
            
        startupSequence = DefaultStartupSequence;
    }

    public string ServiceId => "WpfApp";

    public void ReplaceStartupSequence(WpfAppStartupSequence<TApp, TMainForm> newSequence) => startupSequence = newSequence;

    public void SetMainForm(TMainForm form) => mainForm = form;

    public void Run()
    {
        app.Dispatcher.BeginInvoke(() =>
        {
            startupSequence(kernel, app, this);
        }, DispatcherPriority.Send);

        app.Run();
    }

    public void Close()
    {
        mainForm?.Dispatcher.Invoke(() => mainForm.Close());
    }

    private void DefaultStartupSequence(IKernel k, TApp a, WpfAppService<TApp, TMainForm> service)
    {
        mainForm = k.Get<TMainForm>();

        mainForm.Show();
    }
}