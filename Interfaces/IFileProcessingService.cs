using AZUCENA_11262026.Models;

namespace AZUCENA_11262026.Interfaces
{
    public interface IFileProcessingService
    {
        Task<FileProcessingResult> ProcessFileAsync(Stream fileStream, CancellationToken cancellationToken = default);
    }
}
