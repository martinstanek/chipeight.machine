namespace ChipEight.Machine.Input;

public interface IRemoteKeyPad
{
    byte? GetLastKeyPressed();

    void AckLastKeyPressed();

    bool[] Keys { get; }
}