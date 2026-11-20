namespace Marketplace.Application.Files;

public interface IFileStorage
{
    Task<StoredFile> SaveAsync(Stream content, string extension, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
    string GetAbsolutePath(string relativePath);
}
