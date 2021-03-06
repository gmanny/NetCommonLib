using Ninject.Syntax;

namespace Monitor.ServiceCommon.Services.DiEager;

public class EagerSingleton<TComponent> : IEagerSingleton
{
#pragma warning disable IDE0060
    public EagerSingleton(TComponent component)
#pragma warning restore IDE0060
    {
        // do nothing. DI created the component for this constructor.
    }
}

public static class EagerSingleton
{
    public static IBindingNamedWithOrOnSyntax<T> AsEagerSingleton<T>(this IBindingInSyntax<T> binding)
    {
        IBindingNamedWithOrOnSyntax<T> r = binding.InSingletonScope();
            
        // ReSharper disable once PossibleNullReferenceException
        binding.Kernel.Bind<IEagerSingleton>().To<EagerSingleton<T>>().InSingletonScope();

        return r;
    }
}