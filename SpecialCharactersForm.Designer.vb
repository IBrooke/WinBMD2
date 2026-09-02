<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SpecialCharactersForm
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
        btnCharacterTemplate = New Button()
        characterTable = New TableLayoutPanel()
        SuspendLayout()
        ' 
        ' btnCharacterTemplate
        ' 
        btnCharacterTemplate.Location = New Point(12, 12)
        btnCharacterTemplate.Name = "btnCharacterTemplate"
        btnCharacterTemplate.Size = New Size(36, 36)
        btnCharacterTemplate.TabIndex = 0
        btnCharacterTemplate.Text = "À"
        btnCharacterTemplate.UseVisualStyleBackColor = True
        ' 
        ' characterTable
        ' 
        characterTable.ColumnCount = 10
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 10F))
        characterTable.Dock = DockStyle.Fill
        characterTable.Location = New Point(0, 0)
        characterTable.Margin = New Padding(0)
        characterTable.Name = "characterTable"
        characterTable.Padding = New Padding(2)
        characterTable.RowCount = 6
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.RowStyles.Add(New RowStyle(SizeType.Percent, 16.666666F))
        characterTable.Size = New Size(174, 171)
        characterTable.TabIndex = 1
        ' 
        ' SpecialCharactersForm
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(174, 171)
        Controls.Add(characterTable)
        Controls.Add(btnCharacterTemplate)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        KeyPreview = True
        MaximizeBox = False
        MinimizeBox = False
        Name = "SpecialCharactersForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Special Characters"
        ResumeLayout(False)
    End Sub

    Friend WithEvents btnCharacterTemplate As Button
    Friend WithEvents characterTable As TableLayoutPanel
End Class
