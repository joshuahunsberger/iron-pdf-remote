using Worker;
using IronPdf.GrpcLayer;

var engineHost = Environment.GetEnvironmentVariable("IRONPDF_ENGINE_HOST") ?? "localhost";
var enginePort = int.TryParse(Environment.GetEnvironmentVariable("IRONPDF_ENGINE_PORT"), out var p) ? p : 33350;


var config = IronPdfConnectionConfiguration.RemoteServer($"http://{engineHost}:{enginePort}");

Installation.ConnectToIronPdfHost(config);

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHostedService<PdfGenerationWorker>();

var host = builder.Build();
host.Run();
