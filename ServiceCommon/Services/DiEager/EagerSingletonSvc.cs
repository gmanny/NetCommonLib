namespace Monitor.ServiceCommon.Services.DiEager;

public class EagerSingletonSvc
{
    public EagerSingletonSvc(IEagerSingleton[] singletons)
    {
        // do nothing. DI created all the singletons for this constructor.
    }
}