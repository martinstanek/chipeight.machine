namespace ChipEight.Machine.Instructions;

public sealed class InstructionSkipIfRegistersEqual : Instruction
{
    public InstructionSkipIfRegistersEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x5000 && (opcode & 0x000F) == 0;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        
        if (valY == valX)
        {
            Chip.Registers.Pc += 2;
        }
    }
}