using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

const string databaseName = "AspireTestApp";
const string counterContainerName = "counters";
const string leaseContainerName = "leases";

var cosmos = builder.AddAzureCosmosDB("cosmosdb");
var database = cosmos.AddCosmosDatabase("cosmosdb-database", databaseName);
database.AddContainer("cosmosdb-counters", partitionKeyPath: "/PartitionKey", containerName: counterContainerName);
database.AddContainer("cosmosdb-leases", partitionKeyPath: "/id", containerName: leaseContainerName);

var apiService = builder.AddProject<Projects.AspireTestApp_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(cosmos)
    .WithEnvironment("CosmosDb__DatabaseName", databaseName)
    .WithEnvironment("CosmosDb__ContainerName", counterContainerName);

builder.AddProject<Projects.AspireTestApp_Functions>("counterprocessor")
    .WithReference(cosmos)
    .WithEnvironment("CosmosDb__DatabaseName", databaseName)
    .WithEnvironment("CosmosDb__ContainerName", counterContainerName)
    .WithEnvironment("CosmosDb__LeaseContainerName", leaseContainerName)
    .WaitFor(cosmos);

builder.AddProject<Projects.AspireTestApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
