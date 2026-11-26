using AZUCENA_11262026.Middleware;
using AZUCENA_11262026.Options;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllers();

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register API key configuration
builder.Services
    .AddOptions<ApiKeyOptions>()
    .BindConfiguration(ApiKeyOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();


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
app.MapGet("/api/auth-check", () =>
    Results.Ok(new { message = "Authenticated" }));
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
