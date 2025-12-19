using System;
using System.Net.Http;

namespace ChipEight.Machine.Output;

public sealed class RemoteDisplay : IRemoteDisplay
{
    private readonly Lazy<HttpClient> _httpClient;

    public RemoteDisplay(string remoteUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteUrl);

        _httpClient = new Lazy<HttpClient>(() => GetClient(remoteUrl));
    }

    private static HttpClient GetClient(string remoteUrl)
    {
        var httpClient = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 2
        })
        {
            BaseAddress = new Uri(remoteUrl)
        };

        return httpClient;
    }

    public void Clear()
    {
        try
        {
            _httpClient.Value.GetAsync("/clear").GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void DrawSprite(byte x, byte y, byte[] sprite)
    {
        var hexString = Convert.ToHexStringLower(sprite);
        
        try
        {
            _httpClient.Value.GetAsync($"/sprite/{x}/{y}/{hexString}").GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}