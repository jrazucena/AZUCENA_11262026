using AZUCENA_11262026.Services;
using System.Text;

namespace AZUCENA_11262026.Tests.Services
{
    public class JsonFileProcessingServiceTests
    {
        [Fact]
        public async Task ProcessFileAsync_ValidProducts_ReturnsLowStockProducts()
        {
            // Arrange
            var service = new JsonFileProcessingService();

            var json = """
        [
            {
                "productCode": "PRD001",
                "productName": "Keyboard",
                "quantityOnHand": 25,
                "reorderLevel": 10
            },
            {
                "productCode": "PRD002",
                "productName": "Mouse",
                "quantityOnHand": 5,
                "reorderLevel": 10
            },
            {
                "productCode": "PRD003",
                "productName": "Monitor",
                "quantityOnHand": 10,
                "reorderLevel": 10
            }
        ]
        """;

            using var stream = new MemoryStream(
                Encoding.UTF8.GetBytes(json));

            // Act
            var result = await service.ProcessFileAsync(stream);

            // Assert
            Assert.Equal(3, result.TotalRecords);
            Assert.Equal(2, result.MatchedRecords);

            Assert.Contains(result.Results,
                p => p.ProductCode == "PRD002");

            Assert.Contains(result.Results,
                p => p.ProductCode == "PRD003");

            Assert.DoesNotContain(result.Results,
                p => p.ProductCode == "PRD001");
        }
    }
}
