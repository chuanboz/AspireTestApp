using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add health checks
builder.Services.AddHealthChecks();

// Add startup delay to allow Cosmos DB emulator to fully initialize
var startupDelayStr = builder.Configuration["COSMOS_DB_STARTUP_DELAY"];
if (int.TryParse(startupDelayStr, out var startupDelay) && startupDelay > 0 && builder.Environment.IsDevelopment())
{
    Console.WriteLine($"Waiting {startupDelay}ms for Cosmos DB to initialize...");
    await Task.Delay(startupDelay);
    Console.WriteLine("Cosmos DB startup delay completed. Starting Functions host...");
}

// Configure CosmosDB client to trust the emulator certificate in development
if (builder.Environment.IsDevelopment())
{
    /*
    builder.Services.AddOptions<Microsoft.Azure.Cosmos.CosmosClientOptions>()
        .Configure(options =>
        {
            // Bypass SSL validation for Cosmos DB emulator
            options.HttpClientFactory = () =>
            {
                return new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            };
        });
    */
}

builder.Build().Run();
