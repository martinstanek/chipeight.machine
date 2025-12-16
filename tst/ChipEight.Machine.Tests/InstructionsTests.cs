using Shouldly;
using Xunit;

namespace ChipEight.Machine.Tests;

public class InstructionsTests
{
    // TODO: all the registers shift from 0 one as default
    
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
    public void Chip_JumpToAddress()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x10,
            0xB2, 0x00
        ]);
        chip.Run(cycles: 2);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x210);
    }
    
    [Fact]
    public void Chip_RandomToRegister()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x81,
            0xC0, 0x01
        ]);
        chip.Run(cycles: 2);
        
        chip.Registers.V[0x0].ShouldNotBe((byte) 0x81);
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
    public void Chip_SkipIfKey()
    {
        var chip = new Chip();
        
        chip.Keypad.Keys[0x2] = true;
        chip.Load([
            0x61, 0x02,
            0xE1, 0x9E
        ]);
        chip.Run(cycles: 2);
        
        chip.Registers.Pc.ShouldBe((ushort) 0x206);
    }
    
    [Fact]
    public void Chip_SkipIfNotKey()
    {
        var chip = new Chip();
        
        chip.Keypad.Keys[0x2] = false;
        chip.Load([
            0x61, 0x02,
            0xE1, 0xA1
        ]);
        chip.Run(cycles: 2);
        
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
            0x61, 0x05,
            0x31, 0x05,
            0x61, 0x01,
            0x61, 0x02
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0x1].ShouldBe((byte) 0x02);
    }
    
    [Fact]
    public void Chip_SkipIfNotEqual()
    {
        var chip = new Chip();
        
        chip.Load([
            0x61, 0x05,
            0x41, 0x04,
            0x61, 0x01,
            0x61, 0x02
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[1].ShouldBe((byte) 0x02);
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
    public void Chip_AddRegisterToI()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0x01,
            0xA0, 0x01,
            0xF0, 0x1E
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0x01);
        chip.Registers.I.ShouldBe((byte) 0x02);
    }
    
    [Fact]
    public void Chip_BCD()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xFE,
            0xA2, 0x50,
            0xF0, 0x33
        ]);
        
        chip.Run(cycles: 3);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFE);
        chip.Registers.I.ShouldBe((ushort) 0x250);
        chip.ReadWriteMemory.Raw[chip.Registers.I].ShouldBe((byte) 2);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 1].ShouldBe((byte) 5);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 2].ShouldBe((byte) 4);
    }
    
    [Fact]
    public void Chip_StoreRegistersToMemory()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xFE,
            0x61, 0xEF,
            0x62, 0xFF,
            0xA2, 0x50,
            0xF3, 0x55
        ]);
        
        chip.Run(cycles: 5);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFE);
        chip.Registers.V[1].ShouldBe((byte) 0xEF);
        chip.Registers.V[2].ShouldBe((byte) 0xFF);
        chip.Registers.I.ShouldBe((ushort) 0x250);
        chip.ReadWriteMemory.Raw[chip.Registers.I].ShouldBe((byte) 0xFE);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 1].ShouldBe((byte) 0xEF);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 2].ShouldBe((byte) 0xFF);
    }
    
    [Fact]
    public void Chip_LoadRegistersFromMemory()
    {
        var chip = new Chip();
        
        chip.Load([
            0x60, 0xFE,
            0x61, 0xEF,
            0x62, 0xFF,
            0xA2, 0x50,
            0xF3, 0x55,
            0x60, 0x00,
            0x61, 0x00,
            0x62, 0x00,
            0xF3, 0x65
        ]);
        
        chip.Run(cycles: 9);
        
        chip.Registers.V[0].ShouldBe((byte) 0xFE);
        chip.Registers.V[1].ShouldBe((byte) 0xEF);
        chip.Registers.V[2].ShouldBe((byte) 0xFF);
        chip.Registers.I.ShouldBe((ushort) 0x250);
        chip.ReadWriteMemory.Raw[chip.Registers.I].ShouldBe((byte) 0xFE);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 1].ShouldBe((byte) 0xEF);
        chip.ReadWriteMemory.Raw[chip.Registers.I + 2].ShouldBe((byte) 0xFF);
    }
    
    [Fact]
    public void Chip_DrawSprite()
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
}