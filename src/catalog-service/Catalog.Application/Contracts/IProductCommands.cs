using Catalog.Application.DTOs;
using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts
{
    public interface IProductCommands
    {
        Task<int> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<bool> UpdateStatusAsync(int productId, ProductStatus newStatus, CancellationToken ct = default);
        Task<bool> UpdateSaleMethodAsync(int productId, SaleMethod newMethod, CancellationToken ct = default);
        Task<bool> MarkAsVerifiedAsync(int productId, CancellationToken ct = default);
        Task<bool> UnmarkAsVerifiedAsync(int productId, CancellationToken ct = default);
        Task<bool> MarkAsSpamAsync(int productId, CancellationToken ct = default);
        Task<bool> UnmarkAsSpamAsync(int productId, CancellationToken ct = default);
        Task<bool> DeleteProductAsync(int productId, CancellationToken ct = default);
    }
}
