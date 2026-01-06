using Catalog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostingEnvironment _environment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public LocalFileStorageService(
        IHostingEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null");
        }

        if (!IsValidImageFile(file))
        {
            throw new ArgumentException("Invalid file type. Only images are allowed.");
        }

        if (file.Length > MaxFileSize)
        {
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB");
        }

        try
        {
            // Create unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

            // Create folder path
            var folderPath = Path.Combine(_environment.WebRootPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Created directory: {FolderPath}", folderPath);
            }

            // Full file path
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);

            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, string folder, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, folder, fileName);

            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath), cancellationToken);
                _logger.LogInformation("File deleted successfully: {FileName}", fileName);
                return true;
            }

            _logger.LogWarning("File not found for deletion: {FileName}", fileName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return false;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null) return false;

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return false;
        }

        // Check MIME type
        var allowedMimeTypes = new[]
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        return allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant());
    }

    public string GetFileUrl(string fileName, string folder)
    {
        return $"/{folder}/{fileName}";
    }
}
