# JP Morgan Statement Extractor

A .NET 8 Windows Forms application that extracts **Fund To Be Paid** and **Confirmed Cash Entry** values from JP Morgan PDF statements using a user-provided list of account numbers.

## Features

- Extract data from large PDF statements (1000+ pages)
- Process 1000+ account numbers in a single run
- Read the PDF only once and index account sections in memory
- Parse account numbers from comma, newline, or mixed formats
- Paste account numbers directly from the clipboard
- Export results to CSV
- Async processing with cancel support and live progress reporting

## Requirements

- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for building from source)

## Project Structure

```
JP Morgan Statement Extractor/
├── JP Morgan Statement Extractor.sln
└── JP Morgan Statement Extractor/
    ├── Program.cs
    ├── MainForm.cs
    ├── MainForm.Designer.cs
    ├── Models/
    │   └── AccountExtractionResult.cs
    ├── Services/
    │   ├── AccountNumberParser.cs
    │   ├── PdfTextExtractionService.cs
    │   ├── StatementExtractionEngine.cs
    │   └── CsvExportService.cs
    └── Properties/PublishProfiles/
        └── FolderProfile.pubxml
```

## Build and Run

### Run from source

```powershell
cd "JP Morgan Statement Extractor"
dotnet run --project "JP Morgan Statement Extractor\JP Morgan Statement Extractor.csproj"
```

### Build Release

```powershell
dotnet build "JP Morgan Statement Extractor\JP Morgan Statement Extractor.csproj" -c Release
```

### Publish self-contained executable

```powershell
dotnet publish "JP Morgan Statement Extractor\JP Morgan Statement Extractor.csproj" -c Release -p:PublishProfile=FolderProfile
```

Published output:

```
JP Morgan Statement Extractor\bin\Release\net8.0-windows\win-x64\publish\JP Morgan Statement Extractor.exe
```

## Usage

1. Launch the application.
2. Select a JP Morgan PDF statement using **Browse** in the PDF File section.
3. Enter account numbers in the **Account Numbers** text box, or click **Paste From Clipboard**.
4. Choose an output CSV path.
5. Click **Process**.
6. Review progress in the log window and open the generated CSV when complete.

Use **Cancel** to stop processing, or **Clear** to reset all fields.

## Account Number Input Formats

The application accepts account numbers in any of these formats:

```
4B16C,4A70C,4D01C
```

```
4B16C
4A70C
4D01C
```

```
4B16C,
4A70C,
4D01C
```

```
4B16C
4A70C,4D01C
A267C
```

Parsing rules:

- Comma-separated values are supported
- Newline-separated values are supported
- Mixed comma and newline values are supported
- Duplicate account numbers are ignored
- Blank values are ignored
- All account numbers are converted to uppercase

## Extracted Values

For each account number, the application locates the account section in the PDF and extracts two values from the **Funds to be Paid** transaction row only.

### Confirmed Cash Entry (Debit/Credit column)

- **Section:** `THE FOLLOWING CASH ENTRIES HAVE BEEN POSTED TO YOUR ACCOUNT`
- **Row:** `Funds to be Paid` in the `DESCRIPTION` column
- **Column:** `DEBIT(DR)/CREDIT()` (rightmost column in the posted entries table)
- **Example:** `716,681.62DR` → `716681.62`

### Fund To Be Paid (Amount column)

- **Section:** `*THE FOLLOWING PENDING CASH ENTRIES HAVE BEEN MADE TO YOUR ACCOUNT`
- **Row:** `Funds to be Paid` in the `DESCRIPTION` column
- **Column:** `AMOUNT` (rightmost column in the pending entries table)
- **Example:** `1,072,021.72DR` → `1072021.72`

### Extraction accuracy

The engine uses targeted column extraction rather than generic number matching:

- Locates the **Funds to be Paid** row in each section
- Builds the full row block (including continuation lines from PDF text wrapping)
- Extracts the **rightmost amount with DR/CR suffix** after the description — matching the Debit/Credit or Amount column
- Ignores summary table rows such as `CASH & ADJUSTMENTS` and `*PENDING CASH TOTAL`
- Stops the pending section before the balance summary (`OPENING BALANCE`, `** US SEG EUR **`)

Amount values are normalized by removing commas and the `DR`/`CR` suffix.

## CSV Output

Output file columns:

```csv
AccountNumber,FundToBePaid,ConfirmedCashEntry
4B16C,1072021.72,716681.62
A267C,91557.88,NOT_FOUND
```
What changed
1. Column-specific extraction

Confirmed Cash Entry → DEBIT(DR)/CREDIT() column in the posted entries table
Fund To Be Paid → AMOUNT column in the pending entries table
2. Row-block parsing PDF text often wraps rows across multiple lines. The engine now:

Finds the Funds to be Paid description row
Collects continuation lines (reference codes like 7609299, 04MMD0LWKV)
Extracts the rightmost amount with DR/CR after the description — the actual column value
3. Summary table exclusion Values like 716,681.62DR in CASH & ADJUSTMENTS and 1,072,021.72DR in *PENDING CASH TOTAL are ignored. Only the transaction row amounts are used.

4. Strict amount matching Requires the format 716,681.62DR / 1,072,021.72DR (with optional DR/CR suffix) to avoid false matches from dates or reference IDs.
## Error Handling

| Condition | Output |
|---|---|
| Account section not found in PDF | `ACCOUNT_NOT_FOUND` |
| Account found but value missing | `NOT_FOUND` |

Processing continues for remaining accounts when an error occurs.

## Performance

- PDF text is extracted once using iText7
- All text is stored in memory
- Account sections are indexed by account number for fast lookup
- Full PDF scans are not repeated for each account

## Dependencies

- [.NET 8 Windows Forms](https://learn.microsoft.com/dotnet/desktop/winforms/)
- [iText7](https://www.nuget.org/packages/itext7) 8.0.5

## Tests

Unit tests verify extraction against the statement layout shown in the JP Morgan sample (account `4B16C`):

```powershell
dotnet test
```

Expected values for the sample account:

| Field | Raw PDF Value | Extracted |
|---|---|---|
| Confirmed Cash Entry | `716,681.62DR` | `716681.62` |
| Fund To Be Paid | `1,072,021.72DR` | `1072021.72` |

## License

Internal use. Verify compliance with JP Morgan document handling policies before processing production statements.
