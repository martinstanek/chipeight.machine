namespace ChipEight.Machine.Instructions;

public sealed class InstructionAddressToI : Instruction
{
    public InstructionAddressToI(Chip chip) : base(chip) { }
    
    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xA000;
    }
    
    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.I = nnn;
    }
}