using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add health checks
builder.Services.AddHealthChecks();

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
