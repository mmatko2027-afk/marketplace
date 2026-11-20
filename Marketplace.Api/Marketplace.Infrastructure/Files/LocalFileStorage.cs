using Marketplace.Application.Files;

namespace Marketplace.Infrastructure.Files;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _contentRootPath;
    private readonly string _rootPath;

    public LocalFileStorage(string contentRootPath)
    {
        _contentRootPath = contentRootPath;
        _rootPath = Path.Combine(contentRootPath, "uploads", "listings");
    }

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string extension,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine("uploads", "listings", fileName);
        var absolutePath = GetAbsolutePath(relativePath);

        await using var target = File.Create(absolutePath);
        await content.CopyToAsync(target, cancellationToken);
        return new StoredFile(relativePath, contentType);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);
        return Task.CompletedTask;
    }

    public string GetAbsolutePath(string relativePath)
    {
        var fullRoot = Path.GetFullPath(_rootPath + Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_contentRootPath, relativePath));

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Некоректний шлях до файлу.");

        return fullPath;
    }
}
