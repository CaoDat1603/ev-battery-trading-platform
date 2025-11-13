using Complaints.Domain.Abtractions;

namespace Complaints.Infrastructure.Services
{
    public class LocalFileStorage : ILocalFileStorage
    {
        private readonly IWebHostEnvironment _env;
        public LocalFileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> SaveFileAsync(string folder, string fileName, Stream fileStream, CancellationToken cancellationToken)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream), "Luồng dữ liệu file không được null.");
            var userFolder = Path.Combine(_env.WebRootPath, folder);
            try
            {
                if (!Directory.Exists(userFolder))
                    Directory.CreateDirectory(userFolder);

                var filePath = Path.Combine(userFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }

                return $"/{folder}/{fileName}";
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"Không có quyền ghi file tại '{userFolder}'.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException($"Lỗi I/O khi lưu file tại '{userFolder}'.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Lỗi không xác định khi lưu file tại '{userFolder}'.", ex);
            }
        }
    }
}
