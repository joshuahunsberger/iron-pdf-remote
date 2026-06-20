using Azure.Storage.Blobs;

namespace Worker;

public class PdfGenerationWorker(
    ILogger<PdfGenerationWorker> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    BlobContainerClient containerClient)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var renderer = new ChromePdfRenderer();
        var now = DateTime.UtcNow;
        var pdf = await renderer.RenderHtmlAsPdfAsync($"<h1>Hello World!</h1><h3>Generated at {now:O}</h3>");
        logger.LogInformation("PDF rendered");

        await containerClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
        var blobClient = containerClient.GetBlobClient("ironpdf.pdf");

        using var stream = new MemoryStream(pdf.BinaryData);
        await blobClient.UploadAsync(stream, true, stoppingToken);
        logger.LogInformation("PDF saved to Azure Blob Storage");

        hostApplicationLifetime.StopApplication();
    }
}
