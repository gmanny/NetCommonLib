using System;

namespace MonitorCommon;

public static class ReqIdUtil
{
    private static readonly Random rnd = new();

    public static string RandomId(int len) => rnd.NextLong().ToString("X").Substring(0, len);
}