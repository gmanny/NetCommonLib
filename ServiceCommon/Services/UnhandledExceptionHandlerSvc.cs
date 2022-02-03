using System;
using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services;

public class UnhandledExceptionHandlerSvc
{
    private readonly ILogger logger;

    public UnhandledExceptionHandlerSvc(ILogger logger)
    {
        this.logger = logger;

        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
    }

    private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        switch (e.ExceptionObject)
        {
            case Exception err:
                logger.LogCritical(err, "Unhandled exception");
                break;

            default:
                logger.LogCritical($"Unhandled exception object: {e.ExceptionObject}");
                break;
        }
            
        Environment.Exit(1);
    }
}