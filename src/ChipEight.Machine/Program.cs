using System;
using System.IO;

namespace ChipEight.Machine;

public static class Program
{
    // TODO: app host (bucket?)
    
    public static void Main()
    {
        Console.WriteLine("Chip8");

        var rom = File.ReadAllBytes("");
        new Chip().Load(rom) .Run();
    }
}