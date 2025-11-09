namespace Infrastructure.TextGen;

public interface ITextGen
{
    Task<string> CompleteAsync(string prompt, CancellationToken ct = default);
}
