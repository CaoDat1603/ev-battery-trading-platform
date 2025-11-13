namespace Rating.Application.Abstractions
{
    public interface IInternalTokenService
    {
        Task<string> GetSystemTokenAsync(CancellationToken ct);
    }
}
