using AZUCENA_11262026.Models;

namespace AZUCENA_11262026.Interfaces
{
    public interface IJsonFileProcessingService
    {
        Task<FileProcessingResult> ProcessFileAsync(Stream fileStream, CancellationToken cancellationToken = default);
    }
}
