using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Request;
using AZUCENA_11262026.Models.Responses;

namespace AZUCENA_11262026.Interfaces
{
    public interface IFileProcessingService
    {
        Task<FileProcessingResponse> ProcessFileAsync(FileProcessingRequest request, CancellationToken cancellationToken = default);
    }
}
