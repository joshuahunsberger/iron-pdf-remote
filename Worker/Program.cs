using Worker;
using IronPdf.GrpcLayer;

var builder = Host.CreateApplicationBuilder(args);

builder.AddAzureBlobServiceClient("blobs");

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
    var connectionConfig = IronPdfConnectionConfiguration.RemoteServer(ironPdfConfiguration.EngineUrl);
    Installation.ConnectToIronPdfHost(connectionConfig);
    Installation.LicenseKey = ironPdfConfiguration.LicenseKey;
}

internal record IronPdfConfiguration
{
    public required string EngineUrl { get; init; }
    public required string LicenseKey { get; init; }
}
