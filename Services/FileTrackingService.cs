using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Responses;

namespace AZUCENA_11262026.Services
{
    public sealed class FileTrackingService : IFileTrackingService
    {
        private const int MaxHistoryRecords = 100;

        private readonly object _lock = new();

        private readonly Queue<FileProcessingRecord> _records = new();

        private long _successfulFiles;
        private long _failedFiles;

        public void Record(FileProcessingRecord record)
        {
            lock (_lock)
            {
                if (record.Status == ProcessingStatus.Success)
                {
                    _successfulFiles++;
                }
                else
                {
                    _failedFiles++;
                }

                _records.Enqueue(record);

                if (_records.Count > MaxHistoryRecords)
                {
                    _records.Dequeue();
                }
            }
        }

        public FileProcessingReport GetReport()
        {
            lock (_lock)
            {
                return new FileProcessingReport
                {
                    TotalAttempts = _successfulFiles + _failedFiles,
                    SuccessfulFiles = _successfulFiles,
                    FailedFiles = _failedFiles,
                    Records = _records.Reverse().ToArray()
                };
            }
        }
    }
}
