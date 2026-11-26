using AZUCENA_11262026.Models;
using AZUCENA_11262026.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AZUCENA_11262026.Tests.Services
{
    public class FileTrackingServiceTests
    {
        [Fact]
        public void GetReport_NoFiles_ReturnsEmptyReport()
        {
            var service = new FileTrackingService();

            var report = service.GetReport();

            Assert.Equal(0L, report.TotalAttempts);
            Assert.Equal(0L, report.SuccessfulFiles);
            Assert.Equal(0L, report.FailedFiles);
            Assert.Empty(report.Records);
        }

        [Fact]
        public void Record_MultipleFiles_ReturnsCorrectTotals()
        {
            var service = new FileTrackingService();

            service.Record(CreateRecord(ProcessingStatus.Success));
            service.Record(CreateRecord(ProcessingStatus.Success));
            service.Record(CreateRecord(ProcessingStatus.Failed));

            var report = service.GetReport();

            Assert.Equal(3L, report.TotalAttempts);
            Assert.Equal(2L, report.SuccessfulFiles);
            Assert.Equal(1L, report.FailedFiles);
            Assert.Equal(3, report.Records.Count);
        }

        [Fact]
        public void Record_ConcurrentRequests_MaintainsAccurateTotals()
        {
            var service = new FileTrackingService();

            Parallel.For(0, 1000, _ =>
            {
                service.Record(CreateRecord(ProcessingStatus.Success));
            });

            var report = service.GetReport();

            Assert.Equal(1000L, report.TotalAttempts);
            Assert.Equal(1000L, report.SuccessfulFiles);
            Assert.Equal(0L, report.FailedFiles);
            Assert.Equal(100, report.Records.Count);
        }

        private static FileProcessingRecord CreateRecord(
            ProcessingStatus status)
        {
            return new FileProcessingRecord
            {
                ProcessingId = Guid.NewGuid(),
                FileName = "products.json",
                FileSizeBytes = 100,
                ProcessedAtUtc = DateTimeOffset.UtcNow,
                ProcessingTimeMs = 10,
                Status = status
            };
        }
    }
}
