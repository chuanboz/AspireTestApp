using Newtonsoft.Json;

namespace AspireTestApp.Models;

public sealed record CounterDocument
{
    [JsonProperty("id")]
    public required string Id { get; init; }

    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("value")]
    public required int Value { get; init; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
