using System;

namespace ChipEight.Machine;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Chip8");
        
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x10,
            0xB2, 0x00
        ]);

        Console.WriteLine(chip.ShowState());
        
        while (true)
        {
            Console.ReadLine();
            
            chip.Step();
            
            Console.WriteLine(chip.ShowState());
        }
    }
}