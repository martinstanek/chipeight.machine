using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace ChipEight.Machine;

public sealed class Chip
{
    public const ushort FontSetAddress = 0x050;
    public const ushort StartAddress = 0x200;

    private readonly InstructionSet _instructionSet = new();
    private readonly Registers _registers = new();
    private readonly Keyboard _keyboard = new();
    private readonly Display _display = new(new PixelDisplayClient());
    private readonly Memory _memory = new();
    
    public Chip()
    {
        // TODO: clean this up, ca be way less chatty
        _instructionSet.RegisterPrimary(0x0, new InstructionClear(this));
        _instructionSet.RegisterPrimary(0x0, new InstructionReturn(this));
        _instructionSet.RegisterPrimary(0x1, new InstructionJump(this));
        _instructionSet.RegisterPrimary(0x2, new InstructionCallAddress(this));
        _instructionSet.RegisterPrimary(0x3, new InstructionSkipIfEqual(this));
        _instructionSet.RegisterPrimary(0x4, new InstructionSkipIfNotEqual(this));
        _instructionSet.RegisterPrimary(0x5, new InstructionSkipIfRegistersEqual(this));
        _instructionSet.RegisterPrimary(0x6, new InstructionLoadVxImmediate(this));
        _instructionSet.RegisterPrimary(0x7, new InstructionAddImmediateValueToRegister(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersMove(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersOr(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersAnd(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersXor(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersAdd(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersSubtract(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersSubtractReverse(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersShiftRight(this));
        _instructionSet.RegisterPrimary(0x8, new InstructionRegistersShiftLeft(this));
        _instructionSet.RegisterPrimary(0x9, new InstructionSkipIfRegistersNotEqual(this));
        _instructionSet.RegisterPrimary(0xA, new InstructionAddressToI(this));
        _instructionSet.RegisterPrimary(0xD, new InstructionDrawSprite(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionGetDelayTimer(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionSetDelayTimer(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionWaitForKeyPress(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionSetSoundTimer(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionAddRegisterToI(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionSetFontSpriteAddress(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionStoreRegistersToMemory(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionLoadRegistersFromMemory(this));
        _instructionSet.RegisterPrimary(0xF, new InstructionBinaryCodedDecimal(this));
        
        Memory.Load(Font.FontSet, FontSetAddress);
    }

    public void Load(byte[] program)
    {
        _memory.Load(program, StartAddress);
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

public interface IRemoteDisplay
{
    void Clear();

    void DrawSprite(byte x, byte y, byte[] sprite);
}

public sealed class Display
{
    public const byte Width = 64;
    public const byte Height = 32;

    private readonly bool[,] _pixels = new bool[Width, Height];
    private readonly IRemoteDisplay _remoteDisplay;

    public Display(IRemoteDisplay remoteDisplay)
    {
        _remoteDisplay = remoteDisplay;
    }

    public void Clear()
    {
        Array.Clear(_pixels);
        _remoteDisplay.Clear();
    }

    public bool DrawSprite(byte x, byte y, byte[] sprite)
    {
        var collisionDetected = DrawSpriteInternal(x, y, sprite);
        
        _remoteDisplay.DrawSprite(x, y, sprite);

        return collisionDetected;
    }
    
    public bool GetPixel(byte x, byte y)
    {
        return _pixels[x, y];
    }

    private bool DrawSpriteInternal(byte x, byte y, byte[] sprite)
    {
        var collision = false;

        for (var row = 0; row < sprite.Length; row++)
        {
            var spriteByte = sprite[row];

            for (var bit = 0; bit < 8; bit++)
            {
                if ((spriteByte & (0x80 >> bit)) == 0)
                {
                    continue;
                }

                var px = (x + bit) % Width;
                var py = (y + row) % Height;

                if (_pixels[px, py])
                {
                    collision = true;
                }

                _pixels[px, py] ^= true;
            }
        }

        return collision;
    }
}

public sealed class Keyboard
{
    public byte WaitForKeyPress()
    {
        return 0x00;
    }
}

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

public sealed class InstructionSkipIfEqual : Instruction
{
    public InstructionSkipIfEqual(Chip chip) : base(chip) { }

    public override bool CanExecute(ushort opcode)
    {
        return (opcode & 0xF000) == 0x3000;
    }
    
    public override void Execute(ushort opcode)
    {
        var reg = (byte) (opcode & 0x0F00) >> 8;
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
        var reg = (byte) (opcode & 0x0F00) >> 8;
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
        var regX = (byte) (opcode & 0x0F00) >> 8;
        var regY = (byte) (opcode & 0x00F0) >> 4;
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
        var regX = (byte) (opcode & 0x0F00) >> 8;
        var regY = (byte) (opcode & 0x00F0) >> 4;
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
        var regX = (byte) (opcode & 0x0F00) >> 8;
        var regY = (byte) (opcode & 0x00F0) >> 4;
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

        Chip.Registers.V[reg] = Chip.Keyboard.WaitForKeyPress(); // TODO: implement
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

        Chip.Registers.I = (byte) (Chip.Registers.I + Chip.Registers.V[reg]);
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

        for (var i = 0; i < x; i++)
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

        for (var i = 0; i < x; i++)
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
