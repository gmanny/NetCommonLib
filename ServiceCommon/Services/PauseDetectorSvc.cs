using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Monitor.ServiceCommon.Services
{
    public class PauseDetectorSvc
    {
        private readonly TimeSpan sleepTime = TimeSpan.FromSeconds(0.5);
        private readonly TimeSpan detectionThreshold = TimeSpan.FromSeconds(2);

        private readonly ILogger logger;

        public PauseDetectorSvc(IConfiguration config, ILogger logger)
        {
            this.logger = logger;

            IConfigurationSection svcConf = config.GetSection("pause-detector");
            if (svcConf.Exists())
            {
                sleepTime = svcConf.GetValue<TimeSpan>("sleep-time");
                detectionThreshold = svcConf.GetValue<TimeSpan>("threshold");
            }

            var detector = new Thread(PauseDetectorLoop);
            detector.Name = "Pause detector";
            detector.IsBackground = true;

            detector.Start();
        }

        private void PauseDetectorLoop()
        {
            while (true)
            {
                DateTime start = DateTime.UtcNow;
                
                Thread.Sleep(sleepTime);

                DateTime end = DateTime.UtcNow;
                if (end - start > detectionThreshold)
                {
                    logger.LogInformation($"Unusual pause of {(end-start).TotalSeconds:0.###} seconds detected. (GC pause, thread starvation or clock leap may be the causes)");
                }
            }
        }
    }
}