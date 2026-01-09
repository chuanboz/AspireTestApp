using AspireTestApp.Models;
using AspireTestApp.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace AspireTestApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CounterController(CosmosClient cosmosClient, ILogger<CounterController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<int>> GetCounter([FromQuery] string name = "default", CancellationToken cancellationToken = default)
    {
        try
        {
            var container = cosmosClient
                .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
                .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

            var response = await container.ReadItemAsync<CounterDocument>(
                name,
                new PartitionKey(name),
                cancellationToken: cancellationToken);

            logger.LogInformation("Retrieved counter '{Name}' with value {Value}", name, response.Resource.Value);
            return Ok(response.Resource.Value);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Counter '{Name}' not found, returning default value 0", name);
            return Ok(0);
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to retrieve counter '{Name}'. StatusCode: {StatusCode}", name, ex.StatusCode);
            return StatusCode(500, "Failed to retrieve counter");
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<object>> GetCounterStatus([FromQuery] string name = "default", CancellationToken cancellationToken = default)
    {
        try
        {
            var container = cosmosClient
                .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
                .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

            var response = await container.ReadItemAsync<CounterDocument>(
                name,
                new PartitionKey(name),
                cancellationToken: cancellationToken);

            return Ok(new
            {
                exists = true,
                id = response.Resource.Id,
                name = response.Resource.Name,
                value = response.Resource.Value,
                updatedAt = response.Resource.UpdatedAt
            });
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Ok(new { exists = false, message = $"Counter '{name}' not yet initialized" });
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to retrieve counter '{Name}' status. StatusCode: {StatusCode}", name, ex.StatusCode);
            return StatusCode(500, new { error = "Failed to retrieve counter status" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> IncrementCounter([FromQuery] string name = "default", CancellationToken cancellationToken = default)
    {
        var container = cosmosClient
            .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
            .GetContainer(AspireConstants.CosmosDb.CounterContainerName);

        CounterDocument counterDoc;
        try
        {
            var response = await container.ReadItemAsync<CounterDocument>(
                name,
                new PartitionKey(name),
                cancellationToken: cancellationToken);

            counterDoc = response.Resource with
            {
                Value = response.Resource.Value + 1,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            logger.LogInformation("Incrementing existing counter '{Name}' from {OldValue} to {NewValue}", 
                name, response.Resource.Value, counterDoc.Value);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("Counter '{Name}' not found during increment, creating new counter with value 1", name);
            counterDoc = new CounterDocument
            {
                Id = name,
                Name = name,
                Value = 1,
                UpdatedAt = DateTimeOffset.UtcNow
            };
        }

        try
        {
            await container.UpsertItemAsync(
                counterDoc,
                new PartitionKey(name),
                cancellationToken: cancellationToken);

            logger.LogInformation("Counter '{Name}' successfully updated to {Value}", name, counterDoc.Value);
            return Ok(counterDoc.Value);
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to upsert counter '{Name}' document. StatusCode: {StatusCode}", name, ex.StatusCode);
            return StatusCode(500, "Failed to update counter");
        }
    }
}
