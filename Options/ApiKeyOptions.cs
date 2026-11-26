using System.ComponentModel.DataAnnotations;

namespace AZUCENA_11262026.Options
{
    public sealed class ApiKeyOptions
    {
        public const string SectionName = "ApiKey";

        [Required]
        public string Key { get; init; } = string.Empty;
    }
}
