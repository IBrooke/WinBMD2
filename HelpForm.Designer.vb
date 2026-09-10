<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class HelpForm
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
        splHelp = New SplitContainer()
        lstTopics = New ListBox()
        rtbHelp = New RichTextBox()
        CType(splHelp, ComponentModel.ISupportInitialize).BeginInit()
        splHelp.Panel1.SuspendLayout()
        splHelp.Panel2.SuspendLayout()
        splHelp.SuspendLayout()
        SuspendLayout()
        ' 
        ' splHelp
        ' 
        splHelp.Dock = DockStyle.Fill
        splHelp.FixedPanel = FixedPanel.Panel1
        splHelp.Location = New Point(0, 0)
        splHelp.Name = "splHelp"
        ' 
        ' splHelp.Panel1
        ' 
        splHelp.Panel1.Controls.Add(lstTopics)
        ' 
        ' splHelp.Panel2
        ' 
        splHelp.Panel2.Controls.Add(rtbHelp)
        splHelp.Size = New Size(800, 450)
        splHelp.SplitterDistance = 200
        splHelp.TabIndex = 0
        ' 
        ' lstTopics
        ' 
        lstTopics.Dock = DockStyle.Fill
        lstTopics.DrawMode = DrawMode.OwnerDrawFixed
        lstTopics.FormattingEnabled = True
        lstTopics.IntegralHeight = False
        lstTopics.ItemHeight = 15
        lstTopics.Location = New Point(0, 0)
        lstTopics.Name = "lstTopics"
        lstTopics.Size = New Size(200, 450)
        lstTopics.TabIndex = 0
        ' 
        ' rtbHelp
        ' 
        rtbHelp.BorderStyle = BorderStyle.None
        rtbHelp.Dock = DockStyle.Fill
        rtbHelp.Location = New Point(0, 0)
        rtbHelp.Name = "rtbHelp"
        rtbHelp.ReadOnly = True
        rtbHelp.Size = New Size(596, 450)
        rtbHelp.TabIndex = 0
        rtbHelp.Text = ""
        ' 
        ' HelpForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(splHelp)
        Name = "HelpForm"
        Text = "HelpForm"
        splHelp.Panel1.ResumeLayout(False)
        splHelp.Panel2.ResumeLayout(False)
        CType(splHelp, ComponentModel.ISupportInitialize).EndInit()
        splHelp.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents splHelp As SplitContainer
    Friend WithEvents lstTopics As ListBox
    Friend WithEvents rtbHelp As RichTextBox
End Class
