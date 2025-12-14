namespace ChipEight.Machine.Output;

public interface IRemoteDisplay
{
    void Clear();

    void DrawSprite(byte x, byte y, byte[] sprite);
}