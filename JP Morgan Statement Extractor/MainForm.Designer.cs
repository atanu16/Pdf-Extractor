#nullable disable
namespace JPMorganStatementExtractor;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pdfPathLabel = new Label();
        pdfPathTextBox = new TextBox();
        pdfBrowseButton = new Button();
        accountListLabel = new Label();
        AccountListTextBox = new TextBox();
        pasteFromClipboardButton = new Button();
        outputCsvLabel = new Label();
        outputCsvTextBox = new TextBox();
        outputCsvBrowseButton = new Button();
        processButton = new Button();
        cancelButton = new Button();
        clearButton = new Button();
        progressBar = new ProgressBar();
        currentAccountLabel = new Label();
        totalProcessedLabel = new Label();
        logLabel = new Label();
        logTextBox = new TextBox();
        pdfGroupBox = new GroupBox();
        accountGroupBox = new GroupBox();
        outputGroupBox = new GroupBox();
        progressGroupBox = new GroupBox();
        pdfGroupBox.SuspendLayout();
        accountGroupBox.SuspendLayout();
        outputGroupBox.SuspendLayout();
        progressGroupBox.SuspendLayout();
        SuspendLayout();
        //
        // pdfGroupBox
        //
        pdfGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pdfGroupBox.Controls.Add(pdfPathLabel);
        pdfGroupBox.Controls.Add(pdfPathTextBox);
        pdfGroupBox.Controls.Add(pdfBrowseButton);
        pdfGroupBox.Location = new Point(12, 12);
        pdfGroupBox.Name = "pdfGroupBox";
        pdfGroupBox.Size = new Size(860, 58);
        pdfGroupBox.TabIndex = 0;
        pdfGroupBox.TabStop = false;
        pdfGroupBox.Text = "PDF File";
        //
        // pdfPathLabel
        //
        pdfPathLabel.AutoSize = true;
        pdfPathLabel.Location = new Point(12, 26);
        pdfPathLabel.Name = "pdfPathLabel";
        pdfPathLabel.Size = new Size(55, 15);
        pdfPathLabel.TabIndex = 0;
        pdfPathLabel.Text = "PDF Path";
        //
        // pdfPathTextBox
        //
        pdfPathTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pdfPathTextBox.Location = new Point(80, 22);
        pdfPathTextBox.Name = "pdfPathTextBox";
        pdfPathTextBox.Size = new Size(685, 23);
        pdfPathTextBox.TabIndex = 1;
        //
        // pdfBrowseButton
        //
        pdfBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pdfBrowseButton.Location = new Point(771, 21);
        pdfBrowseButton.Name = "pdfBrowseButton";
        pdfBrowseButton.Size = new Size(75, 25);
        pdfBrowseButton.TabIndex = 2;
        pdfBrowseButton.Text = "Browse";
        pdfBrowseButton.UseVisualStyleBackColor = true;
        pdfBrowseButton.Click += PdfBrowseButton_Click;
        //
        // accountGroupBox
        //
        accountGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        accountGroupBox.Controls.Add(accountListLabel);
        accountGroupBox.Controls.Add(AccountListTextBox);
        accountGroupBox.Controls.Add(pasteFromClipboardButton);
        accountGroupBox.Location = new Point(12, 76);
        accountGroupBox.Name = "accountGroupBox";
        accountGroupBox.Size = new Size(860, 170);
        accountGroupBox.TabIndex = 1;
        accountGroupBox.TabStop = false;
        accountGroupBox.Text = "Account Numbers";
        //
        // accountListLabel
        //
        accountListLabel.AutoSize = true;
        accountListLabel.Location = new Point(12, 22);
        accountListLabel.Name = "accountListLabel";
        accountListLabel.Size = new Size(338, 15);
        accountListLabel.TabIndex = 0;
        accountListLabel.Text = "Paste account numbers (comma, newline, or mixed separated)";
        //
        // AccountListTextBox
        //
        AccountListTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        AccountListTextBox.Location = new Point(12, 42);
        AccountListTextBox.Multiline = true;
        AccountListTextBox.Name = "AccountListTextBox";
        AccountListTextBox.ScrollBars = ScrollBars.Vertical;
        AccountListTextBox.Size = new Size(754, 116);
        AccountListTextBox.TabIndex = 1;
        //
        // pasteFromClipboardButton
        //
        pasteFromClipboardButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        pasteFromClipboardButton.Location = new Point(772, 42);
        pasteFromClipboardButton.Name = "pasteFromClipboardButton";
        pasteFromClipboardButton.Size = new Size(75, 50);
        pasteFromClipboardButton.TabIndex = 2;
        pasteFromClipboardButton.Text = "Paste From Clipboard";
        pasteFromClipboardButton.UseVisualStyleBackColor = true;
        pasteFromClipboardButton.Click += PasteFromClipboardButton_Click;
        //
        // outputGroupBox
        //
        outputGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        outputGroupBox.Controls.Add(outputCsvLabel);
        outputGroupBox.Controls.Add(outputCsvTextBox);
        outputGroupBox.Controls.Add(outputCsvBrowseButton);
        outputGroupBox.Location = new Point(12, 252);
        outputGroupBox.Name = "outputGroupBox";
        outputGroupBox.Size = new Size(860, 58);
        outputGroupBox.TabIndex = 2;
        outputGroupBox.TabStop = false;
        outputGroupBox.Text = "Output CSV";
        //
        // outputCsvLabel
        //
        outputCsvLabel.AutoSize = true;
        outputCsvLabel.Location = new Point(12, 26);
        outputCsvLabel.Name = "outputCsvLabel";
        outputCsvLabel.Size = new Size(31, 15);
        outputCsvLabel.TabIndex = 0;
        outputCsvLabel.Text = "Path";
        //
        // outputCsvTextBox
        //
        outputCsvTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        outputCsvTextBox.Location = new Point(80, 22);
        outputCsvTextBox.Name = "outputCsvTextBox";
        outputCsvTextBox.Size = new Size(685, 23);
        outputCsvTextBox.TabIndex = 1;
        //
        // outputCsvBrowseButton
        //
        outputCsvBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        outputCsvBrowseButton.Location = new Point(771, 21);
        outputCsvBrowseButton.Name = "outputCsvBrowseButton";
        outputCsvBrowseButton.Size = new Size(75, 25);
        outputCsvBrowseButton.TabIndex = 2;
        outputCsvBrowseButton.Text = "Browse";
        outputCsvBrowseButton.UseVisualStyleBackColor = true;
        outputCsvBrowseButton.Click += OutputCsvBrowseButton_Click;
        //
        // processButton
        //
        processButton.Location = new Point(12, 322);
        processButton.Name = "processButton";
        processButton.Size = new Size(90, 30);
        processButton.TabIndex = 3;
        processButton.Text = "Process";
        processButton.UseVisualStyleBackColor = true;
        processButton.Click += ProcessButton_Click;
        //
        // cancelButton
        //
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(108, 322);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 30);
        cancelButton.TabIndex = 4;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += CancelButton_Click;
        //
        // clearButton
        //
        clearButton.Location = new Point(204, 322);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(90, 30);
        clearButton.TabIndex = 5;
        clearButton.Text = "Clear";
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Click += ClearButton_Click;
        //
        // progressGroupBox
        //
        progressGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressGroupBox.Controls.Add(progressBar);
        progressGroupBox.Controls.Add(currentAccountLabel);
        progressGroupBox.Controls.Add(totalProcessedLabel);
        progressGroupBox.Location = new Point(12, 358);
        progressGroupBox.Name = "progressGroupBox";
        progressGroupBox.Size = new Size(860, 82);
        progressGroupBox.TabIndex = 6;
        progressGroupBox.TabStop = false;
        progressGroupBox.Text = "Progress";
        //
        // progressBar
        //
        progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(12, 22);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(836, 23);
        progressBar.TabIndex = 0;
        //
        // currentAccountLabel
        //
        currentAccountLabel.AutoSize = true;
        currentAccountLabel.Location = new Point(12, 52);
        currentAccountLabel.Name = "currentAccountLabel";
        currentAccountLabel.Size = new Size(103, 15);
        currentAccountLabel.TabIndex = 1;
        currentAccountLabel.Text = "Current: (none)";
        //
        // totalProcessedLabel
        //
        totalProcessedLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        totalProcessedLabel.AutoSize = true;
        totalProcessedLabel.Location = new Point(720, 52);
        totalProcessedLabel.Name = "totalProcessedLabel";
        totalProcessedLabel.Size = new Size(128, 15);
        totalProcessedLabel.TabIndex = 2;
        totalProcessedLabel.Text = "Processed: 0 / 0";
        totalProcessedLabel.TextAlign = ContentAlignment.TopRight;
        //
        // logLabel
        //
        logLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        logLabel.AutoSize = true;
        logLabel.Location = new Point(12, 448);
        logLabel.Name = "logLabel";
        logLabel.Size = new Size(28, 15);
        logLabel.TabIndex = 7;
        logLabel.Text = "Log";
        //
        // logTextBox
        //
        logTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        logTextBox.Location = new Point(12, 466);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Size = new Size(860, 183);
        logTextBox.TabIndex = 8;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(884, 661);
        Controls.Add(logTextBox);
        Controls.Add(logLabel);
        Controls.Add(progressGroupBox);
        Controls.Add(clearButton);
        Controls.Add(cancelButton);
        Controls.Add(processButton);
        Controls.Add(outputGroupBox);
        Controls.Add(accountGroupBox);
        Controls.Add(pdfGroupBox);
        MinimumSize = new Size(900, 700);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "JP Morgan Statement Extractor";
        pdfGroupBox.ResumeLayout(false);
        pdfGroupBox.PerformLayout();
        accountGroupBox.ResumeLayout(false);
        accountGroupBox.PerformLayout();
        outputGroupBox.ResumeLayout(false);
        outputGroupBox.PerformLayout();
        progressGroupBox.ResumeLayout(false);
        progressGroupBox.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private GroupBox pdfGroupBox;
    private Label pdfPathLabel;
    private TextBox pdfPathTextBox;
    private Button pdfBrowseButton;
    private GroupBox accountGroupBox;
    private Label accountListLabel;
    private TextBox AccountListTextBox;
    private Button pasteFromClipboardButton;
    private GroupBox outputGroupBox;
    private Label outputCsvLabel;
    private TextBox outputCsvTextBox;
    private Button outputCsvBrowseButton;
    private Button processButton;
    private Button cancelButton;
    private Button clearButton;
    private GroupBox progressGroupBox;
    private ProgressBar progressBar;
    private Label currentAccountLabel;
    private Label totalProcessedLabel;
    private Label logLabel;
    private TextBox logTextBox;
}
