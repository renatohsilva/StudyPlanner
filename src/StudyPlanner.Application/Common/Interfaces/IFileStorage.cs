namespace StudyPlanner.Application.Common.Interfaces;

/// <summary>
/// Abstração de armazenamento de arquivos. Implementação local em disco no MVP;
/// trocável por blob storage (S3/Azure) sem tocar Application/Domain.
/// </summary>
public interface IFileStorage
{
    /// <summary>Salva o conteúdo e retorna uma URL/caminho relativo para recuperá-lo depois.</summary>
    Task<string> SaveAsync(string folder, string fileName, Stream content, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string fileUrl, CancellationToken cancellationToken);
}
