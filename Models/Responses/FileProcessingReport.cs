namespace AZUCENA_11262026.Models.Responses
{
    public sealed class FileProcessingReport
    {
        public long TotalAttempts { get; init; }

        public long SuccessfulFiles { get; init; }

        public long FailedFiles { get; init; }

        public required IReadOnlyList<FileProcessingRecord>
            Records
        { get; init; }
    }
}
