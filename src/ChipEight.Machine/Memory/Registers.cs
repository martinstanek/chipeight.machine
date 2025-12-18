using System;
using ChipEight.Machine.Exceptions;

namespace ChipEight.Machine.Memory;

public sealed class Registers
{
    private readonly ushort[] _stack = new ushort[16];
    private readonly byte[] _general = new byte[16];
    private ushort _pc = 0x200;
    private ushort _i = 0;
    private byte _sp = 0;
    private byte _dt = 0;
    private byte _st = 0;

    public void Push(ushort value)
    {
        if (_sp + 1 >= _stack.Length)
        {
            Console.WriteLine("Stack Overflow");
            throw new ChipStackOverflowException();
        }

        _stack[_sp++] = value;
    }

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