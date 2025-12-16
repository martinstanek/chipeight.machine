namespace ChipEight.Machine.Instructions;

public sealed class InstructionCallAddress : Instruction
{
    public InstructionCallAddress(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x2000;
    }
    
    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.Push(Chip.Registers.Pc);
        Chip.Registers.Pc = nnn;
    }
}