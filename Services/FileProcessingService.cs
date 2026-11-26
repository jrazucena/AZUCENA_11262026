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
        private readonly ILogger<FileProcessingService> _logger;
        public FileProcessingService(IJsonFileProcessingService jsonFileProcessingService, IFileTrackingService fileTrackingService, ILogger<FileProcessingService> logger)
        {
            _jsonFileProcessingService = jsonFileProcessingService;
            _fileTrackingService = fileTrackingService;
            _logger = logger;


        }


        public async Task<FileProcessingResponse> ProcessFileAsync(FileProcessingRequest request, CancellationToken cancellationToken = default)
        {
            var file = request.File ?? throw new ValidationException("Please provide a JSON file.");

            var processingId = Guid.NewGuid();

            var fileName = Path.GetFileName(file.FileName.Replace('\\', '/'));

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("File processing started. ProcessingId: {ProcessingId}, FileSize: {FileSizeBytes}", processingId, file.Length);
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

                _logger.LogWarning("File processing failed. ProcessingId: {ProcessingId}, " + "Duration: {DurationMs} ms", processingId, stopwatch.ElapsedMilliseconds);

                throw;
            }

            RecordProcessing(ProcessingStatus.Success, result);

            _logger.LogInformation("File processing completed. ProcessingId: {ProcessingId}, " + "TotalRecords: {TotalRecords}, MatchedRecords: {MatchedRecords}, " + "Duration: {DurationMs} ms",
                processingId,
                result.TotalRecords,
                result.MatchedRecords,
                stopwatch.ElapsedMilliseconds);

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