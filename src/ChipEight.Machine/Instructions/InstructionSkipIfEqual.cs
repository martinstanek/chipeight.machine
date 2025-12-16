namespace ChipEight.Machine.Instructions;

public sealed class InstructionSkipIfEqual : Instruction
{
    public InstructionSkipIfEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x3000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var regVal = Chip.Registers.V[reg];
        var kk = (byte) (opcode & 0x00FF);

        if (regVal == kk)
        {
            Chip.Registers.Pc += 2;
        }
    }
}