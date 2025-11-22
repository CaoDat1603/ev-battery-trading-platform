using Auction.Application.DTOs;
using Auction.Domain.Enums;

namespace Auction.Application.Contracts
{
    public interface IBidCommand
    {
        Task<int> PlaceBidAsync(PlaceBidDto dto, CancellationToken ct = default);
        Task<bool> UpdateBidStatusAsync(int bidId, DepositStatus newStatus, CancellationToken ct = default);
        Task<bool> UpdateContactInfoAsync(int bidId, string? newEmail, string? newPhone, CancellationToken ct = default);
        Task<bool> UpdateWinningBidAsync(int bidId, bool isWinning, CancellationToken ct = default);
        Task<bool> UpdateBidAmountAsync(int bidId, decimal newAmount, CancellationToken ct = default);
        Task<bool> UpdateTransactionAsync(int bidId, int transactionId, CancellationToken ct = default);
        Task<bool> DeleteBidAsync(int bidId, CancellationToken ct = default);
    }
}
