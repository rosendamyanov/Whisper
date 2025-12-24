using Microsoft.AspNetCore.Hosting;
using Whisper.DTOs.Internal;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class LocalFileStorageService : ILocalFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string UploadsFolder = "uploads";

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<FileSaveResult> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, long length)
        {
            var category = contentType.StartsWith("image") ? "images" : "files";

            var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var folderPath = Path.Combine(webRootPath, UploadsFolder, category);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            var fullFilePath = Path.Combine(folderPath, safeFileName);

            using (var targetStream = new FileStream(fullFilePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(targetStream);
            }

            return new FileSaveResult
            {
                Url = $"/{UploadsFolder}/{category}/{safeFileName}",
                FileName = originalFileName,
                FileType = contentType,
                FileSize = length
            };

        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return Task.CompletedTask;

            var relativePath = fileUrl.TrimStart('/', '\\');

            // Result: C:\...\wwwroot\uploads\images\abc.png
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }
    }
}
