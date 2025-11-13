using Rating.Application.DTOs;
using System.Runtime.InteropServices;

namespace Rating.Application.Contracts
{
    public interface IRateCommands
    {
        Task<RateResponse> CreateAsync(CreateRateRequest request, CancellationToken ct = default);
        Task<RateResponse> CreateUserAsync(int userId, CreateRateRequest request, CancellationToken ct = default);
        Task<RateResponse> CreateProductAsync(int productId, CreateRateRequest request, CancellationToken ct = default);

        Task UpdateAsync(int rateId, UpdateRateRequest request, CancellationToken ct = default);
        Task DeleteAsync(int rateId, CancellationToken ct = default);

        Task<IReadOnlyList<RateImageDto>> AddImagesAsync(int rateId, IFormFileCollection files, CancellationToken ct = default);
        Task ReplaceImagesAsync(int rateId, IFormFileCollection files, CancellationToken ct = default);
        Task UpdateImageUrlAsync(int rateImageId, string newUrl, CancellationToken ct = default);
        Task RemoveImageAsync(int rateImageId, CancellationToken ct = default);
    }
}
