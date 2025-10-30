using Catalog.Application.DTOs;
using Catalog.Domain.Enums;

namespace Catalog.Application.Contracts
{
    public interface IProductCommands
    {
        Task<int> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
        Task<bool> UpdateStatusAsync(int productId, ProductStatus newStatus, CancellationToken ct = default);
    }
}
