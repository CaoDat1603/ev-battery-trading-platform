namespace Rating.Application.Abstractions
{
    public interface IRateImageHandler
    {
        Task<string?> HandleImageAsync(IFormFile file, int rateId, CancellationToken ct = default);
        Task<IReadOnlyList<string>> HandleImagesAsync(IFormFileCollection? files, int rateId, CancellationToken ct = default);
    }
}
