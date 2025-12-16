using System.Linq;

namespace ChipEight.Machine.Input;

public sealed class NullKeyPad : IRemoteKeyPad
{
    private readonly bool[] _keys = new bool[16];
    
    public byte? GetLastKeyPressed()
    {
        return null;
    }

    public void AckLastKeyPressed() { }

    public bool[] Keys => _keys;
}