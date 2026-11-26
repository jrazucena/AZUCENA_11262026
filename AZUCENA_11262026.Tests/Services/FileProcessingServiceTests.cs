
using AZUCENA_11262026.Models;
using AZUCENA_11262026.Models.Request;
using AZUCENA_11262026.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Xunit;

namespace AZUCENA_11262026.Tests.Services;

public class FileProcessingServiceTests
{
    private readonly FileTrackingService _trackingService;
    private readonly FileProcessingService _service;

    public FileProcessingServiceTests()
    {
        _trackingService = new FileTrackingService();
        _service = new FileProcessingService(new JsonFileProcessingService(), _trackingService, NullLogger<FileProcessingService>.Instance);
    }

    [Fact]
    public async Task ProcessFileAsync_ValidFile_ReturnsResult()
    {
        // Arrange
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
            }
        ]
        """;

        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes(json));

        var request = new FileProcessingRequest
        {
            File = new FormFile(
                stream, 0, stream.Length,
                "File", "products.json")
        };

        // Act
        var result = await _service.ProcessFileAsync(request);

        var report = _trackingService.GetReport();

        // Assert
        Assert.NotEqual(Guid.Empty, result.ProcessingId);
        Assert.Equal("products.json", result.FileName);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(1, result.MatchedRecords);
        Assert.Single(result.Results);

        Assert.Equal(1L, report.SuccessfulFiles);
        Assert.Equal(0L, report.FailedFiles);

        var record = Assert.Single(report.Records);

        Assert.Equal(result.ProcessingId, record.ProcessingId);
        Assert.Equal(ProcessingStatus.Success, record.Status);
    }

    [Fact]
    public async Task ProcessFileAsync_InvalidJson_TracksFailure()
    {
        // Arrange
        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes("invalid json"));

        var request = new FileProcessingRequest
        {
            File = new FormFile(
                stream, 0, stream.Length,
                "File", "invalid.json")
        };

        // Act
        await Assert.ThrowsAsync<JsonException>(
            () => _service.ProcessFileAsync(request));

        var report = _trackingService.GetReport();

        // Assert
        Assert.Equal(1L, report.TotalAttempts);
        Assert.Equal(0L, report.SuccessfulFiles);
        Assert.Equal(1L, report.FailedFiles);

        var record = Assert.Single(report.Records);

        Assert.Equal(ProcessingStatus.Failed, record.Status);
    }

    [Fact]
    public async Task ProcessFileAsync_MissingFile_ThrowsValidationException()
    {
        // Arrange
        var request = new FileProcessingRequest
        {
            File = null
        };

        // Act
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.ProcessFileAsync(request));

        var report = _trackingService.GetReport();

        // Assert
        Assert.Equal(0L, report.TotalAttempts);
        Assert.Empty(report.Records);
    }
}
