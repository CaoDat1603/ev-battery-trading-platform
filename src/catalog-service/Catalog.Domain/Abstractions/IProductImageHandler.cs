namespace Catalog.Domain.Abstractions
{
    public interface IProductImageHandler
    {
        Task<string> SaveImageAsync(IFormFile image, int productId, CancellationToken ct = default);
    }
}
