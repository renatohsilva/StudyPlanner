using Microsoft.Extensions.Options;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Infrastructure.Storage;

public class LocalFileStorageOptions
{
    /// <summary>Diretório raiz no disco onde os arquivos são salvos. Trocável por blob storage sem afetar a Application.</summary>
    public string RootPath { get; set; } = "storage";
}

public class LocalFileStorage(IOptions<LocalFileStorageOptions> options) : IFileStorage
{
    private readonly string _rootPath = options.Value.RootPath;

    public async Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(_rootPath, folder);
        Directory.CreateDirectory(directory);

        var safeFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(directory, safeFileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return Path.Combine(folder, safeFileName).Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string fileUrl, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_rootPath, fileUrl);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }
}
