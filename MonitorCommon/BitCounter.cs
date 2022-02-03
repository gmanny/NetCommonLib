using System;

namespace MonitorCommon;

public static class BitCounter
{
    private static readonly int[] BitsSet = new int[256];

    static BitCounter()
    {
        for (uint b = Byte.MinValue; b <= Byte.MaxValue; b++)
        {
            BitsSet[b] = CountBits(b);
        }
    }

    private static int CountBits(uint value)
    {
        int count = 0;
        while (value != 0)
        {
            count++;
            value &= value - 1;
        }
        return count;
    }

    public static int BitCount(this ushort i)
    {
        return BitsSet[i & 0xff] + BitsSet[i >> 8];
    }
}