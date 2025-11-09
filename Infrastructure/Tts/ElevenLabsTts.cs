using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Tts;

public sealed class ElevenLabsTts(HttpClient http, IConfiguration cfg) : ITts
{
    public async Task<byte[]> SpeakAsync(string text, CancellationToken ct = default)
    {
        var apiKey = cfg["ElevenLabs:ApiKey"] ?? Environment.GetEnvironmentVariable("ElevenLabs__ApiKey");
        var voiceId = cfg["ElevenLabs:VoiceId"] ?? Environment.GetEnvironmentVariable("ElevenLabs__VoiceId");
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(voiceId))
            throw new InvalidOperationException("ElevenLabs ApiKey or VoiceId missing");

        var url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("xi-api-key", apiKey);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));

        var body = new
        {
            text,
            model_id = "eleven_turbo_v2_5",
            voice_settings = new { stability = 0.45, similarity_boost = 0.75, style = 0.3, use_speaker_boost = true }
        };
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsByteArrayAsync(ct);
    }
}
