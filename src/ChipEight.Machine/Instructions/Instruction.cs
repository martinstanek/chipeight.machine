namespace ChipEight.Machine.Instructions;

public abstract class Instruction
{
    private readonly Chip _chip;

    protected Instruction(Chip chip)
    {
        _chip = chip;
    }
    
    public abstract void Execute(ushort opcode);

    public abstract bool CanExecute(ushort opcode);

    protected Chip Chip => _chip;
}