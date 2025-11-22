namespace Catalog.Application.Abstractions
{
    public interface IIdentityClient
    {
        Task<bool> UserExistsAsync(int userId, CancellationToken ct);
    }
}
