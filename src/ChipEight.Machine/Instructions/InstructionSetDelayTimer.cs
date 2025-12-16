namespace ChipEight.Machine.Instructions;

public sealed class InstructionSetDelayTimer : Instruction
{
    public InstructionSetDelayTimer(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x15;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.Dt = Chip.Registers.V[reg]; // TODO: implement
    }
}