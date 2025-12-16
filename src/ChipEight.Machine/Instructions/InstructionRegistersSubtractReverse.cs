namespace ChipEight.Machine.Instructions;

public sealed class InstructionRegistersSubtractReverse : Instruction
{
    public InstructionRegistersSubtractReverse(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 7;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        var shouldNotBorrow = valY >= valX;

        Chip.Registers.V[regX] = (byte) (valY - valX);
        Chip.Registers.V[0xF] = shouldNotBorrow ? (byte) 0x1 : (byte) 0x0;
    }
}