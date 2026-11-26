using AZUCENA_11262026.Exceptions;
using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Request;
using AZUCENA_11262026.Models.Responses;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;

namespace AZUCENA_11262026.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        private readonly IJsonFileProcessingService _jsonFileProcessingService;

        public FileProcessingService(IJsonFileProcessingService jsonFileProcessingService)
        {
            _jsonFileProcessingService = jsonFileProcessingService;
        }

        public async Task<FileProcessingResponse> ProcessFileAsync(FileProcessingRequest request, CancellationToken cancellationToken = default)
        {
            var file = request.File;

            ValidateFile(file);

            var processingId = Guid.NewGuid();

            var stopwatch = Stopwatch.StartNew();

            await using var fileStream = file!.OpenReadStream();

            var result = await _jsonFileProcessingService.ProcessFileAsync(fileStream, cancellationToken);

            stopwatch.Stop();

            return new FileProcessingResponse
            {
                ProcessingId = processingId,
                FileName = Path.GetFileName(file.FileName.Replace('\\', '/')),
                TotalRecords = result.TotalRecords,
                MatchedRecords = result.MatchedRecords,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Results = result.Results
            };
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