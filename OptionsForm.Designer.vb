<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class OptionsForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(OptionsForm))
        mainSplitContainer = New SplitContainer()
        btnGeneral = New Button()
        btnEntry = New Button()
        btnCapitalisation = New Button()
        btnAdvanced = New Button()
        headerPanel = New Panel()
        lblTitle = New Label()
        lblSubtitle = New Label()
        footerPanel = New Panel()
        btnCancel = New Button()
        btnOk = New Button()
        lblPageTitle = New Label()
        lblPageDescription = New Label()
        CType(mainSplitContainer, ComponentModel.ISupportInitialize).BeginInit()
        mainSplitContainer.Panel1.SuspendLayout()
        mainSplitContainer.Panel2.SuspendLayout()
        mainSplitContainer.SuspendLayout()
        headerPanel.SuspendLayout()
        footerPanel.SuspendLayout()
        SuspendLayout()
        ' 
        ' mainSplitContainer
        ' 
        mainSplitContainer.Dock = DockStyle.Fill
        mainSplitContainer.FixedPanel = FixedPanel.Panel1
        mainSplitContainer.IsSplitterFixed = True
        mainSplitContainer.Location = New Point(0, 72)
        mainSplitContainer.Name = "mainSplitContainer"
        ' 
        ' mainSplitContainer.Panel1
        ' 
        mainSplitContainer.Panel1.Controls.Add(btnAdvanced)
        mainSplitContainer.Panel1.Controls.Add(btnCapitalisation)
        mainSplitContainer.Panel1.Controls.Add(btnEntry)
        mainSplitContainer.Panel1.Controls.Add(btnGeneral)
        ' 
        ' mainSplitContainer.Panel2
        ' 
        mainSplitContainer.Panel2.Controls.Add(lblPageDescription)
        mainSplitContainer.Panel2.Controls.Add(lblPageTitle)
        mainSplitContainer.Size = New Size(764, 449)
        mainSplitContainer.SplitterDistance = 180
        mainSplitContainer.SplitterWidth = 1
        mainSplitContainer.TabIndex = 0
        ' 
        ' btnGeneral
        ' 
        btnGeneral.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnGeneral.FlatStyle = FlatStyle.Flat
        btnGeneral.ImageAlign = ContentAlignment.MiddleLeft
        btnGeneral.Location = New Point(8, 8)
        btnGeneral.Name = "btnGeneral"
        btnGeneral.Padding = New Padding(12, 0, 0, 0)
        btnGeneral.Size = New Size(160, 36)
        btnGeneral.TabIndex = 0
        btnGeneral.Text = "General"
        btnGeneral.TextAlign = ContentAlignment.MiddleLeft
        btnGeneral.TextImageRelation = TextImageRelation.ImageBeforeText
        btnGeneral.UseVisualStyleBackColor = False
        ' 
        ' btnEntry
        ' 
        btnEntry.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnEntry.FlatStyle = FlatStyle.Flat
        btnEntry.ImageAlign = ContentAlignment.MiddleLeft
        btnEntry.Location = New Point(8, 52)
        btnEntry.Name = "btnEntry"
        btnEntry.Padding = New Padding(12, 0, 0, 0)
        btnEntry.Size = New Size(160, 36)
        btnEntry.TabIndex = 1
        btnEntry.Text = "Entry"
        btnEntry.TextAlign = ContentAlignment.MiddleLeft
        btnEntry.TextImageRelation = TextImageRelation.ImageBeforeText
        btnEntry.UseVisualStyleBackColor = False
        ' 
        ' btnCapitalisation
        ' 
        btnCapitalisation.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        btnCapitalisation.FlatStyle = FlatStyle.Flat
        btnCapitalisation.ImageAlign = ContentAlignment.MiddleLeft
        btnCapitalisation.Location = New Point(8, 96)
        btnCapitalisation.Name = "btnCapitalisation"
        btnCapitalisation.Padding = New Padding(12, 0, 0, 0)
        btnCapitalisation.Size = New Size(160, 36)
        btnCapitalisation.TabIndex = 2
        btnCapitalisation.Text = "Capitalisation"
        btnCapitalisation.TextAlign = ContentAlignment.MiddleLeft
        btnCapitalisation.TextImageRelation = TextImageRelation.ImageBeforeText
        btnCapitalisation.UseVisualStyleBackColor = False
        ' 
        ' btnAdvanced
        ' 
        btnAdvanced.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        btnAdvanced.FlatStyle = FlatStyle.Flat
        btnAdvanced.ImageAlign = ContentAlignment.MiddleLeft
        btnAdvanced.Location = New Point(8, 401)
        btnAdvanced.Name = "btnAdvanced"
        btnAdvanced.Padding = New Padding(12, 0, 0, 0)
        btnAdvanced.Size = New Size(160, 36)
        btnAdvanced.TabIndex = 3
        btnAdvanced.Text = "Advanced"
        btnAdvanced.TextAlign = ContentAlignment.MiddleLeft
        btnAdvanced.TextImageRelation = TextImageRelation.ImageBeforeText
        btnAdvanced.UseVisualStyleBackColor = False
        ' 
        ' headerPanel
        ' 
        headerPanel.Controls.Add(lblSubtitle)
        headerPanel.Controls.Add(lblTitle)
        headerPanel.Dock = DockStyle.Top
        headerPanel.Location = New Point(0, 0)
        headerPanel.Name = "headerPanel"
        headerPanel.Padding = New Padding(22, 14, 22, 10)
        headerPanel.Size = New Size(764, 72)
        headerPanel.TabIndex = 1
        ' 
        ' lblTitle
        ' 
        lblTitle.AutoSize = True
        lblTitle.Location = New Point(20, 10)
        lblTitle.Name = "lblTitle"
        lblTitle.Size = New Size(49, 15)
        lblTitle.TabIndex = 0
        lblTitle.Text = "Options"
        ' 
        ' lblSubtitle
        ' 
        lblSubtitle.AutoSize = True
        lblSubtitle.Location = New Point(22, 42)
        lblSubtitle.Name = "lblSubtitle"
        lblSubtitle.Size = New Size(160, 15)
        lblSubtitle.TabIndex = 1
        lblSubtitle.Text = "Configure WinBMD2 settings"
        ' 
        ' footerPanel
        ' 
        footerPanel.Controls.Add(btnOk)
        footerPanel.Controls.Add(btnCancel)
        footerPanel.Dock = DockStyle.Bottom
        footerPanel.Location = New Point(0, 469)
        footerPanel.Name = "footerPanel"
        footerPanel.Padding = New Padding(18, 9, 18, 9)
        footerPanel.Size = New Size(764, 52)
        footerPanel.TabIndex = 2
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnCancel.Location = New Point(519, 10)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(90, 30)
        btnCancel.TabIndex = 0
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' btnOk
        ' 
        btnOk.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOk.Location = New Point(637, 10)
        btnOk.Name = "btnOk"
        btnOk.Size = New Size(90, 30)
        btnOk.TabIndex = 1
        btnOk.Text = "Ok"
        btnOk.UseVisualStyleBackColor = True
        ' 
        ' lblPageTitle
        ' 
        lblPageTitle.AutoSize = True
        lblPageTitle.Location = New Point(24, 20)
        lblPageTitle.Name = "lblPageTitle"
        lblPageTitle.Size = New Size(47, 15)
        lblPageTitle.TabIndex = 0
        lblPageTitle.Text = "General"
        ' 
        ' lblPageDescription
        ' 
        lblPageDescription.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        lblPageDescription.Location = New Point(24, 62)
        lblPageDescription.Name = "lblPageDescription"
        lblPageDescription.Size = New Size(480, 60)
        lblPageDescription.TabIndex = 1
        lblPageDescription.Text = "General WinBMD2 settings."
        ' 
        ' OptionsForm
        ' 
        AcceptButton = btnOk
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New Size(764, 521)
        Controls.Add(footerPanel)
        Controls.Add(mainSplitContainer)
        Controls.Add(headerPanel)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(700, 500)
        Name = "OptionsForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Options"
        mainSplitContainer.Panel1.ResumeLayout(False)
        mainSplitContainer.Panel2.ResumeLayout(False)
        mainSplitContainer.Panel2.PerformLayout()
        CType(mainSplitContainer, ComponentModel.ISupportInitialize).EndInit()
        mainSplitContainer.ResumeLayout(False)
        headerPanel.ResumeLayout(False)
        headerPanel.PerformLayout()
        footerPanel.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents mainSplitContainer As SplitContainer
    Friend WithEvents btnAdvanced As Button
    Friend WithEvents btnCapitalisation As Button
    Friend WithEvents btnEntry As Button
    Friend WithEvents btnGeneral As Button
    Friend WithEvents headerPanel As Panel
    Friend WithEvents lblSubtitle As Label
    Friend WithEvents lblTitle As Label
    Friend WithEvents footerPanel As Panel
    Friend WithEvents btnOk As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents lblPageTitle As Label
    Friend WithEvents lblPageDescription As Label
End Class
