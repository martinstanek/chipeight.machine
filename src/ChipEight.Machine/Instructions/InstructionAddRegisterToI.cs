namespace ChipEight.Machine.Instructions;

public sealed class InstructionAddRegisterToI : Instruction
{
    public InstructionAddRegisterToI(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x1E;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.I = (ushort) (Chip.Registers.I + Chip.Registers.V[reg]);
    }
}