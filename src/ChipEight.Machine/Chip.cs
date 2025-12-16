using System;
using System.Linq;
using System.Threading;
using ChipEight.Machine.Input;
using ChipEight.Machine.Instructions;
using ChipEight.Machine.Output;

namespace ChipEight.Machine;

public sealed class Chip
{
    public const ushort FontSetAddress = 0x050;
    public const ushort StartAddress = 0x200;

    private readonly Lazy<InstructionSet> _instructionSet;
    private readonly Lazy<Display> _display;
    private readonly Lazy<Memory> _memory;
    private readonly Lazy<Keypad> _keypad;
    private readonly Registers _registers = new();
    private readonly Random _random = new();
    private IRemoteDisplay? _remoteDisplay;
    private IRemoteKeyPad? _remoteKeyPad;
    private bool _shallRun = true;
    
    public Chip()
    {
        _instructionSet = new Lazy<InstructionSet>(() => GetInstructionSet(this));
        _display = new Lazy<Display>(GetDisplay);
        _memory = new Lazy<Memory>(GetMemory);
        _keypad = new Lazy<Keypad>(GetKeypad);
    }
    
    public Chip Load(byte[] program)
    {
        _memory.Value.Load(program, StartAddress);

        return this;
    }

    public Chip Run(ushort cycles)
    {
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

    public Chip WithRemoteKeyPad(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        _remoteKeyPad = new KeyPadClient(url);

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

    private Keypad GetKeypad()
    {
        return _remoteKeyPad is not null
            ? new Keypad(_remoteKeyPad)
            : new Keypad();
    }

    private static Memory GetMemory()
    {
        return new Memory().Init();
    }

    public Memory Memory => _memory.Value;

    public Registers Registers => _registers;

    public Display Display => _display.Value;

    public Keypad Keypad => _keypad.Value;

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