<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class TranscriptionForm
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
        filePanel = New CollapsiblePanel()
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
        ' TranscriptionForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(884, 481)
        Controls.Add(filePanel)
        MinimumSize = New Size(700, 400)
        Name = "TranscriptionForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "WinBMD2"
        ResumeLayout(False)
    End Sub

    Friend WithEvents filePanel As CollapsiblePanel
End Class
