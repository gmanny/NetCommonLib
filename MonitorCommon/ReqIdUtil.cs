using System;

namespace MonitorCommon;

public static class ReqIdUtil
{
    private static readonly Random Rnd = new();

    public static string RandomId(int len) => Rnd.NextLong().ToString("X")[..len];
}