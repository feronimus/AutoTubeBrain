using Application.Contracts;
using Domain;
using Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace AutoTubeBrain;

public sealed class PlanKickJob(ILogger<PlanKickJob> log, YtbDbContext db, IPublishEndpoint bus) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var ep = await db.Episodes
            .Where(e => e.Status == EpisodeStatus.Planned)
            .OrderBy(e => e.CreatedAt)
            .FirstOrDefaultAsync();

        if (ep is null)
        {
            log.LogInformation("PlanKickJob found no Planned episodes");
            return;
        }

        log.LogInformation("PlanKickJob publishing EpisodePlanRequested for {EpisodeId}", ep.Id);
        await bus.Publish(new EpisodePlanRequested(ep.Id));
    }
}
