using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JPMorganStatementExtractor.Models;

namespace JPMorganStatementExtractor.Services;

public sealed class StatementExtractionEngine
{
    public const string AccountNotFound = "ACCOUNT_NOT_FOUND";
    public const string ValueNotFound = "NOT_FOUND";

    private const int MaxRowContinuationLines = 8;

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

    private static readonly Regex PendingSectionEndPattern = new(
        @"(?:\*\*\s*US\s+SEG|OPENING\s+BALANCE|=+\s*EQUIVALENT\s+TOTAL\s*=+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Debit/Credit and Amount column values: 716,681.62DR or 91557.88CR
    /// </summary>
    private static readonly Regex DebitCreditColumnAmountPattern = new(
        @"(?<amount>\d{1,3}(?:,\d{3})*|\d+)\.(?<cents>\d{2})\s*(?<suffix>DR|CR)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PlainColumnAmountPattern = new(
        @"(?<amount>\d{1,3}(?:,\d{3})*|\d+)\.(?<cents>\d{2})(?!\d)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NextTableRowPattern = new(
        @"^\s*\d{1,2}\s+[A-Z]{3}\s+\d{2}\b",
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

    public IReadOnlyList<AccountExtractionResult> ExtractAll(
        IReadOnlyList<string> accountNumbers,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
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
        return ExtractDebitCreditColumnValue(subsection);
    }

    private static string ExtractFundToBePaid(ReadOnlySpan<char> section)
    {
        var subsection = GetSubsection(section, PendingCashSectionPattern, PendingSectionEndPattern);
        return ExtractAmountColumnValue(subsection);
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

    /// <summary>
    /// Extracts the DEBIT(DR)/CREDIT() column value from the "Funds to be Paid" row
    /// in the posted cash entries table.
    /// </summary>
    private static string ExtractDebitCreditColumnValue(ReadOnlySpan<char> subsection)
    {
        return ExtractColumnValueFromFundsToBePaidRow(subsection, requireDebitCreditSuffix: true);
    }

    /// <summary>
    /// Extracts the AMOUNT column value from the "Funds to be Paid" row
    /// in the pending cash entries table.
    /// </summary>
    private static string ExtractAmountColumnValue(ReadOnlySpan<char> subsection)
    {
        return ExtractColumnValueFromFundsToBePaidRow(subsection, requireDebitCreditSuffix: true);
    }

    private static string ExtractColumnValueFromFundsToBePaidRow(
        ReadOnlySpan<char> subsection,
        bool requireDebitCreditSuffix)
    {
        if (subsection.IsEmpty)
        {
            return ValueNotFound;
        }

        var lines = subsection.ToString().Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            if (!IsFundsToBePaidDescriptionLine(lines[i]))
            {
                continue;
            }

            var rowBlock = BuildRowBlock(lines, i);
            var amount = ExtractRightmostColumnAmount(rowBlock, requireDebitCreditSuffix);
            if (amount is not null)
            {
                return amount;
            }
        }

        return ValueNotFound;
    }

    private static bool IsFundsToBePaidDescriptionLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        if (line.Contains("PENDING CASH TOTAL", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (line.Contains("CASH & ADJUSTMENTS", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return line.Contains("Funds to be Paid", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRowBlock(string[] lines, int anchorIndex)
    {
        var builder = new StringBuilder();
        builder.Append(lines[anchorIndex].Trim());

        for (var i = anchorIndex + 1; i < lines.Length && i <= anchorIndex + MaxRowContinuationLines; i++)
        {
            var nextLine = lines[i].Trim();
            if (nextLine.Length == 0)
            {
                continue;
            }

            if (IsNewTableRow(nextLine) && !nextLine.Contains("Funds to be Paid", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (IsSectionHeader(nextLine))
            {
                break;
            }

            builder.Append(' ');
            builder.Append(nextLine);

            if (ContainsDebitCreditAmount(nextLine))
            {
                break;
            }
        }

        return builder.ToString();
    }

    private static bool IsNewTableRow(string line)
    {
        return NextTableRowPattern.IsMatch(line);
    }

    private static bool IsSectionHeader(string line)
    {
        return line.Contains("THE FOLLOWING", StringComparison.OrdinalIgnoreCase)
               || line.Contains("OPENING BALANCE", StringComparison.OrdinalIgnoreCase)
               || line.Contains("US SEG", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsDebitCreditAmount(string text)
    {
        return DebitCreditColumnAmountPattern.IsMatch(text);
    }

    /// <summary>
    /// Returns the rightmost amount in the row block — the AMOUNT or DEBIT(DR)/CREDIT() column.
    /// </summary>
    private static string? ExtractRightmostColumnAmount(string rowBlock, bool preferDebitCreditSuffix)
    {
        if (preferDebitCreditSuffix)
        {
            var debitCreditAmount = FindLastDebitCreditAmount(rowBlock);
            if (debitCreditAmount is not null)
            {
                return debitCreditAmount;
            }
        }

        return FindLastPlainAmountAfterDescription(rowBlock);
    }

    private static string? FindLastDebitCreditAmount(string rowBlock)
    {
        string? lastAmount = null;
        Match? lastMatch = null;

        foreach (Match match in DebitCreditColumnAmountPattern.Matches(rowBlock))
        {
            if (IsAfterFundsToBePaidDescription(rowBlock, match.Index))
            {
                lastMatch = match;
            }
        }

        if (lastMatch is not null)
        {
            lastAmount = NormalizeAmountFromMatch(lastMatch);
        }

        return lastAmount;
    }

    private static string? FindLastPlainAmountAfterDescription(string rowBlock)
    {
        var descriptionIndex = rowBlock.IndexOf("Funds to be Paid", StringComparison.OrdinalIgnoreCase);
        if (descriptionIndex < 0)
        {
            return null;
        }

        var searchText = rowBlock[(descriptionIndex + "Funds to be Paid".Length)..];
        string? lastAmount = null;

        foreach (Match match in PlainColumnAmountPattern.Matches(searchText))
        {
            lastAmount = NormalizeAmountFromMatch(match);
        }

        return lastAmount;
    }

    private static bool IsAfterFundsToBePaidDescription(string rowBlock, int matchIndex)
    {
        var descriptionIndex = rowBlock.IndexOf("Funds to be Paid", StringComparison.OrdinalIgnoreCase);
        return descriptionIndex >= 0 && matchIndex >= descriptionIndex;
    }

    private static string NormalizeAmountFromMatch(Match match)
    {
        var wholePart = match.Groups["amount"].Value.Replace(",", string.Empty);
        var cents = match.Groups["cents"].Value;
        var normalized = $"{wholePart}.{cents}";

        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        return normalized;
    }
}
