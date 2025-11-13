using Auction.Application.DTOs;
using Auction.Domain.Enums;

namespace Auction.Application.Contracts
{
    public interface IAuctionCommand
    {
        Task<int> CreateAuctionAsync(CreateAuctionDto dto, CancellationToken ct = default);
        Task<bool> UpdateAuctionStatusAsync(int auctionId, AuctionStatus newStatus, CancellationToken ct = default);
        Task<bool> CompleteAuctionAsync(int auctionId, int transactionId, CancellationToken ct = default);
        Task<bool> UpdateCurrentPrice(int auctionId, decimal newPrice, CancellationToken ct = default);
        Task<bool> UpdateSellerContact(int auctionId, string? newEmail, string? newPhone, CancellationToken ct = default);
        Task<bool> DeleteAuctionAsync(int auctionId, CancellationToken ct = default);
    }
}
