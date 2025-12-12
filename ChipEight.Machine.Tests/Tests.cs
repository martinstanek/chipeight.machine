using System.IO;
using Shouldly;
using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class Tests
{
    [Fact]
    public void Chip_Basic()
    {
        var chip = new Chip();
        
        chip.Load([0x00, 0xE0]);
        chip.Run(cycles: 1);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x202);
    }
    
    [Fact]
    public void Chip_FirstRom()
    {
        var rom = File.ReadAllBytes("/Users/martinstanek/Documents/dev/git/martinstanek/chipeight.machine/ChipEight.Machine.Tests/Roms/1-chip8-logo.ch8");
        var chip = new Chip();
        
        chip.Load(rom);
        chip.Run(cycles: 39);
    }
}