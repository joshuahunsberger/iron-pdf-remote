namespace Worker;

public class PdfGenerationWorker(ILogger<PdfGenerationWorker> logger, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var renderer = new ChromePdfRenderer();
        var now = DateTime.UtcNow;
        var pdf = await renderer.RenderHtmlAsPdfAsync($"<h1>Hello World!</h1><h3>Generated at {now:O}</h3>");
        logger.LogInformation("PDF rendered");
        pdf.SaveAs("pdfOutput/ironpdf.pdf");
        logger.LogInformation("PDF saved");
        
        hostApplicationLifetime.StopApplication();
    }
}
