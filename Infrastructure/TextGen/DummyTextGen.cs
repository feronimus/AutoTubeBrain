using System.Text.Json;

namespace Infrastructure.TextGen;

public sealed class DummyTextGen : ITextGen
{
    public Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
    {
        var obj = new
        {
            Title = "Pilot",
            Language = "en",
            MusicMood = "dramatic",
            Scenes = Enumerable.Range(1, 5).Select(i => new
            {
                Id = $"s{i:00}",
                Narration = $"This is narration beat {i}.",
                VisualPrompt = "Epic, logo free, cinematic still.",
                DurationSec = 8 + i,
                Beat = i,
                Keywords = new[] { "cinematic", "clean" }
            })
        };

        return Task.FromResult(JsonSerializer.Serialize(obj));
    }
}
