namespace Calm.Sample.Winforms.Views;

partial class AboutForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        tableGridLayout = new TableLayoutPanel();
        btnOK = new Button();
        lblAppName = new Label();
        lblVersionTitle = new Label();
        lblLisenceTitle = new Label();
        lblThirdPartyNoticesTitle = new Label();
        lblHomeTitle = new Label();
        linkHome = new LinkLabel();
        linkThirdPartyNotices = new LinkLabel();
        lblVersion = new Label();
        lblCopyright = new Label();
        lblCopyrightTitle = new Label();
        lblBuildTitle = new Label();
        lblBuild = new Label();
        txtLicense = new TextBox();
        tableGridLayout.SuspendLayout();
        SuspendLayout();
        // 
        // tableGridLayout
        // 
        tableGridLayout.AutoSize = true;
        tableGridLayout.ColumnCount = 12;
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8.333332F));
        tableGridLayout.Controls.Add(btnOK, 10, 9);
        tableGridLayout.Controls.Add(lblAppName, 0, 0);
        tableGridLayout.Controls.Add(lblVersionTitle, 0, 2);
        tableGridLayout.Controls.Add(lblThirdPartyNoticesTitle, 0, 5);
        tableGridLayout.Controls.Add(lblHomeTitle, 0, 6);
        tableGridLayout.Controls.Add(linkHome, 3, 6);
        tableGridLayout.Controls.Add(linkThirdPartyNotices, 3, 5);
        tableGridLayout.Controls.Add(lblVersion, 3, 2);
        tableGridLayout.Controls.Add(lblLisenceTitle, 0, 7);
        tableGridLayout.Controls.Add(lblCopyrightTitle, 0, 4);
        tableGridLayout.Controls.Add(lblBuildTitle, 0, 3);
        tableGridLayout.Controls.Add(lblCopyright, 3, 4);
        tableGridLayout.Controls.Add(lblBuild, 3, 3);
        tableGridLayout.Controls.Add(txtLicense, 0, 8);
        tableGridLayout.Dock = DockStyle.Fill;
        tableGridLayout.Location = new Point(8, 8);
        tableGridLayout.Name = "tableGridLayout";
        tableGridLayout.RowCount = 10;
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F));
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableGridLayout.RowStyles.Add(new RowStyle());
        tableGridLayout.Size = new Size(534, 384);
        tableGridLayout.TabIndex = 0;
        // 
        // btnOK
        // 
        btnOK.Anchor = AnchorStyles.None;
        tableGridLayout.SetColumnSpan(btnOK, 2);
        btnOK.Location = new Point(449, 358);
        btnOK.Name = "btnOK";
        btnOK.Size = new Size(75, 23);
        btnOK.TabIndex = 0;
        btnOK.Text = "OK";
        btnOK.UseVisualStyleBackColor = true;
        // 
        // lblAppName
        // 
        lblAppName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblAppName.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblAppName, 12);
        lblAppName.Font = new Font("Yu Gothic UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 128);
        lblAppName.Location = new Point(3, 0);
        lblAppName.Name = "lblAppName";
        lblAppName.Size = new Size(528, 25);
        lblAppName.TabIndex = 1;
        lblAppName.Text = "lblAppName";
        // 
        // lblVersionTitle
        // 
        lblVersionTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblVersionTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblVersionTitle, 3);
        lblVersionTitle.Location = new Point(3, 38);
        lblVersionTitle.Margin = new Padding(3);
        lblVersionTitle.Name = "lblVersionTitle";
        lblVersionTitle.Size = new Size(126, 15);
        lblVersionTitle.TabIndex = 2;
        lblVersionTitle.Text = "Version:";
        // 
        // lblLisenceTitle
        // 
        lblLisenceTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblLisenceTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblLisenceTitle, 3);
        lblLisenceTitle.Location = new Point(3, 143);
        lblLisenceTitle.Margin = new Padding(3);
        lblLisenceTitle.Name = "lblLisenceTitle";
        lblLisenceTitle.Size = new Size(126, 15);
        lblLisenceTitle.TabIndex = 4;
        lblLisenceTitle.Text = "License:";
        // 
        // lblThirdPartyNoticesTitle
        // 
        lblThirdPartyNoticesTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblThirdPartyNoticesTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblThirdPartyNoticesTitle, 3);
        lblThirdPartyNoticesTitle.Location = new Point(3, 101);
        lblThirdPartyNoticesTitle.Margin = new Padding(3);
        lblThirdPartyNoticesTitle.Name = "lblThirdPartyNoticesTitle";
        lblThirdPartyNoticesTitle.Size = new Size(126, 15);
        lblThirdPartyNoticesTitle.TabIndex = 5;
        lblThirdPartyNoticesTitle.Text = "Third-Party Notices:";
        // 
        // lblHomeTitle
        // 
        lblHomeTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblHomeTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblHomeTitle, 3);
        lblHomeTitle.Location = new Point(3, 122);
        lblHomeTitle.Margin = new Padding(3);
        lblHomeTitle.Name = "lblHomeTitle";
        lblHomeTitle.Size = new Size(126, 15);
        lblHomeTitle.TabIndex = 6;
        lblHomeTitle.Text = "Home:";
        // 
        // linkHome
        // 
        linkHome.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        linkHome.AutoEllipsis = true;
        linkHome.AutoSize = true;
        tableGridLayout.SetColumnSpan(linkHome, 9);
        linkHome.Location = new Point(135, 122);
        linkHome.Name = "linkHome";
        linkHome.Size = new Size(396, 15);
        linkHome.TabIndex = 7;
        linkHome.TabStop = true;
        linkHome.Text = "linkHome";
        // 
        // linkThirdPartyNotices
        // 
        linkThirdPartyNotices.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        linkThirdPartyNotices.AutoEllipsis = true;
        linkThirdPartyNotices.AutoSize = true;
        tableGridLayout.SetColumnSpan(linkThirdPartyNotices, 9);
        linkThirdPartyNotices.Location = new Point(135, 101);
        linkThirdPartyNotices.Name = "linkThirdPartyNotices";
        linkThirdPartyNotices.Size = new Size(396, 15);
        linkThirdPartyNotices.TabIndex = 8;
        linkThirdPartyNotices.TabStop = true;
        linkThirdPartyNotices.Text = "linkThirdPartyNotices";
        // 
        // lblVersion
        // 
        lblVersion.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblVersion.AutoEllipsis = true;
        lblVersion.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblVersion, 9);
        lblVersion.Location = new Point(135, 38);
        lblVersion.Name = "lblVersion";
        lblVersion.Size = new Size(396, 15);
        lblVersion.TabIndex = 11;
        lblVersion.Text = "lblVersion";
        // 
        // lblCopyright
        // 
        lblCopyright.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblCopyright.AutoEllipsis = true;
        lblCopyright.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblCopyright, 9);
        lblCopyright.Location = new Point(135, 80);
        lblCopyright.Name = "lblCopyright";
        lblCopyright.Size = new Size(396, 15);
        lblCopyright.TabIndex = 10;
        lblCopyright.Text = "lblCopyright";
        // 
        // lblCopyrightTitle
        // 
        lblCopyrightTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblCopyrightTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblCopyrightTitle, 3);
        lblCopyrightTitle.Location = new Point(3, 80);
        lblCopyrightTitle.Margin = new Padding(3);
        lblCopyrightTitle.Name = "lblCopyrightTitle";
        lblCopyrightTitle.Size = new Size(126, 15);
        lblCopyrightTitle.TabIndex = 3;
        lblCopyrightTitle.Text = "Copyright:";
        // 
        // lblBuildTitle
        // 
        lblBuildTitle.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblBuildTitle.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblBuildTitle, 3);
        lblBuildTitle.Location = new Point(3, 59);
        lblBuildTitle.Margin = new Padding(3);
        lblBuildTitle.Name = "lblBuildTitle";
        lblBuildTitle.Size = new Size(126, 15);
        lblBuildTitle.TabIndex = 13;
        lblBuildTitle.Text = "Build:";
        // 
        // lblBuild
        // 
        lblBuild.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        lblBuild.AutoSize = true;
        tableGridLayout.SetColumnSpan(lblBuild, 9);
        lblBuild.Location = new Point(135, 59);
        lblBuild.Name = "lblBuild";
        lblBuild.Size = new Size(396, 15);
        lblBuild.TabIndex = 14;
        lblBuild.Text = "lblBuild";
        // 
        // txtLicense
        // 
        tableGridLayout.SetColumnSpan(txtLicense, 12);
        txtLicense.Dock = DockStyle.Fill;
        txtLicense.Font = new Font("Consolas", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        txtLicense.Location = new Point(3, 164);
        txtLicense.Multiline = true;
        txtLicense.Name = "txtLicense";
        txtLicense.ReadOnly = true;
        txtLicense.ScrollBars = ScrollBars.Both;
        txtLicense.Size = new Size(528, 188);
        txtLicense.TabIndex = 15;
        // 
        // AboutForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(550, 400);
        Controls.Add(tableGridLayout);
        FormBorderStyle = FormBorderStyle.None;
        Name = "AboutForm";
        Padding = new Padding(8);
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "AboutForm";
        tableGridLayout.ResumeLayout(false);
        tableGridLayout.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TableLayoutPanel tableGridLayout;
    private Label lblAppName;
    private Label lblVersionTitle;
    private Label lblCopyrightTitle;
    private Label lblLisenceTitle;
    private Label lblThirdPartyNoticesTitle;
    private Button btnOK;
    private Label lblHomeTitle;
    private LinkLabel linkHome;
    private LinkLabel linkThirdPartyNotices;
    private Label lblCopyright;
    private Label lblVersion;
    private Label lblBuildTitle;
    private Label lblBuild;
    private TextBox txtLicense;
}