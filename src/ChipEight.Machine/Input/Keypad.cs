namespace ChipEight.Machine.Input;

public sealed class Keypad
{
    private readonly IRemoteKeyPad _remoteKeyPad;
    
    public Keypad()
    {
        _remoteKeyPad = new NullKeyPad();
    }

    public Keypad(IRemoteKeyPad remoteKeyPad)
    {
        _remoteKeyPad = remoteKeyPad;
    }

    private readonly bool[] _keys = new bool[16];
    
    public byte WaitForKeyPress()
    {
        return 0x00;
    }

    public bool[] Keys => _keys;
}