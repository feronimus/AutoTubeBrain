using Application.Contracts;
using Domain;
using Infrastructure;
using Infrastructure.ImageGen;
using Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoTubeBrain;

public sealed class EpisodeImagesRequestedConsumer(
    ILogger<EpisodeImagesRequestedConsumer> log,
    YtbDbContext db,
    IImageGen imageGen,
    IStorage storage,
    IConfiguration cfg) : IConsumer<EpisodeImagesRequested>
{
    public async Task Consume(ConsumeContext<EpisodeImagesRequested> ctx)
    {
        var id = ctx.Message.EpisodeId;
        var ep = await db.Episodes.FirstOrDefaultAsync(x => x.Id == id, ctx.CancellationToken);
        if (ep is null || ep.Status != EpisodeStatus.TtsReady) return;

        var dry = string.Equals(Environment.GetEnvironmentVariable("DRY_RUN_IMAGES"), "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(cfg["DryRunImages"], "true", StringComparison.OrdinalIgnoreCase);

        var root = JsonSerializer.Deserialize<SceneJsonRoot>(ep.SceneJson!)!;
        var limitStr = cfg["Images:MaxScenes"] ?? Environment.GetEnvironmentVariable("IMAGES__MaxScenes");
        var limit = int.TryParse(limitStr, out var m) ? Math.Max(m, 1) : int.MaxValue;

        var bucket = cfg["Storage:Bucket"] ?? "media";
        var basePath = $"episodes/{ep.Id}/images";

        var count = 0;
        foreach (var sc in root.Scenes.OrderBy(s => s.Beat).Take(limit))
        {
            var key = $"{basePath}/{sc.Id}.png";
            if (await storage.ExistsAsync(bucket, key, ctx.CancellationToken))
                continue;

            byte[] img = dry ? Array.Empty<byte>() : await imageGen.GenerateAsync(sc.VisualPrompt, ctx.CancellationToken);
            using var ms = new MemoryStream(img);
            await storage.PutAsync(bucket, key, ms, "image/png", ctx.CancellationToken);

            db.Assets.Add(new Asset
            {
                EpisodeId = ep.Id,
                Kind = AssetKind.Image,
                Path = $"{bucket}/{key}",
                Mime = "image/png",
                Bytes = img.LongLength
            });

            count++;
        }

        ep.Status = EpisodeStatus.VisualsReady;
        ep.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ctx.CancellationToken);

        log.LogInformation("Images ready for {EpisodeId} count={Count}", id, count);
    }
}
