using System;
using System.Linq;
using System.Threading;
using System.Timers;
using ChipEight.Machine.Exceptions;
using ChipEight.Machine.Input;
using ChipEight.Machine.Instructions;
using ChipEight.Machine.Memory;
using ChipEight.Machine.Output;

namespace ChipEight.Machine;

public sealed class Chip : IDisposable
{
    public const ushort FontSetAddress = 0x050;
    public const ushort StartAddress = 0x200;

    private readonly Lazy<InstructionSet> _instructionSet;
    private readonly Lazy<ReadWriteMemory> _memory;
    private readonly Lazy<Display> _display;
    private readonly Lazy<Keypad> _keypad;
    private readonly Lazy<Buzzer> _buzzer;
    private readonly Registers _registers = new();
    private readonly Random _random = new();
    private readonly System.Timers.Timer _timer;
    private IRemoteDisplay? _remoteDisplay;
    private IRemoteKeyPad? _remoteKeyPad;
    private IRemoteBuzzer? _remoteBuzzer;
    private bool _shallRun = true;
    
    public Chip()
    {
        _instructionSet = new Lazy<InstructionSet>(() => GetInstructionSet(this));
        _memory = new Lazy<ReadWriteMemory>(GetMemory);
        _display = new Lazy<Display>(GetDisplay);
        _keypad = new Lazy<Keypad>(GetKeypad);
        _buzzer = new Lazy<Buzzer>(GetBuzzer);
        _timer = new System.Timers.Timer();
        _timer.Interval = 16;
        _timer.Elapsed += TimerOnElapsed;
    }

    public Chip Load(byte[] program)
    {
        _timer.Start();
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

    public Chip Run(TimeSpan runFor)
    {
        var cancellationTokenSource = new CancellationTokenSource(runFor);

        return Run(cancellationToken: cancellationTokenSource.Token);
    }

    public Chip Run()
    {
        return Run(CancellationToken.None);
    }

    public Chip WithRemoteDisplay(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        
        _remoteDisplay = new RemoteDisplay(url);

        return this;
    }

    public Chip WithRemoteKeyPad(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        _remoteKeyPad = new KeyPadClient(url);

        return this;
    }

    public Chip WithRemoteBuzzer(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        _remoteBuzzer = new RemoteBuzzer(url);

        return this;
    }

    public void Step()
    {
        Opcode = _memory.Value.GetOpcode(_registers.Pc);

        _registers.Pc += 2;

        if (!_instructionSet.Value.Execute(Opcode.Value))
        {
            Console.WriteLine($"Failed to execute opcode: 0x{Opcode:X4}");
            Console.WriteLine(ToString());

            throw new ChipUnprocessableCodeException();
        }
    }

    public void Stop()
    {
        _shallRun = false;
        _timer.Stop();
    }

    public void Dispose()
    {
        Stop();
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

    private Buzzer GetBuzzer()
    {
        return _remoteBuzzer is not null
            ? new Buzzer(_remoteBuzzer)
            : new Buzzer();
    }
    
    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        if (Registers.Dt > 0)
        {
            Registers.Dt--;
        }
        
        if (Registers.St > 0)
        {
            Buzzer.On();
            Registers.St--;
        }
        else
        {
            Buzzer.Off();    
        }
    }

    private static ReadWriteMemory GetMemory()
    {
        return new ReadWriteMemory().Init();
    }

    public ReadWriteMemory Memory => _memory.Value;

    public Registers Registers => _registers;

    public Display Display => _display.Value;

    public Keypad Keypad => _keypad.Value;

    public Buzzer Buzzer => _buzzer.Value;

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
        var registers = $"Vx: [{string.Join(',', Registers.V.Select(s => $"{s:X2}"))}]";

        return $"PC: {programCounterHex}, SP: {stackPointerHex}, I: {memoryRegisterHex}, {registers}, Opcode: {opcodeHex}, Mem: {Memory.BytesInMemory}B";
    }
}