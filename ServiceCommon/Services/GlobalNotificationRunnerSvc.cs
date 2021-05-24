using System;
using Microsoft.Extensions.Logging;
using MonitorCommon.Threading;

namespace Monitor.ServiceCommon.Services
{
    public class GlobalNotificationRunnerSvc
    {
        private readonly NotificationRunner runner;

        public GlobalNotificationRunnerSvc(ILogger logger)
        {
            runner = new NotificationRunner("Global notif", logger);
            if (!runner.TryStart())
            {
                throw new Exception("Couldn't start runner");
            }
        }

        public NotificationRunner Runner => runner;
    }
}