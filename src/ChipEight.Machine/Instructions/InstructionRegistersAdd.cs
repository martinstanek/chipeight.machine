namespace ChipEight.Machine.Instructions;

public sealed class InstructionRegistersAdd : Instruction
{
    public InstructionRegistersAdd(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 4;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        var shouldCarry = valY + valX > 255;

        Chip.Registers.V[regX] = (byte) (valX + valY);
        Chip.Registers.V[0xF] = shouldCarry ? (byte) 0x1 : (byte) 0x0;
    }
}