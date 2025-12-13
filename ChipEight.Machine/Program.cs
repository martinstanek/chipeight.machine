using System;

namespace ChipEight.Machine;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Chip8");
        
        var chip = new Chip();
        
        chip.Load([
            0x22, 0x06,
            0x60, 0x01,
            0x60, 0x02,
            0x60, 0x0A,
            0x00, 0xEE
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