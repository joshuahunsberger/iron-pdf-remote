using Worker;
using IronPdf.GrpcLayer;

var builder = Host.CreateApplicationBuilder(args);

builder.AddAzureBlobContainerClient("blobs",
    settings => settings.BlobContainerName = "pdfs");

SetupIronPdf(builder.Configuration);

builder.AddServiceDefaults();
builder.Services.AddHostedService<PdfGenerationWorker>();

var host = builder.Build();
host.Run();
return;

void SetupIronPdf(ConfigurationManager configuration)
{
    var ironPdfConfiguration = configuration.GetSection("IronPdf").Get<IronPdfConfiguration>();
    if (ironPdfConfiguration == null) return;
    var connectionConfig = IronPdfConnectionConfiguration.RemoteServer(ironPdfConfiguration.Address);
    Installation.ConnectToIronPdfHost(connectionConfig);
    Installation.LicenseKey = ironPdfConfiguration.LicenseKey;
}

internal record IronPdfConfiguration
{
    public required string EngineHost { get; init; }
    public required string EnginePort { get; init; }
    public required string LicenseKey { get; init; }
    public string Address =>  $"http://{EngineHost}:{EnginePort}";
}
