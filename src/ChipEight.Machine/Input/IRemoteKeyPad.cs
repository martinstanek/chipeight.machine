namespace ChipEight.Machine.Input;

public interface IRemoteKeyPad
{
    bool[] GetKeys();

    byte? GetLastKeyPressed();

    void AckLastKeyPressed();
}