using System.Linq;

namespace ChipEight.Machine.Input;

public sealed class NullKeyPad : IRemoteKeyPad
{
    public bool[] GetKeys()
    {
        return Enumerable.Repeat(false, 16).ToArray();
    }

    public byte? GetLastKeyPressed()
    {
        return null;
    }

    public void AckLastKeyPressed() { }
}