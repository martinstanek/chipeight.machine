using System;
using System.Net.Http;
using ChipEight.Machine.Output;

namespace ChipEight.Machine;

public sealed class PixelDisplayClient : IRemoteDisplay
{
    private readonly Lazy<HttpClient> _httpClient;

    public PixelDisplayClient(string remoteUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(remoteUrl);

        _httpClient = new Lazy<HttpClient>(() => GetClient(remoteUrl));
    }

    private static HttpClient GetClient(string remoteUrl)
    {
        var httpClient = new HttpClient
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