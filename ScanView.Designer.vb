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
        btnRuler = New Button()
        btnRotateRight = New ScanCommandButton()
        btnRotateLeft = New ScanCommandButton()
        btnZoomIn = New ScanCommandButton()
        btnZoomOut = New ScanCommandButton()
        scanStatusLabel = New Label()
        ScanProgressBar = New ProgressBar()
        scanProgressPanel = New FlowLayoutPanel()
        btnFindScan = New Button()
        btnOpenScan = New Button()
        scanTopPanel.SuspendLayout()
        SuspendLayout()
        ' 
        ' scanTopPanel
        ' 
        scanTopPanel.Controls.Add(btnRuler)
        scanTopPanel.Controls.Add(btnRotateRight)
        scanTopPanel.Controls.Add(btnRotateLeft)
        scanTopPanel.Controls.Add(btnZoomIn)
        scanTopPanel.Controls.Add(btnZoomOut)
        scanTopPanel.Controls.Add(scanStatusLabel)
        scanTopPanel.Controls.Add(ScanProgressBar)
        scanTopPanel.Controls.Add(scanProgressPanel)
        scanTopPanel.Controls.Add(btnFindScan)
        scanTopPanel.Controls.Add(btnOpenScan)
        scanTopPanel.Dock = DockStyle.Top
        scanTopPanel.Location = New Point(0, 0)
        scanTopPanel.Name = "scanTopPanel"
        scanTopPanel.Size = New Size(777, 41)
        scanTopPanel.TabIndex = 0
        ' 
        ' btnRuler
        ' 
        btnRuler.Location = New Point(168, 6)
        btnRuler.Name = "btnRuler"
        btnRuler.Size = New Size(74, 25)
        btnRuler.TabIndex = 10
        btnRuler.Text = "Ruler"
        btnRuler.UseVisualStyleBackColor = True
        ' 
        ' btnRotateRight
        ' 
        btnRotateRight.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRotateRight.BackColor = Color.White
        btnRotateRight.CommandIcon = ScanCommandIcon.RotateRight
        btnRotateRight.FlatAppearance.BorderColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateRight.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateRight.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateRight.FlatStyle = FlatStyle.Flat
        btnRotateRight.Font = New Font("Segoe UI Symbol", 14.0F)
        btnRotateRight.ForeColor = Color.FromArgb(CByte(35), CByte(42), CByte(52))
        btnRotateRight.Location = New Point(566, 2)
        btnRotateRight.Margin = New Padding(3, 0, 3, 0)
        btnRotateRight.Name = "btnRotateRight"
        btnRotateRight.Size = New Size(36, 36)
        btnRotateRight.TabIndex = 9
        btnRotateRight.TabStop = False
        btnRotateRight.Text = "⟳"
        btnRotateRight.TextAlign = ContentAlignment.BottomCenter
        btnRotateRight.UseVisualStyleBackColor = False
        ' 
        ' btnRotateLeft
        ' 
        btnRotateLeft.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnRotateLeft.BackColor = Color.White
        btnRotateLeft.CommandIcon = ScanCommandIcon.RotateLeft
        btnRotateLeft.FlatAppearance.BorderColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateLeft.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateLeft.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnRotateLeft.FlatStyle = FlatStyle.Flat
        btnRotateLeft.Font = New Font("Segoe UI Symbol", 14.0F)
        btnRotateLeft.ForeColor = Color.FromArgb(CByte(35), CByte(42), CByte(52))
        btnRotateLeft.Location = New Point(608, 2)
        btnRotateLeft.Margin = New Padding(3, 0, 3, 0)
        btnRotateLeft.Name = "btnRotateLeft"
        btnRotateLeft.Size = New Size(36, 36)
        btnRotateLeft.TabIndex = 8
        btnRotateLeft.TabStop = False
        btnRotateLeft.Text = "⟲"
        btnRotateLeft.TextAlign = ContentAlignment.BottomCenter
        btnRotateLeft.UseVisualStyleBackColor = False
        ' 
        ' btnZoomIn
        ' 
        btnZoomIn.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnZoomIn.BackColor = Color.White
        btnZoomIn.CommandIcon = ScanCommandIcon.ZoomIn
        btnZoomIn.FlatAppearance.BorderColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomIn.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomIn.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomIn.FlatStyle = FlatStyle.Flat
        btnZoomIn.Font = New Font("Segoe UI Symbol", 14.0F)
        btnZoomIn.ForeColor = Color.FromArgb(CByte(35), CByte(42), CByte(52))
        btnZoomIn.Location = New Point(682, 2)
        btnZoomIn.Margin = New Padding(3, 0, 3, 0)
        btnZoomIn.Name = "btnZoomIn"
        btnZoomIn.Size = New Size(36, 36)
        btnZoomIn.TabIndex = 7
        btnZoomIn.TabStop = False
        btnZoomIn.Text = "＋"
        btnZoomIn.TextAlign = ContentAlignment.BottomCenter
        btnZoomIn.UseVisualStyleBackColor = False
        ' 
        ' btnZoomOut
        ' 
        btnZoomOut.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnZoomOut.BackColor = Color.White
        btnZoomOut.CommandIcon = ScanCommandIcon.ZoomOut
        btnZoomOut.FlatAppearance.BorderColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomOut.FlatAppearance.MouseDownBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomOut.FlatAppearance.MouseOverBackColor = Color.FromArgb(CByte(220), CByte(245), CByte(244))
        btnZoomOut.FlatStyle = FlatStyle.Flat
        btnZoomOut.Font = New Font("Segoe UI Symbol", 14.0F)
        btnZoomOut.ForeColor = Color.FromArgb(CByte(35), CByte(42), CByte(52))
        btnZoomOut.Location = New Point(724, 2)
        btnZoomOut.Margin = New Padding(3, 0, 3, 0)
        btnZoomOut.Name = "btnZoomOut"
        btnZoomOut.Size = New Size(36, 36)
        btnZoomOut.TabIndex = 6
        btnZoomOut.TabStop = False
        btnZoomOut.Text = "－"
        btnZoomOut.TextAlign = ContentAlignment.BottomCenter
        btnZoomOut.UseVisualStyleBackColor = False
        ' 
        ' scanStatusLabel
        ' 
        scanStatusLabel.AutoSize = True
        scanStatusLabel.Location = New Point(258, 11)
        scanStatusLabel.Margin = New Padding(0, 4, 8, 0)
        scanStatusLabel.Name = "scanStatusLabel"
        scanStatusLabel.Size = New Size(90, 15)
        scanStatusLabel.TabIndex = 5
        scanStatusLabel.Text = "Locating Scan..."
        scanStatusLabel.Visible = False
        ' 
        ' ScanProgressBar
        ' 
        ScanProgressBar.Location = New Point(349, 6)
        ScanProgressBar.MarqueeAnimationSpeed = 50
        ScanProgressBar.Name = "ScanProgressBar"
        ScanProgressBar.Size = New Size(192, 25)
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
        btnFindScan.Location = New Point(88, 7)
        btnFindScan.Name = "btnFindScan"
        btnFindScan.Size = New Size(74, 25)
        btnFindScan.TabIndex = 1
        btnFindScan.Text = "Find Scan"
        btnFindScan.UseVisualStyleBackColor = True
        ' 
        ' btnOpenScan
        ' 
        btnOpenScan.AutoSize = True
        btnOpenScan.Location = New Point(8, 6)
        btnOpenScan.Name = "btnOpenScan"
        btnOpenScan.Size = New Size(74, 25)
        btnOpenScan.TabIndex = 0
        btnOpenScan.Text = "Open Scan"
        btnOpenScan.UseVisualStyleBackColor = True
        ' 
        ' ScanView
        ' 
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(777, 481)
        Controls.Add(scanTopPanel)
        KeyPreview = True
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
    Friend WithEvents btnRotateRight As ScanCommandButton
    Friend WithEvents btnRotateLeft As ScanCommandButton
    Friend WithEvents btnZoomIn As ScanCommandButton
    Friend WithEvents btnZoomOut As ScanCommandButton
    Friend WithEvents btnRuler As Button
End Class
