namespace Application;

using Domain;

public interface IScriptFactory
{
    Task<ScriptResult> CreateAsync(Episode episode, CancellationToken ct = default);
}

public sealed record ScriptResult(string Json, string Title);
