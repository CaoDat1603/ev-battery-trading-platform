using Complaints.Application.DTOs;

namespace Complaints.Application.Contracts
{
    public interface IOrderClient
    {
        Task<TransactionInfoDto> GetTransactionInfoAsync(int tranId, CancellationToken ct = default);
    }
}
