namespace Catalog.Domain.Abstractions
{
    public interface IProductFileHandler
    {
        Task<string> SaveDocumentAsync(IFormFile file, int detailId, CancellationToken ct = default);
    }
}
