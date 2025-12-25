using Whisper.DTOs.Internal;

namespace Whisper.Services.Services.Interfaces
{
    public interface ILocalFileStorageService
    {
        Task<FileSaveResult> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, long length);
        Task DeleteFileAsync(string fileUrl);
    }
}
