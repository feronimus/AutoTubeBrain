using Application;
using Application.Contracts;
using Domain;
using Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AutoTubeBrain;

public sealed class EpisodePlanRequestedConsumer : IConsumer<EpisodePlanRequested>
{
    private readonly ILogger<EpisodePlanRequestedConsumer> _log;
    private readonly YtbDbContext _db;
    private readonly IScriptFactory _scripts;

    public EpisodePlanRequestedConsumer(
        ILogger<EpisodePlanRequestedConsumer> log,
        YtbDbContext db,
        IScriptFactory scripts)
    {
        _log = log;
        _db = db;
        _scripts = scripts;
    }

    public async Task Consume(ConsumeContext<EpisodePlanRequested> ctx)
    {
        var id = ctx.Message.EpisodeId;
        var ep = await _db.Episodes.FirstOrDefaultAsync(x => x.Id == id, ctx.CancellationToken);
        if (ep is null)
        {
            _log.LogWarning("Episode {EpisodeId} not found", id);
            return;
        }

        if (ep.Status != EpisodeStatus.Planned)
        {
            _log.LogInformation("Episode {EpisodeId} in state {Status}, skipping", id, ep.Status);
            return;
        }

        _log.LogInformation("Generating SceneJson for {EpisodeId}", id);
        var result = await _scripts.CreateAsync(ep, ctx.CancellationToken);

        ep.SceneJson = result.Json;
        ep.Status = EpisodeStatus.Scripted;
        ep.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ctx.CancellationToken);
        await ctx.Publish(new EpisodeTtsRequested(ep.Id), ctx.CancellationToken);

        _log.LogInformation("Scripted {EpisodeId}. Length={Len}", id, result.Json.Length);
    }
}
