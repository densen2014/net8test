using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace BlazorWebApp.Services;

public class HttpClientSetupService : BackgroundService
{
    private readonly HttpClient _httpClient;
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public HttpClientSetupService(
        HttpClient httpClient,
        IServer server,
        IHostApplicationLifetime applicationLifetime)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var applicationStartedToken = _applicationLifetime.ApplicationStarted;
        if (applicationStartedToken.IsCancellationRequested)
        {
            ConfigureHttpClient();
        }
        else
        {
            applicationStartedToken.Register(ConfigureHttpClient);
        }

        return Task.CompletedTask;
    }

    private void ConfigureHttpClient()
    {
        var serverAddresses = _server.Features.Get<IServerAddressesFeature>();
        var address = serverAddresses!.Addresses.FirstOrDefault();
        if (address == null)
        {
            // Default ASP.NET Core Kestrel endpoint
            address = "https://localhost:7241";
        }
        else
        {
            address = address.Replace("*", "localhost", StringComparison.Ordinal);
            address = address.Replace("+", "localhost", StringComparison.Ordinal);
            address = address.Replace("[::]", "localhost", StringComparison.Ordinal);
        }

        var baseUri = new Uri(address);
        _httpClient.BaseAddress = baseUri;
    }
}