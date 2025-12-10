using System.Collections.Generic;
using System.Linq;

namespace ChipEight.Machine;

public sealed class Chip
{
    private readonly Memory _memory = new();
    private readonly Registers _registers = new();
    private readonly InstructionSet _instructionSet = new();

    public Chip()
    {
        _instructionSet.Register(new InstructionNull(this));
    }

    public void Execute(byte[] program)
    {
        var chunks = program.Chunk(4);

        foreach (var chunk in chunks)
        {
            var machineCode = System.BitConverter.ToUInt16(chunk);
            
            _instructionSet.Execute(machineCode);
        }
    }
}

public sealed class Memory
{
    private readonly byte[] _memory = new byte[4096];
}

public sealed class Registers
{
    private readonly ushort[] _stack = new ushort[16];
    private readonly byte[] _general = new byte[16];
    private ushort _i = 0;
    private ushort _pc = 0x200;
    private byte _sp = 0;
    private byte _dt = 0;
    private byte _st = 0;

    public void Push(ushort value) => _stack[_sp++] = value;
    
    public ushort Pop() => _stack[--_sp];
    
    public bool[] Flags => Essentials.FromByte(_general[15]);

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

    public void Execute(ushort machineCode)
    {
        var instruction = _instructions.Single(i => i.CanExecute(machineCode));
        
        instruction.Execute(machineCode);
    }
}

public abstract class Instruction
{
    private readonly Chip _chip;

    protected Instruction(Chip chip)
    {
        _chip = chip;
    }
    
    public abstract void Execute(ushort machineCode);

    public abstract bool CanExecute(ushort machineCode);
}

public sealed class InstructionNull : Instruction
{
    public InstructionNull(Chip chip) : base(chip) { }

    public override void Execute(ushort maschineCode) { }

    public override bool CanExecute(ushort machineCode) => machineCode == 0;
}