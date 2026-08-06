Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private _configuringGrid As Boolean
    Private ReadOnly _document As New TranscriptionDocument
    Private _refreshingGrid As Boolean
    Private ReadOnly _pickListPopup As New PickListPopup()
    Private ReadOnly _navigationManager As NavigationManager

    Public Sub New(commandExecutor As ICommandExecutor)

        InitializeComponent()
        _navigationManager = New NavigationManager(Me)
        ApplyTranscriptionAppearance()
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

        _pickListPopup.Hide()
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

    Private Function GetFirstDataColumn() As Integer

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
    Private Function GetLastDataColumn() As Integer

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
    ' The DataGridView routes some navigation keys differently depending on
    ' the caret position inside the editing TextBox.
    '
    ' - Left/Right within the text are received via the editor's KeyDown.
    ' - Left at the beginning and Right at the end bypass KeyDown and arrive
    '   here via ProcessCmdKey.
    '
    ' We forward these keys to NavigationManager so navigation behaviour
    ' remains consistent.
    Protected Overrides Function ProcessCmdKey(
    ByRef msg As Message,
    keyData As Keys) As Boolean

        Dim keyCode As Keys =
        keyData And Keys.KeyCode

        If transcriptionGrid.ContainsFocus Then

            Dim editor As TextBox =
            TryCast(
                transcriptionGrid.EditingControl,
                TextBox)

            Select Case keyCode

                Case Keys.Tab, Keys.Enter

                    Dim e As New KeyEventArgs(keyData)

                    If _navigationManager.HandleKey(editor, e) Then
                        Return True
                    End If

                Case Keys.Right

                    If editor IsNot Nothing AndAlso
                   editor.SelectionLength = 0 AndAlso
                   editor.SelectionStart = editor.TextLength Then

                        Dim e As New KeyEventArgs(keyData)

                        If _navigationManager.HandleKey(editor, e) Then
                            Return True
                        End If

                    End If

                Case Keys.Left

                    If editor IsNot Nothing AndAlso
                   editor.SelectionLength = 0 AndAlso
                   editor.SelectionStart = 0 Then

                        Dim e As New KeyEventArgs(keyData)

                        If _navigationManager.HandleKey(editor, e) Then
                            Return True
                        End If

                    End If

            End Select

        End If

        Return MyBase.ProcessCmdKey(msg, keyData)

    End Function
    Private Sub transcriptionGrid_EditingControlShowing(
    sender As Object,
    e As DataGridViewEditingControlShowingEventArgs) _
    Handles transcriptionGrid.EditingControlShowing

        Dim editor As TextBox =
        TryCast(e.Control, TextBox)

        If editor Is Nothing Then
            Return
        End If

        RemoveHandler editor.KeyDown, AddressOf NavigationKeyDown
        AddHandler editor.KeyDown, AddressOf NavigationKeyDown

        RemoveHandler editor.TextChanged, AddressOf ForenameEditor_TextChanged
        AddHandler editor.TextChanged, AddressOf ForenameEditor_TextChanged

        editor.BackColor = UiColors.EditBackground
        editor.ForeColor = UiColors.UserText

        If editor.Parent IsNot Nothing Then
            editor.Parent.BackColor = UiColors.EditBackground
        End If

        RefreshPickList(editor)

    End Sub
    Private Sub transcriptionGrid_CellEnter(
    sender As Object,
    e As DataGridViewCellEventArgs) _
    Handles transcriptionGrid.CellEnter

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        If Not IsDataColumn(e.ColumnIndex) Then
            _pickListPopup.Hide()
            Return
        End If

        ' CellEnter also fires while the form is being constructed.
        If Not IsHandleCreated OrElse IsDisposed OrElse Disposing Then
            Return
        End If

        BeginInvoke(
        Sub()

            If IsDisposed OrElse Disposing Then
                Return
            End If

            If transcriptionGrid.CurrentCell Is Nothing Then
                Return
            End If

            If transcriptionGrid.CurrentCell.RowIndex <> e.RowIndex OrElse
               transcriptionGrid.CurrentCell.ColumnIndex <> e.ColumnIndex Then

                Return
            End If

            If transcriptionGrid.IsCurrentCellInEditMode Then
                Return
            End If

            transcriptionGrid.BeginEdit(selectAll:=False)

        End Sub)

    End Sub
    Private Sub ForenameEditor_TextChanged(
    sender As Object,
    e As EventArgs)

        Dim editor As TextBox =
        TryCast(sender, TextBox)

        If editor Is Nothing Then
            Return
        End If

        RefreshPickList(editor)

    End Sub
    Private Sub RefreshPickList(
    editor As TextBox)

        If transcriptionGrid.CurrentCell Is Nothing Then
            _pickListPopup.Hide()
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(
            transcriptionGrid.CurrentCell.ColumnIndex)

        If column.Tag Is Nothing Then
            _pickListPopup.Hide()
            Return
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        If Not FieldMetaData.Meta(field).UsesPicklist Then
            _pickListPopup.Hide()
            Return
        End If

        Dim items As List(Of PickListItem)

        Select Case field

            Case GridField.Forename

                items =
                ForenameData.
                    GetMatches(editor.Text, 9).
                    Select(
                        Function(name As String)
                            Return New PickListItem With {
                                .Text = name
                            }
                        End Function).
                    ToList()

            Case GridField.District

                items =
                DistrictData.
                    GetMatches(editor.Text, 9).
                    Select(
                        Function(match As DistrictData.DistrictMatch)
                            Return New PickListItem With {
                                .Text = match.Name,
                                .Volume = match.Volume
                            }
                        End Function).
                    ToList()

            Case Else

                _pickListPopup.Hide()
                Return

        End Select

        _pickListPopup.SetItems(items)

        If items.Count = 0 Then
            _pickListPopup.Hide()
            Return
        End If

        _pickListPopup.ApplyTheme()

        Dim cellBounds As Rectangle =
        transcriptionGrid.GetCellDisplayRectangle(
            transcriptionGrid.CurrentCell.ColumnIndex,
            transcriptionGrid.CurrentCell.RowIndex,
            cutOverflow:=True)

        _pickListPopup.Show(
        transcriptionGrid,
        cellBounds)

    End Sub
    Friend Function CopyFromAboveIfBlank(editor As TextBox,
    Optional allowPickList As Boolean = False) As Boolean

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return False
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        If currentRow <= 0 Then
            Return False
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(currentColumn)

        If column.Tag Is Nothing Then
            Return False
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        ' Picklist fields will be handled separately using the
        ' IgnoreAutoComplete setting.
        If FieldMetaData.Meta(field).UsesPicklist AndAlso Not allowPickList Then

            Return False

        End If

        Dim currentText As String

        If editor IsNot Nothing Then
            currentText = editor.Text
        Else
            currentText =
            If(
                transcriptionGrid.CurrentCell.Value,
                "").ToString()
        End If

        If Not String.IsNullOrWhiteSpace(currentText) Then
            Return False
        End If

        Dim previousCell As DataGridViewCell =
        transcriptionGrid.Rows(currentRow - 1).
        Cells(currentColumn)

        Dim previousText As String =
        If(previousCell.Value, "").ToString().Trim()

        If String.IsNullOrWhiteSpace(previousText) Then
            Return False
        End If

        If previousText.StartsWith("+") OrElse
       previousText.StartsWith("#") Then

            Return False

        End If

        If editor IsNot Nothing Then

            editor.Text = previousText
            editor.SelectionStart = editor.TextLength
            editor.SelectionLength = 0

        Else

            transcriptionGrid.CurrentCell.Value =
            previousText

        End If

        Return True

    End Function
    Private Sub btnOptionsTest_Click(
    sender As Object,
    e As EventArgs) Handles btnOptionsTest.Click

        Using form As New OptionsForm()

            If form.ShowDialog(Me) = DialogResult.OK Then
                ApplyTranscriptionAppearance()
            End If

        End Using

    End Sub
    Private Sub ApplyTranscriptionAppearance()

        ThemeManager.Apply(Me)

        transcriptionGrid.EnableHeadersVisualStyles = False

        transcriptionGrid.BackgroundColor =
        UiColors.PanelBackground

        transcriptionGrid.GridColor =
        UiColors.GridLine

        transcriptionGrid.DefaultCellStyle.BackColor =
        UiColors.PanelBackground

        transcriptionGrid.DefaultCellStyle.ForeColor =
        UiColors.UserText

        transcriptionGrid.DefaultCellStyle.SelectionBackColor =
        UiColors.Selected

        transcriptionGrid.DefaultCellStyle.SelectionForeColor =
        UiColors.UserText

        transcriptionGrid.AlternatingRowsDefaultCellStyle.BackColor =
        UiColors.AlternateRowBackground

        transcriptionGrid.ColumnHeadersDefaultCellStyle.BackColor =
        UiColors.ThemeShaded

        transcriptionGrid.ColumnHeadersDefaultCellStyle.ForeColor =
        UiColors.TextPrimary

        transcriptionGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
        UiColors.ThemeShaded

        transcriptionGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
        UiColors.TextPrimary

        transcriptionGrid.RowHeadersDefaultCellStyle.BackColor =
        UiColors.ThemeSoft

        transcriptionGrid.RowHeadersDefaultCellStyle.ForeColor =
        UiColors.TextPrimary

        transcriptionGrid.Invalidate()

    End Sub
    Private Sub NavigationKeyDown(
    sender As Object,
    e As KeyEventArgs) Handles transcriptionGrid.KeyDown

        If _navigationManager.HandleKey(sender, e) Then
            e.Handled = True
            e.SuppressKeyPress = True
        End If

    End Sub
    Friend ReadOnly Property CurrentGridCell As DataGridViewCell
        Get
            Return transcriptionGrid.CurrentCell
        End Get
    End Property
    Friend Sub EndGridEdit()

        transcriptionGrid.EndEdit()

    End Sub
    Friend Sub MoveToNextDataCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        ' We have just left the District field.
        '
        ' If selecting a district automatically filled the associated
        ' Volume/DistNum field, determine where navigation should go:
        '
        '   • If another data field lies between District and the
        '     Volume/DistNum field, move to that intervening field.
        '
        '   • If the linked field is a Volume (or a complete DistNum),
        '     skip over it because it has already been filled.
        '
        '   • If the linked field is a three-character DistNum, move
        '     into it with the caret at the end so the user can type
        '     the remaining two characters.
        Dim currentField As GridField =
    DirectCast(
        transcriptionGrid.Columns(currentColumn).Tag,
        GridField)

        If currentField = GridField.District Then

            Dim nextColumn As Integer =
        GetNextColumnAfterDistrict(
            currentRow,
            currentColumn)

            If nextColumn >= 0 Then

                transcriptionGrid.EndEdit()

                transcriptionGrid.CurrentCell =
            transcriptionGrid.Rows(currentRow).
            Cells(nextColumn)

                transcriptionGrid.BeginEdit(selectAll:=False)

                If DirectCast(
            transcriptionGrid.Columns(nextColumn).Tag,
            GridField) = GridField.DistNum Then

                    BeginInvoke(
                Sub()

                    Dim editor As TextBox =
                        TryCast(
                            transcriptionGrid.EditingControl,
                            TextBox)

                    If editor Is Nothing Then
                        Return
                    End If

                    editor.Focus()
                    editor.SelectionStart =
                        editor.TextLength
                    editor.SelectionLength = 0

                End Sub)

                End If

                Return

            End If

        End If

        Dim targetRow As Integer =
        currentRow

        Dim targetColumn As Integer =
        -1

        For columnIndex As Integer =
        currentColumn + 1 To transcriptionGrid.Columns.Count - 1

            If IsDataColumn(columnIndex) Then
                targetColumn = columnIndex
                Exit For
            End If

        Next

        If targetColumn < 0 AndAlso
       currentRow < transcriptionGrid.Rows.Count - 1 Then

            targetRow = currentRow + 1
            targetColumn = GetFirstDataColumn()

        End If

        If targetColumn < 0 Then
            Return
        End If

        transcriptionGrid.EndEdit()

        transcriptionGrid.CurrentCell =
    transcriptionGrid.Rows(targetRow).Cells(targetColumn)

        If Not IsHandleCreated OrElse IsDisposed OrElse Disposing Then
            Return
        End If

        BeginInvoke(
    Sub()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        If transcriptionGrid.CurrentCell.RowIndex <> targetRow OrElse
           transcriptionGrid.CurrentCell.ColumnIndex <> targetColumn Then

            Return
        End If

        If Not transcriptionGrid.IsCurrentCellInEditMode Then
            transcriptionGrid.BeginEdit(selectAll:=False)
        End If

        Dim editor As TextBox =
            TryCast(transcriptionGrid.EditingControl, TextBox)

        If editor Is Nothing Then
            Return
        End If

        editor.Focus()
        editor.Select(0, 0)

    End Sub)

    End Sub
    Private Function GetNextColumnAfterDistrict(
    currentRow As Integer,
    districtColumn As Integer) As Integer

        Dim codeColumn As Integer = -1
        Dim codeField As GridField

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            If field = GridField.Volume Then

                codeColumn = column.Index
                codeField = GridField.Volume
                Exit For

            End If

            If field = GridField.DistNum Then

                codeColumn = column.Index
                codeField = GridField.DistNum
                Exit For

            End If

        Next

        If codeColumn < 0 Then
            Return -1
        End If

        ' If something lies between District and the code field,
        ' visit that field normally.
        If codeColumn > districtColumn + 1 Then
            Return districtColumn + 1
        End If

        Dim code As String =
        If(
            transcriptionGrid.Rows(currentRow).
                Cells(codeColumn).Value,
            "").ToString()

        If String.IsNullOrWhiteSpace(code) Then
            Return codeColumn
        End If

        ' DistNum may contain only the three-character district
        ' code supplied by the picklist. The user must then add
        ' the final two characters manually.
        If codeField = GridField.DistNum AndAlso
       code.Length = 3 Then

            Return codeColumn

        End If

        ' The linked code is complete, so skip over it.
        For columnIndex As Integer =
        codeColumn + 1 To transcriptionGrid.Columns.Count - 1

            If IsDataColumn(columnIndex) Then
                Return columnIndex
            End If

        Next

        Return -1

    End Function
    Friend Sub MoveToPreviousDataCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        Dim currentColumn As Integer =
        transcriptionGrid.CurrentCell.ColumnIndex

        Dim targetRow As Integer =
        currentRow

        Dim targetColumn As Integer =
        -1

        For columnIndex As Integer =
        currentColumn - 1 To 0 Step -1

            If IsDataColumn(columnIndex) Then
                targetColumn = columnIndex
                Exit For
            End If

        Next

        If targetColumn < 0 AndAlso currentRow > 0 Then
            targetRow = currentRow - 1
            targetColumn = GetLastDataColumn()
        End If

        If targetColumn < 0 Then
            Return
        End If

        transcriptionGrid.CurrentCell =
        transcriptionGrid.Rows(targetRow).Cells(targetColumn)

        transcriptionGrid.BeginEdit(selectAll:=False)

        Dim editor As TextBox =
    TryCast(transcriptionGrid.EditingControl, TextBox)

        If editor Is Nothing Then
            Return
        End If

        If Not IsHandleCreated OrElse IsDisposed OrElse Disposing Then
            Return
        End If

        editor.BeginInvoke(
    Sub()

        If editor.IsDisposed Then
            Return
        End If

        editor.Focus()
        editor.Select(editor.TextLength, 0)

    End Sub)

    End Sub
    Friend ReadOnly Property CurrentFieldUsesPickList As Boolean
        Get

            If transcriptionGrid.CurrentCell Is Nothing Then
                Return False
            End If

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(
                transcriptionGrid.CurrentCell.ColumnIndex)

            If column.Tag Is Nothing Then
                Return False
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            Return FieldMetaData.Meta(field).UsesPicklist

        End Get
    End Property
    Friend Function AcceptCurrentPickListSelection(
    editor As TextBox) As Boolean

        If editor Is Nothing Then
            Return False
        End If

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return False
        End If

        Dim selectedItem As PickListItem =
        _pickListPopup.SelectedItem

        If selectedItem Is Nothing Then
            Return False
        End If

        Dim currentColumn As DataGridViewColumn =
        transcriptionGrid.Columns(
            transcriptionGrid.CurrentCell.ColumnIndex)

        If currentColumn.Tag Is Nothing Then
            Return False
        End If

        Dim currentField As GridField =
        DirectCast(currentColumn.Tag, GridField)

        editor.Text = selectedItem.Text
        editor.SelectionStart = editor.TextLength
        editor.SelectionLength = 0

        If currentField = GridField.District AndAlso
       Not String.IsNullOrWhiteSpace(selectedItem.Volume) Then

            Dim currentRow As Integer =
            transcriptionGrid.CurrentCell.RowIndex

            For Each column As DataGridViewColumn In transcriptionGrid.Columns

                If column.Tag Is Nothing Then
                    Continue For
                End If

                Dim field As GridField =
                DirectCast(column.Tag, GridField)

                If field = GridField.Volume OrElse
               field = GridField.DistNum Then

                    transcriptionGrid.Rows(currentRow).
                    Cells(column.Index).Value =
                    selectedItem.Volume

                    Exit For

                End If

            Next

        End If

        _pickListPopup.Hide()

        Return True

    End Function
    Private Sub TranscriptionForm_FormClosing(
    sender As Object,
    e As FormClosingEventArgs) Handles Me.FormClosing

        _pickListPopup.Dispose()
        SaveFormBounds()

    End Sub

    Private Sub filePanel_ExpandedChanged(
        sender As Object,
        e As EventArgs) Handles filePanel.ExpandedChanged

        ProjectValues.FilePanelExpanded = filePanel.Expanded
        ProjectValuesStore.Save()

    End Sub

End Class