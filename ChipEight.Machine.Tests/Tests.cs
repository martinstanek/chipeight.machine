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
    public void Chip_Call()
    {
        var chip = new Chip();
        
        chip.Load([
            0x22, 0x06,
            0x60, 0x01,
            0x60, 0x0A,
            0x00, 0xEE
        ]);
        chip.Run(cycles: 4);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x206);
    }
    
    [Fact]
    public void Chip_CallAndReturn()
    {
        var chip = new Chip();
        
        chip.Load([
            0x22, 0x06,
            0x60, 0x01,
            0x60, 0x02,
            0x60, 0x0A,
            0x00, 0xEE
        ]);
        
        chip.Run(cycles: 6);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x208);
        chip.Registers.Sp.ShouldBe((byte) 0);
        chip.Opcode.ShouldNotBeNull().ShouldBe((ushort) 0x600A );
    }

    [Fact]
    public void Chip_SkipIfEqual()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x05,
            0x30, 0x05,
            0x60, 0x01,
            0x60, 0x02
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
    }
    
    [Fact]
    public void Chip_SkipIfNotEqual()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x05,
            0x40, 0x04,
            0x60, 0x01,
            0x60, 0x02
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
    }
    
    [Fact]
    public void Chip_SkipIfRegistersEqual()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x05,
            0x61, 0x05,
            0x50, 0x10,
            0x60, 0x01,
            0x60, 0x02
        ]);
        
        chip.Run(cycles: 4);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
    }
    
    [Fact]
    public void Chip_SkipIfRegistersNotEqual()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x05,
            0x61, 0x03,
            0x90, 0x10,
            0x60, 0x01,
            0x60, 0x02
        ]);
        
        chip.Run(cycles: 4);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
    }

    [Fact]
    public void Chip_RegistersMove()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x05,
            0x61, 0x03,
            0x80, 0x10
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x03);
    }
    
    [Fact]
    public void Chip_RegistersOr()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x00,
            0x61, 0xFF,
            0x80, 0x11
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFF);
    }
    
    [Fact]
    public void Chip_RegistersAnd()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xFF,
            0x61, 0xF0,
            0x80, 0x12
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0xF0);
    }
    
    [Fact]
    public void Chip_RegistersXor()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xF0,
            0x61, 0xF0,
            0x80, 0x13
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x00);
    }
    
    [Fact]
    public void Chip_RegistersAdd_NotOverFlow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x01,
            0x61, 0x01,
            0x80, 0x14
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
        chip.Registers.V[0xF].ShouldBe((byte) 0x0);
    }
    
    [Fact]
    public void Chip_RegistersAdd_WithOverFlow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xFF,
            0x61, 0x02,
            0x80, 0x14
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x01);
        chip.Registers.V[0xF].ShouldBe((byte) 0x1);
    }
    
    [Fact]
    public void Chip_RegistersSubtract_NotBorrow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x02,
            0x61, 0x01,
            0x80, 0x15
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x1);
        chip.Registers.V[0xF].ShouldBe((byte) 0x1);
    }
    
    [Fact]
    public void Chip_RegistersSubtract_WithBorrow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x01,
            0x61, 0x02,
            0x80, 0x15
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFF);
        chip.Registers.V[0xF].ShouldBe((byte) 0x0);
    }
   
    [Fact]
    public void Chip_RegistersSubtractReverse_NotBorrow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x01,
            0x61, 0x02,
            0x80, 0x17
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x1);
        chip.Registers.V[0xF].ShouldBe((byte) 0x1);
    }
    
    [Fact]
    public void Chip_RegistersSubtractReverse_WithBorrow()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x02,
            0x61, 0x01,
            0x80, 0x17
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFF);
        chip.Registers.V[0xF].ShouldBe((byte) 0x0);
    }

    [Fact]
    public void Chip_RegistersShiftRight()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x02,
            0x80, 0x16
        ]);
        
        chip.Run(cycles: 2);
        
        chip.Registers.V[0].ShouldBe((byte) 0x01);
        chip.Registers.V[0xF].ShouldBe((byte) 0x0);
    }
    
    [Fact]
    public void Chip_RegistersShiftLeft()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x01,
            0x80, 0x1E
        ]);
        
        chip.Run(cycles: 2);
        
        chip.Registers.V[0].ShouldBe((byte) 0x02);
        chip.Registers.V[0xF].ShouldBe((byte) 0x0);
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

    [Fact]
    public void Chip_Rom_Corax()
    {
        var rom = File.ReadAllBytes("./Roms/3-corax+.ch8");
        var chip = new Chip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        chip.Load(rom);
        chip.Run(cycles: 25);
    }
    
    [Fact]
    public void Chip_Rom_Flags()
    {
        var rom = File.ReadAllBytes("./Roms/4-flags.ch8");
        var chip = new Chip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        chip.Load(rom);
        chip.Run(cancellation.Token);
    }
}