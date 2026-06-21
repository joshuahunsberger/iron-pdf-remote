using System.Diagnostics;
using Azure.Storage.Blobs;

namespace Worker;

public class PdfGenerationWorker(
    ILogger<PdfGenerationWorker> logger,
    BlobServiceClient serviceClient)
    : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("Worker");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await GeneratePdf(stoppingToken);

            logger.LogInformation("Waiting 5 minutes before generating the next PDF...");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task GeneratePdf(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity();

        try
        {
            var renderer = new ChromePdfRenderer();
            var now = DateTime.UtcNow;
            var pdf = await renderer.RenderHtmlAsPdfAsync($"<h1>Hello World!</h1><h3>Generated at {now:O}</h3>");
            logger.LogInformation("PDF rendered");

            var containerClient = serviceClient.GetBlobContainerClient("pdf-data");
            await containerClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
            var blobClient = containerClient.GetBlobClient("ironpdf.pdf");

            using var stream = new MemoryStream(pdf.BinaryData);
            await blobClient.UploadAsync(stream, true, stoppingToken);
            logger.LogInformation("PDF saved to Azure Blob Storage");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            logger.LogError(ex, "An error occurred during PDF generation");
        }
    }
}
