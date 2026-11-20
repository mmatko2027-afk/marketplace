namespace Marketplace.Application.Files;

public sealed record ImageUpload(
    Stream Content,
    long Length,
    string Extension,
    string ContentType);
