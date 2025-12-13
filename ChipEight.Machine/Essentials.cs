using System;

namespace ChipEight.Machine;

public static class Essentials
{
    public static bool[] ByteToBooleans(byte b)
    {
        var result = new bool[8];
        
        for (var i = 7; i >= 0; i--)
        {
            result[i] = (b & (1 << i)) != 0;
        }

        result.Reverse();

        return result;
    }
}