using Ninject;
using Ninject.Syntax;

namespace Monitor.ServiceCommon.Util
{
    // from https://stackoverflow.com/a/5195818/579817
    public static class DiExtensions
    {
        public static ToExistingSingletonSyntax<T> ToExisting<T>(this IBindingToSyntax<T> binding)
        {
            return new ToExistingSingletonSyntax<T>(binding);
        }
    }

    public class ToExistingSingletonSyntax<T>
    {
        private IBindingToSyntax<T> binding;

        public ToExistingSingletonSyntax(IBindingToSyntax<T> binding)
        {
            this.binding = binding;
        }

        public IBindingWhenInNamedWithOrOnSyntax<TImplementation> Binding<TImplementation>() where TImplementation : T
        {
            return binding.ToMethod(ctx => ctx.Kernel.Get<TImplementation>());
        }

        public IBindingNamedWithOrOnSyntax<TImplementation> Singleton<TImplementation>() where TImplementation : T
        {
            return binding.ToMethod(ctx => ctx.Kernel.Get<TImplementation>()).InSingletonScope();
        }
    }
}