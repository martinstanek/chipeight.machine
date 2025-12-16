namespace ChipEight.Machine.Instructions;

public sealed class InstructionRegistersShiftLeft : Instruction
{
    public InstructionRegistersShiftLeft(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 0xE;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        Chip.Registers.V[regX] = (byte) (valX << 1);
        Chip.Registers.V[0xF] = (byte) ((valX & 0x80) >> 7);
    }
}