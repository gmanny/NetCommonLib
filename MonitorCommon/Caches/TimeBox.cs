using System;

namespace MonitorCommon.Caches;

public record TimeBox<T>(DateTime Time, T Value) where T : notnull;