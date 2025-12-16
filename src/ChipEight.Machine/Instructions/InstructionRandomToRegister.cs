namespace ChipEight.Machine.Instructions;

public sealed class InstructionRandomToRegister : Instruction
{
    public InstructionRandomToRegister(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xC000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var kk = (byte) (opcode & 0x00FF);
        var r = (byte) Chip.Random.Next(0, 256);

        Chip.Registers.V[reg] = (byte) (r & kk);
    }
}