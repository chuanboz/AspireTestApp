using AspireTestApp.Models;
using AspireTestApp.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace AspireTestApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CounterController(CosmosClient cosmosClient, ILogger<CounterController> logger) : ControllerBase
{
    private const string CounterId = "default";

    [HttpGet]
    public async Task<ActionResult<int>> GetCounter(CancellationToken cancellationToken = default)
    {
        try
        {
            var container = cosmosClient
                .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
                .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

            var response = await container.ReadItemAsync<CounterDocument>(
                CounterId,
                new PartitionKey("counter"),
                cancellationToken: cancellationToken);

            return Ok(response.Resource.Value);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("Counter not found, returning 0");
            return Ok(0);
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> IncrementCounter(CancellationToken cancellationToken = default)
    {
        var container = cosmosClient
            .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
            .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

        CounterDocument counterDoc;
        try
        {
            var response = await container.ReadItemAsync<CounterDocument>(
                CounterId,
                new PartitionKey("counter"),
                cancellationToken: cancellationToken);

            counterDoc = response.Resource with
            {
                Value = response.Resource.Value + 1,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogInformation("Counter not found, creating new counter");
            counterDoc = new CounterDocument
            {
                Id = CounterId,
                Value = 1,
                PartitionKey = "counter"
            };
        }

        await container.UpsertItemAsync(
            counterDoc,
            new PartitionKey(counterDoc.PartitionKey),
            cancellationToken: cancellationToken);

        logger.LogInformation("Counter incremented to {Value}", counterDoc.Value);
        return Ok(counterDoc.Value);
    }
}
