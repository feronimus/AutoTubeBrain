namespace Infrastructure.Tts;

public interface ITts
{
    Task<byte[]> SpeakAsync(string text, CancellationToken ct = default);
}
