using System;
using System.Net.Http;
using Ninject.Activation;

namespace Monitor.ServiceCommon.Services;

public class HttpClientSvc : Provider<HttpClient>
{
    public HttpClient HttpClient { get; } = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    protected override HttpClient CreateInstance(IContext context)
    {
        return HttpClient;
    }
}