using Rating.Domain.Abstractions;
using System.Reflection.Metadata.Ecma335;

namespace Rating.Infrastructure.Services
{
    public class RateImageHandler : IRateImageHandler
    {
        private ILocalFileStorage _localStorage;
        public RateImageHandler (ILocalFileStorage localStorage) => _localStorage = localStorage;
        private static readonly HashSet<string> _allowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        public async Task<string?> HandleImageAsync(IFormFile file, int rateId, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !_allowedExt.Contains(ext)) throw new InvalidOperationException($"Unsupported format {ext}");

            var safeName = $"rate_{rateId}_{Guid.NewGuid():N}{ext}";
            var folderPath = $"uploads/rates/{rateId}";

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _localStorage.SaveFileAsync(folderPath, safeName, stream, ct);
                return url;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"Unable to write file to directory '{folderPath}'.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException("I/O error while saving review image", ex);
            }
            catch (Exception ex)
            {
                throw new IOException("Unknown error while saving review image.", ex);
            }
        }

        public async Task<IReadOnlyList<string>> HandleImagesAsync(IFormFileCollection? files, int rateId, CancellationToken ct = default)
        {
            if (files == null || files.Count == 0) return Array.Empty<string>();

            var urls = new List<string>(files.Count);
            foreach (var f in files)
            {
                // bỏ qua file rỗng thay vì ném lỗi cả batch
                if (f == null || f.Length == 0) continue;

                var url = await HandleImageAsync(f, rateId, ct);
                if (!string.IsNullOrEmpty(url))
                    urls.Add(url!);
            }
            return urls;
        }
    }
}
