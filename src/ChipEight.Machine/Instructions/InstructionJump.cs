namespace ChipEight.Machine.Instructions;

public sealed class InstructionJump : Instruction
{
    public InstructionJump(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x1000;
    }

    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.Pc = nnn;
    }
}