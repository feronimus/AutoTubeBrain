using Application.Contracts;
using Domain;
using Infrastructure;
using Infrastructure.Storage;
using Infrastructure.Tts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoTubeBrain;

public sealed class EpisodeTtsRequestedConsumer(
    ILogger<EpisodeTtsRequestedConsumer> log,
    YtbDbContext db,
    ITts tts,
    IStorage storage,
    IConfiguration cfg) : IConsumer<EpisodeTtsRequested>
{
    public async Task Consume(ConsumeContext<EpisodeTtsRequested> ctx)
    {
        var id = ctx.Message.EpisodeId;

        var ep = await db.Episodes.FirstOrDefaultAsync(x => x.Id == id, ctx.CancellationToken);
        if (ep is null)
        {
            log.LogWarning("Episode {EpisodeId} not found", id);
            return;
        }

        var dryRun =
            string.Equals(Environment.GetEnvironmentVariable("DRY_RUN"), "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(cfg["DryRun"], "true", StringComparison.OrdinalIgnoreCase);

        if (dryRun)
        {
            if (ep.Status == EpisodeStatus.Scripted)
            {
                ep.Status = EpisodeStatus.TtsReady;
                ep.UpdatedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ctx.CancellationToken);
                log.LogInformation("DryRun set TtsReady for {EpisodeId}", id);
            }
            return;
        }

        if (ep.Status != EpisodeStatus.Scripted)
        {
            log.LogInformation("Episode {EpisodeId} in state {Status}, TTS skipped", id, ep.Status);
            return;
        }

        var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var root = JsonSerializer.Deserialize<SceneJsonRoot>(ep.SceneJson!, jsonOpts);
        if (root is null || root.Scenes.Count == 0)
        {
            log.LogWarning("Episode {EpisodeId} has empty SceneJson", id);
            return;
        }

        var limitStr = cfg["TTS:MaxScenes"] ?? Environment.GetEnvironmentVariable("TTS__MaxScenes");
        var limit = int.TryParse(limitStr, out var m) ? Math.Max(m, 1) : int.MaxValue;

        var bucket = cfg["Storage:Bucket"] ?? "media";
        var basePath = $"episodes/{ep.Id}/audio";

        foreach (var sc in root.Scenes.OrderBy(s => s.Beat).Take(limit))
        {
            var key = $"{basePath}/{sc.Id}.mp3";
            if (await storage.ExistsAsync(bucket, key, ctx.CancellationToken))
                continue;

            var audio = await RetryAsync(
                () => tts.SpeakAsync(sc.Narration, ctx.CancellationToken),
                attempts: 3,
                delayMs: 1500,
                log,
                ctx.CancellationToken);

            using var ms = new MemoryStream(audio, writable: false);
            await storage.PutAsync(bucket, key, ms, "audio/mpeg", ctx.CancellationToken);
            log.LogInformation("Stored {Key}", key);
        }

        ep.Status = EpisodeStatus.TtsReady;
        ep.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ctx.CancellationToken);
        await ctx.Publish(new EpisodeImagesRequested(ep.Id), ctx.CancellationToken);

        log.LogInformation("TTS ready for {EpisodeId}", id);
    }

    static async Task<byte[]> RetryAsync(
        Func<Task<byte[]>> action,
        int attempts,
        int delayMs,
        ILogger log,
        CancellationToken ct)
    {
        for (var i = 1; ; i++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (i < attempts)
            {
                log.LogWarning(ex, "TTS attempt {Attempt} failed, retrying", i);
                await Task.Delay(delayMs, ct);
            }
        }
    }
}
