<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ForenameEditorForm
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

        lvForenames = New ListView()
        Forename = New ColumnHeader()
        TopName = New ColumnHeader()
        tbEdit = New TextBox()
        btnAddNew = New Button()
        btnEdit = New Button()
        btnDelete = New Button()
        btnClose = New Button()
        btnCancel = New Button()
        btnSave = New Button()

        SuspendLayout()

        '
        ' lvForenames
        '
        lvForenames.Columns.AddRange(New ColumnHeader() {Forename, TopName})
        lvForenames.FullRowSelect = True
        lvForenames.Location = New Point(10, 10)
        lvForenames.MultiSelect = False
        lvForenames.Name = "lvForenames"
        lvForenames.Size = New Size(248, 260)
        lvForenames.TabIndex = 0
        lvForenames.UseCompatibleStateImageBehavior = False
        lvForenames.View = View.Details

        '
        ' Forename
        '
        Forename.Text = "Forename"
        Forename.Width = 210

        '
        ' TopName
        '
        TopName.Text = "*"
        TopName.Width = 24

        '
        ' tbEdit
        '
        tbEdit.Enabled = False
        tbEdit.Location = New Point(10, 280)
        tbEdit.Name = "tbEdit"
        tbEdit.Size = New Size(248, 23)
        tbEdit.TabIndex = 1

        '
        ' btnAddNew
        '
        btnAddNew.Location = New Point(10, 315)
        btnAddNew.Name = "btnAddNew"
        btnAddNew.Size = New Size(78, 28)
        btnAddNew.TabIndex = 2
        btnAddNew.Text = "Add New"
        btnAddNew.UseVisualStyleBackColor = True

        '
        ' btnEdit
        '
        btnEdit.Enabled = False
        btnEdit.Location = New Point(95, 315)
        btnEdit.Name = "btnEdit"
        btnEdit.Size = New Size(78, 28)
        btnEdit.TabIndex = 3
        btnEdit.Text = "Edit"
        btnEdit.UseVisualStyleBackColor = True

        '
        ' btnDelete
        '
        btnDelete.Enabled = False
        btnDelete.Location = New Point(180, 315)
        btnDelete.Name = "btnDelete"
        btnDelete.Size = New Size(78, 28)
        btnDelete.TabIndex = 4
        btnDelete.Text = "Delete"
        btnDelete.UseVisualStyleBackColor = True

        '
        ' btnClose
        '
        btnClose.Location = New Point(180, 353)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(78, 28)
        btnClose.TabIndex = 7
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = True

        '
        ' btnCancel
        '
        btnCancel.Enabled = False
        btnCancel.Location = New Point(95, 353)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(78, 28)
        btnCancel.TabIndex = 6
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True

        '
        ' btnSave
        '
        btnSave.Enabled = False
        btnSave.Location = New Point(10, 353)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(78, 28)
        btnSave.TabIndex = 5
        btnSave.Text = "Save"
        btnSave.UseVisualStyleBackColor = True

        '
        ' ForenameEditorForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(268, 393)

        Controls.Add(btnClose)
        Controls.Add(btnCancel)
        Controls.Add(btnSave)
        Controls.Add(btnDelete)
        Controls.Add(btnEdit)
        Controls.Add(btnAddNew)
        Controls.Add(tbEdit)
        Controls.Add(lvForenames)

        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "ForenameEditorForm"
        StartPosition = FormStartPosition.CenterParent
        Text = "Forename Editor"

        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lvForenames As ListView
    Friend WithEvents Forename As ColumnHeader
    Friend WithEvents TopName As ColumnHeader
    Friend WithEvents tbEdit As TextBox
    Friend WithEvents btnAddNew As Button
    Friend WithEvents btnEdit As Button
    Friend WithEvents btnDelete As Button
    Friend WithEvents btnClose As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnSave As Button

End Class
