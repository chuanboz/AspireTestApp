using Aspire.Hosting;
using AspireTestApp.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB(AspireConstants.Resources.CosmosDb)
    .RunAsEmulator();
var database = cosmos.AddCosmosDatabase(
    AspireConstants.Resources.CosmosDatabase, 
    AspireConstants.CosmosDb.DatabaseName);
database.AddContainer(
    AspireConstants.Resources.CosmosCountersContainer, 
    partitionKeyPath: AspireConstants.CosmosDb.CounterPartitionKeyPath, 
    containerName: AspireConstants.CosmosDb.CounterContainerName);
database.AddContainer(
    AspireConstants.Resources.CosmosLeasesContainer, 
    partitionKeyPath: AspireConstants.CosmosDb.LeasePartitionKeyPath, 
    containerName: AspireConstants.CosmosDb.LeaseContainerName);

var apiService = builder.AddProject<Projects.AspireTestApp_ApiService>(AspireConstants.Resources.ApiService)
    .WithHttpHealthCheck(AspireConstants.HealthEndpoints.Health)
    .WithReference(cosmos);

builder.AddProject<Projects.AspireTestApp_Functions>(AspireConstants.Resources.CounterFunction)
    .WithHttpEndpoint(port: AspireConstants.Functions.DefaultHttpPort, name: AspireConstants.Functions.HttpEndpointName)
    .WithHttpHealthCheck(AspireConstants.HealthEndpoints.AdminHealth)
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.AddProject<Projects.AspireTestApp_Web>(AspireConstants.Resources.WebFrontend)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck(AspireConstants.HealthEndpoints.Health)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
