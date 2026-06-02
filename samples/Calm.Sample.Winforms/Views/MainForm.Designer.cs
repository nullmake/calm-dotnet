using Calm.Sample.Winforms.ViewModels;
using Microsoft.Extensions.Logging;

namespace Calm.Sample.Winforms.Views;

/// <summary>
/// The main form of the sample application.
/// </summary>
partial class MainForm
{
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _logger.LogInformation("Disposing '{Class}' instance.", nameof(MainForm));
            if (disposing)
            {
                dgvRecompressStatus.DataSource = null;
                _progressStatusSource.Dispose();
            }
            _disposed = true;
        }

        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        btnFolderSelect = new Button();
        txtFolderPath = new TextBox();
        tpnlGrid = new TableLayoutPanel();
        lblObjective = new Label();
        tpnlFolderSelect = new TableLayoutPanel();
        lblStep2 = new Label();
        chkRecursive = new CheckBox();
        lblFolderPathError = new Label();
        tpnlGenerate = new TableLayoutPanel();
        btnGenerate = new Button();
        lblStep1 = new Label();
        comboTestDataSize = new ComboBox();
        lblTestDataCount = new Label();
        comboTestDataCount = new ComboBox();
        linkGenerateFolder = new LinkLabel();
        tpnlStart = new TableLayoutPanel();
        lblStep3 = new Label();
        btnStart = new Button();
        dgvRecompressStatus = new DataGridView();
        statusStrip1 = new StatusStrip();
        tsslSystemResource = new ToolStripStatusLabel();
        tsslItemCount = new ToolStripStatusLabel();
        menuStrip1 = new MenuStrip();
        tsmiFile = new ToolStripMenuItem();
        tsmiExit = new ToolStripMenuItem();
        tsmlTool = new ToolStripMenuItem();
        tsmlOpenLogDir = new ToolStripMenuItem();
        tsmlHelp = new ToolStripMenuItem();
        tsmlAbout = new ToolStripMenuItem();
        tpnlGrid.SuspendLayout();
        tpnlFolderSelect.SuspendLayout();
        tpnlGenerate.SuspendLayout();
        tpnlStart.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRecompressStatus).BeginInit();
        statusStrip1.SuspendLayout();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // btnFolderSelect
        // 
        btnFolderSelect.Location = new Point(392, 3);
        btnFolderSelect.Name = "btnFolderSelect";
        btnFolderSelect.Size = new Size(26, 23);
        btnFolderSelect.TabIndex = 0;
        btnFolderSelect.Text = "...";
        btnFolderSelect.UseVisualStyleBackColor = true;
        // 
        // txtFolderPath
        // 
        txtFolderPath.AllowDrop = true;
        txtFolderPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtFolderPath.Location = new Point(45, 5);
        txtFolderPath.Margin = new Padding(3, 5, 3, 3);
        txtFolderPath.Name = "txtFolderPath";
        txtFolderPath.Size = new Size(341, 23);
        txtFolderPath.TabIndex = 1;
        // 
        // tpnlGrid
        // 
        tpnlGrid.ColumnCount = 12;
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333333F));
        tpnlGrid.Controls.Add(lblObjective, 0, 0);
        tpnlGrid.Controls.Add(tpnlFolderSelect, 0, 2);
        tpnlGrid.Controls.Add(tpnlGenerate, 0, 1);
        tpnlGrid.Controls.Add(tpnlStart, 0, 3);
        tpnlGrid.Controls.Add(dgvRecompressStatus, 0, 4);
        tpnlGrid.Controls.Add(statusStrip1, 0, 5);
        tpnlGrid.Dock = DockStyle.Fill;
        tpnlGrid.Location = new Point(0, 24);
        tpnlGrid.Name = "tpnlGrid";
        tpnlGrid.RowCount = 6;
        tpnlGrid.RowStyles.Add(new RowStyle());
        tpnlGrid.RowStyles.Add(new RowStyle());
        tpnlGrid.RowStyles.Add(new RowStyle());
        tpnlGrid.RowStyles.Add(new RowStyle());
        tpnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tpnlGrid.RowStyles.Add(new RowStyle());
        tpnlGrid.Size = new Size(509, 283);
        tpnlGrid.TabIndex = 1;
        // 
        // lblObjective
        // 
        lblObjective.AutoEllipsis = true;
        lblObjective.AutoSize = true;
        lblObjective.BorderStyle = BorderStyle.FixedSingle;
        tpnlGrid.SetColumnSpan(lblObjective, 12);
        lblObjective.Dock = DockStyle.Top;
        lblObjective.Location = new Point(3, 3);
        lblObjective.Margin = new Padding(3);
        lblObjective.Name = "lblObjective";
        lblObjective.Size = new Size(503, 62);
        lblObjective.TabIndex = 2;
        lblObjective.Text = resources.GetString("lblObjective.Text");
        // 
        // tpnlFolderSelect
        // 
        tpnlFolderSelect.AutoSize = true;
        tpnlFolderSelect.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tpnlFolderSelect.ColumnCount = 4;
        tpnlGrid.SetColumnSpan(tpnlFolderSelect, 12);
        tpnlFolderSelect.ColumnStyles.Add(new ColumnStyle());
        tpnlFolderSelect.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tpnlFolderSelect.ColumnStyles.Add(new ColumnStyle());
        tpnlFolderSelect.ColumnStyles.Add(new ColumnStyle());
        tpnlFolderSelect.Controls.Add(btnFolderSelect, 2, 0);
        tpnlFolderSelect.Controls.Add(txtFolderPath, 1, 0);
        tpnlFolderSelect.Controls.Add(lblStep2, 0, 0);
        tpnlFolderSelect.Controls.Add(chkRecursive, 3, 0);
        tpnlFolderSelect.Controls.Add(lblFolderPathError, 1, 1);
        tpnlFolderSelect.Dock = DockStyle.Fill;
        tpnlFolderSelect.Location = new Point(3, 108);
        tpnlFolderSelect.Name = "tpnlFolderSelect";
        tpnlFolderSelect.RowCount = 2;
        tpnlFolderSelect.RowStyles.Add(new RowStyle());
        tpnlFolderSelect.RowStyles.Add(new RowStyle());
        tpnlFolderSelect.Size = new Size(503, 46);
        tpnlFolderSelect.TabIndex = 7;
        // 
        // lblStep2
        // 
        lblStep2.Anchor = AnchorStyles.None;
        lblStep2.AutoSize = true;
        lblStep2.Location = new Point(3, 8);
        lblStep2.Name = "lblStep2";
        lblStep2.Size = new Size(36, 15);
        lblStep2.TabIndex = 0;
        lblStep2.Text = "Step2";
        // 
        // chkRecursive
        // 
        chkRecursive.Anchor = AnchorStyles.None;
        chkRecursive.AutoSize = true;
        chkRecursive.Checked = true;
        chkRecursive.CheckState = CheckState.Checked;
        chkRecursive.Location = new Point(424, 6);
        chkRecursive.Name = "chkRecursive";
        chkRecursive.Size = new Size(76, 19);
        chkRecursive.TabIndex = 2;
        chkRecursive.Text = "Recursive";
        chkRecursive.UseVisualStyleBackColor = true;
        // 
        // lblFolderPathError
        // 
        lblFolderPathError.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblFolderPathError.AutoSize = true;
        lblFolderPathError.ForeColor = Color.Brown;
        lblFolderPathError.Location = new Point(45, 31);
        lblFolderPathError.Name = "lblFolderPathError";
        lblFolderPathError.Size = new Size(341, 15);
        lblFolderPathError.TabIndex = 3;
        lblFolderPathError.Text = "lblFolderPathError";
        // 
        // tpnlGenerate
        // 
        tpnlGenerate.AutoSize = true;
        tpnlGenerate.ColumnCount = 6;
        tpnlGrid.SetColumnSpan(tpnlGenerate, 12);
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle());
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle());
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle());
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle());
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle());
        tpnlGenerate.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tpnlGenerate.Controls.Add(btnGenerate, 4, 0);
        tpnlGenerate.Controls.Add(lblStep1, 0, 0);
        tpnlGenerate.Controls.Add(comboTestDataSize, 1, 0);
        tpnlGenerate.Controls.Add(lblTestDataCount, 2, 0);
        tpnlGenerate.Controls.Add(comboTestDataCount, 3, 0);
        tpnlGenerate.Controls.Add(linkGenerateFolder, 5, 0);
        tpnlGenerate.Dock = DockStyle.Fill;
        tpnlGenerate.Location = new Point(3, 71);
        tpnlGenerate.Name = "tpnlGenerate";
        tpnlGenerate.RowCount = 1;
        tpnlGenerate.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tpnlGenerate.Size = new Size(503, 31);
        tpnlGenerate.TabIndex = 8;
        // 
        // btnGenerate
        // 
        btnGenerate.Anchor = AnchorStyles.None;
        btnGenerate.AutoSize = true;
        btnGenerate.Location = new Point(188, 3);
        btnGenerate.Name = "btnGenerate";
        btnGenerate.Size = new Size(114, 25);
        btnGenerate.TabIndex = 3;
        btnGenerate.Text = "Generate Test Data";
        btnGenerate.UseVisualStyleBackColor = true;
        // 
        // lblStep1
        // 
        lblStep1.Anchor = AnchorStyles.None;
        lblStep1.AutoSize = true;
        lblStep1.Location = new Point(3, 8);
        lblStep1.Name = "lblStep1";
        lblStep1.Size = new Size(36, 15);
        lblStep1.TabIndex = 4;
        lblStep1.Text = "Step1";
        lblStep1.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // comboTestDataSize
        // 
        comboTestDataSize.Anchor = AnchorStyles.None;
        comboTestDataSize.DropDownStyle = ComboBoxStyle.DropDownList;
        comboTestDataSize.FormattingEnabled = true;
        comboTestDataSize.Location = new Point(45, 4);
        comboTestDataSize.Name = "comboTestDataSize";
        comboTestDataSize.Size = new Size(70, 23);
        comboTestDataSize.TabIndex = 6;
        // 
        // lblTestDataCount
        // 
        lblTestDataCount.Anchor = AnchorStyles.None;
        lblTestDataCount.AutoSize = true;
        lblTestDataCount.Location = new Point(118, 8);
        lblTestDataCount.Margin = new Padding(0);
        lblTestDataCount.Name = "lblTestDataCount";
        lblTestDataCount.Size = new Size(13, 15);
        lblTestDataCount.TabIndex = 7;
        lblTestDataCount.Text = "x";
        // 
        // comboTestDataCount
        // 
        comboTestDataCount.DropDownStyle = ComboBoxStyle.DropDownList;
        comboTestDataCount.FormattingEnabled = true;
        comboTestDataCount.Location = new Point(134, 3);
        comboTestDataCount.Name = "comboTestDataCount";
        comboTestDataCount.Size = new Size(48, 23);
        comboTestDataCount.TabIndex = 8;
        // 
        // linkGenerateFolder
        // 
        linkGenerateFolder.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        linkGenerateFolder.AutoEllipsis = true;
        linkGenerateFolder.AutoSize = true;
        linkGenerateFolder.Location = new Point(308, 8);
        linkGenerateFolder.Name = "linkGenerateFolder";
        linkGenerateFolder.Size = new Size(192, 15);
        linkGenerateFolder.TabIndex = 9;
        linkGenerateFolder.TabStop = true;
        linkGenerateFolder.Text = "linkGenerateFolder";
        // 
        // tpnlStart
        // 
        tpnlStart.AutoSize = true;
        tpnlStart.ColumnCount = 2;
        tpnlGrid.SetColumnSpan(tpnlStart, 12);
        tpnlStart.ColumnStyles.Add(new ColumnStyle());
        tpnlStart.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tpnlStart.Controls.Add(lblStep3, 0, 0);
        tpnlStart.Controls.Add(btnStart, 1, 0);
        tpnlStart.Dock = DockStyle.Fill;
        tpnlStart.Location = new Point(3, 160);
        tpnlStart.Name = "tpnlStart";
        tpnlStart.RowCount = 1;
        tpnlStart.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tpnlStart.Size = new Size(503, 31);
        tpnlStart.TabIndex = 9;
        // 
        // lblStep3
        // 
        lblStep3.Anchor = AnchorStyles.None;
        lblStep3.AutoSize = true;
        lblStep3.Location = new Point(3, 8);
        lblStep3.Name = "lblStep3";
        lblStep3.Size = new Size(36, 15);
        lblStep3.TabIndex = 0;
        lblStep3.Text = "Step3";
        // 
        // btnStart
        // 
        btnStart.AutoSize = true;
        btnStart.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        btnStart.Location = new Point(45, 3);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(41, 25);
        btnStart.TabIndex = 1;
        btnStart.Text = "Start";
        btnStart.UseVisualStyleBackColor = true;
        // 
        // dgvRecompressStatus
        // 
        dgvRecompressStatus.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        tpnlGrid.SetColumnSpan(dgvRecompressStatus, 12);
        dgvRecompressStatus.Dock = DockStyle.Fill;
        dgvRecompressStatus.Location = new Point(3, 197);
        dgvRecompressStatus.Name = "dgvRecompressStatus";
        dgvRecompressStatus.ReadOnly = true;
        dgvRecompressStatus.Size = new Size(503, 61);
        dgvRecompressStatus.TabIndex = 10;
        // 
        // statusStrip1
        // 
        statusStrip1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tpnlGrid.SetColumnSpan(statusStrip1, 12);
        statusStrip1.Dock = DockStyle.None;
        statusStrip1.Items.AddRange(new ToolStripItem[] { tsslSystemResource, tsslItemCount });
        statusStrip1.Location = new Point(0, 261);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(509, 22);
        statusStrip1.TabIndex = 11;
        statusStrip1.Text = "statusStrip1";
        // 
        // tsslSystemResource
        // 
        tsslSystemResource.Name = "tsslSystemResource";
        tsslSystemResource.Size = new Size(387, 17);
        tsslSystemResource.Spring = true;
        tsslSystemResource.Text = "toolStripStatusLabel1";
        tsslSystemResource.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tsslItemCount
        // 
        tsslItemCount.Name = "tsslItemCount";
        tsslItemCount.Size = new Size(107, 17);
        tsslItemCount.Text = "toolStripItemCount";
        tsslItemCount.TextAlign = ContentAlignment.MiddleRight;
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { tsmiFile, tsmlTool, tsmlHelp });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(509, 24);
        menuStrip1.TabIndex = 2;
        menuStrip1.Text = "menuStrip1";
        // 
        // tsmiFile
        // 
        tsmiFile.DropDownItems.AddRange(new ToolStripItem[] { tsmiExit });
        tsmiFile.Name = "tsmiFile";
        tsmiFile.Size = new Size(37, 20);
        tsmiFile.Text = "File";
        // 
        // tsmiExit
        // 
        tsmiExit.Name = "tsmiExit";
        tsmiExit.Size = new Size(180, 22);
        tsmiExit.Text = "Exit";
        // 
        // tsmlTool
        // 
        tsmlTool.DropDownItems.AddRange(new ToolStripItem[] { tsmlOpenLogDir });
        tsmlTool.Name = "tsmlTool";
        tsmlTool.Size = new Size(41, 20);
        tsmlTool.Text = "Tool";
        // 
        // tsmlOpenLogDir
        // 
        tsmlOpenLogDir.Name = "tsmlOpenLogDir";
        tsmlOpenLogDir.Size = new Size(180, 22);
        tsmlOpenLogDir.Text = "Open Log Folder";
        // 
        // tsmlHelp
        // 
        tsmlHelp.DropDownItems.AddRange(new ToolStripItem[] { tsmlAbout });
        tsmlHelp.Name = "tsmlHelp";
        tsmlHelp.Size = new Size(44, 20);
        tsmlHelp.Text = "Help";
        // 
        // tsmlAbout
        // 
        tsmlAbout.Name = "tsmlAbout";
        tsmlAbout.Size = new Size(180, 22);
        tsmlAbout.Text = "About";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(509, 307);
        Controls.Add(tpnlGrid);
        Controls.Add(menuStrip1);
        MainMenuStrip = menuStrip1;
        Name = "MainForm";
        Text = "Calm Sample Application";
        tpnlGrid.ResumeLayout(false);
        tpnlGrid.PerformLayout();
        tpnlFolderSelect.ResumeLayout(false);
        tpnlFolderSelect.PerformLayout();
        tpnlGenerate.ResumeLayout(false);
        tpnlGenerate.PerformLayout();
        tpnlStart.ResumeLayout(false);
        tpnlStart.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRecompressStatus).EndInit();
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Button btnFolderSelect;
    private TextBox txtFolderPath;
    private TableLayoutPanel tpnlGrid;
    private Label lblObjective;
    private Button btnGenerate;
    private Label lblStep1;
    private Label lblStep2;
    private TableLayoutPanel tpnlFolderSelect;
    private TableLayoutPanel tpnlGenerate;
    private CheckBox chkRecursive;
    private TableLayoutPanel tpnlStart;
    private Label lblStep3;
    private Button btnStart;
    private DataGridView dgvRecompressStatus;
    private ComboBox comboTestDataSize;
    private Label lblTestDataCount;
    private ComboBox comboTestDataCount;
    private LinkLabel linkGenerateFolder;
    private StatusStrip statusStrip1;
    private Label lblFolderPathError;
    private ToolStripStatusLabel tsslItemCount;
    private ToolStripStatusLabel tsslSystemResource;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem tsmiFile;
    private ToolStripMenuItem tsmiExit;
    private ToolStripMenuItem tsmlTool;
    private ToolStripMenuItem tsmlOpenLogDir;
    private ToolStripMenuItem tsmlHelp;
    private ToolStripMenuItem tsmlAbout;
}
