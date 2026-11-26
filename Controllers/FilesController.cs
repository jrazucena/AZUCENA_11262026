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

        public FilesController(
            IFileProcessingService fileProcessingService)
        {
            _fileProcessingService = fileProcessingService;
        }

        [HttpPost("process")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<FileProcessingResponse>(
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        public async Task<ActionResult<FileProcessingResponse>> ProcessFileAsync(
            [FromForm] FileProcessingRequest request,
            CancellationToken cancellationToken)
        {
            // 1. Validate the uploaded file
            if (request.File is null || request.File.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or the file is empty." });
            }
            var extension = Path.GetExtension(request.File.FileName);

            if (extension != ".json")
            {
                return BadRequest(new { message = "Invalid file type. Please upload a JSON file." });
            }

            // 2. Generate processing ID

            // 3. Start measuring processing time

            // 4. Open the file stream

            // 5. Call the processing service

            // 6. Construct the response

            // 7. Return HTTP 200 OK

            throw new NotImplementedException();
        }
    }
}
