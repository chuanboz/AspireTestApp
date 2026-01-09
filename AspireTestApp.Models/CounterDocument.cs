namespace AspireTestApp.Models;

public sealed record CounterDocument
{
    public required string Id { get; init; }

    public string PartitionKey { get; init; } = "counter";

    public required int Value { get; init; }

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
