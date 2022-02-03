using System;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Monitor.ServiceCommon.Services;

public class LoggingSvc
{
    public ILoggerFactory Factory { get; } = new LoggerFactory();

    public LoggingSvc(ProcessLifetimeSvc lifetime /* this dependency is to let lifetimeSvc register to app exit events before NLog */)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
        catch
        {
            // ignored
            // occurs when there's no console
        }

        NLog.LogManager.LoadConfiguration("conf/nlog.config");
        Factory.AddNLog();

        lifetime.SetLogger(Factory.CreateLogger(lifetime.GetType()));
    }

    public ILoggerProvider MakeProvider() => new LoggerProvider(Factory);
}
    
public class LoggerProvider : ILoggerProvider
{
    private readonly ILoggerFactory factory;

    private bool disposed;

    public LoggerProvider(ILoggerFactory factory)
    {
        this.factory = factory;
    }

    public void Dispose()
    {
        disposed = true;
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(LoggerProvider));
        }

        return factory.CreateLogger(categoryName);
    }
}