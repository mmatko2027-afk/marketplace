using Marketplace.Application.Files;
using Marketplace.Application.Repositories;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Services;

public class ListingImageService
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly IListingImageRepository _repository;
    private readonly IFileStorage _fileStorage;

    public ListingImageService(IListingImageRepository repository, IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public Task<ListingImage?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<List<ListingImage>> GetByListingAsync(int listingId) => _repository.GetByListingAsync(listingId);
    public Task<ListingImage> AddAsync(ListingImage image) => _repository.AddAsync(image);

    public async Task<ListingImage> UploadAsync(
        int listingId,
        ImageUpload upload,
        CancellationToken cancellationToken = default)
    {
        var extension = upload.Extension.Trim().ToLowerInvariant();
        if (upload.Length <= 0 || upload.Length > MaxFileSize)
            throw new InvalidDataException("Файл має бути непорожнім і не перевищувати 5 MB.");

        if (!AllowedTypes.TryGetValue(extension, out var expectedContentType) ||
            !string.Equals(expectedContentType, upload.ContentType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Дозволені лише JPEG, PNG та WebP з коректним MIME-типом.");

        var storedFile = await _fileStorage.SaveAsync(
            upload.Content, extension, expectedContentType, cancellationToken);

        try
        {
            return await _repository.AddAsync(new ListingImage
            {
                ListingId = listingId,
                FilePath = storedFile.RelativePath,
                SortOrder = (await _repository.GetByListingAsync(listingId)).Count
            });
        }
        catch
        {
            await _fileStorage.DeleteAsync(storedFile.RelativePath, cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(ListingImage image, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(image.Id);
        if (deleted)
            await _fileStorage.DeleteAsync(image.FilePath, cancellationToken);
        return deleted;
    }

    public string GetAbsolutePath(string relativePath) => _fileStorage.GetAbsolutePath(relativePath);
}
