using Rating.Domain.Abstractions;

namespace Rating.Infrastructure.Services
{
    public class LocalFileStorage : ILocalFileStorage
    {
        private IWebHostEnvironment _env;
        public LocalFileStorage (IWebHostEnvironment env) => _env = env;
        public async Task<string?> SaveFileAsync(string folder, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
        {
            if(fileStream == null) throw new ArgumentNullException(nameof(fileStream));
            var objectFolder = Path.Combine(_env.WebRootPath, folder);
            try
            {
                if(!Directory.Exists(objectFolder)) Directory.CreateDirectory(objectFolder);
                var filePath = Path.Combine(objectFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream, cancellationToken);
                }

                return $"/{folder}/{fileName}";
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"Không có quyền ghi file tại '{objectFolder}'.", ex);
            }
            catch (IOException ex)
            {
                throw new IOException($"Lỗi I/O khi lưu file tại '{objectFolder}'.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Lỗi không xác định khi lưu file tại '{objectFolder}'.", ex);
            }
        }
    }
}
