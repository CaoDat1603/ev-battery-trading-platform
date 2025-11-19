using Complaints.Application.DTOs;

namespace Complaints.Application.Contracts
{
    public interface ICatalogClient
    {
        Task<ProductInfoDto> GetProductInfoAsync(int productId, CancellationToken ct = default);
    }
}
