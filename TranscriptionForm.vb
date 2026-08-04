Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private _configuringGrid As Boolean
    Private ReadOnly _document As New TranscriptionDocument
    Private _refreshingGrid As Boolean

    Public Sub New(commandExecutor As ICommandExecutor)

        InitializeComponent()

        ThemeManager.Apply(Me)
        ThemeManager.ApplyNavigationButton(btnOptionsTest)
        _commandExecutor = commandExecutor

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2"
        filePanel.Expanded = ProjectValues.FilePanelExpanded

        LoadCurrentBatch()

        DebugLog.WriteAlways("======= TRANSCRIPTION FORM OPENED =======")
        DebugLog.WriteAlways($"Batch Type  : '{ProjectValues.BatchType}'")
        DebugLog.WriteAlways($"Year        : {ProjectValues.Year}")
        DebugLog.WriteAlways($"Quarter     : {ProjectValues.Quarter}")
        DebugLog.WriteAlways($"Page        : {ProjectValues.Page}")
        DebugLog.WriteAlways($"Page Letter : '{ProjectValues.PageLetter}'")
        DebugLog.WriteAlways($"Source Ref  : '{ProjectValues.SourceRef}'")
        DebugLog.WriteAlways("=========================================")

    End Sub

    Private Sub RestoreFormBounds()

        Dim layoutKey As String =
        GetGridLayoutKey()

        Dim savedBounds As FormBoundsData = Nothing

        If Not ProjectValues.TranscriptionFormBounds.
        TryGetValue(layoutKey, savedBounds) Then

            savedBounds = New FormBoundsData()

        End If

        DebugLog.WriteAlways(
        "[FORM] Requested transcription bounds: " &
        $"Layout={layoutKey}, " &
        $"Left={savedBounds.Left}, " &
        $"Top={savedBounds.Top}, " &
        $"Width={savedBounds.Width}, " &
        $"Height={savedBounds.Height}, " &
        $"Maximized={savedBounds.Maximized}")

        FormBoundsHelper.RestoreForm(
        Me,
        savedBounds)

    End Sub
    Private Sub LoadCurrentBatch()

        RestoreFormBounds()
        ConfigureGridColumns()
        RefreshGrid()

        DebugLog.WriteAlways(
        "[BATCH] Loaded into transcription form: " &
        $"Type='{ProjectValues.BatchType}', " &
        $"Year={ProjectValues.Year}, " &
        $"Quarter={ProjectValues.Quarter}, " &
        $"Page={ProjectValues.Page}")

    End Sub
    Private Sub RefreshGrid()

        _refreshingGrid = True

        Try
            transcriptionGrid.Rows.Clear()

            Dim fields() As GridField =
            GridLayout.GetVisibleFields()

            For rowIndex As Integer = 0 To _document.Rows.Count - 1

                Dim transcriptionRow As TranscriptionRow =
                _document.Rows(rowIndex)

                Dim gridRowIndex As Integer =
                transcriptionGrid.Rows.Add()

                Dim gridRow As DataGridViewRow =
                transcriptionGrid.Rows(gridRowIndex)

                gridRow.Cells("RowNumber").Value =
                rowIndex + 1

                For Each field As GridField In fields
                    gridRow.Cells(field.ToString()).Value =
                    transcriptionRow.GetValue(field)
                Next

            Next

            AddBlankEntryRow(fields)

            If transcriptionGrid.Rows.Count > 0 Then

                Dim firstDataColumnIndex As Integer =
                    transcriptionGrid.Columns(
                        GridField.Surname.ToString()).Index

                transcriptionGrid.CurrentCell =
                    transcriptionGrid.Rows(0).Cells(firstDataColumnIndex)

            End If

            UpdateStatusPosition()

        Finally
            _refreshingGrid = False
        End Try

    End Sub
    Private Sub AddBlankEntryRow(fields() As GridField)

        Dim gridRowIndex As Integer =
        transcriptionGrid.Rows.Add()

        Dim gridRow As DataGridViewRow =
        transcriptionGrid.Rows(gridRowIndex)

        gridRow.Cells("RowNumber").Value =
        _document.Rows.Count + 1

        gridRow.Tag = "NewRow"

        For Each field As GridField In fields
            gridRow.Cells(field.ToString()).Value = ""
        Next

    End Sub
    Private Sub SaveFormBounds()

        Dim layoutKey As String = GetGridLayoutKey()

        Dim savedBounds As FormBoundsData = FormBoundsHelper.GetBoundsDataToSave(Me)

        ProjectValues.TranscriptionFormBounds(layoutKey) = savedBounds

        ProjectValuesStore.Save()

        DebugLog.Write(
        "[FORM] Transcription bounds saved: " &
        $"Layout={layoutKey}, " &
        $"Left={savedBounds.Left}, " &
        $"Top={savedBounds.Top}, " &
        $"Width={savedBounds.Width}, " &
        $"Height={savedBounds.Height}, " &
        $"Maximized={savedBounds.Maximized}")

    End Sub
    Private Sub ConfigureGridColumns()

        _configuringGrid = True

        transcriptionGrid.Columns.Clear()
        transcriptionGrid.AutoGenerateColumns = False
        transcriptionGrid.AllowUserToAddRows = False

        AddRowNumberColumn()

        Dim fields() As GridField = GridLayout.GetVisibleFields()
        Dim layoutKey As String = GetGridLayoutKey()
        Dim savedWidths As Dictionary(Of String, Integer) = Nothing

        ProjectValues.GridColumnWidths.TryGetValue(layoutKey, savedWidths)

        Try

            For Each field As GridField In fields

                Dim fieldInformation As FieldMeta = FieldMetaData.Meta(field)

                Dim column As New DataGridViewTextBoxColumn With {
                .Name = field.ToString(),
                .HeaderText = fieldInformation.Header,
                .Width = fieldInformation.PreferredWidth,
                .MinimumWidth = fieldInformation.MinWidth,
                .SortMode = DataGridViewColumnSortMode.NotSortable,
                .ReadOnly = Not fieldInformation.IsDataColumn,
                .Tag = field
            }
                If savedWidths IsNot Nothing Then

                    Dim savedWidth As Integer

                    If savedWidths.TryGetValue(field.ToString(), savedWidth) Then
                        column.Width = Math.Max(column.MinimumWidth, savedWidth)
                    End If

                End If
                column.DefaultCellStyle.Alignment =
            GetGridAlignment(fieldInformation.Align)

                column.HeaderCell.Style.Alignment =
            DataGridViewContentAlignment.MiddleCenter

                transcriptionGrid.Columns.Add(column)

            Next

            ThemeManager.ApplyDataGrid(transcriptionGrid)

            DebugLog.WriteAlways(
                "[GRID] Created " &
                transcriptionGrid.Columns.Count.ToString() &
                " columns.")

        Finally
            _configuringGrid = False
        End Try

    End Sub
    Private Sub AddRowNumberColumn()

        Dim column As New DataGridViewTextBoxColumn With {
        .Name = "RowNumber",
        .HeaderText = "Row",
        .Width = 46,
        .MinimumWidth = 40,
        .ReadOnly = True,
        .Frozen = True,
        .SortMode = DataGridViewColumnSortMode.NotSortable
    }

        column.DefaultCellStyle.Alignment =
        DataGridViewContentAlignment.MiddleRight

        column.HeaderCell.Style.Alignment =
        DataGridViewContentAlignment.MiddleCenter

        transcriptionGrid.Columns.Add(column)

    End Sub
    Private Shared Function GetGridAlignment(
    alignment As CellTextAlign) As DataGridViewContentAlignment

        Select Case alignment

            Case CellTextAlign.Center
                Return DataGridViewContentAlignment.MiddleCenter

            Case CellTextAlign.Right
                Return DataGridViewContentAlignment.MiddleRight

            Case Else
                Return DataGridViewContentAlignment.MiddleLeft

        End Select

    End Function
    Private Function GetGridLayoutKey() As String

        Return ProjectValues.BatchType & "|" &
           ProjectValues.Year.ToString() & "|" &
           ProjectValues.Quarter.ToString()

    End Function
    Private Sub transcriptionGrid_ColumnWidthChanged(
    sender As Object,
    e As DataGridViewColumnEventArgs) Handles transcriptionGrid.ColumnWidthChanged

        If _configuringGrid Then
            Return
        End If

        If e.Column Is Nothing Then
            Return
        End If

        If e.Column.Name = "RowNumber" Then
            Return
        End If

        If e.Column.Tag Is Nothing Then
            Return
        End If

        Dim layoutKey As String = GetGridLayoutKey()
        Dim savedWidths As Dictionary(Of String, Integer) = Nothing

        If Not ProjectValues.GridColumnWidths.TryGetValue(layoutKey, savedWidths) Then
            savedWidths = New Dictionary(Of String, Integer)
            ProjectValues.GridColumnWidths(layoutKey) = savedWidths
        End If

        savedWidths(e.Column.Name) = e.Column.Width

        ProjectValuesStore.Save()

    End Sub
    Private Sub transcriptionGrid_CurrentCellChanged(
    sender As Object,
    e As EventArgs) Handles transcriptionGrid.CurrentCellChanged

        UpdateStatusPosition()

    End Sub
    Private Sub transcriptionGrid_CellValueChanged(
    sender As Object,
    e As DataGridViewCellEventArgs) Handles transcriptionGrid.CellValueChanged

        If _refreshingGrid Then
            Return
        End If

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(e.ColumnIndex)

        If column.Name = "RowNumber" OrElse
       column.Tag Is Nothing Then

            Return
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        Dim cell As DataGridViewCell =
        transcriptionGrid.Rows(e.RowIndex).Cells(e.ColumnIndex)

        Dim value As String =
        If(cell.Value, "").ToString()

        UpdateDocumentFromGrid(
        e.RowIndex,
        field,
        value)

    End Sub
    Private Sub UpdateDocumentFromGrid(
    rowIndex As Integer,
    field As GridField,
    value As String)

        Dim transcriptionRow As TranscriptionRow

        If rowIndex < _document.Rows.Count Then

            ' An existing document row has been edited.
            transcriptionRow = _document.Rows(rowIndex)

        ElseIf rowIndex = _document.Rows.Count Then

            ' The user has entered something into the visual entry row.
            If String.IsNullOrEmpty(value) Then
                Return
            End If

            transcriptionRow = _document.AddRow()

            Dim gridRow As DataGridViewRow =
            transcriptionGrid.Rows(rowIndex)

            gridRow.Tag = Nothing

        Else

            ' The grid and document should never become this far out of step.
            Return

        End If

        transcriptionRow.SetValue(field, value)
        _document.HasChanges = True

        EnsureBlankEntryRow()
        UpdateRowNumbers()

    End Sub
    Private Sub EnsureBlankEntryRow()

        If transcriptionGrid.Rows.Count = 0 Then
            AddBlankEntryRow(GridLayout.GetVisibleFields())
            Return
        End If

        Dim lastRow As DataGridViewRow =
        transcriptionGrid.Rows(
            transcriptionGrid.Rows.Count - 1)

        If IsBlankEntryRow(lastRow) Then
            Return
        End If

        AddBlankEntryRow(GridLayout.GetVisibleFields())

    End Sub
    Private Sub UpdateRowNumbers()

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

            transcriptionGrid.Rows(rowIndex).
            Cells("RowNumber").Value = rowIndex + 1

        Next

    End Sub
    Private Shared Function IsBlankEntryRow(
    gridRow As DataGridViewRow) As Boolean

        Return String.Equals(
        TryCast(gridRow.Tag, String),
        "NewRow",
        StringComparison.Ordinal)

    End Function
    ' Returns the index of the visible Volume or DistNum column.
    ' Returns 0 if the current layout has no volume-equivalent column.
    Public Function GetVolumeColumn() As Integer

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            Dim fieldInformation As FieldMeta =
            FieldMetaData.Meta(field)

            If fieldInformation.IsVolumeField Then
                Return column.Index
            End If

        Next

        Return 0

    End Function
    Private Sub UpdateStatusPosition()

        If transcriptionGrid.CurrentCell Is Nothing Then
            statusPositionLabel.Text = ""
            Return
        End If

        Dim rowNumber As Integer =
        transcriptionGrid.CurrentCell.RowIndex + 1

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(
            transcriptionGrid.CurrentCell.ColumnIndex)

        If column.Name = "RowNumber" Then
            statusPositionLabel.Text =
            $"Row {rowNumber}"
            Return
        End If

        statusPositionLabel.Text =
        $"Row {rowNumber}, {column.HeaderText}"

    End Sub
    Private Function IsDataColumn(columnIndex As Integer) As Boolean

        If columnIndex < 0 OrElse
       columnIndex >= transcriptionGrid.Columns.Count Then

            Return False
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(columnIndex)

        If column.Tag Is Nothing Then
            Return False
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        Return FieldMetaData.Meta(field).IsDataColumn

    End Function
    Private Sub MoveToPreviousDataCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        For columnIndex As Integer =
        currentColumn - 1 To 0 Step -1

            If IsDataColumn(columnIndex) Then

                transcriptionGrid.CurrentCell =
                transcriptionGrid.Rows(currentRow).
                Cells(columnIndex)

                Return
            End If

        Next

        If currentRow > 0 Then

            transcriptionGrid.CurrentCell =
            transcriptionGrid.Rows(currentRow - 1).
            Cells(GetLastDataColumn())

        End If

    End Sub
    Public Function GetFirstDataColumn() As Integer

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            If FieldMetaData.Meta(field).IsDataColumn Then
                Return column.Index
            End If

        Next

        Return 0

    End Function
    Public Function GetLastDataColumn() As Integer

        For columnIndex As Integer =
        transcriptionGrid.Columns.Count - 1 To 0 Step -1

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            If FieldMetaData.Meta(field).IsDataColumn Then
                Return column.Index
            End If

        Next

        Return 0

    End Function
    Private Sub transcriptionGrid_EditingControlShowing(
    sender As Object,
    e As DataGridViewEditingControlShowingEventArgs) _
    Handles transcriptionGrid.EditingControlShowing

        Dim editor As TextBox = TryCast(e.Control, TextBox)

        If editor Is Nothing Then
            Return
        End If

        RemoveHandler editor.KeyDown, AddressOf NavigationKeyDown
        AddHandler editor.KeyDown, AddressOf NavigationKeyDown

        BeginInvoke(
        Sub()
            editor.BackColor = UiColors.EditBackground
            editor.ForeColor = UiColors.UserText

            If editor.Parent IsNot Nothing Then
                editor.Parent.BackColor = UiColors.EditBackground
            End If
        End Sub)

    End Sub
    Private Sub btnOptionsTest_Click(
    sender As Object,
    e As EventArgs) Handles btnOptionsTest.Click

        Using form As New OptionsForm()
            form.ShowDialog(Me)
        End Using

    End Sub
    Private Sub NavigationKeyDown(sender As Object, e As KeyEventArgs) Handles transcriptionGrid.KeyDown
        ' This also handles the navigation keys when the user is editing a cell.
        Dim editor As TextBox =
        TryCast(sender, TextBox)

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        Dim firstDataColumn As Integer =
        GetFirstDataColumn()

        Dim lastDataColumn As Integer =
        GetLastDataColumn()

        Select Case e.KeyCode

            Case Keys.Left

                ' If we're editing, only leave the cell when the caret
                ' is already at the beginning.
                If editor IsNot Nothing Then

                    If editor.SelectionStart <> 0 OrElse
                   editor.SelectionLength <> 0 Then
                        Return
                    End If

                End If

                If currentColumn = firstDataColumn AndAlso
               currentRow > 0 Then

                    transcriptionGrid.EndEdit()

                    transcriptionGrid.CurrentCell =
                    transcriptionGrid.Rows(currentRow - 1).
                    Cells(lastDataColumn)

                    e.Handled = True
                    e.SuppressKeyPress = True

                End If

            Case Keys.Right

                ' If we're editing, only leave the cell when the caret
                ' is already at the end.
                If editor IsNot Nothing Then

                    If editor.SelectionStart <> editor.TextLength OrElse
                   editor.SelectionLength <> 0 Then
                        Return
                    End If

                End If

                If currentColumn = lastDataColumn AndAlso
               currentRow < transcriptionGrid.Rows.Count - 1 Then

                    transcriptionGrid.EndEdit()

                    transcriptionGrid.CurrentCell =
                    transcriptionGrid.Rows(currentRow + 1).
                    Cells(firstDataColumn)

                    e.Handled = True
                    e.SuppressKeyPress = True

                End If
            Case Keys.Tab

                transcriptionGrid.EndEdit()

                If e.Shift Then
                    MoveToPreviousDataCell()
                Else
                    MoveToNextDataCell()
                End If

                e.Handled = True
                e.SuppressKeyPress = True
        End Select

    End Sub
    Private Sub MoveToNextDataCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        For columnIndex As Integer =
        currentColumn + 1 To transcriptionGrid.Columns.Count - 1

            If IsDataColumn(columnIndex) Then

                transcriptionGrid.CurrentCell =
                transcriptionGrid.Rows(currentRow).
                Cells(columnIndex)

                Return
            End If

        Next

        ' No later data column exists, so move to the first data column
        ' of the following row.
        If currentRow < transcriptionGrid.Rows.Count - 1 Then

            transcriptionGrid.CurrentCell =
            transcriptionGrid.Rows(currentRow + 1).
            Cells(GetFirstDataColumn())

        End If

    End Sub
    Private Sub TranscriptionForm_FormClosing(
        sender As Object,
        e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub

    Private Sub filePanel_ExpandedChanged(
        sender As Object,
        e As EventArgs) Handles filePanel.ExpandedChanged

        ProjectValues.FilePanelExpanded = filePanel.Expanded
        ProjectValuesStore.Save()

    End Sub

End Class