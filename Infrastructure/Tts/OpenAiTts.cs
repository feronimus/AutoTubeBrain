using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Tts;

public sealed class OpenAiTts(HttpClient http, IConfiguration cfg) : ITts
{
    public async Task<byte[]> SpeakAsync(string text, CancellationToken ct = default)
    {
        var apiKey = cfg["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OpenAI__ApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI ApiKey missing");

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(new
        {
            model = "tts-1",
            voice = "alloy",
            input = text,
            format = "mp3"
        }), Encoding.UTF8, "application/json");

        using var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsByteArrayAsync(ct);
    }
}
