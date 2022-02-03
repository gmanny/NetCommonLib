using System;

namespace MonitorCommon.Caches;

// ReSharper disable InconsistentNaming
public record TimeBox<T>(DateTime time, T value);
// ReSharper restore InconsistentNaming