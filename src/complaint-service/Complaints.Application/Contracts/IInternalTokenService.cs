namespace Complaints.Application.Contracts
{
    public interface IInternalTokenService
    {
        Task<string> GetSystemTokenAsync(CancellationToken ct);
    }
}
