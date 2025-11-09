using Application.Contracts;
using Domain;
using Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoTubeBrain;

public sealed class ImagesKickJob(ILogger<ImagesKickJob> log, YtbDbContext db, IPublishEndpoint bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var ep = await db.Episodes
            .Where(e => e.Status == EpisodeStatus.TtsReady)
            .OrderBy(e => e.UpdatedAt)
            .FirstOrDefaultAsync();

        if (ep is null) return;

        log.LogInformation("ImagesKickJob publishing EpisodeImagesRequested for {EpisodeId}", ep.Id);
        await bus.Publish(new Application.Contracts.EpisodeImagesRequested(ep.Id));
    }
}
