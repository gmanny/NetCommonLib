using System;

namespace MonitorCommon;

public static class DateTimeUtil
{
    public static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}