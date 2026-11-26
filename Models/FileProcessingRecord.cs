namespace AZUCENA_11262026.Models
{
    public sealed class FileProcessingRecord
    {
        public required Guid ProcessingId { get; init; }

        public required string FileName { get; init; }

        public required long FileSizeBytes { get; init; }

        public required DateTimeOffset ProcessedAtUtc { get; init; }

        public required long ProcessingTimeMs { get; init; }

        public required ProcessingStatus Status { get; init; }

        public int? TotalRecords { get; init; }

        public int? MatchedRecords { get; init; }
    }
}
