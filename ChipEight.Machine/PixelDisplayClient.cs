using System;
using System.Net.Http;

namespace ChipEight.Machine;

public sealed class PixelDisplayClient
{
    private const string RemoteDisplayUrl = "http://10.0.1.106:8090";
    
    private readonly Lazy<HttpClient> _httpClient = new(GetClient);

    private static HttpClient GetClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(RemoteDisplayUrl)
        };

        return httpClient;
    }

    public void ClearDisplay()
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

    public void SetPixel(byte x, byte y, bool state)
    {
        try
        {
            _httpClient.Value.GetAsync($"/set/{x}/{y}/{state}").GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}