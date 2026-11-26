using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Models.Request;
using AZUCENA_11262026.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AZUCENA_11262026.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class FilesController : ControllerBase
    {
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IFileTrackingService _fileTrackingService;
        public FilesController(IFileProcessingService fileProcessingService, IFileTrackingService fileTrackingService)
        {
            _fileProcessingService = fileProcessingService;
            _fileTrackingService = fileTrackingService;
        }

        [HttpPost("process")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        [ProducesResponseType<FileProcessingResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<FileProcessingResponse>> ProcessFileAsync([FromForm] FileProcessingRequest request, CancellationToken cancellationToken)
        {
            var result = await _fileProcessingService.ProcessFileAsync(request, cancellationToken);

            return Ok(result);
        }
        [HttpGet("report")]
        [ProducesResponseType<FileProcessingReport>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<FileProcessingReport> GetReport()
        {
            var report = _fileTrackingService.GetReport();

            return Ok(report);
        }
    }
}
