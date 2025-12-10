using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class Tests
{
    [Fact]
    public void Chip_Basic()
    {
        var chip = new Chip();
        
        chip.Execute(new byte[] {0, 0});
    }
}