using System.ComponentModel.DataAnnotations;

namespace AZUCENA_11262026.Models
{
    public sealed class Product
    {
        [Required]
        public required string ProductCode { get; set; }
        [Required]
        public required string ProductName { get; set; }
        [Range(0, int.MaxValue)]
        public required int QuantityOnHand { get; set; }
        [Range(0, int.MaxValue)]
        public required int ReorderLevel { get; set; }
    }
}
