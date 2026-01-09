using Newtonsoft.Json;

namespace AspireTestApp.Models;

public sealed record CounterDocument
{
    [JsonProperty("id")]
    public required string Id { get; init; }

    [JsonProperty("partitionKey")]
    public string PartitionKey { get; init; } = "counter";

    [JsonProperty("value")]
    public required int Value { get; init; }

    [JsonProperty("updatedAt")]
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
