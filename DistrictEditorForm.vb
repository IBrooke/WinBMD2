Public Class DistrictEditorForm
    Public Event DistrictEdited(oldDistrict As String, oldVolume As String, newDistrict As String, newVolume As String)
    Private _editingDistrict As DistrictData.DistrictMatch = Nothing

    Public Sub New()

        InitializeComponent()

    End Sub

    Private Sub DistrictEditorForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        LoadDistrictList()

    End Sub
    Private Sub LoadDistrictList()

        lvDistricts.Items.Clear()

        For Each districtEntry As DistrictData.DistrictMatch In DistrictData.GetDistricts()
            Dim item As New ListViewItem(districtEntry.Name)
            item.SubItems.Add(districtEntry.Volume)
            item.SubItems.Add(If(districtEntry.Supplementary, "*", ""))
            item.Tag = districtEntry
            lvDistricts.Items.Add(item)
        Next

    End Sub
    Private Sub lvDistricts_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lvDistricts.SelectedIndexChanged

        Dim canEdit As Boolean = False

        If lvDistricts.SelectedItems.Count > 0 Then
            Dim districtEntry As DistrictData.DistrictMatch = DirectCast(lvDistricts.SelectedItems(0).Tag, DistrictData.DistrictMatch)
            canEdit = districtEntry.Supplementary
        End If

        btnEdit.Enabled = canEdit
        btnDelete.Enabled = canEdit

    End Sub
    Private Sub btnAddNew_Click(sender As Object, e As EventArgs) Handles btnAddNew.Click

        _editingDistrict = Nothing
        lvDistricts.Enabled = False
        tbEdit.Clear()
        tbEdit.Enabled = True

        btnAddNew.Enabled = False
        btnEdit.Enabled = False
        btnDelete.Enabled = False
        btnSave.Enabled = True
        btnCancel.Enabled = True
        btnClose.Enabled = False

        tbEdit.Focus()

    End Sub
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

        _editingDistrict = Nothing
        tbEdit.Clear()
        tbEdit.Enabled = False
        lvDistricts.Enabled = True

        btnAddNew.Enabled = True
        btnSave.Enabled = False
        btnCancel.Enabled = False
        btnClose.Enabled = True

        Dim canEdit As Boolean = False

        If lvDistricts.SelectedItems.Count > 0 Then
            Dim districtEntry As DistrictData.DistrictMatch = DirectCast(lvDistricts.SelectedItems(0).Tag, DistrictData.DistrictMatch)
            canEdit = districtEntry.Supplementary
        End If

        btnEdit.Enabled = canEdit
        btnDelete.Enabled = canEdit

        lvDistricts.Focus()

    End Sub
    Private Sub btnEdit_Click(sender As Object, e As EventArgs) Handles btnEdit.Click

        If lvDistricts.SelectedItems.Count = 0 Then Return

        Dim districtEntry As DistrictData.DistrictMatch = DirectCast(lvDistricts.SelectedItems(0).Tag, DistrictData.DistrictMatch)

        If Not districtEntry.Supplementary Then Return

        _editingDistrict = districtEntry

        tbEdit.Text = districtEntry.Name & "," & districtEntry.Volume
        tbEdit.Enabled = True
        lvDistricts.Enabled = False

        btnAddNew.Enabled = False
        btnEdit.Enabled = False
        btnDelete.Enabled = False
        btnSave.Enabled = True
        btnCancel.Enabled = True
        btnClose.Enabled = False

        tbEdit.Focus()

    End Sub
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click

        Dim separatorIndex As Integer = tbEdit.Text.LastIndexOf(","c)

        If separatorIndex < 0 Then
            MessageBox.Show("Enter the district name and volume separated by a comma.", "District Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            tbEdit.Focus()
            Return
        End If

        Dim districtName As String = tbEdit.Text.Substring(0, separatorIndex).Trim()
        Dim volume As String = tbEdit.Text.Substring(separatorIndex + 1).Trim()
        Dim saved As Boolean

        If _editingDistrict IsNot Nothing Then

            ' An unchanged district does not need saving.
            If String.Equals(_editingDistrict.Name, districtName, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(_editingDistrict.Volume, volume, StringComparison.OrdinalIgnoreCase) Then
                btnCancel.PerformClick()
                Return
            End If

            If DistrictData.ContainsDistrictAndCode(districtName, volume) Then
                MessageBox.Show("This district and volume combination already exists.", "District Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                tbEdit.Focus()
                Return
            End If

            saved = DistrictData.EditSupplementaryDistrict(_editingDistrict.Name, _editingDistrict.Volume, districtName, volume)

        Else

            If DistrictData.ContainsDistrictAndCode(districtName, volume) Then
                MessageBox.Show("This district and volume combination already exists.", "District Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                tbEdit.Focus()
                Return
            End If

            saved = DistrictData.AddSupplementaryDistrict(districtName, volume)

        End If

        If Not saved Then
            MessageBox.Show("The district could not be saved. Check the name, volume format and whether the combination already exists.", "District Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            tbEdit.Focus()
            Return
        End If

        If _editingDistrict IsNot Nothing Then
            RaiseEvent DistrictEdited(_editingDistrict.Name, _editingDistrict.Volume, districtName, volume)
        End If

        _editingDistrict = Nothing

        LoadDistrictList()

        tbEdit.Clear()
        tbEdit.Enabled = False
        lvDistricts.Enabled = True

        btnAddNew.Enabled = True
        btnEdit.Enabled = False
        btnDelete.Enabled = False
        btnSave.Enabled = False
        btnCancel.Enabled = False
        btnClose.Enabled = True

    End Sub
    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click

        If lvDistricts.SelectedItems.Count = 0 Then Return

        Dim districtEntry As DistrictData.DistrictMatch =
        TryCast(lvDistricts.SelectedItems(0).Tag, DistrictData.DistrictMatch)

        If districtEntry Is Nothing OrElse Not districtEntry.Supplementary Then Return

        Dim answer As DialogResult = MessageBox.Show(
        Me,
        "Are you sure you want to delete this supplementary district?" &
        Environment.NewLine & Environment.NewLine &
        districtEntry.Name & ", " & districtEntry.Volume,
        "Delete District",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question)

        If answer <> DialogResult.Yes Then Return

        If Not DistrictData.DeleteSupplementaryDistrict(districtEntry.Name, districtEntry.Volume) Then

            MessageBox.Show(
            Me,
            "The supplementary district could not be deleted.",
            "Delete District",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)

            Return

        End If

        LoadDistrictList()

        btnEdit.Enabled = False
        btnDelete.Enabled = False

    End Sub
    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

End Class