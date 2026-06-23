using System.Text.RegularExpressions;

namespace JPMorganStatementExtractor.Services;

public static class AccountNumberParser
{
    private static readonly Regex TokenPattern = new(@"[A-Za-z0-9]+", RegexOptions.Compiled);

    public static IReadOnlyList<string> Parse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Array.Empty<string>();
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var accounts = new List<string>();

        foreach (Match match in TokenPattern.Matches(input))
        {
            var account = match.Value.ToUpperInvariant();
            if (seen.Add(account))
            {
                accounts.Add(account);
            }
        }

        return accounts;
    }
}
