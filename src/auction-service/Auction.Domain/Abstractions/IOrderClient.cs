using Auction.Domain.Entities;
using Auction.Domain.Enums;

namespace Auction.Application.Abstractions
{
    public interface IOrderClient
    {
        Task<bool> IsTransactionCompleted(int transactionId, decimal Amount, CancellationToken ct);
    }
}
