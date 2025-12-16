using System;
using System.Linq;
using System.Net.Http;

namespace ChipEight.Machine.Input;

public sealed class KeyPadClient : IRemoteKeyPad
{
    private readonly Lazy<HttpClient> _httpClient;
    
    public KeyPadClient(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        _httpClient = new Lazy<HttpClient>(GetHttpClient(url));
    }
    
    public bool[] GetKeys()
    {
        var keys = _httpClient.Value.GetStringAsync("/keys").Result;

        if (string.IsNullOrWhiteSpace(keys))
        {
            throw new InvalidOperationException("Unexpected response");
        }

        var onlyPadKeys = keys.Substring(2, 16);
        var result = onlyPadKeys.Select(s => s == '1').ToArray();

        return result;
    }

    public byte? GetLastKeyPressed()
    {
        var keys = _httpClient.Value.GetStringAsync("/keys").Result;
         
        if (string.IsNullOrWhiteSpace(keys))
        {
            throw new InvalidOperationException("Unexpected response");
        }

        var lastKeyIndex = keys.Substring(0, 2);

        return lastKeyIndex.Equals("XX", StringComparison.OrdinalIgnoreCase)
            ? null
            : Convert.FromHexString(lastKeyIndex)[0];
    }

    public void AckLastKeyPressed()
    {
        var ack = _httpClient.Value.GetAsync("/keys/ack").Result;

        ack.EnsureSuccessStatusCode();
    }

    private static HttpClient GetHttpClient(string url)
    {
        var client = new HttpClient();

        client.BaseAddress = new Uri(url);

        return client;
    }
}