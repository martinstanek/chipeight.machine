namespace ChipEight.Machine.Instructions;

public sealed class InstructionLoadVxImmediate : Instruction
{
    public InstructionLoadVxImmediate(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x6000;
    }

    public override void Execute(ushort opcode)
    {
        var x = (byte) ((opcode & 0x0F00) >> 8);
        var nn = (byte) (opcode & 0x00FF);

        Chip.Registers.V[x] = nn;
    }
}