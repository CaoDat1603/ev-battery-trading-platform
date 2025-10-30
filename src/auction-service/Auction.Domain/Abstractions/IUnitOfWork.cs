namespace Auction.Domain.Abstractions
{
    public interface IUnitOfWork
    {
           Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
