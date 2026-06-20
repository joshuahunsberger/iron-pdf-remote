namespace Worker;

public class PdfGenerationWorker(ILogger<PdfGenerationWorker> logger, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var renderer = new ChromePdfRenderer();
        var pdf = renderer.RenderHtmlAsPdf("<h1>Hello World!</h1>");
        logger.LogInformation("PDF rendered");
        pdf.SaveAs("ironpdf.pdf");
        logger.LogInformation("PDF saved");
        
        hostApplicationLifetime.StopApplication();

        return Task.CompletedTask;
    }
}
