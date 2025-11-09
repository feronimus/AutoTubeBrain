namespace Infrastructure;

public sealed class OutboxMessage
{
    public long Id { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Dispatched { get; set; }
}
