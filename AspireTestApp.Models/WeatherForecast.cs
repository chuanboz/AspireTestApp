using Newtonsoft.Json;

namespace AspireTestApp.Models;

public sealed record WeatherForecast
{
    [JsonProperty("id")]
    public required string Id { get; init; }
    
    [JsonProperty("partitionKey")]
    public string PartitionKey { get; init; } = "weather";
    
    [JsonProperty("date")]
    public required DateOnly Date { get; init; }
    
    [JsonProperty("temperatureC")]
    public required int TemperatureC { get; init; }
    
    [JsonProperty("summary")]
    public required string? Summary { get; init; }
    
    [JsonProperty("temperatureF")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    
    [JsonProperty("createdAt")]
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
