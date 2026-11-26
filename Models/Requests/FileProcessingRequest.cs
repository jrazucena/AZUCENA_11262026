using System.ComponentModel.DataAnnotations;

namespace AZUCENA_11262026.Models.Request
{
    public sealed class FileProcessingRequest
    {
        [Required]
        public  IFormFile? File { get; set; }
    }
}
