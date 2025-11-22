using Catalog.Domain.Entities;
using Catalog.Domain.Enums;

namespace Catalog.Application.Abstractions
{
    public interface IOrderClient
    {
        Task<bool> IsTransactionCompleted(int transactionId, Product product, ProductStatus productType, CancellationToken ct);
    }
}
