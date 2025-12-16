using System;

namespace ChipEight.Machine.Memory;

public sealed class ReadWriteMemory
{
    private readonly byte[] _memory = new byte[4096];

    public void Load(byte[] data, ushort address)
    {
        Array.Copy(data, 0, _memory, address, data.Length);

        BytesInMemory = (ushort) data.Length;
    }

    public ReadWriteMemory Init()
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