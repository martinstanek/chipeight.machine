using System;
using System.IO;
using System.Net;

namespace ChipEight.Machine;

public static class Program
{
    // TODO: app host (bucket?)
    
    public static void Main()
    {
        Console.WriteLine("Chip8");

        var rom = File.ReadAllBytes("/Users/martinstanek/Documents/dev/git/martinstanek/chipeight.machine/ChipEight.Machine.Tests/Roms/2-ibm-logo.ch8");
        new Chip() .Load(rom) .Run();
    }
}