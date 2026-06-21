using System.Diagnostics;
using Azure.Storage.Blobs;

namespace Worker;

public class PdfGenerationWorker(
    ILogger<PdfGenerationWorker> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    BlobContainerClient containerClient)
    : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("Worker");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity();

        try
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
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            logger.LogError(ex, "An error occurred during PDF generation");
            throw;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }
}
