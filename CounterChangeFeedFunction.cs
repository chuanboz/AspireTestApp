using AspireTestApp.ServiceDefaults;
using AspireTestApp.Shared;
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
            Connection = AspireConstants.CosmosDb.ConnectionStringKey,
            LeaseContainerName = AspireConstants.CosmosDb.LeaseContainerName,
            CreateLeaseContainerIfNotExists = true)]
        IReadOnlyList<CounterDocument> input,
        FunctionContext context)
    {
        if (input is null || input.Count == 0)
        {
            return;
        }

        foreach (var doc in input)
        {
            logger.LogInformation("Counter changed: id={Id} value={Value} updatedAt={UpdatedAt}", doc.Id, doc.Value, doc.UpdatedAt);
        }
    }
}
