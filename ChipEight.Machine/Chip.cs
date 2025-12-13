using System;
using System.Linq;
using System.Threading;
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
        // TODO: clean this up, ca be way less chatty
        _instructionSet.RegisterPrimary(0x0, new InstructionClear(this));
        _instructionSet.RegisterPrimary(0x0, new InstructionReturn(this));
        _instructionSet.RegisterPrimary(0x1, new InstructionJump(this));
        _instructionSet.RegisterPrimary(0x2, new InstructionCallAddress(this));
        _instructionSet.RegisterPrimary(0x6, new InstructionLoadVxImmediate(this));
        _instructionSet.RegisterPrimary(0x7, new InstructionAddImmediateValueToRegister(this));
        _instructionSet.RegisterPrimary(0xA, new InstructionAddressToI(this));
        _instructionSet.RegisterPrimary(0xD, new InstructionDrawSprite(this));
    }

    public void Load(byte[] program)
    {
        _memory.Load(program, 0x200);
    }

    public void Run(ushort cycles)
    {   
        for (var c = 0; c < cycles; c++)
        {
            Step();
        }
    }
    
    public void Run(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {   
            Step();
        }
    }

    public void Step()
    {
        Opcode = _memory.GetOpcode(_registers.Pc);

        _registers.Pc += 2;

        _instructionSet.Execute(Opcode.Value);
    }

    public void Stop()
    {
    }

    public Memory Memory => _memory;

    public Registers Registers => _registers;

    public Display Display => _display;

    public Keyboard Keyboard => _keyboard;

    public ushort? Opcode { get; private set; }

    public string ShowState()
    {
        var programCounterHex = Convert.ToHexString(BitConverter.GetBytes(Registers.Pc).Reverse().ToArray());
        var stackPointerHex = Convert.ToHexString([Registers.Sp]);
        var memoryRegisterHex = Convert.ToHexString(BitConverter.GetBytes(Registers.I).Reverse().ToArray());
        var opcodeHex = Opcode != null
            ? Convert.ToHexString(BitConverter.GetBytes(Opcode.Value).Reverse().ToArray())
            : "____";

        return $"PC: {programCounterHex}, SP: {stackPointerHex}, I: {memoryRegisterHex}, Opcode: {opcodeHex}, Mem: {Memory.BytesInMemory}B";
    }
}

public sealed class Memory
{
    private readonly byte[] _memory = new byte[4096];

    public void Load(byte[] data, ushort address)
    {
        Array.Copy(data, 0, _memory, address, data.Length);

        BytesInMemory = (ushort) data.Length;
    }

    public ushort GetOpcode(ushort programCounter)
    {
        return (ushort) ( (_memory[programCounter] << 8) | _memory[programCounter + 1] );
    }

    public byte[] Raw => _memory;

    public ushort BytesInMemory { get; private set; }
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

    public byte[] V => _general;
}

public sealed class Display
{
    private readonly PixelDisplayClient _pixelDisplay = new();

    public void Clear()
    {
        _pixelDisplay.ClearDisplay();
    }

    public void SetPixel(byte x, byte y, bool on)
    {
        _pixelDisplay.SetPixel(x, y, on);
    }

    public void DrawSprite(byte x, byte y, byte[] sprite)
    {
        _pixelDisplay.DrawSprite(x, y, sprite);
    }

    public bool GetPixel(byte x, byte y)
    {
        return false;
    }
}

public sealed class Keyboard { }

public sealed class InstructionSet
{
    private readonly Dictionary<byte, List<Instruction>> _set = new();

    public void RegisterPrimary(byte index, Instruction instruction)
    {
        if (!_set.ContainsKey(index))
        {
            _set[index] = new List<Instruction>();
        }

        _set[index].Add(instruction);
    }
    
    public void Execute(ushort opcode)
    {
        var high = (byte) (opcode >> 12);
        var instructions = _set[high];
        var instruction = instructions.FirstOrDefault(i => i.CanExecute(opcode));

        if (instruction is null)
        {
            throw new InvalidOperationException($"Unknown opcode: 0x{opcode:X4}");
        }

        instruction.Execute(opcode);
    }
}

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

public sealed class InstructionClear : Instruction
{
    public InstructionClear(Chip chip) : base(chip) { }

    public override void Execute(ushort opcode)
    {
        Chip.Display.Clear();
    }

    public override bool CanExecute(ushort opcode) => opcode == 0x00E0;
}

public sealed class InstructionLoadVxImmediate : Instruction
{
    public InstructionLoadVxImmediate(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x6000;
    }

    public override void Execute(ushort opcode)
    {
        var x = (byte) ((opcode & 0x0F00) >> 8);
        var nn = (byte) (opcode & 0x00FF);

        Chip.Registers.V[x] = nn;
    }
}

public sealed class InstructionAddressToI : Instruction
{
    public InstructionAddressToI(Chip chip) : base(chip) { }
    
    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xA000;
    }
    
    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.I = nnn;
    }
}

public sealed class InstructionDrawSprite : Instruction
{
    public InstructionDrawSprite(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xD000;
    }
    
    public override void Execute(ushort opcode)
    {
        var xReg = (opcode & 0x0F00) >> 8;
        var yReg = (opcode & 0x00F0) >> 4;
        var n = (opcode & 0x000F);
        var x = Chip.Registers.V[xReg];
        var y = Chip.Registers.V[yReg];
        var sprite = new Span<byte>(Chip.Memory.Raw, Chip.Registers.I, n);

        Chip.Display.DrawSprite(x, y, sprite.ToArray());
    }
}

public sealed class InstructionAddImmediateValueToRegister: Instruction
{
    public InstructionAddImmediateValueToRegister(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x7000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) (opcode & 0x0F00) >> 8;
        var valKk = Chip.Registers.V[reg];
        var kk = (byte) (opcode & 0x00FF);

        Chip.Registers.V[reg] = (byte) (valKk + kk);
    }
}

public sealed class InstructionJump : Instruction
{
    public InstructionJump(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x1000;
    }

    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.Pc = nnn;
    }
}

public sealed class InstructionCallAddress : Instruction
{
    public InstructionCallAddress(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x2000;
    }
    
    public override void Execute(ushort opcode)
    {
        var nnn = (ushort) (opcode & 0x0FFF);

        Chip.Registers.Push(Chip.Registers.Pc);
        Chip.Registers.Pc = nnn;
    }
}

public sealed class InstructionReturn : Instruction
{
    public InstructionReturn(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return opcode == 0x00EE;
    }
    
    public override void Execute(ushort opcode)
    {
        Chip.Registers.Pc = Chip.Registers.Pop();
    }
}