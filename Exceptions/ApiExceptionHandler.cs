using Microsoft.AspNetCore.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AZUCENA_11262026.Exceptions
{
    public sealed class ApiExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                FileSizeLimitExceededException =>
                    (
                        StatusCodes.Status413PayloadTooLarge,
                        "File too large",
                        "File size must not exceed 5 MB."
                    ),

                ValidationException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Validation failed",
                        "The uploaded file or product records are invalid."
                    ),

                JsonException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Invalid JSON",
                        "The file contains invalid JSON or an unsupported structure."
                    ),

                _ => (0, string.Empty, string.Empty)
            };

            if (statusCode == 0)
            {
                return false;
            }

            await Results.Problem(statusCode: statusCode, title: title, detail: detail)
                .ExecuteAsync(httpContext);

            return true;
        }
    }
}
