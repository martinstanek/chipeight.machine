namespace ChipEight.Machine.Instructions;

public sealed class InstructionJumpToAddress : Instruction
{
    public InstructionJumpToAddress(Chip chip) : base(chip) { }
    
    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xB000;
    }
    
    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);
        
        Chip.Registers.Pc = (ushort) (nnn + Chip.Registers.V[0x0]);
    }
}