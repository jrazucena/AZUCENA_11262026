using AZUCENA_11262026.Interfaces;
using AZUCENA_11262026.Middleware;
using AZUCENA_11262026.Options;
using AZUCENA_11262026.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    const string securitySchemeId = "ApiKey";

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Processing API",
        Version = "v1",
        Description = "API for processing and reporting inventory files."
    });

    options.AddSecurityDefinition(securitySchemeId,
        new OpenApiSecurityScheme
        {
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "Enter your API key."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                securitySchemeId, document)] = []
        });
});

// Register API key configuration
builder.Services
    .AddOptions<ApiKeyOptions>()
    .BindConfiguration(ApiKeyOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IJsonFileProcessingService, JsonFileProcessingService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    branch =>
    {
        branch.UseMiddleware<ApiKeyMiddleware>();
    });

// Uncomment the following line to add an authentication check endpoint
//app.MapGet("/api/auth-check", () =>
//    Results.Ok(new { message = "Authenticated" }));


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
