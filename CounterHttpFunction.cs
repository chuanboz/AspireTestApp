using AspireTestApp.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Functions;

public sealed class CounterHttpFunction
{
    private readonly ILogger<CounterHttpFunction> _logger;

    public CounterHttpFunction(ILogger<CounterHttpFunction> logger)
    {
        _logger = logger;
    }

    [Function("GetCounter")]
    public IActionResult GetCounter(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "counter/{id}")] HttpRequest req,
        string id)
    {
        _logger.LogInformation("Getting counter with id: {Id}", id);

        // This is a sample HTTP trigger
        // In a real scenario, you would retrieve the counter from Cosmos DB
        var counter = new CounterDocument
        {
            Id = id,
            Value = Random.Shared.Next(1, 100),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return new OkObjectResult(counter);
    }

    [Function("CreateCounter")]
    public async Task<IActionResult> CreateCounter(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "counter")] HttpRequest req)
    {
        _logger.LogInformation("Creating new counter");

        // This is a sample HTTP trigger
        // In a real scenario, you would create the counter in Cosmos DB
        var counter = new CounterDocument
        {
            Id = Guid.NewGuid().ToString(),
            Value = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return new CreatedResult($"/counter/{counter.Id}", counter);
    }
}
