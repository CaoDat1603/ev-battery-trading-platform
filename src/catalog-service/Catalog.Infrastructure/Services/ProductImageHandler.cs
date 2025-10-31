using Catalog.Domain.Abstractions;

namespace Catalog.Infrastructure.Services
{
    /// <summary>
    /// Provides functionality to handle saving and validating product images.
    /// </summary>
    public class ProductImageHandler : IProductImageHandler
    {
        private readonly ILocalFileStorage _storage;

        /// <summary>
        /// Set of allowed image file extensions for validation.
        /// </summary>
        private static readonly HashSet<string> _allowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductImageHandler"/> class.
        /// </summary>
        /// <param name="storage">Local file storage service used to save image files.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided storage is null.</exception>
        public ProductImageHandler(ILocalFileStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Saves a product image to the local file storage.
        /// </summary>
        /// <param name="image">The uploaded image file to save.</param>
        /// <param name="productId">The unique ID of the product to associate the image with.</param>
        /// <param name="ct">Optional cancellation token for the operation.</param>
        /// <returns>
        /// A relative URL pointing to the saved image file.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when the provided image is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the image file format is not supported.</exception>
        /// <exception cref="IOException">Thrown when the image cannot be saved due to I/O or permission errors.</exception>
        public async Task<string> SaveImageAsync(IFormFile image, int productId, CancellationToken ct = default)
        {
            // Validate file input
            if (image == null || image.Length == 0)
                throw new ArgumentException("Invalid product image.", nameof(image));

            // Extract and validate file extension
            var ext = Path.GetExtension(image.FileName);
            if (string.IsNullOrEmpty(ext) || !_allowedExt.Contains(ext))
                throw new InvalidOperationException($"File format '{ext}' is not supported.");

            // Build destination path and filename
            var fileName = $"product_{productId}{ext}";
            var folderPath = $"uploads/products/{productId}";

            // Save the file to local storage
            await using var stream = image.OpenReadStream();
            var url = await _storage.SaveFileAsync(folderPath, fileName, stream, ct);

            // Validate storage result
            if (string.IsNullOrEmpty(url))
                throw new IOException("Failed to save image — storage returned null.");

            return url;
        }
    }
}
