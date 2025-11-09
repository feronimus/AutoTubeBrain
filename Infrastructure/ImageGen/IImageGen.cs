namespace Infrastructure.ImageGen;

public interface IImageGen
{
    Task<byte[]> GenerateAsync(string prompt, CancellationToken ct = default);
}
