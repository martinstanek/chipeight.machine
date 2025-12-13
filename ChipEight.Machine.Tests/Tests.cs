using System;
using System.IO;
using System.Threading;
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
    public void Chip_Jump()
    {
        var chip = new Chip();
        
        chip.Load([0x12, 0x06]);
        chip.Run(cycles: 1);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x206);
    }
    
    [Fact]
    public void Chip_Sprite()
    {
        var chip = new Chip();
        
        chip.Load([
            0x62, 0x00, 
            0x63, 0x00, 
            0xA2, 0x0A, 
            0xD2, 0x36, 
            0x12, 0x08,
            0x20, 0x70, 
            0x70, 0xF8, 
            0xD8, 0x88]);
        chip.Run(cycles: 8);
    }
    
    [Fact]
    public void Chip_FirstRom()
    {
        var rom = File.ReadAllBytes("./Roms/1-chip8-logo.ch8");
        var chip = new Chip();
        
        chip.Load(rom);
        chip.Run(cycles: 39);
    }
    
    [Fact]
    public void Chip_FirstRom_Loop()
    {
        var rom = File.ReadAllBytes("./Roms/1-chip8-logo.ch8");
        var chip = new Chip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        
        chip.Load(rom);
        chip.Run(cancellation.Token);
    }
    
    [Fact]
    public void Chip_IbmRom()
    {
        var rom = File.ReadAllBytes("./Roms/2-ibm-logo.ch8");
        var chip = new Chip();
        
        chip.Load(rom);
        chip.Run(cycles: 20);
    }
}