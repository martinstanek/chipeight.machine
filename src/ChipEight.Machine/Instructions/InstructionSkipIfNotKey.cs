namespace ChipEight.Machine.Instructions;

public sealed class InstructionSkipIfNotKey : Instruction
{
    public InstructionSkipIfNotKey(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xE000 && (((opcode & 0x00F0) >> 4) == 0xA) && (opcode & 0x000F) == 0x1;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        if (!Chip.Keypad.Keys[valX])
        {
            Chip.Registers.Pc += 2;
        }
    }
}