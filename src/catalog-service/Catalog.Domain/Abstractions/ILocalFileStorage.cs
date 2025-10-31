namespace Catalog.Domain.Abstractions
{
    public interface ILocalFileStorage
    {
        Task<string?> SaveFileAsync(string folder, string fileName, Stream fileStream, CancellationToken cancellationToken);
    }
}
