using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ChipEight.Machine.Output;

namespace ChipEight.Machine;

public sealed class Chip
{
    public const ushort FontSetAddress = 0x050;
    public const ushort StartAddress = 0x200;

    private readonly Lazy<InstructionSet> _instructionSet;
    private readonly Lazy<Display> _display;
    private readonly Lazy<Memory> _memory;
    private readonly Registers _registers = new();
    private readonly Keypad _keypad = new();
    private readonly Random _random = new();
    private IRemoteDisplay? _remoteDisplay;
    private bool _shallRun = true;
    
    public Chip()
    {
        _instructionSet = new Lazy<InstructionSet>(() => GetInstructionSet(this));
        _display = new Lazy<Display>(GetDisplay);
        _memory = new Lazy<Memory>(GetMemory);
    }
    
    public Chip Load(byte[] program)
    {
        _memory.Value.Load(program, StartAddress);

        return this;
    }

    public Chip Run(ushort cycles)
    {
        _shallRun = true;
        
        for (var c = 0; c < cycles; c++)
        {
            Step();
        }

        return this;
    }
    
    public Chip Run(CancellationToken cancellationToken)
    {
        _shallRun = true;
        
        while (_shallRun && !cancellationToken.IsCancellationRequested)
        {   
            Step();
        }

        return this;
    }

    public Chip Run()
    {
        return Run(CancellationToken.None);
    }

    public Chip WithRemoteDisplay(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        
        _remoteDisplay = new PixelDisplayClient(url);

        return this;
    }

    public void Step()
    {
        Opcode = _memory.Value.GetOpcode(_registers.Pc);

        _registers.Pc += 2;

        _instructionSet.Value.Execute(Opcode.Value);
    }

    public void Stop()
    {
        _shallRun = false;
    }

    private static InstructionSet GetInstructionSet(Chip chip)
    {
        return new InstructionSet().Build(chip);
    }

    private Display GetDisplay()
    {
        return _remoteDisplay is not null
            ? new Display(_remoteDisplay)
            : new Display();
    }

    private static Memory GetMemory()
    {
        return new Memory().Init();
    }

    public Memory Memory => _memory.Value;

    public Registers Registers => _registers;

    public Display Display => _display.Value;

    public Keypad Keypad => _keypad;

    public Random Random => _random;

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

public sealed class Font
{
    public static readonly byte[] FontSet =
    [
        0xF0, 0x90, 0x90, 0x90, 0xF0,
        0x20, 0x60, 0x20, 0x20, 0x70,
        0xF0, 0x10, 0xF0, 0x80, 0xF0,
        0xF0, 0x10, 0xF0, 0x10, 0xF0,
        0x90, 0x90, 0xF0, 0x10, 0x10,
        0xF0, 0x80, 0xF0, 0x10, 0xF0,
        0xF0, 0x80, 0xF0, 0x90, 0xF0,
        0xF0, 0x10, 0x20, 0x40, 0x40,
        0xF0, 0x90, 0xF0, 0x90, 0xF0,
        0xF0, 0x90, 0xF0, 0x10, 0xF0,
        0xF0, 0x90, 0xF0, 0x90, 0x90,
        0xE0, 0x90, 0xE0, 0x90, 0xE0,
        0xF0, 0x80, 0x80, 0x80, 0xF0,
        0xE0, 0x90, 0x90, 0x90, 0xE0,
        0xF0, 0x80, 0xF0, 0x80, 0xF0,
        0xF0, 0x80, 0xF0, 0x80, 0x80
    ];
}

public sealed class Memory
{
    private readonly byte[] _memory = new byte[4096];

    public void Load(byte[] data, ushort address)
    {
        Array.Copy(data, 0, _memory, address, data.Length);

        BytesInMemory = (ushort) data.Length;
    }

    public Memory Init()
    {
        Load(Font.FontSet, Chip.FontSetAddress);

        return this;
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

public sealed class Keypad
{
    private readonly bool[] _keys = new bool[16];
    
    public byte WaitForKeyPress()
    {
        return 0x00;
    }

    public bool[] Keys => _keys;
}

public sealed class InstructionSet
{
    private readonly Dictionary<byte, List<Instruction>> _set = new();

    public InstructionSet Build(Chip chip)
    {
        RegisterPrimary(0x0, new InstructionClear(chip));
        RegisterPrimary(0x0, new InstructionReturn(chip));
        RegisterPrimary(0x1, new InstructionJump(chip));
        RegisterPrimary(0x2, new InstructionCallAddress(chip));
        RegisterPrimary(0x3, new InstructionSkipIfEqual(chip));
        RegisterPrimary(0x4, new InstructionSkipIfNotEqual(chip));
        RegisterPrimary(0x5, new InstructionSkipIfRegistersEqual(chip));
        RegisterPrimary(0x6, new InstructionLoadVxImmediate(chip));
        RegisterPrimary(0x7, new InstructionAddImmediateValueToRegister(chip));
        RegisterPrimary(0x8, new InstructionRegistersMove(chip));
        RegisterPrimary(0x8, new InstructionRegistersOr(chip));
        RegisterPrimary(0x8, new InstructionRegistersAnd(chip));
        RegisterPrimary(0x8, new InstructionRegistersXor(chip));
        RegisterPrimary(0x8, new InstructionRegistersAdd(chip));
        RegisterPrimary(0x8, new InstructionRegistersSubtract(chip));
        RegisterPrimary(0x8, new InstructionRegistersSubtractReverse(chip));
        RegisterPrimary(0x8, new InstructionRegistersShiftRight(chip));
        RegisterPrimary(0x8, new InstructionRegistersShiftLeft(chip));
        RegisterPrimary(0x9, new InstructionSkipIfRegistersNotEqual(chip));
        RegisterPrimary(0xA, new InstructionAddressToI(chip));
        RegisterPrimary(0xB, new InstructionJumpToAddress(chip));
        RegisterPrimary(0xC, new InstructionRandomToRegister(chip));
        RegisterPrimary(0xD, new InstructionDrawSprite(chip));
        RegisterPrimary(0xE, new InstructionSkipIfKey(chip));
        RegisterPrimary(0xE, new InstructionSkipIfNotKey(chip));
        RegisterPrimary(0xF, new InstructionGetDelayTimer(chip));
        RegisterPrimary(0xF, new InstructionSetDelayTimer(chip));
        RegisterPrimary(0xF, new InstructionWaitForKeyPress(chip));
        RegisterPrimary(0xF, new InstructionSetSoundTimer(chip));
        RegisterPrimary(0xF, new InstructionAddRegisterToI(chip));
        RegisterPrimary(0xF, new InstructionSetFontSpriteAddress(chip));
        RegisterPrimary(0xF, new InstructionStoreRegistersToMemory(chip));
        RegisterPrimary(0xF, new InstructionLoadRegistersFromMemory(chip));
        RegisterPrimary(0xF, new InstructionBinaryCodedDecimal(chip));

        return this;
    }

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
        var n = opcode & 0x000F;
        var x = Chip.Registers.V[xReg];
        var y = Chip.Registers.V[yReg];
        var sprite = new Span<byte>(Chip.Memory.Raw, Chip.Registers.I, n);

        Chip.Registers.V[0xF] = 0x0;
        
        var collisionDetected = Chip.Display.DrawSprite(x, y, sprite.ToArray());

        if (collisionDetected)
        {
            Chip.Registers.V[0xF] = 0x1;
        }
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
        var reg = (byte) ((opcode & 0x0F00) >> 8);
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

public sealed class InstructionSkipIfEqual : Instruction
{
    public InstructionSkipIfEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x3000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var regVal = Chip.Registers.V[reg];
        var kk = (byte) (opcode & 0x00FF);

        if (regVal == kk)
        {
            Chip.Registers.Pc += 2;
        }
    }
}

public sealed class InstructionSkipIfNotEqual : Instruction
{
    public InstructionSkipIfNotEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x4000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var regVal = Chip.Registers.V[reg];
        var kk = (byte) (opcode & 0x00FF);

        if (regVal != kk)
        {
            Chip.Registers.Pc += 2;
        }
    }
}

public sealed class InstructionSkipIfRegistersEqual : Instruction
{
    public InstructionSkipIfRegistersEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x5000 && (opcode & 0x000F) == 0;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        
        if (valY == valX)
        {
            Chip.Registers.Pc += 2;
        }
    }
}

public sealed class InstructionSkipIfRegistersNotEqual : Instruction
{
    public InstructionSkipIfRegistersNotEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x9000 && (opcode & 0x000F) == 0;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        
        if (valY != valX)
        {
            Chip.Registers.Pc += 2;
        }
    }
}

public sealed class InstructionRegistersMove : Instruction
{
    public InstructionRegistersMove(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 0;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valY = Chip.Registers.V[regY];

        Chip.Registers.V[regX] = valY;
    }
}

public sealed class InstructionRegistersOr : Instruction
{
    public InstructionRegistersOr(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 1;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];

        Chip.Registers.V[regX] = (byte) (valX | valY);
    }
}

public sealed class InstructionRegistersAnd : Instruction
{
    public InstructionRegistersAnd(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 2;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];

        Chip.Registers.V[regX] = (byte) (valX & valY);
    }
}

public sealed class InstructionRegistersXor : Instruction
{
    public InstructionRegistersXor(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 3;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];

        Chip.Registers.V[regX] = (byte) (valX ^ valY);
    }
}

public sealed class InstructionRegistersAdd : Instruction
{
    public InstructionRegistersAdd(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 4;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        var shouldCarry = valY + valX > 255;

        Chip.Registers.V[regX] = (byte) (valX + valY);
        Chip.Registers.V[0xF] = shouldCarry ? (byte) 0x1 : (byte) 0x0;
    }
}

public sealed class InstructionRegistersSubtract : Instruction
{
    public InstructionRegistersSubtract(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 5;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var regY = (byte) ((opcode & 0x00F0) >> 4);
        var valX = Chip.Registers.V[regX];
        var valY = Chip.Registers.V[regY];
        var shouldNotBorrow = valX >= valY;

        Chip.Registers.V[regX] = (byte) (valX - valY);
        Chip.Registers.V[0xF] = shouldNotBorrow ? (byte) 0x1 : (byte) 0x0;
    }
}

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

public sealed class InstructionRegistersShiftRight : Instruction
{
    public InstructionRegistersShiftRight(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 6;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        Chip.Registers.V[regX] = (byte) (valX >> 1);
        Chip.Registers.V[0xF] = (byte) (valX & 0x1);
    }
}

public sealed class InstructionRegistersShiftLeft : Instruction
{
    public InstructionRegistersShiftLeft(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x8000 && (opcode & 0x000F) == 0xE;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        Chip.Registers.V[regX] = (byte) (valX << 1);
        Chip.Registers.V[0xF] = (byte) ((valX & 0x80) >> 7);
    }
}

public sealed class InstructionGetDelayTimer : Instruction
{
    public InstructionGetDelayTimer(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x07;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.V[reg] = Chip.Registers.Dt; // TODO: implement
    }
}

public sealed class InstructionSetDelayTimer : Instruction
{
    public InstructionSetDelayTimer(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x15;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.Dt = Chip.Registers.V[reg]; // TODO: implement
    }
}

public sealed class InstructionSetSoundTimer : Instruction
{
    public InstructionSetSoundTimer(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x18;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.St = Chip.Registers.V[reg]; // TODO: implement
    }
}

public sealed class InstructionWaitForKeyPress : Instruction
{
    public InstructionWaitForKeyPress(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x0A;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.V[reg] = Chip.Keypad.WaitForKeyPress(); // TODO: implement
    }
}

public sealed class InstructionAddRegisterToI : Instruction
{
    public InstructionAddRegisterToI(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x1E;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.I = (ushort) (Chip.Registers.I + Chip.Registers.V[reg]);
    }
}

public sealed class InstructionSetFontSpriteAddress : Instruction
{
    public InstructionSetFontSpriteAddress(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x29;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (opcode & 0x0F00) >> 8;

        Chip.Registers.I = (ushort) (Chip.FontSetAddress + Chip.Registers.V[reg] * 5);
    }
}

public sealed class InstructionStoreRegistersToMemory : Instruction
{
    public InstructionStoreRegistersToMemory(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xF000 && ( opcode & 0x00FF) == 0x55;
    }
    
    public override void Execute(ushort opcode)
    {
        var x = (opcode & 0x0F00) >> 8;

        for (var i = 0; i <= x; i++)
        {
            Chip.Memory.Raw[Chip.Registers.I + i] = Chip.Registers.V[i];
        }
    }
}

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

public sealed class InstructionRandomToRegister : Instruction
{
    public InstructionRandomToRegister(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xC000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) ((opcode & 0x0F00) >> 8);
        var kk = (byte) (opcode & 0x00FF);
        var r = (byte) Chip.Random.Next(0, 256);

        Chip.Registers.V[reg] = (byte) (r & kk);
    }
}

public sealed class InstructionSkipIfKey : Instruction
{
    public InstructionSkipIfKey(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0xE000 && (((opcode & 0x00F0) >> 4) == 0x9) && (opcode & 0x000F) == 0xE;
    }
    
    public override void Execute(ushort opcode)
    {
        var regX = (byte) ((opcode & 0x0F00) >> 8);
        var valX = Chip.Registers.V[regX];

        if (Chip.Keypad.Keys[valX])
        {
            Chip.Registers.Pc += 2;
        }
    }
}

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
