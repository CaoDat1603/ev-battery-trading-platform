using Catalog.Domain.Abstractions;

namespace Catalog.Infrastructure.Services
{
    /// <summary>
    /// Provides local file storage functionality for saving uploaded files into the server's file system.
    /// </summary>
    public class LocalFileStorage : ILocalFileStorage
    {
        private readonly string _basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileStorage"/> class.
        /// </summary>
        /// <param name="env">The web hosting environment that provides access to the web root path.</param>
        public LocalFileStorage(IWebHostEnvironment env)
        {
            // If WebRootPath is null (e.g., wwwroot folder not created yet),
            // fallback to the current working directory and create "wwwroot" if needed.
            _basePath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            // Ensure the "uploads" folder exists
            var uploads = Path.Combine(_basePath, "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);
        }

        /// <summary>
        /// Saves a file to a specified folder under the "uploads" directory.
        /// </summary>
        /// <param name="folder">The folder name inside the uploads directory.</param>
        /// <param name="fileName">The name of the file to be saved.</param>
        /// <param name="fileStream">The input stream containing file data.</param>
        /// <param name="cancellationToken">A cancellation token for async operation.</param>
        /// <returns>
        /// A relative URL path (e.g., <c>/uploads/{folder}/{fileName}</c>) that can be stored in the database or returned to the client.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileStream"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="folder"/> is null or whitespace.</exception>
        /// <exception cref="IOException">Thrown when the file cannot be written due to I/O or permission issues.</exception>
        public async Task<string?> SaveFileAsync(string folder, string fileName, Stream fileStream, CancellationToken cancellationToken)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream), "File stream cannot be null.");

            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException("Folder name cannot be empty.", nameof(folder));

            // Build the full directory path for the specific user or category
            var userFolder = Path.Combine(_basePath, "uploads", folder);

            try
            {
                // Ensure the target directory exists
                if (!Directory.Exists(userFolder))
                    Directory.CreateDirectory(userFolder);

                var filePath = Path.Combine(userFolder, fileName);

                // Copy file data asynchronously to the target file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }

                // Return the relative path to the uploaded file
                return $"/uploads/{folder}/{fileName}";
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"No write permission at '{userFolder}'.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException($"I/O error occurred while saving file at '{userFolder}'.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Unexpected error occurred while saving file at '{userFolder}'.", ex);
            }
        }
    }
}
