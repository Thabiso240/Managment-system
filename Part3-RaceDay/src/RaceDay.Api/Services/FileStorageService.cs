using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace RaceDay.Api.Services;

/// <summary>
/// Abstraction over cloud file storage. The MVC application never talks to Azure
/// directly - every upload is proxied through this API.
/// </summary>
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default);
}

/// <summary>
/// Stores uploaded images in Azure Blob Storage and returns the public blob URL.
/// </summary>
public class AzureBlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _container;
    private readonly ILogger<AzureBlobFileStorageService> _logger;
    private bool _containerReady;

    public AzureBlobFileStorageService(IOptions<BlobStorageOptions> options, ILogger<AzureBlobFileStorageService> logger)
    {
        _logger = logger;
        var client = new BlobServiceClient(options.Value.ConnectionString);
        _container = client.GetBlobContainerClient(options.Value.ContainerName);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        if (!_containerReady)
        {
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
            _containerReady = true;
        }

        var blobName = $"{folder}/{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var blob = _container.GetBlobClient(blobName);

        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);

        _logger.LogInformation("Uploaded {BlobName} to Azure Blob Storage", blobName);
        return blob.Uri.ToString();
    }
}

/// <summary>
/// Development fallback that writes uploads to the API's local wwwroot.
/// Keeps the exact same contract as the Azure implementation so the MVC front end
/// behaves identically whether or not an Azure account is configured.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var directory = Path.Combine(webRoot, "uploads", folder);
        Directory.CreateDirectory(directory);

        var storedName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(directory, storedName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request is null ? string.Empty : $"{request.Scheme}://{request.Host}";
        var url = $"{baseUrl}/uploads/{folder}/{storedName}";

        _logger.LogInformation("Stored upload at {Path} (served as {Url})", fullPath, url);
        return url;
    }
}
