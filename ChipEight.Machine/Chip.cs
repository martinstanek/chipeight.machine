using System;
using System.Linq;
using System.Collections.Generic;

namespace ChipEight.Machine;

public sealed class Chip
{
    private readonly InstructionSet _instructionSet = new();
    private readonly Registers _registers = new();
    private readonly Keyboard _keyboard = new();
    private readonly Display _display = new();
    private readonly Memory _memory = new();
    
    public Chip()
    {
        _instructionSet.Register(new InstructionClear(this));
    }

    public void Load(byte[] program)
    {
        _memory.Load(program, 0x200);
    }

    public void Run() { }

    public void Step()
    {
        var opcode = _memory.GetOpcode(_registers.Pc);

        _registers.Pc += 2; // TODO not mutate directly?

        _instructionSet.Execute(opcode);
    }

    public void Stop()
    {
    }

    public Memory Memory => _memory;

    public Registers Registers => _registers;

    public Display Display => _display;

    public Keyboard Keyboard => _keyboard;
}

public sealed class Memory
{
    private readonly byte[] _memory = new byte[4096];

    public void Load(byte[] data, ushort address)
    {
        Array.Copy(data, 0, _memory, address, data.Length);
    }

    public ushort GetOpcode(ushort programCounter)
    {
        return (ushort) ( (_memory[programCounter] << 8) | _memory[programCounter + 1] );
    }
}

public sealed class Registers
{
    private readonly ushort[] _stack = new ushort[16];
    private readonly byte[] _general = new byte[16];
    private ushort _pc = 0x200;
    private ushort _i = 0;
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

public sealed class Display
{
    public void Clear() { }
}

public sealed class Keyboard { }

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

    protected Chip Chip => _chip;
}

public sealed class InstructionClear : Instruction
{
    public InstructionClear(Chip chip) : base(chip) { }

    public override void Execute(ushort maschineCode)
    {
        Chip.Display.Clear();
    }

    public override bool CanExecute(ushort machineCode) => machineCode == 0x00E0;
}