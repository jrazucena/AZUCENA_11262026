# AZUCENA_11262026
A RESTful Web API built with ASP.NET Core (.NET 10) that accepts JSON inventory files,
identifies products that need replenishment, and provides a report of file processing activity.

A product is considered low stock when its quantity on hand is less than or equal to its reorder level (QuantityOnHand <= ReorderLevel).

## Features
 - API key authentication using custom middleware and the X-API-Key request header.
 - JSON file upload, validation, and low-stock product filtering.
 - Thread-safe, in-memory tracking of successful and failed file processing attempts.
 - Processing reports with counters and recent history.
 - Centralized exception handling using ProblemDetails and built-in application logging.
 - Swagger UI for API testing, xUnit tests, and a multi-stage Dockerfile.

## Prerequisites
 - [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) for local development and testing.
 - Docker Desktop or Docker Engine for containerized deployment.

 Run the following commands from the repository root. The examples use PowerShell on Windows.

## Run locally

 Restore and build the application:

	```powershell
	dotnet restore .\AZUCENA_11262026.csproj
	dotnet build .\AZUCENA_11262026.csproj
	```

 Configure a temporary API key and start the application:

	```powershell
	$env:ApiKey__Key = [guid]::NewGuid().ToString("N")
	$env:ASPNETCORE_ENVIRONMENT = "Development"
	dotnet run --project .\AZUCENA_11262026.csproj --no-launch-profile -- --urls http://localhost:5167
	```

 Open Swagger UI at [http://localhost:5167/swagger](http://localhost:5167/swagger).

 To retrieve the temporary key, use `$env:ApiKey__Key` in the PowerShell session where it was set (after stopping the application if needed). Copy the value into Swagger's **Authorize** dialog.

	## Run with Docker

 Build the image:

	```powershell
	docker build -t file-processing-api:1.0 .
	```

 Generate a temporary API key and run the container:

	```powershell
	$env:ApiKey__Key = [guid]::NewGuid().ToString("N")
	docker run -d --name file-processing-container -p 127.0.0.1:8090:8080 -e ApiKey__Key -e ASPNETCORE_ENVIRONMENT=Development file-processing-api:1.0
	```

 Open Swagger UI at [http://localhost:8090/swagger](http://localhost:8090/swagger). Use `$env:ApiKey__Key` to retrieve the key for the **Authorize** dialog.

 Alternatively, run the image from Docker Desktop using host port `8090` mapped to container port `8080`, with the same two environment variables. Only use this configuration in a trusted local environment.

 Useful Docker commands:

	```powershell
	docker ps
	docker logs file-processing-container
	docker stop file-processing-container
	docker rm file-processing-container
	```

 If a container with the same name already exists, stop and remove that container before creating a new one. Removing or restarting the container clears the in-memory processing history.

## API usage

All `/api` endpoints require the `X-API-Key` header. The key is configured at runtime using the `ApiKey__Key` environment variable, which maps to `ApiKey:Key` in ASP.NET Core configuration.

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/api/Files/process` | Upload a JSON file and return low-stock products. |
| GET | `/api/Files/report` | Retrieve file processing counters and recent history. |

### Upload and process a JSON file

**Content type:** `multipart/form-data`  
**Form field:** `File`  
**Maximum file size:** 5 MiB (5,242,880 bytes)

Example input (`products.json`):

```json
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
```

To test in Swagger, authorize with your API key, expand `POST /api/Files/process`, click **Try it out**, select a JSON file, and click **Execute**.

To test using the command line, save the example above as `products.json` in the current directory and run the following against the Docker container:

```powershell
$baseUrl = "http://localhost:8090"
curl.exe -X POST "$baseUrl/api/Files/process" -H "X-API-Key: $env:ApiKey__Key" -F "File=@products.json;type=application/json"
```

For local execution without Docker, change `$baseUrl` to `http://localhost:5167`.

Example successful response (`200 OK`):

```json
{
  "processingId": "b1535fa1-104b-4aef-8dd8-5f92a81f0042",
  "fileName": "products.json",
  "totalRecords": 2,
  "matchedRecords": 1,
  "processingTimeMs": 12,
  "results": [
    {
      "productCode": "PRD002",
      "productName": "Mouse",
      "quantityOnHand": 5,
      "reorderLevel": 10
    }
  ]
}
```

Processing IDs and durations vary per request. An empty JSON array or a file with no low-stock products is valid and returns an empty results collection.

### Retrieve the processing report

```powershell
curl.exe "$baseUrl/api/Files/report" -H "X-API-Key: $env:ApiKey__Key"
```

Example report after the successful upload above:

```json
{
  "totalAttempts": 1,
  "successfulFiles": 1,
  "failedFiles": 0,
  "records": [
    {
      "processingId": "b1535fa1-104b-4aef-8dd8-5f92a81f0042",
      "fileName": "products.json",
      "fileSizeBytes": 258,
      "processedAtUtc": "2026-11-26T15:00:00+00:00",
      "processingTimeMs": 12,
      "status": "Success",
      "totalRecords": 2,
      "matchedRecords": 1
    }
  ]
}
```

The report contains cumulative counters and the most recent 100 processing records, with the latest recorded attempt first. Failed attempts may have null product counts.

### Error handling

| Status | Description |
| --- | --- |
| 200 OK | Request completed successfully. |
| 400 Bad Request | Missing or empty file, unsupported file extension, malformed JSON, or invalid product data. |
| 401 Unauthorized | Missing or incorrect API key. |
| 413 Payload Too Large | Uploaded file exceeds the configured size limit (when handled by the application). |
| 415 Unsupported Media Type | Request content type is not supported. |
| 500 Internal Server Error | Unexpected application error. |

Application-handled errors use ProblemDetails responses. Hosting-level and model-binding errors may have different response details. Invalid product records cause the entire file to be rejected.

To verify authentication, call the report endpoint without a key:

```powershell
curl.exe -i "$baseUrl/api/Files/report"
```

The expected result is `401 Unauthorized`.

## Run tests

```powershell
dotnet test .\AZUCENA_11262026.Tests\AZUCENA_11262026.Tests.csproj
```

Tests can also be run using **Test > Test Explorer > Run All Tests** in Visual Studio. The test suite covers JSON transformation, processing validation, tracking counters, and concurrent tracking operations.

## Application structure

```text
Controllers/    HTTP endpoints
Interfaces/     Service contracts
Services/       File processing, JSON transformation, and tracking
Models/         Request, response, and processing models
Middleware/     API key authentication
Exceptions/     Centralized error handling
Options/        API key configuration
```

Controllers delegate processing to services. The JSON processing service accepts a stream so it can be tested independently of HTTP. The tracking service is registered as a singleton and synchronizes access to shared state. The Dockerfile uses separate build and runtime stages and runs the application as a non-root user.