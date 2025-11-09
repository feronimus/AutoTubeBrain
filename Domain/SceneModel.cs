namespace Domain;

public sealed class SceneJsonRoot
{
    public string Title { get; set; } = null!;
    public string Language { get; set; } = "en";
    public string MusicMood { get; set; } = "neutral";
    public List<SceneItem> Scenes { get; set; } = [];
}
public sealed class SceneItem
{
    public string Id { get; set; } = null!;
    public string Narration { get; set; } = null!;
    public string VisualPrompt { get; set; } = null!;
    public int DurationSec { get; set; }
    public int Beat { get; set; }
    public List<string> Keywords { get; set; } = [];
}
