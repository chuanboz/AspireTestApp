using AspireTestApp.Models;
using AspireTestApp.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace AspireTestApp.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController(CosmosClient cosmosClient, ILogger<WeatherController> logger) : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecast(
        [FromQuery] int days = 5,
        CancellationToken cancellationToken = default)
    {
        var container = cosmosClient
            .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
            .GetContainer(AspireConstants.CosmosDb.WeatherContainerName);

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.PartitionKey = @partitionKey ORDER BY c.Date DESC OFFSET 0 LIMIT @limit")
            .WithParameter("@partitionKey", "weather")
            .WithParameter("@limit", days);

        var forecasts = new List<WeatherForecast>();
        using var iterator = container.GetItemQueryIterator<WeatherForecast>(query);

        while (iterator.HasMoreResults && forecasts.Count < days)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            forecasts.AddRange(response);
        }

        if (forecasts.Count == 0)
        {
            logger.LogInformation("No weather forecasts found in database, generating new ones");
            forecasts = await GenerateAndSaveForecasts(container, days, cancellationToken);
        }

        return Ok(forecasts.Take(days));
    }

    [HttpPost("generate")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GenerateWeatherForecast(
        [FromQuery] int days = 5,
        CancellationToken cancellationToken = default)
    {
        var container = cosmosClient
            .GetDatabase(AspireConstants.CosmosDb.DatabaseName)
            .GetContainer(AspireConstants.CosmosDb.WeatherContainerName);

        var forecasts = await GenerateAndSaveForecasts(container, days, cancellationToken);
        return Ok(forecasts);
    }

    private async Task<List<WeatherForecast>> GenerateAndSaveForecasts(
        Container container,
        int days,
        CancellationToken cancellationToken)
    {
        var forecasts = new List<WeatherForecast>();

        for (int i = 1; i <= days; i++)
        {
            var forecast = new WeatherForecast
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(i)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };

            await container.UpsertItemAsync(
                forecast,
                new PartitionKey(forecast.PartitionKey),
                cancellationToken: cancellationToken);

            forecasts.Add(forecast);
            logger.LogInformation("Saved weather forecast: {Date} - {Temp}°C - {Summary}",
                forecast.Date, forecast.TemperatureC, forecast.Summary);
        }

        return forecasts;
    }
}
