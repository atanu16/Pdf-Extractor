using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace JPMorganStatementExtractor.Services;

public sealed class PdfTextExtractionService
{
    public string ExtractAllText(string pdfPath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        progress?.Report("Opening PDF file...");

        using var reader = new PdfReader(pdfPath);
        using var document = new PdfDocument(reader);

        var pageCount = document.GetNumberOfPages();
        var builder = new System.Text.StringBuilder(pageCount * 4096);

        for (var pageNumber = 1; pageNumber <= pageCount; pageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (pageNumber == 1 || pageNumber % 50 == 0 || pageNumber == pageCount)
            {
                progress?.Report($"Reading PDF page {pageNumber} of {pageCount}...");
            }

            var page = document.GetPage(pageNumber);
            var strategy = new LocationTextExtractionStrategy();
            var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

            builder.Append(pageText);
            builder.Append('\n');
        }

        progress?.Report($"PDF read complete ({pageCount} pages).");
        return builder.ToString();
    }
}
