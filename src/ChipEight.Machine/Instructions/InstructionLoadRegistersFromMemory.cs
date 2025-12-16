namespace ChipEight.Machine.Instructions;

public sealed class InstructionLoadRegistersFromMemory : Instruction
{
    public InstructionLoadRegistersFromMemory(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x65;
    }
    
    public override void Execute(ushort opcode)
    {
        var x = (opcode & 0x0F00) >> 8;

        for (var i = 0; i <= x; i++)
        {
            Chip.Registers.V[i] = Chip.Memory.Raw[Chip.Registers.I + i];
        }
    }
}