using Moq;
using Shouldly;
using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class DisplayTests
{
    [Fact]
    public void InitialState_AllPixelsOff()
    {
        var remoteDisplay = new Mock<IRemoteDisplay>();
        var display = new Display(remoteDisplay.Object);
    
        for (byte x = 0; x < Display.Width; x++)
        {
            for (byte y = 0; y < Display.Height; y++)
            {
                display.GetPixel(x, y).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public void DrawSprite_SpriteDrawn()
    {
        var remoteDisplay = new Mock<IRemoteDisplay>();
        var display = new Display(remoteDisplay.Object);

        display.DrawSprite(0, 0, [0x81, 0xFF, 0x00, 0x81]);
        
        display.GetPixel(0, 0).ShouldBeTrue();
        display.GetPixel(7, 0).ShouldBeTrue();
        display.GetPixel(0, 1).ShouldBeTrue();
        display.GetPixel(1, 1).ShouldBeTrue();
        display.GetPixel(2, 1).ShouldBeTrue();
        display.GetPixel(3, 1).ShouldBeTrue();
        display.GetPixel(4, 1).ShouldBeTrue();
        display.GetPixel(5, 1).ShouldBeTrue();
        display.GetPixel(6, 1).ShouldBeTrue();
        display.GetPixel(7, 1).ShouldBeTrue();
        display.GetPixel(0, 2).ShouldBeFalse();
        display.GetPixel(7, 2).ShouldBeFalse();
        display.GetPixel(0, 3).ShouldBeTrue();
        display.GetPixel(7, 3).ShouldBeTrue();
    }
    
    [Fact]
    public void RedrawSprite_SpriteIsGone()
    {
        var remoteDisplay = new Mock<IRemoteDisplay>();
        var display = new Display(remoteDisplay.Object);

        display.DrawSprite(0, 0, [0x81, 0xFF, 0x00, 0x81]);
        display.DrawSprite(0, 0, [0x81, 0xFF, 0x00, 0x81]);
        
        display.GetPixel(0, 0).ShouldBeFalse();
        display.GetPixel(7, 0).ShouldBeFalse();
        display.GetPixel(0, 1).ShouldBeFalse();
        display.GetPixel(1, 1).ShouldBeFalse();
        display.GetPixel(2, 1).ShouldBeFalse();
        display.GetPixel(3, 1).ShouldBeFalse();
        display.GetPixel(4, 1).ShouldBeFalse();
        display.GetPixel(5, 1).ShouldBeFalse();
        display.GetPixel(6, 1).ShouldBeFalse();
        display.GetPixel(7, 1).ShouldBeFalse();
        display.GetPixel(0, 2).ShouldBeFalse();
        display.GetPixel(7, 2).ShouldBeFalse();
        display.GetPixel(0, 3).ShouldBeFalse();
        display.GetPixel(7, 3).ShouldBeFalse();
    }
    
    [Fact]
    public void DrawSprites_CollisionDetected()
    {
    }

    [Fact]
    public void Clear_AllPixelsOff()
    {
        var remoteDisplay = new Mock<IRemoteDisplay>();
        var display = new Display(remoteDisplay.Object);

        display.DrawSprite(0, 0, [0x81, 0xFF, 0x00, 0x81]);
        display.Clear();
        
        for (byte x = 0; x < Display.Width; x++)
        {
            for (byte y = 0; y < Display.Height; y++)
            {
                display.GetPixel(x, y).ShouldBeFalse();
            }
        }
    }
}