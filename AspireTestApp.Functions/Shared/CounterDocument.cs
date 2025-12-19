namespace AspireTestApp.Shared;

public sealed record CounterDocument
{
    public required string Id { get; init; }

    // Using a fixed partition key keeps this sample simple.
    public string PartitionKey { get; init; } = "counter";

    public required int Value { get; init; }

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}
