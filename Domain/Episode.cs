namespace Domain;

public enum EpisodeStatus
{
    Planned, Scripted, TtsReady, VisualsReady, Composed, Uploaded, Public, Failed
}

public sealed class Episode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ChannelKey { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public EpisodeStatus Status { get; set; } = EpisodeStatus.Planned;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? SceneJson { get; set; }
    public string? VideoPath { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
