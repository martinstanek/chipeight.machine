using System;
using System.IO;

namespace ChipEight.Machine;

public static class Program
{
    // TODO: app host (bucket?)
    
    public static void Main()
    {
        Console.WriteLine("Chip8");

        var rom = File.ReadAllBytes("/Users/martinstanek/Documents/dev/git/martinstanek/chipeight.machine/tst/ChipEight.Machine.Tests/Roms/3-corax+.ch8");
        //new Chip().WithRemoteDisplay("http://10.0.1.106:8090").Load(rom) .Run();
        new Chip().Load(rom) .Run();
    }
}