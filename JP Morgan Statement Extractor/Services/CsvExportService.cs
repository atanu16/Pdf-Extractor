using System.Text;
using JPMorganStatementExtractor.Models;

namespace JPMorganStatementExtractor.Services;

public static class CsvExportService
{
    public static void Export(IReadOnlyList<AccountExtractionResult> results, string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(outputPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        writer.WriteLine("AccountNumber,FundToBePaid,ConfirmedCashEntry");

        foreach (var result in results)
        {
            writer.WriteLine(string.Join(",",
                EscapeCsv(result.AccountNumber),
                EscapeCsv(result.FundToBePaid),
                EscapeCsv(result.ConfirmedCashEntry)));
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
