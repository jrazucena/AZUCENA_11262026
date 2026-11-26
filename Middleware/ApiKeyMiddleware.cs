using AZUCENA_11262026.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace AZUCENA_11262026.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly RequestDelegate _next;
        private readonly string _apiKey;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _apiKey = options.Value.Key;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) || providedKey.Count != 1 || string.IsNullOrWhiteSpace(providedKey[0])
                || !IsValidApiKey(providedKey[0]!))
            {
                _logger.LogWarning("Unauthorized request to {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing API key."});
                return;
            }

            await _next(context);
        }

        private bool IsValidApiKey(string providedKey)
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(providedKey),
                Encoding.UTF8.GetBytes(_apiKey));
        }
    }
}
