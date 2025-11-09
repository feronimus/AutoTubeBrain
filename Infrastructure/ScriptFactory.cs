using Application;
using Domain;
using Infrastructure.TextGen;

namespace Infrastructure;

public sealed class ScriptFactory(ITextGen textGen) : IScriptFactory
{
    public async Task<ScriptResult> CreateAsync(Episode episode, CancellationToken ct = default)
    {
        var prompt = $"Return strictly valid JSON for a SceneJson titled '{episode.Title}'.";
        var json = await textGen.CompleteAsync(prompt, ct);
        return new ScriptResult(json, episode.Title);
    }
}
