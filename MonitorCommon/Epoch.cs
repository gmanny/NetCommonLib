using System;
using System.Runtime.CompilerServices;

namespace MonitorCommon;

public static class Epoch
{
    public static readonly DateTime Start = DateTime.UnixEpoch;
    public const long StartTicks = 621355968000000000;

    public static long ToEpoch(this DateTime time) => (long) (time - Start).TotalSeconds;
    public static long ToEpochMs(this DateTime time) => (long) (time - Start).TotalMilliseconds;
    public static long ToEpochMcs(this DateTime time) => ToEpochFileTicks(time) / 10;
    public static long ToEpochFileTicks(this DateTime time) => time.Ticks - StartTicks;

    public static DateTime AsEpoch(this long epoch) => Start.AddSeconds(epoch);
    public static DateTime AsEpochMs(this long epoch) => Start.AddMilliseconds(epoch);
    public static DateTime AsEpochMcs(this long epoch) => AsEpochFileTicks(epoch * 10);
    public static DateTime AsEpochFileTicks(this long epoch) => new DateTime(epoch + StartTicks, DateTimeKind.Utc);

    public static long NowFileTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DateTimeOffset.UtcNow.Ticks - StartTicks;
    }

    public static long NowMs
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static long Now
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}