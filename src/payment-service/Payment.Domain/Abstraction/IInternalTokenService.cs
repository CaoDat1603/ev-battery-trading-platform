namespace Payment.Domain.Abstraction
{
    public interface IInternalTokenService
    {
        Task<string> GetSystemTokenAsync(CancellationToken ct);
    }
}
