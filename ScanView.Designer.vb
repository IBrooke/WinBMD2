<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ScanView
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        scanTopPanel = New Panel()
        ScanProgressBar = New ProgressBar()
        scanProgressPanel = New FlowLayoutPanel()
        btnFindScan = New Button()
        btnOpenScan = New Button()
        scanStatusLabel = New Label()
        scanTopPanel.SuspendLayout()
        SuspendLayout()
        ' 
        ' scanTopPanel
        ' 
        scanTopPanel.Controls.Add(scanStatusLabel)
        scanTopPanel.Controls.Add(ScanProgressBar)
        scanTopPanel.Controls.Add(scanProgressPanel)
        scanTopPanel.Controls.Add(btnFindScan)
        scanTopPanel.Controls.Add(btnOpenScan)
        scanTopPanel.Dock = DockStyle.Top
        scanTopPanel.Location = New Point(0, 0)
        scanTopPanel.Name = "scanTopPanel"
        scanTopPanel.Size = New Size(684, 40)
        scanTopPanel.TabIndex = 0
        ' 
        ' ScanProgressBar
        ' 
        ScanProgressBar.Location = New Point(274, 6)
        ScanProgressBar.MarqueeAnimationSpeed = 50
        ScanProgressBar.Name = "ScanProgressBar"
        ScanProgressBar.Size = New Size(241, 25)
        ScanProgressBar.Style = ProgressBarStyle.Marquee
        ScanProgressBar.TabIndex = 4
        ScanProgressBar.Visible = False
        ' 
        ' scanProgressPanel
        ' 
        scanProgressPanel.AutoScroll = True
        scanProgressPanel.AutoSize = True
        scanProgressPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink
        scanProgressPanel.Location = New Point(0, 0)
        scanProgressPanel.Name = "scanProgressPanel"
        scanProgressPanel.Size = New Size(0, 0)
        scanProgressPanel.TabIndex = 3
        scanProgressPanel.Visible = False
        scanProgressPanel.WrapContents = False
        ' 
        ' btnFindScan
        ' 
        btnFindScan.Location = New Point(89, 7)
        btnFindScan.Name = "btnFindScan"
        btnFindScan.Size = New Size(75, 25)
        btnFindScan.TabIndex = 1
        btnFindScan.Text = "Find Scan"
        btnFindScan.UseVisualStyleBackColor = True
        ' 
        ' btnOpenScan
        ' 
        btnOpenScan.AutoSize = True
        btnOpenScan.Location = New Point(8, 6)
        btnOpenScan.Name = "btnOpenScan"
        btnOpenScan.Size = New Size(75, 25)
        btnOpenScan.TabIndex = 0
        btnOpenScan.Text = "Open Scan"
        btnOpenScan.UseVisualStyleBackColor = True
        ' 
        ' scanStatusLabel
        ' 
        scanStatusLabel.AutoSize = True
        scanStatusLabel.Location = New Point(183, 11)
        scanStatusLabel.Margin = New Padding(0, 4, 8, 0)
        scanStatusLabel.Name = "scanStatusLabel"
        scanStatusLabel.Size = New Size(90, 15)
        scanStatusLabel.TabIndex = 5
        scanStatusLabel.Text = "Locating Scan..."
        scanStatusLabel.Visible = False
        ' 
        ' ScanView
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(684, 481)
        Controls.Add(scanTopPanel)
        MinimumSize = New Size(400, 300)
        Name = "ScanView"
        StartPosition = FormStartPosition.Manual
        Text = "WinBMD2 Scan"
        scanTopPanel.ResumeLayout(False)
        scanTopPanel.PerformLayout()
        ResumeLayout(False)
    End Sub
    Friend WithEvents scanTopPanel As Panel
    Friend WithEvents btnOpenScan As Button
    Friend WithEvents btnFindScan As Button
    Friend WithEvents scanProgressPanel As FlowLayoutPanel
    Friend WithEvents ScanProgressBar As ProgressBar
    Friend WithEvents scanStatusLabel As Label
End Class
