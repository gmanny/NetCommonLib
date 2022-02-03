using System.Collections.Generic;
using Ninject.Modules;
using Ninject.Syntax;

namespace Monitor.ServiceCommon.Services.DiRouter;

public class DiNamedRouter<K, T> : Dictionary<K, T> where T : IDiNamedInstance<K>
{
    public DiNamedRouter(T[] allInstances)
    {
        foreach (T instance in allInstances)
        {
            Add(instance.DiName, instance);
        }
    }

    public static IBindingNamedWithOrOnSyntax<DiNamedRouter<K, T>> Bind(NinjectModule module) =>
        module.Bind<DiNamedRouter<K, T>>().ToSelf().InSingletonScope();
}