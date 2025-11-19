using Rating.Application.DTOs;

namespace Rating.Application.Abstractions
{
    public interface ICatalogClient
    {
        Task<ProductInfoDto> GetProductInfoAsync(int productId, CancellationToken ct = default);
    }
}
