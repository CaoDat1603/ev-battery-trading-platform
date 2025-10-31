using Catalog.Domain.Abstractions;

namespace Catalog.Infrastructure.Services
{
    /// <summary>
    /// Handles the saving of product-related document files (e.g., PDFs)
    /// using a local file storage provider.
    /// </summary>
    public class ProductFileHandler : IProductFileHandler
    {
        private readonly ILocalFileStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFileHandler"/> class.
        /// </summary>
        /// <param name="storage">The local file storage service used to save files.</param>
        public ProductFileHandler(ILocalFileStorage storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Saves a PDF document associated with a product detail.
        /// </summary>
        /// <param name="file">The uploaded file to be saved.</param>
        /// <param name="detailId">The identifier of the related product detail.</param>
        /// <param name="ct">The cancellation token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A relative URL to the saved PDF file (e.g., <c>/uploads/product-details/{detailId}/document_{detailId}.pdf</c>).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the provided file is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the file is not in PDF format.
        /// </exception>
        /// <exception cref="IOException">
        /// Thrown when a file system error occurs during the save operation.
        /// </exception>
        public async Task<string> SaveDocumentAsync(IFormFile file, int detailId, CancellationToken ct = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid document file.", nameof(file));

            var ext = Path.GetExtension(file.FileName);
            if (!string.Equals(ext, ".pdf", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF files are supported.");

            var fileName = $"document_{detailId}.pdf";
            var folderPath = $"uploads/product-details/{detailId}";

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _storage.SaveFileAsync(folderPath, fileName, stream, ct);

                if (string.IsNullOrEmpty(url))
                    throw new IOException("Failed to save PDF file: storage returned null or empty path.");

                return url;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"Access denied when writing to directory '{folderPath}'.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException($"I/O error occurred while saving PDF file to '{folderPath}'.", ex);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new IOException($"Unexpected error occurred while saving PDF file: {ex.Message}", ex);
            }
        }
    }
}
