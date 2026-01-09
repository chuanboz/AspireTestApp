using AspireTestApp.Models;
using AspireTestApp.ServiceDefaults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AspireTestApp.Functions;

public sealed class CounterChangeFeedFunction(ILogger<CounterChangeFeedFunction> logger)
{
    [Function(nameof(CounterChangeFeedFunction))]
    public void Run(
        [CosmosDBTrigger(
            databaseName: AspireConstants.CosmosDb.DatabaseName,
            containerName: AspireConstants.CosmosDb.CounterContainerName,
            Connection = AspireConstants.CosmosDb.ConnectionStringKey)]
        IReadOnlyList<CounterDocument> input,
        FunctionContext context)
    {
        if (input is null || input.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} counter changes", input.Count);

        foreach (var doc in input)
        {
            logger.LogInformation("Counter changed: name={Name} id={Id} value={Value} updatedAt={UpdatedAt}", 
                doc.Name, doc.Id, doc.Value, doc.UpdatedAt);
        }
    }
}
