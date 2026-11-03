Imports System.Windows.Forms

Public Class ForenameEditorForm

    Private _editing As Boolean
    Private _adding As Boolean
    Private _originalName As String
    Private _editingTopName As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub ForenameEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lvForenames.HideSelection = False
        lvForenames.Sorting = SortOrder.None

        RefreshNames()
        UpdateButtons()

    End Sub

    Private Sub RefreshNames(Optional selectedName As String = Nothing, Optional selectedTopName As Boolean = False)

        lvForenames.BeginUpdate()

        Try
            lvForenames.Items.Clear()

            For Each name As String In ForenameData.TopNames

                Dim item As New ListViewItem(name)
                item.SubItems.Add("*")
                item.Tag = True
                lvForenames.Items.Add(item)

                If selectedTopName AndAlso String.Equals(name, selectedName, StringComparison.OrdinalIgnoreCase) Then
                    item.Selected = True
                    item.Focused = True
                End If

            Next

            For Each name As String In ForenameData.Names

                Dim item As New ListViewItem(name)
                item.SubItems.Add("")
                item.Tag = False
                lvForenames.Items.Add(item)

                If Not selectedTopName AndAlso String.Equals(name, selectedName, StringComparison.OrdinalIgnoreCase) Then
                    item.Selected = True
                    item.Focused = True
                End If

            Next

            If lvForenames.SelectedItems.Count > 0 Then
                lvForenames.SelectedItems(0).EnsureVisible()
            End If

        Finally
            lvForenames.EndUpdate()
        End Try

    End Sub

    Private Sub UpdateButtons()

        Dim hasSelection As Boolean = lvForenames.SelectedItems.Count > 0
        Dim selectedTopName As Boolean = False

        If hasSelection Then
            selectedTopName = CBool(lvForenames.SelectedItems(0).Tag)
        End If

        lvForenames.Enabled = Not _editing
        tbEdit.Enabled = _editing

        btnAddNew.Enabled = Not _editing
        btnEdit.Enabled = Not _editing AndAlso hasSelection
        btnDelete.Enabled = Not _editing AndAlso hasSelection AndAlso Not selectedTopName

        btnSave.Enabled = _editing AndAlso Not String.IsNullOrWhiteSpace(tbEdit.Text)
        btnCancel.Enabled = _editing

        If _editing Then
            AcceptButton = btnSave
            CancelButton = btnCancel
        Else
            AcceptButton = Nothing
            CancelButton = btnClose
        End If

    End Sub

    Private Sub lvForenames_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvForenames.SelectedIndexChanged

        If Not _editing Then

            If lvForenames.SelectedItems.Count > 0 Then
                tbEdit.Text = lvForenames.SelectedItems(0).Text
            Else
                tbEdit.Clear()
            End If

        End If

        UpdateButtons()

    End Sub

    Private Sub btnAddNew_Click(sender As Object, e As EventArgs) Handles btnAddNew.Click

        _adding = True
        _editing = True
        _originalName = Nothing
        _editingTopName = False

        tbEdit.Clear()
        UpdateButtons()
        tbEdit.Focus()

    End Sub

    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click

        If lvForenames.SelectedItems.Count = 0 Then
            Return
        End If

        Dim item As ListViewItem = lvForenames.SelectedItems(0)

        _adding = False
        _editing = True
        _originalName = item.Text
        _editingTopName = CBool(item.Tag)

        tbEdit.Text = _originalName
        UpdateButtons()

        tbEdit.Focus()
        tbEdit.SelectAll()

    End Sub

    Private Sub lvForenames_DoubleClick(sender As Object, e As EventArgs) Handles lvForenames.DoubleClick

        If btnEdit.Enabled Then
            btnEdit.PerformClick()
        End If

    End Sub

    Private Sub tbEdit_TextChanged(sender As Object, e As EventArgs) Handles tbEdit.TextChanged
        UpdateButtons()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        If Not _editing Then
            Return
        End If

        Dim newName As String = tbEdit.Text.Trim()

        If newName.Length = 0 Then
            Return
        End If

        If newName.StartsWith("@", StringComparison.Ordinal) Then

            MessageBox.Show(Me, "A forename cannot begin with @.", "Forename Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
            tbEdit.Focus()
            Return

        End If

        Dim namesToCheck As System.Collections.Generic.IReadOnlyList(Of String)

        If _editingTopName Then
            namesToCheck = ForenameData.TopNames
        Else
            namesToCheck = ForenameData.Names
        End If

        For Each existingName As String In namesToCheck

            If Not _adding AndAlso String.Equals(existingName, _originalName, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            If String.Equals(existingName, newName, StringComparison.OrdinalIgnoreCase) Then

                MessageBox.Show(Me, "That name already exists in this list.", "Forename Editor", MessageBoxButtons.OK, MessageBoxIcon.Information)
                tbEdit.Focus()
                tbEdit.SelectAll()
                Return

            End If

        Next

        If _adding Then

            ForenameData.AddName(newName)

        ElseIf Not ForenameData.EditName(_originalName, newName, _editingTopName) Then

            MessageBox.Show(Me, "The forename could not be changed.", "Forename Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return

        End If

        If Not ForenameData.Save() Then

            ' Restore the in-memory list if saving failed.
            If _adding Then
                ForenameData.DeleteName(newName)
            Else
                ForenameData.EditName(newName, _originalName, _editingTopName)
            End If

            Return

        End If

        Dim selectedTopName As Boolean = _editingTopName

        _editing = False
        _adding = False
        _originalName = Nothing
        _editingTopName = False

        RefreshNames(newName, selectedTopName)
        UpdateButtons()
        lvForenames.Focus()

    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

        _editing = False
        _adding = False
        _originalName = Nothing
        _editingTopName = False

        If lvForenames.SelectedItems.Count > 0 Then
            tbEdit.Text = lvForenames.SelectedItems(0).Text
        Else
            tbEdit.Clear()
        End If

        UpdateButtons()
        lvForenames.Focus()

    End Sub

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click

        If _editing OrElse lvForenames.SelectedItems.Count = 0 Then
            Return
        End If

        Dim item As ListViewItem = lvForenames.SelectedItems(0)

        If CBool(item.Tag) Then
            Return
        End If

        Dim selectedName As String = item.Text
        Dim selectedIndex As Integer = item.Index

        If MessageBox.Show(Me, "Delete """ & selectedName & """ from the ordinary forenames list?", "Forename Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            Return
        End If

        If Not ForenameData.DeleteName(selectedName) Then
            Return
        End If

        If Not ForenameData.Save() Then
            ForenameData.AddName(selectedName)
            Return
        End If

        RefreshNames()

        If lvForenames.Items.Count > 0 Then

            Dim nextIndex As Integer = Math.Min(selectedIndex, lvForenames.Items.Count - 1)
            Dim nextItem As ListViewItem = lvForenames.Items(nextIndex)

            nextItem.Selected = True
            nextItem.Focused = True
            nextItem.EnsureVisible()

        End If

        UpdateButtons()
        lvForenames.Focus()

    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub ForenameEditorForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        If Not _editing Then
            Return
        End If

        Dim originalText As String = If(_originalName, String.Empty)

        If String.Equals(tbEdit.Text.Trim(), originalText, StringComparison.Ordinal) Then
            Return
        End If

        If MessageBox.Show(Me, "Discard the unsaved forename change?", "Forename Editor", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then
            e.Cancel = True
        End If

    End Sub

End Class