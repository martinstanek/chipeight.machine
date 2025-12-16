namespace ChipEight.Machine.Instructions;

public sealed class InstructionBinaryCodedDecimal : Instruction
{
    public InstructionBinaryCodedDecimal(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x33;
    }
    
    public override void Execute(ushort opcode)
    {
        var x = (opcode & 0x0F00) >> 8;
        var h = Chip.Registers.V[x] / 100;
        var t = Chip.Registers.V[x] / 10 % 10;
        var d = Chip.Registers.V[x] % 10;

        Chip.Memory.Raw[Chip.Registers.I] = (byte) h;
        Chip.Memory.Raw[Chip.Registers.I + 1] = (byte) t;
        Chip.Memory.Raw[Chip.Registers.I + 2] = (byte) d;
    }
}