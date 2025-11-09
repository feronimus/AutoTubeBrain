using System;

namespace Infrastructure.ImageGen;

// 1x1 PNG pixel, transparent
public sealed class DummyImageGen : IImageGen
{
    private static readonly byte[] _png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMB/ay1n6QAAAAASUVORK5CYII=");
    public Task<byte[]> GenerateAsync(string prompt, CancellationToken ct = default)
        => Task.FromResult(_png);
}
