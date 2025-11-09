using Application.Contracts;
using Domain;
using Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoTubeBrain;

public sealed class TtsKickJob(ILogger<TtsKickJob> log, YtbDbContext db, IPublishEndpoint bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var ep = await db.Episodes
            .Where(e => e.Status == EpisodeStatus.Scripted)
            .OrderBy(e => e.UpdatedAt)
            .FirstOrDefaultAsync();

        if (ep is null) return;

        log.LogInformation("TtsKickJob publishing EpisodeTtsRequested for {EpisodeId}", ep.Id);
        await bus.Publish(new EpisodeTtsRequested(ep.Id));
    }
}
