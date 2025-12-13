using Shouldly;
using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class EssentialsTests
{
    [Theory]
    [InlineData(0, new [] {false, false, false, false, false, false, false, false})]
    [InlineData(255, new [] {true, true, true, true, true, true, true, true})]
    [InlineData(128, new [] {true, false, false, false, false, false, false, false})]
    [InlineData(1, new [] {false, false, false, false, false, false, false, true})]
    [InlineData(64, new [] {false, true, false, false, false, false, false, false})]
    public void ByteToBooleans(byte input, bool[] output)
    {
        var result = Essentials.ByteToBooleans(input);
        
        result.ShouldBe(output);
    }
}