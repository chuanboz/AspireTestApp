using Aspire.Hosting;
using AspireTestApp.ServiceDefaults;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var useCosmosVNextEmulator = builder.Configuration
    .GetValue<bool>(AspireConstants.Switches.UseCosmosVNextEmulator);

var cosmos = builder.AddAzureCosmosDB(AspireConstants.Resources.CosmosDb);

if (useCosmosVNextEmulator)
{
#pragma warning disable ASPIRECOSMOSDB001
    cosmos.RunAsPreviewEmulator(emulator =>
     {
         // Run emulator over HTTP to avoid cert trust/setup for local dev.
         emulator.WithDataExplorer();
         //emulator.WithGatewayPort(8081);
         emulator.WithArgs("--protocol", "http");
         //emulator.WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "1");
     })
    .WithHttpEndpoint(name: "health-server", targetPort: 8080)
    .WithHttpHealthCheck(endpointName: "health-server", path: "/ready")
    ;
#pragma warning restore ASPIRECOSMOSDB001
}
else
{
    cosmos.RunAsEmulator(static container =>
    {
        container.WithLifetime(ContainerLifetime.Session);

        // Add volume persistence to speed up restarts
        container.WithDataVolume();

        // Optimize startup performance
        container.WithEnvironment("AZURE_COSMOS_EMULATOR_PARTITION_COUNT", "20")
                 .WithEnvironment("AZURE_COSMOS_EMULATOR_ENABLE_DATA_PERSISTENCE", "true");

        // Workaround to show Cosmos Data Explorer
        var endpoint = container.GetEndpoint("emulator");
        container
            .WithEnvironment("AZURE_COSMOS_EMULATOR_IP_ADDRESS_OVERRIDE", "127.0.0.1")
            .WithEnvironment("AZURE_COSMOS_EMULATOR_DISABLE_CERTIFICATE_AUTHENTICATION", "true")
            .WithHttpsEndpoint(port: endpoint.TargetPort, targetPort: endpoint.TargetPort, isProxied: false)
            .WithUrlForEndpoint("https", url =>
            {
                url.DisplayText = "Data Explorer";
                url.Url = "/_explorer/index.html";
            });
    });
}

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
    //.WithHttpHealthCheck(AspireConstants.HealthEndpoints.AdminHealth)
    .WithReference(cosmos)
    .WaitFor(cosmos);

builder.AddProject<Projects.AspireTestApp_Web>(AspireConstants.Resources.WebFrontend)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck(AspireConstants.HealthEndpoints.Health)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

