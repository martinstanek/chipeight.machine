using System;

namespace ChipEight.Machine.Output;

public sealed class Display
{
    public const byte Width = 64;
    public const byte Height = 32;

    private readonly bool[,] _pixels = new bool[Width, Height];
    private readonly IConsoleDisplay? _consoleDisplay;
    private readonly IRemoteDisplay? _remoteDisplay;

    public Display()
    {
        _consoleDisplay = new ConsoleDisplay();
    }
    
    public Display(IRemoteDisplay remoteDisplay)
    {
        _remoteDisplay = remoteDisplay;
    }
    
    public Display(IConsoleDisplay consoleDisplay)
    {
        _consoleDisplay = consoleDisplay;
    }

    public void Clear()
    {
        Array.Clear(_pixels);

        _consoleDisplay?.Clear();
        _remoteDisplay?.Clear();
    }

    public bool DrawSprite(byte x, byte y, byte[] sprite)
    {
        var collisionDetected = DrawSpriteInternal(x, y, sprite);
        
        _remoteDisplay?.DrawSprite(x, y, sprite);

        return collisionDetected;
    }
    
    public bool GetPixel(byte x, byte y)
    {
        return _pixels[x, y];
    }

    private bool DrawSpriteInternal(byte x, byte y, byte[] sprite)
    {
        var collision = false;
       
        for (var row = 0; row < sprite.Length; row++)
        {
            var spriteByte = sprite[row];

            for (var bit = 0; bit < 8; bit++)
            {
                if ((spriteByte & (0x80 >> bit)) == 0)
                {
                    continue;
                }

                var px = (byte) ((x + bit) % Width);
                var py = (byte) ((y + row) % Height);

                if (_pixels[px, py])
                {
                    collision = true;
                }

                _pixels[px, py] ^= true;
                _consoleDisplay?.SetPixel(px, py, _pixels[px, py]);
            }
        }
        
        _consoleDisplay?.Refresh();

        return collision;
    }
}