namespace ChipEight.Machine.Instructions;

public sealed class InstructionClear : Instruction
{
    public InstructionClear(Chip chip) : base(chip) { }

    public override void Execute(ushort opcode)
    {
        Chip.Display.Clear();
    }

    public override bool CanExecute(ushort opcode) => opcode == 0x00E0;
}