using Ninject.Syntax;

namespace Monitor.ServiceCommon.Services.DiEager
{
    public class EagerSingleton<TComponent> : IEagerSingleton
    {
        public EagerSingleton(TComponent component)
        {
            // do nothing. DI created the component for this constructor.
        }
    }

    public static class EagerSingleton
    {
        public static IBindingNamedWithOrOnSyntax<T> AsEagerSingleton<T>(this IBindingInSyntax<T> binding)
        {
            var r = binding.InSingletonScope();
            
            binding.Kernel.Bind<IEagerSingleton>().To<EagerSingleton<T>>().InSingletonScope();

            return r;
        }
    }
}