using System.Globalization;
using System.Text.RegularExpressions;
using JPMorganStatementExtractor.Models;

namespace JPMorganStatementExtractor.Services;

public sealed class StatementExtractionEngine
{
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string ValueNotFound = "NOT_FOUND";

    private static readonly Regex AccountSectionMarker = new(
        @"ACCOUNT\s+NUMBER",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex AccountIdPattern = new(
        @"ACCOUNT\s+NUMBER\s*:\s*(?:[A-Z]\s+)?([A-Z0-9]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PostedCashSectionPattern = new(
        @"THE\s+FOLLOWING\s+CASH\s+ENTRIES\s+HAVE\s+BEEN\s+POSTED\s+TO\s+YOUR\s+ACCOUNT",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PendingCashSectionPattern = new(
        @"\*?\s*THE\s+FOLLOWING\s+PENDING\s+CASH\s+ENTRIES\s+HAVE\s+BEEN\s+MADE\s+TO\s+YOUR\s+ACCOUNT",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex AmountPattern = new(
        @"(\d{1,3}(?:,\d{3})*\.\d{2})(?:\s*(?:DR|CR))?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly Dictionary<string, ReadOnlyMemory<char>> _accountSections;

    public StatementExtractionEngine(string pdfText)
    {
        _accountSections = BuildAccountIndex(pdfText);
    }

    public int IndexedAccountCount => _accountSections.Count;

    public AccountExtractionResult Extract(string accountNumber)
    {
        var normalized = accountNumber.ToUpperInvariant();

        if (!_accountSections.TryGetValue(normalized, out var sectionMemory))
        {
            return new AccountExtractionResult
            {
                AccountNumber = normalized,
                FundToBePaid = AccountNotFound,
                ConfirmedCashEntry = AccountNotFound
            };
        }

        var section = sectionMemory.Span;

        return new AccountExtractionResult
        {
            AccountNumber = normalized,
            FundToBePaid = ExtractFundToBePaid(section),
            ConfirmedCashEntry = ExtractConfirmedCashEntry(section)
        };
    }

    public IReadOnlyList<AccountExtractionResult> ExtractAll(IReadOnlyList<string> accountNumbers, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var results = new List<AccountExtractionResult>(accountNumbers.Count);

        for (var i = 0; i < accountNumbers.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var account = accountNumbers[i];
            progress?.Report($"Extracting account {account} ({i + 1} of {accountNumbers.Count})...");
            results.Add(Extract(account));
        }

        return results;
    }

    private static Dictionary<string, ReadOnlyMemory<char>> BuildAccountIndex(string pdfText)
    {
        var index = new Dictionary<string, ReadOnlyMemory<char>>(StringComparer.OrdinalIgnoreCase);
        var matches = AccountSectionMarker.Matches(pdfText);

        for (var i = 0; i < matches.Count; i++)
        {
            var start = matches[i].Index;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : pdfText.Length;
            var sectionLength = end - start;

            var headerLength = Math.Min(200, sectionLength);
            var header = pdfText.AsSpan(start, headerLength);

            var idMatch = AccountIdPattern.Match(pdfText, start, headerLength);
            if (!idMatch.Success)
            {
                continue;
            }

            var accountId = idMatch.Groups[1].Value.ToUpperInvariant();
            if (!index.ContainsKey(accountId))
            {
                index[accountId] = pdfText.AsMemory(start, sectionLength);
            }
        }

        return index;
    }

    private static string ExtractConfirmedCashEntry(ReadOnlySpan<char> section)
    {
        var subsection = GetSubsection(section, PostedCashSectionPattern, PendingCashSectionPattern);
        return ExtractFundsToBePaidAmount(subsection);
    }

    private static string ExtractFundToBePaid(ReadOnlySpan<char> section)
    {
        var subsection = GetSubsection(section, PendingCashSectionPattern, null);
        return ExtractFundsToBePaidAmount(subsection);
    }

    private static ReadOnlySpan<char> GetSubsection(ReadOnlySpan<char> section, Regex startPattern, Regex? endPattern)
    {
        var sectionText = section.ToString();
        var startMatch = startPattern.Match(sectionText);
        if (!startMatch.Success)
        {
            return ReadOnlySpan<char>.Empty;
        }

        var startIndex = startMatch.Index + startMatch.Length;
        var endIndex = section.Length;

        if (endPattern is not null)
        {
            var endMatch = endPattern.Match(sectionText, startIndex);
            if (endMatch.Success)
            {
                endIndex = endMatch.Index;
            }
        }

        return section.Slice(startIndex, endIndex - startIndex);
    }

    private static string ExtractFundsToBePaidAmount(ReadOnlySpan<char> subsection)
    {
        if (subsection.IsEmpty)
        {
            return ValueNotFound;
        }

        var lines = subsection.ToString().Split('\n');

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (!line.Contains("Funds to be Paid", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var amount = FindLastAmount(line);
            if (amount is not null)
            {
                return amount;
            }
        }

        return ValueNotFound;
    }

    private static string? FindLastAmount(string line)
    {
        string? lastAmount = null;

        foreach (Match match in AmountPattern.Matches(line))
        {
            lastAmount = NormalizeAmount(match.Groups[1].Value);
        }

        return lastAmount;
    }

    private static string NormalizeAmount(string rawAmount)
    {
        var normalized = rawAmount.Replace(",", string.Empty);

        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return rawAmount.Replace(",", string.Empty);
    }
}
