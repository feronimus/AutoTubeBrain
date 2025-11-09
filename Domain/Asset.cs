namespace Domain;

public enum AssetKind { Audio = 0, Image = 1, Video = 2 }

public sealed class Asset
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EpisodeId { get; set; }
    public AssetKind Kind { get; set; }
    public string Path { get; set; } = null!;
    public string Mime { get; set; } = "application/octet-stream";
    public long Bytes { get; set; }
    public double? DurationSec { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
