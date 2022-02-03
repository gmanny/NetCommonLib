using System;

namespace MonitorCommon.Caches;

// ReSharper disable InconsistentNaming
public record TimeBox<T>(DateTime Time, T Value);
// ReSharper restore InconsistentNaming