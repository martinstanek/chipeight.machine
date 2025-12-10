using System.Collections.Generic;
using System.Linq;

namespace ChipEight.Machine;

public sealed class Chip
{
    private readonly Registers _registers = new();
    private readonly Memory _memory = new();
    private readonly InstructionSet _instructionSet = new();

    public Chip()
    {
        _instructionSet.Register(new InstructionNull(this));
    }
}

public sealed class Memory
{
    private readonly byte[] _memory = new byte[4096];
}

public sealed class Registers
{
    private ushort[] _stack = new ushort[16];
    private byte[] _general = new byte[15];
    private byte _flag = 0;
    private ushort _i = 0;
    private ushort _pc = 0;
    private byte _sp = 0;
    private byte _dt = 0;
    private byte _st = 0;

    public bool[] Flags => Essentials.FromByte(_flag);

    public ushort I
    {
        get => _i;
        set { _i = value; }
    }

    public ushort Pc
    {
        get => _pc;
        set { _pc = value; }
    }
    
    public byte Sp
    {
        get => _sp;
        set { _sp = value; }
    }
    
    public byte Dt
    {
        get => _dt;
        set { _dt = value; }
    }
    
    public byte St
    {
        get => _st;
        set { _st = value; }
    }
}

public sealed class InstructionSet
{
    private readonly List<Instruction> _instructions = new();
    
    public InstructionSet Register(Instruction instruction)
    {
        _instructions.Add(instruction);

        return this;
    }

    public void Execute(byte[] machineCode)
    {
        var instruction = _instructions.Single(i => i.IsApplicable(machineCode));
        
        instruction.Execute();
    }
}

public abstract class Instruction
{
    private readonly Chip _chip;

    protected Instruction(Chip chip)
    {
        _chip = chip;
    }
    
    public abstract void Execute();

    public abstract bool IsApplicable(byte[] machineCode);
}

public sealed class InstructionNull : Instruction
{
    public InstructionNull(Chip chip) : base(chip) { }

    public override void Execute() { }

    public override bool IsApplicable(byte[] machineCode) => machineCode.Equals(new[] { 0, 0, 0, 0 });
}