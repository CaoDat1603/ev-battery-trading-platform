using Auction.Domain.Entities;

namespace Auction.Domain.Abstractions
{
    public interface IAuctionRepository
    {
        Task<AuctionItem?> GetByIdAsync(int auctionId, CancellationToken ct = default);
        Task<IReadOnlyList<AuctionItem>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<AuctionItem>> SearchByProductAsync(int productId, CancellationToken ct = default);
        Task AddAsync(AuctionItem entity, CancellationToken ct = default);
        void Update(AuctionItem auction);
    }
}