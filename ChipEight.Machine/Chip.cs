namespace ChipEight.Machine;

public sealed class Chip
{
    private readonly Memory _memory = new();
    private readonly Registers _registers = new();
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
}