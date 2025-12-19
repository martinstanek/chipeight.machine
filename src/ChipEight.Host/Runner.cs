using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using ChipEight.Machine;

namespace ChipEight.Host;

public class Runner : IHostedService
{
    private readonly Arguments _arguments;
    private readonly Lazy<Chip> _chip;

    public Runner(Arguments arguments)
    {
        _arguments = arguments;
        _chip = new Lazy<Chip>(GetChip(arguments.RomPath, arguments.RemoteHmiUrl));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Chip8");

        Task.Factory.StartNew(
            () => Run(_arguments.DebugMode, cancellationToken),
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

    private void Run(bool isDebug, CancellationToken cancellationToken)
    {
        if (isDebug)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _chip.Value.Step();
                Console.WriteLine(_chip.Value.ToString());
                Console.ReadLine();
            }
            
            return;
        }

        _chip.Value.Run();
    }

    private static Chip GetChip(string romFile, string remoteHmiUrl)
    {
        ArgumentException.ThrowIfNullOrEmpty(romFile);

        var rom = File.ReadAllBytes(romFile);

        return Uri.TryCreate(remoteHmiUrl, UriKind.Absolute, out _)
            ? new Chip().Load(rom).WithRemoteDisplay(remoteHmiUrl).WithRemoteKeyPad(remoteHmiUrl).WithRemoteBuzzer(remoteHmiUrl)
            : new Chip().Load(rom);
    }
}