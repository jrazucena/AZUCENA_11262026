namespace AZUCENA_11262026.Models.Results
{
    public sealed class FileProcessingResult
    {
        public required int TotalRecords { get; init; }

        public required IReadOnlyList<Product> Results { get; init; }

        public int MatchedRecords => Results.Count;
    }
}
