using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Storage;

public interface IStorage
{
    Task PutAsync(string bucket, string objectName, Stream data, string contentType, CancellationToken ct = default);
    Task<bool> ExistsAsync(string bucket, string objectName, CancellationToken ct = default);
}
