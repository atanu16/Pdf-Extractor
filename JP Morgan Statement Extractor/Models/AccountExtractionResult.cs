namespace JPMorganStatementExtractor.Models;

public sealed class AccountExtractionResult
{
    public required string AccountNumber { get; init; }

    public required string FundToBePaid { get; init; }

    public required string ConfirmedCashEntry { get; init; }
}
