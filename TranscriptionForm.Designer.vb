<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TranscriptionForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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

    'NOTE: The following procedure is required by the Windows Form Designer.
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        filePanel = New CollapsiblePanel()
        categoryBarPanel = New Panel()
        categoryStrip = New FlowLayoutPanel()
        btnCategoryFile = New Button()
        btnCategoryScan = New Button()
        btnCategoryVerify = New Button()
        btnCategoryUpload = New Button()
        btnCategoryOptions = New Button()
        btnCategoryHelp = New Button()
        btnSpecialCharacters = New Button()
        commandStrip = New FlowLayoutPanel()
        btnFileOpen = New Button()
        btnFileSave = New Button()
        btnFileSaveAs = New Button()
        btnFileExit = New Button()
        btnFileEditHeader = New Button()
        gridHostPanel = New Panel()
        transcriptionGrid = New DataGridView()
        statusStrip = New StatusStrip()
        statusMessageLabel = New ToolStripStatusLabel()
        statusMessageLabel2 = New ToolStripStatusLabel()
        statusMessageLabel3 = New ToolStripStatusLabel()
        statusSpringLabel = New ToolStripStatusLabel()
        statusPositionLabel = New ToolStripStatusLabel()
        uploadToolTip = New ToolTip(components)
        filePanel.SuspendLayout()
        categoryBarPanel.SuspendLayout()
        categoryStrip.SuspendLayout()
        commandStrip.SuspendLayout()
        CType(transcriptionGrid, ComponentModel.ISupportInitialize).BeginInit()
        statusStrip.SuspendLayout()
        SuspendLayout()
        ' 
        ' filePanel
        ' 
        filePanel.Controls.Add(categoryBarPanel)
        filePanel.Controls.Add(commandStrip)
        filePanel.Dock = DockStyle.Top
        filePanel.HeaderText = ""
        filePanel.Location = New Point(0, 0)
        filePanel.MinimumSize = New Size(0, 20)
        filePanel.Name = "filePanel"
        filePanel.Padding = New Padding(1, 20, 1, 1)
        filePanel.Size = New Size(884, 92)
        filePanel.TabIndex = 0
        ' 
        ' categoryBarPanel
        ' 
        categoryBarPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        categoryBarPanel.Controls.Add(categoryStrip)
        categoryBarPanel.Controls.Add(btnSpecialCharacters)
        categoryBarPanel.Location = New Point(6, 23)
        categoryBarPanel.Name = "categoryBarPanel"
        categoryBarPanel.Size = New Size(872, 28)
        categoryBarPanel.TabIndex = 0
        ' 
        ' categoryStrip
        ' 
        categoryStrip.Controls.Add(btnCategoryFile)
        categoryStrip.Controls.Add(btnCategoryScan)
        categoryStrip.Controls.Add(btnCategoryVerify)
        categoryStrip.Controls.Add(btnCategoryUpload)
        categoryStrip.Controls.Add(btnCategoryOptions)
        categoryStrip.Controls.Add(btnCategoryHelp)
        categoryStrip.Location = New Point(0, 0)
        categoryStrip.Margin = New Padding(0)
        categoryStrip.Name = "categoryStrip"
        categoryStrip.Padding = New Padding(2)
        categoryStrip.Size = New Size(420, 28)
        categoryStrip.TabIndex = 0
        categoryStrip.WrapContents = False
        ' 
        ' btnCategoryFile
        ' 
        btnCategoryFile.FlatStyle = FlatStyle.Flat
        btnCategoryFile.Location = New Point(5, 2)
        btnCategoryFile.Margin = New Padding(3, 0, 3, 0)
        btnCategoryFile.Name = "btnCategoryFile"
        btnCategoryFile.Size = New Size(62, 24)
        btnCategoryFile.TabIndex = 0
        btnCategoryFile.Text = "File"
        btnCategoryFile.UseVisualStyleBackColor = False
        ' 
        ' btnCategoryScan
        ' 
        btnCategoryScan.FlatStyle = FlatStyle.Flat
        btnCategoryScan.Location = New Point(73, 2)
        btnCategoryScan.Margin = New Padding(3, 0, 3, 0)
        btnCategoryScan.Name = "btnCategoryScan"
        btnCategoryScan.Size = New Size(62, 24)
        btnCategoryScan.TabIndex = 3
        btnCategoryScan.Text = "Scan"
        btnCategoryScan.UseVisualStyleBackColor = False
        ' 
        ' btnCategoryVerify
        ' 
        btnCategoryVerify.FlatStyle = FlatStyle.Flat
        btnCategoryVerify.Location = New Point(141, 2)
        btnCategoryVerify.Margin = New Padding(3, 0, 3, 0)
        btnCategoryVerify.Name = "btnCategoryVerify"
        btnCategoryVerify.Size = New Size(62, 24)
        btnCategoryVerify.TabIndex = 2
        btnCategoryVerify.Text = "Verify"
        btnCategoryVerify.UseVisualStyleBackColor = False
        ' 
        ' btnCategoryUpload
        ' 
        btnCategoryUpload.FlatStyle = FlatStyle.Flat
        btnCategoryUpload.Location = New Point(209, 2)
        btnCategoryUpload.Margin = New Padding(3, 0, 3, 0)
        btnCategoryUpload.Name = "btnCategoryUpload"
        btnCategoryUpload.Size = New Size(62, 24)
        btnCategoryUpload.TabIndex = 4
        btnCategoryUpload.Text = "Upload"
        btnCategoryUpload.UseVisualStyleBackColor = False
        ' 
        ' btnCategoryOptions
        ' 
        btnCategoryOptions.FlatStyle = FlatStyle.Flat
        btnCategoryOptions.Location = New Point(277, 2)
        btnCategoryOptions.Margin = New Padding(3, 0, 3, 0)
        btnCategoryOptions.Name = "btnCategoryOptions"
        btnCategoryOptions.Size = New Size(70, 24)
        btnCategoryOptions.TabIndex = 5
        btnCategoryOptions.Text = "Options"
        btnCategoryOptions.UseVisualStyleBackColor = False
        ' 
        ' btnCategoryHelp
        ' 
        btnCategoryHelp.FlatStyle = FlatStyle.Flat
        btnCategoryHelp.Location = New Point(353, 2)
        btnCategoryHelp.Margin = New Padding(3, 0, 3, 0)
        btnCategoryHelp.Name = "btnCategoryHelp"
        btnCategoryHelp.Size = New Size(62, 24)
        btnCategoryHelp.TabIndex = 6
        btnCategoryHelp.Text = "Help"
        btnCategoryHelp.UseVisualStyleBackColor = False
        ' 
        ' btnSpecialCharacters
        ' 
        btnSpecialCharacters.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnSpecialCharacters.Location = New Point(838, 1)
        btnSpecialCharacters.Name = "btnSpecialCharacters"
        btnSpecialCharacters.Size = New Size(34, 25)
        btnSpecialCharacters.TabIndex = 7
        btnSpecialCharacters.TabStop = False
        btnSpecialCharacters.Text = "F4"
        btnSpecialCharacters.UseVisualStyleBackColor = True
        ' 
        ' commandStrip
        ' 
        commandStrip.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        commandStrip.Controls.Add(btnFileOpen)
        commandStrip.Controls.Add(btnFileSave)
        commandStrip.Controls.Add(btnFileSaveAs)
        commandStrip.Controls.Add(btnFileExit)
        commandStrip.Controls.Add(btnFileEditHeader)
        commandStrip.Location = New Point(6, 53)
        commandStrip.Margin = New Padding(0)
        commandStrip.Name = "commandStrip"
        commandStrip.Padding = New Padding(2)
        commandStrip.Size = New Size(872, 34)
        commandStrip.TabIndex = 1
        commandStrip.WrapContents = False
        ' 
        ' btnFileOpen
        ' 
        btnFileOpen.Location = New Point(5, 2)
        btnFileOpen.Margin = New Padding(3, 0, 3, 0)
        btnFileOpen.Name = "btnFileOpen"
        btnFileOpen.Size = New Size(70, 28)
        btnFileOpen.TabIndex = 0
        btnFileOpen.Text = "Open"
        btnFileOpen.UseVisualStyleBackColor = False
        ' 
        ' btnFileSave
        ' 
        btnFileSave.Location = New Point(81, 2)
        btnFileSave.Margin = New Padding(3, 0, 3, 0)
        btnFileSave.Name = "btnFileSave"
        btnFileSave.Size = New Size(70, 28)
        btnFileSave.TabIndex = 1
        btnFileSave.Text = "Save"
        btnFileSave.UseVisualStyleBackColor = False
        ' 
        ' btnFileSaveAs
        ' 
        btnFileSaveAs.Location = New Point(157, 2)
        btnFileSaveAs.Margin = New Padding(3, 0, 3, 0)
        btnFileSaveAs.Name = "btnFileSaveAs"
        btnFileSaveAs.Size = New Size(70, 28)
        btnFileSaveAs.TabIndex = 2
        btnFileSaveAs.Text = "Save As"
        btnFileSaveAs.UseVisualStyleBackColor = False
        ' 
        ' btnFileExit
        ' 
        btnFileExit.Location = New Point(233, 2)
        btnFileExit.Margin = New Padding(3, 0, 3, 0)
        btnFileExit.Name = "btnFileExit"
        btnFileExit.Size = New Size(70, 28)
        btnFileExit.TabIndex = 3
        btnFileExit.Text = "Exit"
        btnFileExit.UseVisualStyleBackColor = False
        ' 
        ' btnFileEditHeader
        ' 
        btnFileEditHeader.AutoSize = True
        btnFileEditHeader.Location = New Point(309, 2)
        btnFileEditHeader.Margin = New Padding(3, 0, 3, 0)
        btnFileEditHeader.Name = "btnFileEditHeader"
        btnFileEditHeader.Size = New Size(78, 28)
        btnFileEditHeader.TabIndex = 4
        btnFileEditHeader.Text = "Edit Header"
        btnFileEditHeader.UseVisualStyleBackColor = False
        ' 
        ' gridHostPanel
        ' 
        gridHostPanel.BackColor = SystemColors.Window
        gridHostPanel.Dock = DockStyle.Fill
        gridHostPanel.Location = New Point(0, 92)
        gridHostPanel.Name = "gridHostPanel"
        gridHostPanel.Size = New Size(884, 389)
        gridHostPanel.TabIndex = 1
        ' 
        ' transcriptionGrid
        ' 
        transcriptionGrid.AllowUserToAddRows = False
        transcriptionGrid.AllowUserToDeleteRows = False
        transcriptionGrid.AllowUserToResizeRows = False
        transcriptionGrid.BorderStyle = BorderStyle.None
        transcriptionGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        transcriptionGrid.Dock = DockStyle.Fill
        transcriptionGrid.Location = New Point(0, 92)
        transcriptionGrid.MultiSelect = False
        transcriptionGrid.Name = "transcriptionGrid"
        transcriptionGrid.RowHeadersVisible = False
        transcriptionGrid.SelectionMode = DataGridViewSelectionMode.CellSelect
        transcriptionGrid.ShowEditingIcon = False
        transcriptionGrid.ShowRowErrors = False
        transcriptionGrid.Size = New Size(884, 389)
        transcriptionGrid.TabIndex = 2
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {statusMessageLabel, statusMessageLabel2, statusMessageLabel3, statusSpringLabel, statusPositionLabel})
        statusStrip.Location = New Point(0, 459)
        statusStrip.Name = "statusStrip"
        statusStrip.Size = New Size(884, 22)
        statusStrip.TabIndex = 3
        statusStrip.Text = "StatusStrip1"
        ' 
        ' statusMessageLabel
        ' 
        statusMessageLabel.Name = "statusMessageLabel"
        statusMessageLabel.Size = New Size(39, 17)
        statusMessageLabel.Text = "Ready"
        statusMessageLabel.TextAlign = ContentAlignment.MiddleLeft
        '
        ' statusMessageLabel2
        '
        statusMessageLabel2.Name = "statusMessageLabel2"
        statusMessageLabel2.Size = New Size(0, 17)
        statusMessageLabel2.TextAlign = ContentAlignment.MiddleLeft
        statusMessageLabel2.Visible = False
        '
        ' statusMessageLabel3
        '
        statusMessageLabel3.Name = "statusMessageLabel3"
        statusMessageLabel3.Size = New Size(0, 17)
        statusMessageLabel3.TextAlign = ContentAlignment.MiddleLeft
        statusMessageLabel3.Visible = False
        ' 
        ' statusSpringLabel
        ' 
        statusSpringLabel.Name = "statusSpringLabel"
        statusSpringLabel.Size = New Size(791, 17)
        statusSpringLabel.Spring = True
        ' 
        ' statusPositionLabel
        ' 
        statusPositionLabel.Name = "statusPositionLabel"
        statusPositionLabel.Size = New Size(39, 17)
        statusPositionLabel.Text = "Row 1"
        statusPositionLabel.TextAlign = ContentAlignment.MiddleRight
        ' 
        ' TranscriptionForm
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(884, 481)
        Controls.Add(statusStrip)
        Controls.Add(transcriptionGrid)
        Controls.Add(gridHostPanel)
        Controls.Add(filePanel)
        MinimumSize = New Size(700, 150)
        Name = "TranscriptionForm"
        StartPosition = FormStartPosition.Manual
        Text = "WinBMD2"
        filePanel.ResumeLayout(False)
        categoryBarPanel.ResumeLayout(False)
        categoryStrip.ResumeLayout(False)
        commandStrip.ResumeLayout(False)
        commandStrip.PerformLayout()
        CType(transcriptionGrid, ComponentModel.ISupportInitialize).EndInit()
        statusStrip.ResumeLayout(False)
        statusStrip.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents filePanel As CollapsiblePanel
    Friend WithEvents gridHostPanel As Panel
    Friend WithEvents transcriptionGrid As DataGridView
    Friend WithEvents statusStrip As StatusStrip
    Friend WithEvents statusMessageLabel As ToolStripStatusLabel
    Friend WithEvents statusMessageLabel2 As ToolStripStatusLabel
    Friend WithEvents statusMessageLabel3 As ToolStripStatusLabel
    Friend WithEvents statusSpringLabel As ToolStripStatusLabel
    Friend WithEvents statusPositionLabel As ToolStripStatusLabel
    Friend WithEvents categoryBarPanel As Panel
    Friend WithEvents categoryStrip As FlowLayoutPanel
    Friend WithEvents commandStrip As FlowLayoutPanel
    Friend WithEvents btnCategoryFile As Button
    Friend WithEvents btnCategoryScan As Button
    Friend WithEvents btnCategoryOptions As Button
    Friend WithEvents btnCategoryVerify As Button
    Friend WithEvents btnCategoryUpload As Button
    Friend WithEvents btnCategoryHelp As Button
    Friend WithEvents btnFileOpen As Button
    Friend WithEvents btnFileSave As Button
    Friend WithEvents btnFileSaveAs As Button
    Friend WithEvents btnFileExit As Button
    Friend WithEvents btnFileEditHeader As Button
    Friend WithEvents uploadToolTip As ToolTip
    Friend WithEvents btnSpecialCharacters As Button

End Class