using System;

namespace MonitorCommon.Caches
{
    public class TimeBox<T>
    {
        public readonly DateTime time;
        public readonly T value;

        public TimeBox(DateTime time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }
}