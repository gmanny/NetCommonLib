namespace Monitor.ServiceCommon.Services.DiEager;

public class EagerSingletonSvc
{
#pragma warning disable IDE0060
    public EagerSingletonSvc(IEagerSingleton[] singletons)
#pragma warning restore IDE0060
    {
        // do nothing. DI created all the singletons for this constructor.
    }
}