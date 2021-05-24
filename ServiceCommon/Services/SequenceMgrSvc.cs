using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitorCommon;

namespace Monitor.ServiceCommon.Services
{
    public class SequenceMgrSvc
    {
        private readonly ConcurrentDictionary<string, AsyncSequentializer> sequences = new ConcurrentDictionary<string, AsyncSequentializer>();

        public SequenceMgrSvc(IConfiguration config, ILogger logger)
        {
            var svcConf = config.GetSection("action-sequences").GetSection("sequences");

            foreach (IConfigurationSection section in svcConf.GetChildren())
            {
                TimeSpan delay = section.Get<TimeSpan>();
                if (!sequences.TryAdd(section.Key, new AsyncSequentializer(delay)))
                {
                    logger.LogWarning($"Couldn't add sequentializer `{section.Key}` because such name already exist (delay = {delay})");
                }
            }
        }

        public IReadOnlyDictionary<string, AsyncSequentializer> Sequences => sequences;
    }
}