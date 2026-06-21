using System.Diagnostics;
using Azure.Storage.Blobs;

namespace Worker;

public class PdfGenerationWorker(
    ILogger<PdfGenerationWorker> logger,
    BlobServiceClient serviceClient)
    : BackgroundService
{
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
        using var rootActivity = Instrumentation.StartActivity("GeneratePdf");

        try
        {
            byte[] pdfData;

            using (var renderActivity = Instrumentation.StartActivity("RenderPdf"))
            {
                var renderer = new ChromePdfRenderer();
                var now = DateTime.UtcNow;
                using var pdf =
                    await renderer.RenderHtmlAsPdfAsync($"<h1>Hello World!</h1><h3>Generated at {now:O}</h3>");
                pdfData = pdf.BinaryData;

                renderActivity?.SetTag("pdf.size_bytes", pdfData.Length);
                logger.LogInformation("PDF rendered");
            }

            using (var uploadActivity = Instrumentation.StartActivity("UploadToBlobStorage"))
            {
                uploadActivity?.SetTag("blob.container", "pdf-data");
                uploadActivity?.SetTag("blob.name", "ironpdf.pdf");

                var containerClient = serviceClient.GetBlobContainerClient("pdf-data");
                await containerClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
                var blobClient = containerClient.GetBlobClient("ironpdf.pdf");

                using var stream = new MemoryStream(pdfData);
                await blobClient.UploadAsync(stream, true, stoppingToken);
                logger.LogInformation("PDF saved to Azure Blob Storage");
            }
        }
        catch (Exception ex)
        {
            rootActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            rootActivity?.AddException(ex);
            logger.LogError(ex, "An error occurred during PDF generation");
        }
    }
}
