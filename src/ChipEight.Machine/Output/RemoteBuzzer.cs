using System;
using System.Net.Http;

namespace ChipEight.Machine.Output;

public class RemoteBuzzer : IRemoteBuzzer
{
    private readonly Lazy<HttpClient> _httpClient;

    public RemoteBuzzer(string remoteUrl)
    {
        _httpClient = new Lazy<HttpClient>(GetHttpClient(remoteUrl));
    }
    
    public void On()
    {
        var response = _httpClient.Value.GetAsync("/buzzer/true").Result;

        response.EnsureSuccessStatusCode();
    }

    public void Off()
    {
        var resposne = _httpClient.Value.GetAsync("/buzzer/false").Result;

        resposne.EnsureSuccessStatusCode();
    }

    private static HttpClient GetHttpClient(string remoteUrl)
    {
        var client = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 1
        });

        client.BaseAddress = new Uri(remoteUrl);
        
        return client;
    }
}