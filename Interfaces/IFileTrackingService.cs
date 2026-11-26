using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Responses;

namespace AZUCENA_11262026.Interfaces
{
    public interface IFileTrackingService
    {
        void Record(FileProcessingRecord record);

        FileProcessingReport GetReport();
    }
}
