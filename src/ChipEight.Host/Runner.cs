using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ChipEight.Machine;

namespace ChipEight.Host;

public class Runner : IHostedService
{
    private readonly Lazy<Chip> _chip;

    public Runner(Arguments arguments)
    {
        _chip = new Lazy<Chip>(GetChip(arguments.RomPath, arguments.RemoteHmiUrl));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Chip8");

        Task.Factory.StartNew(
            () => _chip.Value.Run(),
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _chip.Value.Stop();

        return Task.CompletedTask;
    }

    private static Chip GetChip(string romFile, string remoteHmiUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(romFile);

        var rom = File.ReadAllBytes(romFile);

        return Uri.TryCreate(remoteHmiUrl, UriKind.Absolute, out _)
            ? new Chip().Load(rom).WithRemoteDisplay(remoteHmiUrl).WithRemoteKeyPad(remoteHmiUrl)
            : new Chip().Load(rom);
    }
}