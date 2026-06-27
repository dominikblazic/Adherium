using System.Text.Json.Serialization;
using Adherium.Adherence.Api.Endpoints;
using Adherium.Adherence.Api.Seeding;
using Adherium.Adherence.Core.Extensions;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddCoreServices();
builder.Services.AddSingleton<SampleDataSeeder>();

var app = builder.Build();

// Seed the in-memory stores from the synthetic sample dataset at startup.
var sampleDataPath = Path.Combine(AppContext.BaseDirectory, "sample-batch.json");
app.Services.GetRequiredService<SampleDataSeeder>().Seed(sampleDataPath);

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapSensorLogEndpoints();

app.Run();

/// <summary>Exposed so the integration/host can be referenced from tests if needed.</summary>
public partial class Program;
