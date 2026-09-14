<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DirectiveEditorForm

    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    Protected Overrides Sub Dispose(disposing As Boolean)

        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try

    End Sub

    Private Sub InitializeComponent()
        directiveList = New ListView()
        typeColumn = New ColumnHeader()
        rowsColumn = New ColumnHeader()
        textColumn = New ColumnHeader()
        typeLabel = New Label()
        typeCombo = New ComboBox()
        rowsLabel = New Label()
        rowsTextBox = New TextBox()
        textLabel = New Label()
        textTextBox = New TextBox()
        addButton = New Button()
        updateButton = New Button()
        deleteButton = New Button()
        okButton = New Button()
        closeButton = New Button()
        SuspendLayout()
        ' 
        ' directiveList
        ' 
        directiveList.Columns.AddRange(New ColumnHeader() {typeColumn, rowsColumn, textColumn})
        directiveList.FullRowSelect = True
        directiveList.GridLines = True
        directiveList.Location = New Point(16, 16)
        directiveList.MultiSelect = False
        directiveList.Name = "directiveList"
        directiveList.OwnerDraw = True
        directiveList.Size = New Size(560, 110)
        directiveList.TabIndex = 0
        directiveList.UseCompatibleStateImageBehavior = False
        directiveList.View = View.Details
        ' 
        ' typeColumn
        ' 
        typeColumn.Text = "Type"
        typeColumn.Width = 130
        ' 
        ' rowsColumn
        ' 
        rowsColumn.Text = "Rows"
        ' 
        ' textColumn
        ' 
        textColumn.Text = "Text"
        textColumn.Width = 350
        ' 
        ' typeLabel
        ' 
        typeLabel.AutoSize = True
        typeLabel.Location = New Point(16, 148)
        typeLabel.Name = "typeLabel"
        typeLabel.Size = New Size(32, 15)
        typeLabel.TabIndex = 1
        typeLabel.Text = "Type"
        ' 
        ' typeCombo
        ' 
        typeCombo.DropDownStyle = ComboBoxStyle.DropDownList
        typeCombo.FormattingEnabled = True
        typeCombo.Items.AddRange(New Object() {"COMMENT", "THEORY", "THEORY REF", "Internal comment", "BREAK", "PAGE"})
        typeCombo.Location = New Point(110, 142)
        typeCombo.Name = "typeCombo"
        typeCombo.Size = New Size(190, 23)
        typeCombo.TabIndex = 2
        ' 
        ' rowsLabel
        ' 
        rowsLabel.AutoSize = True
        rowsLabel.Location = New Point(318, 148)
        rowsLabel.Name = "rowsLabel"
        rowsLabel.Size = New Size(35, 15)
        rowsLabel.TabIndex = 3
        rowsLabel.Text = "Rows"
        ' 
        ' rowsTextBox
        ' 
        rowsTextBox.Location = New Point(370, 142)
        rowsTextBox.Name = "rowsTextBox"
        rowsTextBox.Size = New Size(60, 23)
        rowsTextBox.TabIndex = 4
        ' 
        ' textLabel
        ' 
        textLabel.AutoSize = True
        textLabel.Location = New Point(16, 184)
        textLabel.Name = "textLabel"
        textLabel.Size = New Size(65, 15)
        textLabel.TabIndex = 5
        textLabel.Text = "Text / Page"
        ' 
        ' textTextBox
        ' 
        textTextBox.Location = New Point(110, 180)
        textTextBox.Name = "textTextBox"
        textTextBox.Size = New Size(466, 23)
        textTextBox.TabIndex = 6
        ' 
        ' addButton
        ' 
        addButton.Location = New Point(110, 224)
        addButton.Name = "addButton"
        addButton.Size = New Size(100, 32)
        addButton.TabIndex = 7
        addButton.Text = "Add"
        addButton.UseVisualStyleBackColor = True
        ' 
        ' updateButton
        ' 
        updateButton.Location = New Point(216, 224)
        updateButton.Name = "updateButton"
        updateButton.Size = New Size(100, 32)
        updateButton.TabIndex = 8
        updateButton.Text = "Update"
        updateButton.UseVisualStyleBackColor = True
        ' 
        ' deleteButton
        ' 
        deleteButton.Location = New Point(322, 224)
        deleteButton.Name = "deleteButton"
        deleteButton.Size = New Size(100, 32)
        deleteButton.TabIndex = 9
        deleteButton.Text = "Delete"
        deleteButton.UseVisualStyleBackColor = True
        ' 
        ' okButton
        ' 
        okButton.DialogResult = DialogResult.OK
        okButton.Location = New Point(390, 292)
        okButton.Name = "okButton"
        okButton.Size = New Size(90, 32)
        okButton.TabIndex = 10
        okButton.Text = "OK"
        okButton.UseVisualStyleBackColor = True
        ' 
        ' closeButton
        ' 
        closeButton.DialogResult = DialogResult.Cancel
        closeButton.Location = New Point(486, 292)
        closeButton.Name = "closeButton"
        closeButton.Size = New Size(90, 32)
        closeButton.TabIndex = 11
        closeButton.Text = "Cancel"
        closeButton.UseVisualStyleBackColor = True
        ' 
        ' DirectiveEditorForm
        ' 
        AcceptButton = okButton
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(594, 341)
        Controls.Add(closeButton)
        Controls.Add(okButton)
        Controls.Add(deleteButton)
        Controls.Add(updateButton)
        Controls.Add(addButton)
        Controls.Add(textTextBox)
        Controls.Add(textLabel)
        Controls.Add(rowsTextBox)
        Controls.Add(rowsLabel)
        Controls.Add(typeCombo)
        Controls.Add(typeLabel)
        Controls.Add(directiveList)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "DirectiveEditorForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Directives"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents directiveList As ListView
    Friend WithEvents typeColumn As ColumnHeader
    Friend WithEvents rowsColumn As ColumnHeader
    Friend WithEvents textColumn As ColumnHeader

    Friend WithEvents typeLabel As Label
    Friend WithEvents typeCombo As ComboBox

    Friend WithEvents rowsLabel As Label
    Friend WithEvents rowsTextBox As TextBox

    Friend WithEvents textLabel As Label
    Friend WithEvents textTextBox As TextBox

    Friend WithEvents addButton As Button
    Friend WithEvents updateButton As Button
    Friend WithEvents deleteButton As Button

    Friend WithEvents okButton As Button
    Friend WithEvents closeButton As Button

End Class