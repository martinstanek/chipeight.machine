using System;
using Microsoft.Extensions.DependencyInjection;

namespace ChipEight.Host.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChipEight(this IServiceCollection services, string[] args)
    {
        if (args.Length < 1)
        {
            throw new ArgumentException("No ROM provided ...");
        }

        var arguments = new Arguments
        {
            RomPath = args[0],
            RemoteHmiUrl = args.Length > 1 ? args[1] : string.Empty
        };

        return services
            .AddSingleton(arguments)
            .AddHostedService<Runner>();
    }
}