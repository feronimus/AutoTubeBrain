using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Storage;

public sealed class MinioStorage(IMinioClient client) : IStorage
{
    public async Task PutAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct = default)
    {
        var be = new BucketExistsArgs().WithBucket(bucket);
        if (!await client.BucketExistsAsync(be, ct))
            await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);

        var put = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);
        await client.PutObjectAsync(put, ct);
    }

    public async Task<bool> ExistsAsync(string bucket, string objectName, CancellationToken ct = default)
    {
        try
        {
            var stat = new StatObjectArgs().WithBucket(bucket).WithObject(objectName);
            await client.StatObjectAsync(stat, ct);
            return true;
        }
        catch { return false; }
    }
}
