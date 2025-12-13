using System;
using System.Net.Http;

namespace ChipEight.Machine;

public sealed class PixelDisplayClient : IRemoteDisplay
{
    private const string RemoteDisplayUrl = "http://10.0.1.110:8090";
    
    private readonly Lazy<HttpClient> _httpClient = new(GetClient);

    private static HttpClient GetClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(RemoteDisplayUrl)
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