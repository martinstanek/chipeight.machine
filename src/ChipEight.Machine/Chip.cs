using System;
using System.Linq;
using System.Threading;
using ChipEight.Machine.Input;
using ChipEight.Machine.Instructions;
using ChipEight.Machine.Memory;
using ChipEight.Machine.Output;

namespace ChipEight.Machine;

public sealed class Chip
{
    public const ushort FontSetAddress = 0x050;
    public const ushort StartAddress = 0x200;

    private readonly Lazy<InstructionSet> _instructionSet;
    private readonly Lazy<ReadWriteMemory> _memory;
    private readonly Lazy<Display> _display;
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
        _memory = new Lazy<Memory.ReadWriteMemory>(GetMemory);
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

    private static Memory.ReadWriteMemory GetMemory()
    {
        return new Memory.ReadWriteMemory().Init();
    }

    public Memory.ReadWriteMemory ReadWriteMemory => _memory.Value;

    public Registers Registers => _registers;

    public Display Display => _display.Value;

    public Keypad Keypad => _keypad.Value;

    public Random Random => _random;

    public ushort? Opcode { get; private set; }

    public override string ToString()
    {
        var programCounterHex = Convert.ToHexString(BitConverter.GetBytes(Registers.Pc).Reverse().ToArray());
        var stackPointerHex = Convert.ToHexString([Registers.Sp]);
        var memoryRegisterHex = Convert.ToHexString(BitConverter.GetBytes(Registers.I).Reverse().ToArray());
        var opcodeHex = Opcode != null
            ? Convert.ToHexString(BitConverter.GetBytes(Opcode.Value).Reverse().ToArray())
            : "____";

        return $"PC: {programCounterHex}, SP: {stackPointerHex}, I: {memoryRegisterHex}, Opcode: {opcodeHex}, Mem: {ReadWriteMemory.BytesInMemory}B";
    }
}