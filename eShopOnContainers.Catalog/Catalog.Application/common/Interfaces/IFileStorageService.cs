using Microsoft.AspNetCore.Http;

namespace Catalog.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the file path/URL
    /// </summary>
    Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file by its filename
    /// </summary>
    Task<bool> DeleteFileAsync(string fileName, string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a file is an allowed image type
    /// </summary>
    bool IsValidImageFile(IFormFile file);

    /// <summary>
    /// Gets the full URL for a file
    /// </summary>
    string GetFileUrl(string fileName, string folder);
}
