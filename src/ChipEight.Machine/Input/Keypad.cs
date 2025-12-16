namespace ChipEight.Machine.Input;

public sealed class Keypad
{
    private readonly IRemoteKeyPad _remoteKeyPad;
    private readonly bool[] _keys = new bool[16];
    
    public Keypad()
    {
        _remoteKeyPad = new NullKeyPad();
    }

    public Keypad(IRemoteKeyPad remoteKeyPad)
    {
        _remoteKeyPad = remoteKeyPad;
    }

    public bool IsLastKeyPressed()
    {
        return false;
    }

    public byte GetLastKeyPressed()
    {
        return 0x00;
    }

    public bool[] Keys => _keys;
}