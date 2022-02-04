using System;
using Microsoft.Extensions.Configuration;
using Ninject.Modules;

namespace Monitor.ServiceCommon;

public class CommonServiceConfig
{
    public Action<ConfigurationBuilder>? SetUpCommandLine { get; set; } = null;

    public INinjectModule[] AdditionalModules { get; set; } = Array.Empty<INinjectModule>();
}