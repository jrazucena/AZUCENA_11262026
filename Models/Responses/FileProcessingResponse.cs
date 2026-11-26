namespace AZUCENA_11262026.Models.Responses
{
    public sealed class FileProcessingResponse
    {
        public required Guid ProcessingId { get; set; }
        public required string FileName { get; set; }
        public int TotalRecords { get; set; }
        public int MatchedRecords { get; set; }
        public int ProcessingTimeMs { get; set; }
        public IReadOnlyList<Product>? Results { get; set; }

    }
}
