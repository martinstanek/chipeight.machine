namespace ChipEight.Host;

public sealed record Arguments
{
    public required string RomPath { get; init; }

    public required string RemoteHmiUrl { get; init; }
}