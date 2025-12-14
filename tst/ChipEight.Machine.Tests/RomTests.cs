using System;
using System.IO;
using System.Threading;
using Xunit;

namespace ChipEight.Machine.Tests;

public sealed class RomTests
{
    [Fact]
    public void Rom_Logo()
    {
        var rom = File.ReadAllBytes("./Roms/1-chip8-logo.ch8");
        var chip = GetCip();
        
        chip.Load(rom);
        chip.Run(cycles: 39);
    }
    
    [Fact]
    public void Rom_Logo_Loop()
    {
        var rom = File.ReadAllBytes("./Roms/1-chip8-logo.ch8");
        var chip = GetCip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        
        chip.Load(rom);
        chip.Run(cancellation.Token);
    }
    
    [Fact]
    public void Rom_Ibm()
    {
        var rom = File.ReadAllBytes("./Roms/2-ibm-logo.ch8");
        var chip = GetCip();
        
        chip.Load(rom);
        chip.Run(cycles: 20);
    }

    [Fact]
    public void Rom_Corax()
    {
        var rom = File.ReadAllBytes("./Roms/3-corax+.ch8");
        var chip = GetCip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        chip.Load(rom);
        chip.Run(cancellation.Token);
    }
    
    [Fact]
    public void Rom_Flags()
    {
        var rom = File.ReadAllBytes("./Roms/4-flags.ch8");
        var chip = GetCip();
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        chip.Load(rom);
        chip.Run(cancellation.Token);
    }

    private static Chip GetCip()
    {
        return new Chip().WithRemoteDisplay("http://10.0.1.106:8090");
    }
}