using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AZUCENA_11262026.Services
{
    public sealed class JsonFileProcessingService : IFileProcessingService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        public async Task<FileProcessingResult> ProcessFileAsync(Stream fileStream, CancellationToken cancellationToken = default)
        {

            var products = await JsonSerializer.DeserializeAsync<List<Product?>>(fileStream, SerializerOptions, cancellationToken);

            if (products is not null)
            {
                foreach (var product in products)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (product is null)
                    {
                        throw new ValidationException("Product records must not be null.");
                    }

                    Validator.ValidateObject(product, new ValidationContext(product), validateAllProperties: true);
                }

                var lowStockProducts = products
                    .Select(product => product!)
                    .Where(product => product.QuantityOnHand <= product.ReorderLevel)
                    .ToList();

                return new FileProcessingResult
                {
                    TotalRecords = products.Count,
                    Results = lowStockProducts
                };
            }

            throw new ValidationException("The JSON file must contain an array of products.");
        }
    }
}
