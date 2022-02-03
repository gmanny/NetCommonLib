using System;

namespace MonitorCommon;

public static class ParsingExtensions
{
    public static int ToInt(this string str) => Int32.Parse(str);
    public static long ToLong(this string str) => Int64.Parse(str);
    public static double ToDouble(this string str) => Double.Parse(str);

    public static int ToInt(this decimal dec) => (int) dec;
}