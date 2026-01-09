namespace AspireTestApp.Models;

public sealed record WeatherForecast
{
    public required string Id { get; init; }
    
    public string PartitionKey { get; init; } = "weather";
    
    public required DateOnly Date { get; init; }
    
    public required int TemperatureC { get; init; }
    
    public required string? Summary { get; init; }
    
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
