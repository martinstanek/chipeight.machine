namespace ChipEight.Machine.Instructions;

public sealed class InstructionAddImmediateValueToRegister: Instruction
{
    public InstructionAddImmediateValueToRegister(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x7000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var valKk = Chip.Registers.V[reg];
        var kk = (byte) (opcode & 0x00FF);

        Chip.Registers.V[reg] = (byte) (valKk + kk);
    }
}