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

            logger.LogInformation("Retrieved counter with value {Value}", response.Resource.Value);
            return Ok(response.Resource.Value);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Counter not found, returning default value 0");
            return Ok(0);
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to retrieve counter. StatusCode: {StatusCode}", ex.StatusCode);
            return StatusCode(500, "Failed to retrieve counter");
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<object>> GetCounterStatus(CancellationToken cancellationToken = default)
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

            return Ok(new
            {
                exists = true,
                id = response.Resource.Id,
                value = response.Resource.Value,
                updatedAt = response.Resource.UpdatedAt,
                partitionKey = response.Resource.PartitionKey
            });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Ok(new { exists = false, message = "Counter not yet initialized" });
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to retrieve counter status. StatusCode: {StatusCode}", ex.StatusCode);
            return StatusCode(500, new { error = "Failed to retrieve counter status" });
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

            logger.LogInformation("Incrementing existing counter from {OldValue} to {NewValue}", 
                response.Resource.Value, counterDoc.Value);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Counter not found during increment, creating new counter with value 1");
            counterDoc = new CounterDocument
            {
                Id = CounterId,
                PartitionKey = "counter",
                Value = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        try
        {
            await container.UpsertItemAsync(
                counterDoc,
                new PartitionKey(counterDoc.PartitionKey),
                cancellationToken: cancellationToken);

            logger.LogInformation("Counter successfully updated to {Value}", counterDoc.Value);
            return Ok(counterDoc.Value);
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to upsert counter document. StatusCode: {StatusCode}", ex.StatusCode);
            return StatusCode(500, "Failed to update counter");
        }
    }
}
