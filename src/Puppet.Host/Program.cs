using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Puppet.Core;
using Puppet.Host;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AppSession>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<StepStatus>(JsonNamingPolicy.CamelCase));
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<FailureCause>());
});

// editor/dist, served from the same host so there is no CORS and no
// dev-time proxy. Sits two levels above this project's content root
// (src/Puppet.Host) at the repo root.
var editorDist = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "editor", "dist"));
var editorDistExists = Directory.Exists(editorDist);
if (editorDistExists)
{
    builder.Environment.WebRootPath = editorDist;
    builder.Environment.WebRootFileProvider = new PhysicalFileProvider(editorDist);
}

var app = builder.Build();

if (editorDistExists)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapSessionEndpoints();

app.Run();
