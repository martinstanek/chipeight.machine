using Shouldly;
using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class Tests
{
    [Fact]
    public void Chip_Basic()
    {
        var chip = new Chip();
        
        chip.Load([0x00, 0xE0]);
        chip.Step();
        
        chip.Registers.Pc.ShouldBe((ushort) 0x202);
    }
}