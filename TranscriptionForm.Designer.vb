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
        filePanel = New CollapsiblePanel()
        btnOptionsTest = New Button()
        gridHostPanel = New Panel()
        transcriptionGrid = New DataGridView()
        statusStrip = New StatusStrip()
        statusMessageLabel = New ToolStripStatusLabel()
        statusSpringLabel = New ToolStripStatusLabel()
        statusPositionLabel = New ToolStripStatusLabel()
        filePanel.SuspendLayout()
        CType(transcriptionGrid, ComponentModel.ISupportInitialize).BeginInit()
        statusStrip.SuspendLayout()
        SuspendLayout()
        ' 
        ' filePanel
        ' 
        filePanel.Controls.Add(btnOptionsTest)
        filePanel.Dock = DockStyle.Top
        filePanel.HeaderText = "File"
        filePanel.Location = New Point(0, 0)
        filePanel.MinimumSize = New Size(0, 20)
        filePanel.Name = "filePanel"
        filePanel.Padding = New Padding(1, 30, 1, 1)
        filePanel.Size = New Size(884, 50)
        filePanel.TabIndex = 0
        ' 
        ' btnOptionsTest
        ' 
        btnOptionsTest.Location = New Point(10, 22)
        btnOptionsTest.Name = "btnOptionsTest"
        btnOptionsTest.Size = New Size(75, 23)
        btnOptionsTest.TabIndex = 0
        btnOptionsTest.Text = "Options"
        btnOptionsTest.UseVisualStyleBackColor = True
        ' 
        ' gridHostPanel
        ' 
        gridHostPanel.BackColor = SystemColors.Window
        gridHostPanel.Dock = DockStyle.Fill
        gridHostPanel.Location = New Point(0, 50)
        gridHostPanel.Name = "gridHostPanel"
        gridHostPanel.Size = New Size(884, 431)
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
        transcriptionGrid.Location = New Point(0, 50)
        transcriptionGrid.MultiSelect = False
        transcriptionGrid.Name = "transcriptionGrid"
        transcriptionGrid.RowHeadersVisible = False
        transcriptionGrid.SelectionMode = DataGridViewSelectionMode.CellSelect
        transcriptionGrid.ShowEditingIcon = False
        transcriptionGrid.ShowRowErrors = False
        transcriptionGrid.Size = New Size(884, 431)
        transcriptionGrid.TabIndex = 2
        ' 
        ' statusStrip
        ' 
        statusStrip.Items.AddRange(New ToolStripItem() {statusMessageLabel, statusSpringLabel, statusPositionLabel})
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
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(884, 481)
        Controls.Add(statusStrip)
        Controls.Add(transcriptionGrid)
        Controls.Add(gridHostPanel)
        Controls.Add(filePanel)
        MinimumSize = New Size(700, 400)
        Name = "TranscriptionForm"
        StartPosition = FormStartPosition.Manual
        Text = "WinBMD2"
        filePanel.ResumeLayout(False)
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
    Friend WithEvents statusSpringLabel As ToolStripStatusLabel
    Friend WithEvents statusPositionLabel As ToolStripStatusLabel
    Friend WithEvents btnOptionsTest As Button

End Class