namespace ChipEight.Machine.Output;

public interface IConsoleDisplay
{
    void SetPixel(byte x, byte y, bool state);

    void Clear();

    void Refresh();
}