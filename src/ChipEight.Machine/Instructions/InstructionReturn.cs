namespace ChipEight.Machine.Instructions;

public sealed class InstructionReturn : Instruction
{
    public InstructionReturn(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return opcode == 0x00EE;
    }
    
    public override void Execute(ushort opcode)
    {
        Chip.Registers.Pc = Chip.Registers.Pop();
    }
}