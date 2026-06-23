using JPMorganStatementExtractor.Services;
using Xunit;

namespace JPMorganStatementExtractor.Tests;

public class StatementExtractionEngineTests
{
    private const string SampleStatement4B16C = """
        J.P.Morgan
        ACCOUNT NUMBER: M      4B16C
        STATEMENT DATE: 5 MAR 2026
        CASH ACCOUNT CONFIRMATION

        THE FOLLOWING CASH ENTRIES HAVE BEEN POSTED TO YOUR ACCOUNT.
        T/DATE   V/DATE   AT   DESCRIPTION   CC   DEBIT(DR)/CREDIT( )
        05 MAR 26 05 MAR 26 EU Funds to be Paid 7609299 00207 AM9999945A0001 EU 716,681.62DR

        *THE FOLLOWING PENDING CASH ENTRIES HAVE BEEN MADE TO YOUR ACCOUNT.
        T/DATE   V/DATE   AT-CUR   PAYMENT(CP)/RECEIPT(CR)   DESCRIPTION   CUSIP   CUR   AMOUNT
        05 MAR 26 05 MAR 26 EUR CP Funds to be Paid 04MMD0LWKV EUR 1,072,021.72DR

        ** US SEG EUR **   * 30.7 SECURED EUR *   = EQUIVALENT TOTAL =
        OPENING BALANCE
        CASH & ADJUSTMENTS   716,681.62DR
        *PENDING CASH TOTAL   1,072,021.72DR
        """;

    private const string MultiLineRowStatement = """
        ACCOUNT NUMBER: M 4A70C
        THE FOLLOWING CASH ENTRIES HAVE BEEN POSTED TO YOUR ACCOUNT.
        05 MAR 26 05 MAR 26 EU Funds to be Paid
        7609299 00207 AM9999945A0001
        EU 716,681.62DR
        *THE FOLLOWING PENDING CASH ENTRIES HAVE BEEN MADE TO YOUR ACCOUNT.
        05 MAR 26 05 MAR 26 EUR CP Funds to be Paid
        04MMD0LWKV
        EUR 1,072,021.72DR
        ** US SEG EUR **
        """;

    private const string PartialValuesStatement = """
        ACCOUNT NUMBER: A267C
        THE FOLLOWING CASH ENTRIES HAVE BEEN POSTED TO YOUR ACCOUNT.
        05 MAR 26 05 MAR 26 EU Some Other Entry 100.00DR
        *THE FOLLOWING PENDING CASH ENTRIES HAVE BEEN MADE TO YOUR ACCOUNT.
        05 MAR 26 05 MAR 26 EUR CP Funds to be Paid 04MMD0LWKV EUR 91557.88DR
        ** US SEG EUR **
        """;

    [Fact]
    public void Extract_4B16C_ReturnsExactDebitCreditAndAmountColumnValues()
    {
        var engine = new StatementExtractionEngine(SampleStatement4B16C);
        var result = engine.Extract("4B16C");

        Assert.Equal("4B16C", result.AccountNumber);
        Assert.Equal("1072021.72", result.FundToBePaid);
        Assert.Equal("716681.62", result.ConfirmedCashEntry);
    }

    [Fact]
    public void Extract_MultiLineRows_ReturnsExactColumnValues()
    {
        var engine = new StatementExtractionEngine(MultiLineRowStatement);
        var result = engine.Extract("4A70C");

        Assert.Equal("1072021.72", result.FundToBePaid);
        Assert.Equal("716681.62", result.ConfirmedCashEntry);
    }

    [Fact]
    public void Extract_DoesNotConfuseSummaryTableWithTransactionRows()
    {
        var engine = new StatementExtractionEngine(SampleStatement4B16C);
        var result = engine.Extract("4B16C");

        Assert.NotEqual("716681.62", result.FundToBePaid);
        Assert.Equal("1072021.72", result.FundToBePaid);
    }

    [Fact]
    public void Extract_A267C_ReturnsPendingOnlyWhenPostedMissing()
    {
        var engine = new StatementExtractionEngine(PartialValuesStatement);
        var result = engine.Extract("A267C");

        Assert.Equal("91557.88", result.FundToBePaid);
        Assert.Equal(StatementExtractionEngine.ValueNotFound, result.ConfirmedCashEntry);
    }

    [Fact]
    public void Extract_UnknownAccount_ReturnsAccountNotFound()
    {
        var engine = new StatementExtractionEngine(SampleStatement4B16C);
        var result = engine.Extract("ZZZZZ");

        Assert.Equal(StatementExtractionEngine.AccountNotFound, result.FundToBePaid);
        Assert.Equal(StatementExtractionEngine.AccountNotFound, result.ConfirmedCashEntry);
    }

    [Fact]
    public void Extract_CaseInsensitiveAccountMatch()
    {
        var engine = new StatementExtractionEngine(SampleStatement4B16C);
        var result = engine.Extract("4b16c");

        Assert.Equal("1072021.72", result.FundToBePaid);
        Assert.Equal("716681.62", result.ConfirmedCashEntry);
    }
}
