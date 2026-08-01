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
        gridHostPanel = New Panel()
        SuspendLayout()
        '
        ' filePanel
        '
        filePanel.Dock = DockStyle.Top
        filePanel.HeaderText = "File"
        filePanel.Location = New Point(0, 0)
        filePanel.MinimumSize = New Size(0, 20)
        filePanel.Name = "filePanel"
        filePanel.Padding = New Padding(1, 30, 1, 1)
        filePanel.Size = New Size(884, 50)
        filePanel.TabIndex = 0
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
        ' TranscriptionForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(884, 481)
        Controls.Add(gridHostPanel)
        Controls.Add(filePanel)
        MinimumSize = New Size(700, 400)
        Name = "TranscriptionForm"
        StartPosition = FormStartPosition.Manual
        Text = "WinBMD2"
        ResumeLayout(False)
    End Sub

    Friend WithEvents filePanel As CollapsiblePanel
    Friend WithEvents gridHostPanel As Panel

End Class