using System;

namespace ChipEight.Machine.Output;

public sealed class ConsoleDisplay : IConsoleDisplay
{
    private const byte Width = 64;
    private const byte Height = 32;

    private readonly bool[,] _buffer;

    public ConsoleDisplay()
    {
        _buffer = new bool[Height, Width];
        Clear();
    }

    public void SetPixel(byte x, byte y, bool state)
    {
        if (x >= Width || y >= Height)
        {
            return;
        }

        _buffer[y, x] = state;
    }

    public void Clear()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _buffer[y, x] = false;
            }
        }
    }
    
    public void Refresh()
    {
        Console.Write("\x1b[2J\x1b[H");

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                Console.Write(_buffer[y, x] ? "█" : "░");
            }
            Console.WriteLine();
        }
    }
}