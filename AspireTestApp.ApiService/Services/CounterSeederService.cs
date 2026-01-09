using AspireTestApp.Models;
using AspireTestApp.ServiceDefaults;
using Microsoft.Azure.Cosmos;

namespace AspireTestApp.ApiService.Services;

public class CounterSeederService(
    CosmosClient cosmosClient,
    ILogger<CounterSeederService> logger) : IHostedService
{
    private const string CounterId = "default";
    private const string CounterName = "default";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting counter seeding...");

            var container = cosmosClient
                .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
                .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

            // Check if counter already exists
            try
            {
                var existingCounter = await container.ReadItemAsync<CounterDocument>(
                    CounterId,
                    new PartitionKey(CounterName),
                    cancellationToken: cancellationToken);

                logger.LogInformation("Counter '{Name}' already exists with value {Value}", 
                    existingCounter.Resource.Name, existingCounter.Resource.Value);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Counter doesn't exist, create it with initial value of 0
                var initialCounter = new CounterDocument
                {
                    Id = CounterId,
                    Name = CounterName,
                    Value = 0,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                await container.CreateItemAsync(
                    initialCounter,
                    new PartitionKey(CounterName),
                    cancellationToken: cancellationToken);

                logger.LogInformation("Counter '{Name}' seeded successfully with initial value 0", CounterName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding counter data");
            // Don't throw - allow the application to start even if seeding fails
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
