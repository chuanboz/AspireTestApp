using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Tests;

public class AspireAppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    public DistributedApplication App => _app ?? throw new InvalidOperationException("App is not initialized. Ensure InitializeAsync has been called.");

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = CancellationToken.None;

        var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireTestApp_AppHost>(cancellationToken);
        
        appHostBuilder.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Information);
            logging.AddFilter(appHostBuilder.Environment.ApplicationName, LogLevel.Information);
            logging.AddFilter("Aspire.", LogLevel.Warning);
        });
        
        appHostBuilder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        _app = await appHostBuilder.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Wait for critical resources to be healthy
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("cosmosdb", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }
}

[CollectionDefinition("AspireAppHost")]
public class AspireAppHostCollection : ICollectionFixture<AspireAppHostFixture>
{
}
