using AZUCENA_11262026.Exceptions;
using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Request;
using AZUCENA_11262026.Models.Responses;
using AZUCENA_11262026.Models.Results;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;

namespace AZUCENA_11262026.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private readonly IJsonFileProcessingService _jsonFileProcessingService;
        private readonly IFileTrackingService _fileTrackingService;

        public FileProcessingService(IJsonFileProcessingService jsonFileProcessingService, IFileTrackingService fileTrackingService)
        {
            _jsonFileProcessingService = jsonFileProcessingService;
            _fileTrackingService = fileTrackingService;
        }


        public async Task<FileProcessingResponse> ProcessFileAsync(FileProcessingRequest request, CancellationToken cancellationToken = default)
        {
            var file = request.File ?? throw new ValidationException("Please provide a JSON file.");

            var processingId = Guid.NewGuid();

            var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));

            var stopwatch = Stopwatch.StartNew();

            FileProcessingResult result;
            try
            {
                ValidateFile(file);

                await using var fileStream = file.OpenReadStream();

                result = await _jsonFileProcessingService.ProcessFileAsync(
                    fileStream,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                RecordProcessing(ProcessingStatus.Failed);
                throw;
            }

            RecordProcessing(ProcessingStatus.Success, result);

            return new FileProcessingResponse
            {
                ProcessingId = processingId,
                FileName = fileName,
                TotalRecords = result.TotalRecords,
                MatchedRecords = result.MatchedRecords,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Results = result.Results
            };

            void RecordProcessing(ProcessingStatus status, FileProcessingResult? processingResult = null)
            {
                stopwatch.Stop();
                _fileTrackingService.Record(new FileProcessingRecord
                {
                    ProcessingId = processingId,
                    FileName = fileName,
                    FileSizeBytes = file.Length,
                    ProcessedAtUtc = DateTimeOffset.UtcNow,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    Status = status,
                    TotalRecords = processingResult?.TotalRecords,
                    MatchedRecords = processingResult?.MatchedRecords
                });
            }
        }

        private static void ValidateFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                throw new ValidationException("Please provide a valid JSON file.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new FileSizeLimitExceededException("File size must not exceed 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (!string.Equals(
                extension,
                ".json",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("Only JSON files are supported.");
            }
        }
    }
}