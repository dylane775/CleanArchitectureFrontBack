using Catalog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers;

[ApiController]
[Route("api/catalog/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ImagesController> _logger;
    private const string ProductImagesFolder = "images/products";

    public ImagesController(
        IFileStorageService fileStorageService,
        ILogger<ImagesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a product image
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Image URL</returns>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImageUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadProductImage(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading product image: {FileName}", file?.FileName);

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        if (!_fileStorageService.IsValidImageFile(file))
        {
            return BadRequest(new { error = "Invalid file type. Only image files (jpg, jpeg, png, gif, webp) are allowed." });
        }

        try
        {
            var fileName = await _fileStorageService.UploadFileAsync(file, ProductImagesFolder, cancellationToken);
            var fileUrl = _fileStorageService.GetFileUrl(fileName, ProductImagesFolder);

            _logger.LogInformation("Product image uploaded successfully: {FileName}", fileName);

            return Ok(new ImageUploadResponse
            {
                FileName = fileName,
                FileUrl = fileUrl,
                OriginalFileName = file.FileName
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt: {FileName}", file.FileName);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading product image: {FileName}", file.FileName);
            return StatusCode(500, new { error = "An error occurred while uploading the image" });
        }
    }

    /// <summary>
    /// Deletes a product image
    /// </summary>
    /// <param name="fileName">Name of the file to delete</param>
    /// <param name="cancellationToken"></param>
    [HttpDelete("{fileName}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProductImage(
        string fileName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product image: {FileName}", fileName);

        try
        {
            var deleted = await _fileStorageService.DeleteFileAsync(fileName, ProductImagesFolder, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = "File not found" });
            }

            _logger.LogInformation("Product image deleted successfully: {FileName}", fileName);
            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product image: {FileName}", fileName);
            return StatusCode(500, new { error = "An error occurred while deleting the image" });
        }
    }
}

public class ImageUploadResponse
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
}
