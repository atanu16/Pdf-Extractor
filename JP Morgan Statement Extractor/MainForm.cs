using JPMorganStatementExtractor.Services;

namespace JPMorganStatementExtractor;

public partial class MainForm : Form
{
    private CancellationTokenSource? _cancellationTokenSource;

    public MainForm()
    {
        InitializeComponent();
    }

    private void PdfBrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            Title = "Select JP Morgan Statement PDF"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            pdfPathTextBox.Text = dialog.FileName;

            if (string.IsNullOrWhiteSpace(outputCsvTextBox.Text))
            {
                var directory = Path.GetDirectoryName(dialog.FileName) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fileName = Path.GetFileNameWithoutExtension(dialog.FileName) + "_extract.csv";
                outputCsvTextBox.Text = Path.Combine(directory, fileName);
            }
        }
    }

    private void OutputCsvBrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Select Output CSV Path",
            FileName = "statement_extract.csv"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            outputCsvTextBox.Text = dialog.FileName;
        }
    }

    private void PasteFromClipboardButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (!Clipboard.ContainsText())
            {
                MessageBox.Show(this, "Clipboard does not contain text.", "Paste From Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var clipboardText = Clipboard.GetText();
            var accounts = AccountNumberParser.Parse(clipboardText);

            if (accounts.Count == 0)
            {
                MessageBox.Show(this, "No account numbers found in clipboard text.", "Paste From Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AccountListTextBox.Text = string.Join(Environment.NewLine, accounts);
            AppendLog($"Pasted {accounts.Count} unique account number(s) from clipboard.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to read clipboard: {ex.Message}", "Paste From Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void ProcessButton_Click(object? sender, EventArgs e)
    {
        var pdfPath = pdfPathTextBox.Text.Trim();
        var outputPath = outputCsvTextBox.Text.Trim();
        var accounts = AccountNumberParser.Parse(AccountListTextBox.Text);

        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            MessageBox.Show(this, "Please select a PDF file.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(pdfPath))
        {
            MessageBox.Show(this, "The selected PDF file does not exist.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (accounts.Count == 0)
        {
            MessageBox.Show(this, "Please enter at least one account number.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            MessageBox.Show(this, "Please select an output CSV path.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetProcessingState(isProcessing: true);
        logTextBox.Clear();
        progressBar.Value = 0;
        progressBar.Maximum = accounts.Count;
        UpdateProgressLabels(currentAccount: "(starting)", processed: 0, total: accounts.Count);

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        var progress = new Progress<string>(message =>
        {
            AppendLog(message);

            if (message.StartsWith("Extracting account ", StringComparison.Ordinal))
            {
                var openParen = message.IndexOf('(');
                var accountPart = openParen > "Extracting account ".Length
                    ? message["Extracting account ".Length..openParen].Trim()
                    : message["Extracting account ".Length..].Trim();

                var processed = 0;
                var slashIndex = message.IndexOf(" of ", StringComparison.Ordinal);
                if (slashIndex >= 0)
                {
                    var parenStart = message.LastIndexOf('(');
                    var processedText = message[(parenStart + 1)..slashIndex].Trim();
                    int.TryParse(processedText, out processed);
                }

                UpdateProgressLabels(accountPart, processed, accounts.Count);
                progressBar.Value = Math.Min(processed, progressBar.Maximum);
            }
        });

        try
        {
            AppendLog($"Starting extraction for {accounts.Count} account(s)...");

            var results = await Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                var pdfService = new PdfTextExtractionService();
                var pdfText = pdfService.ExtractAllText(pdfPath, progress, token);

                token.ThrowIfCancellationRequested();

                var engine = new StatementExtractionEngine(pdfText);
                ((IProgress<string>)progress).Report($"Indexed {engine.IndexedAccountCount} account section(s) in PDF.");

                return engine.ExtractAll(accounts, progress, token);
            }, token);

            token.ThrowIfCancellationRequested();

            CsvExportService.Export(results, outputPath);

            progressBar.Value = progressBar.Maximum;
            UpdateProgressLabels("(complete)", results.Count, accounts.Count);

            var foundCount = results.Count(r => r.FundToBePaid != StatementExtractionEngine.AccountNotFound || r.ConfirmedCashEntry != StatementExtractionEngine.AccountNotFound);
            AppendLog($"Export complete: {outputPath}");
            AppendLog($"Processed {results.Count} account(s). {foundCount} account section(s) matched in PDF.");

            MessageBox.Show(this, $"Extraction complete.\n\nOutput saved to:\n{outputPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            AppendLog("Processing cancelled by user.");
            UpdateProgressLabels("(cancelled)", progressBar.Value, accounts.Count);
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            SetProcessingState(isProcessing: false);
        }
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        if (_cancellationTokenSource is not null && !_cancellationTokenSource.IsCancellationRequested)
        {
            AppendLog("Cancellation requested...");
            _cancellationTokenSource.Cancel();
            cancelButton.Enabled = false;
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        pdfPathTextBox.Clear();
        AccountListTextBox.Clear();
        outputCsvTextBox.Clear();
        logTextBox.Clear();
        progressBar.Value = 0;
        UpdateProgressLabels("(none)", 0, 0);
    }

    private void SetProcessingState(bool isProcessing)
    {
        processButton.Enabled = !isProcessing;
        cancelButton.Enabled = isProcessing;
        clearButton.Enabled = !isProcessing;
        pdfBrowseButton.Enabled = !isProcessing;
        outputCsvBrowseButton.Enabled = !isProcessing;
        pasteFromClipboardButton.Enabled = !isProcessing;
        pdfPathTextBox.ReadOnly = isProcessing;
        AccountListTextBox.ReadOnly = isProcessing;
        outputCsvTextBox.ReadOnly = isProcessing;
    }

    private void UpdateProgressLabels(string currentAccount, int processed, int total)
    {
        currentAccountLabel.Text = $"Current: {currentAccount}";
        totalProcessedLabel.Text = $"Processed: {processed} / {total}";
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(AppendLog, message);
            return;
        }

        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
