using System;

namespace ChipEight.Machine.Instructions;

public sealed class InstructionDrawSprite : Instruction
{
    public InstructionDrawSprite(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xD000;
    }
    
    public override void Execute(ushort opcode)
    {
        var xReg = (opcode & 0x0F00) >> 8;
        var yReg = (opcode & 0x00F0) >> 4;
        var n = opcode & 0x000F;
        var x = Chip.Registers.V[xReg];
        var y = Chip.Registers.V[yReg];
        var sprite = new Span<byte>(Chip.Memory.Raw, Chip.Registers.I, n);

        Chip.Registers.V[0xF] = 0x0;
        
        var collisionDetected = Chip.Display.DrawSprite(x, y, sprite.ToArray());

        if (collisionDetected)
        {
            Chip.Registers.V[0xF] = 0x1;
        }
    }
}