using System;

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

    public bool IsLastKeyPressed()
    {
        var lastKeyPressed = _remoteKeyPad.GetLastKeyPressed();

        return lastKeyPressed.HasValue;
    }

    public byte GetLastKeyPressed()
    {
        var lastKeyPressed = _remoteKeyPad.GetLastKeyPressed();

        if (!lastKeyPressed.HasValue)
        {
            throw new InvalidOperationException("Do not read without prior check");
        }

        _remoteKeyPad.AckLastKeyPressed();

        return lastKeyPressed.Value;
    }

    public bool[] Keys => _remoteKeyPad.GetKeys();
}