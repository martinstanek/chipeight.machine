namespace ChipEight.Machine.Instructions;

public sealed class InstructionRegistersShiftRight : Instruction
{
    public InstructionRegistersShiftRight(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 6;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        Chip.Registers.V[regX] = (byte) (valX >> 1);
        Chip.Registers.V[0xF] = (byte) (valX & 0x1);
    }
}