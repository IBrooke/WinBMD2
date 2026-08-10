Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private _configuringGrid As Boolean
    Private _hasChanges As Boolean
    Private _refreshingGrid As Boolean
    Private ReadOnly _pickListPopup As New PickListPopup()
    Private ReadOnly _navigationManager As NavigationManager
    Private ReadOnly _statusQueue As New Queue(Of (Message As String, Duration As Integer))
    Private ReadOnly _statusTimer As New Timer()
    Private _statusMessageShowing As Boolean
    Private _baseStatusText As String = "Ready"
    ' Stores the latest validation result for each grid cell.
    Private ReadOnly _cellValidationResults As New Dictionary(Of (Row As Integer, Column As Integer), ValidationResult)

    ' Stores the overall validation state for each row.
    Private ReadOnly _rowValidationStates As New Dictionary(Of Integer, ValidationState)
    Private ReadOnly _validationToolTip As New ToolTip()
    Private _selectedCommandCategory As String = "File"

    Public Sub New(commandExecutor As ICommandExecutor)

        InitializeComponent()

        _statusTimer.Interval = 5000
        AddHandler _statusTimer.Tick, AddressOf StatusTimer_Tick
        _navigationManager = New NavigationManager(Me)
        ApplyTranscriptionAppearance()
        UpdateCommandCategoryAppearance()
        _commandExecutor = commandExecutor
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

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
    Private Sub UpdateCommandStrip()

        btnFileOpen.Visible = False
        btnFileSave.Visible = False
        btnFileSaveAs.Visible = False
        btnFileClose.Visible = False
        btnFileExit.Visible = False

        Select Case _selectedCommandCategory

            Case "File"

                btnFileOpen.Visible = True
                btnFileSave.Visible = True
                btnFileSaveAs.Visible = True
                btnFileClose.Visible = True
                btnFileExit.Visible = True

        End Select

    End Sub
    Public Function LoadBatchFile(
    filePath As String) As Boolean

        _refreshingGrid = True

        Try

            If Not LoadSaveFiles.Load(
            filePath,
            transcriptionGrid,
            AddressOf ConfigureGridColumns) Then

                Return False

            End If

            Dim fields() As GridField =
            GridLayout.GetVisibleFields()

            AddBlankEntryRow(fields)

            For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1
                UpdateDirectiveCell(rowIndex)
            Next

            _hasChanges = False

            Return True

        Finally
            _refreshingGrid = False
        End Try

    End Function
    ' Sets the normal status text shown whenever there is no
    ' temporary message waiting to be displayed.
    Private Sub SetBaseStatus(message As String)

        _baseStatusText = message

        If Not _statusMessageShowing Then
            statusMessageLabel.Text = _baseStatusText
        End If

    End Sub

    ' Displays a temporary status message.
    '
    ' If another temporary message is already being shown, this one
    ' is queued and will be displayed when the current message expires.
    Private Sub ShowStatusMessage(
    message As String,
    Optional duration As Integer = 5000)

        DebugLog.WriteAlways($"[STATUS] ShowStatusMessage called: '{message}'")

        If String.IsNullOrWhiteSpace(message) Then
            Return
        End If

        _statusQueue.Enqueue((message, duration))

        If Not _statusMessageShowing Then
            ShowNextStatusMessage()
        End If

    End Sub

    ' Displays the next queued message, or restores the normal
    ' status text when there are no messages left.
    Private Sub ShowNextStatusMessage()

        _statusTimer.Stop()

        If _statusQueue.Count = 0 Then

            _statusMessageShowing = False
            statusMessageLabel.Text = _baseStatusText

            Return

        End If

        Dim item =
        _statusQueue.Dequeue()

        DebugLog.WriteAlways($"[STATUS] Displaying: '{item.Message}'")

        _statusMessageShowing = True
        statusMessageLabel.Text = item.Message

        _statusTimer.Interval = item.Duration
        _statusTimer.Start()

    End Sub

    Private Sub StatusTimer_Tick(
    sender As Object,
    e As EventArgs)

        ShowNextStatusMessage()

    End Sub
    Private Sub LoadCurrentBatch()

        RestoreFormBounds()
        ConfigureGridColumns()

        _refreshingGrid = True

        Try

            transcriptionGrid.Rows.Clear()

            Dim fields() As GridField =
            GridLayout.GetVisibleFields()

            AddBlankEntryRow(fields)

        Finally

            _refreshingGrid = False

        End Try

        _hasChanges = False

        UpdateStatusPosition()

        DebugLog.WriteAlways(
    "[BATCH] Loaded into transcription form: " &
    $"Type='{ProjectValues.BatchType}', " &
    $"Year={ProjectValues.Year}, " &
    $"Quarter={ProjectValues.Quarter}, " &
    $"Page={ProjectValues.Page}, " &
    $"PageLetter='{ProjectValues.PageLetter}', " &
    $"PageSuffix='{ProjectValues.PageSuffix}'")

    End Sub
    Private Sub AddBlankEntryRow(fields() As GridField)

        Dim gridRowIndex As Integer =
        transcriptionGrid.Rows.Add()

        Dim gridRow As DataGridViewRow =
        transcriptionGrid.Rows(gridRowIndex)

        gridRow.Cells("RowNumber").Value = gridRowIndex + 1

        gridRow.Tag = "NewRow"
        UpdateDirectiveCell(gridRowIndex)

        For Each field As GridField In fields
            gridRow.Cells(field.ToString()).Value = ""
        Next

    End Sub
    Private Sub transcriptionGrid_CellClick(
    sender As Object,
    e As DataGridViewCellEventArgs) _
    Handles transcriptionGrid.CellClick

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(e.ColumnIndex)

        If column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        If field <> GridField.Directive Then
            Return
        End If

        ShowDirectiveEditor(e.RowIndex)

    End Sub
    Private Sub CommandCategory_Click(sender As Object, e As EventArgs) Handles btnCategoryFile.Click, btnCategoryGrid.Click, btnCategoryVerify.Click, btnCategoryScan.Click, btnCategoryUpload.Click, btnCategoryOptions.Click, btnCategoryHelp.Click

        Dim button As Button = TryCast(sender, Button)

        If button Is Nothing Then
            Return
        End If

        If button Is btnCategoryOptions Then

            Using form As New OptionsForm()

                If form.ShowDialog(Me) = DialogResult.OK Then
                    ApplyTranscriptionAppearance()
                    UpdateCommandCategoryAppearance()
                    UpdateCommandStrip()
                End If

            End Using

            Return

        End If

        _selectedCommandCategory = button.Text

        If Not filePanel.Expanded Then
            filePanel.Expanded = True
        End If

        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

    End Sub
    Private Sub UpdateCommandCategoryAppearance()

        For Each control As Control In categoryStrip.Controls

            Dim button As Button = TryCast(control, Button)

            If button Is Nothing Then
                Continue For
            End If

            If button.Text = _selectedCommandCategory Then
                button.BackColor = UiColors.Selected
            Else
                button.BackColor = UiColors.PanelBackground
            End If

        Next

    End Sub
    Private Sub ShowDirectiveEditor(rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then
            Return
        End If

        Dim directiveCell As DataGridViewCell =
        transcriptionGrid.Rows(rowIndex).
        Cells(GridField.Directive.ToString())

        Dim directives As List(Of RowDirective) =
        TryCast(directiveCell.Tag, List(Of RowDirective))

        If directives Is Nothing Then
            directives = New List(Of RowDirective)()
        End If

        Dim suggestedPageNumber As Integer =
        ProjectValues.Page

        For previousRowIndex As Integer = 0 To rowIndex

            Dim previousCell As DataGridViewCell =
            transcriptionGrid.Rows(previousRowIndex).
            Cells(GridField.Directive.ToString())

            Dim previousDirectives As List(Of RowDirective) =
            TryCast(previousCell.Tag, List(Of RowDirective))

            If previousDirectives Is Nothing Then
                Continue For
            End If

            For Each directive As RowDirective In previousDirectives

                If Not directive.DirectiveType.Equals(
                "+PAGE",
                StringComparison.OrdinalIgnoreCase) Then

                    Continue For
                End If

                Dim pageNumber As Integer

                If Integer.TryParse(
                directive.Text,
                pageNumber) Then

                    suggestedPageNumber =
                    pageNumber

                End If

            Next

        Next

        suggestedPageNumber += 1

        Using form As New DirectiveEditorForm(
        rowIndex + 1,
        directives,
        suggestedPageNumber)

            If form.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Dim updatedDirectives As New List(Of RowDirective)

            For Each directive As RowDirective In form.Directives

                directive.RowIndex =
                rowIndex

                updatedDirectives.Add(directive)

            Next

            directiveCell.Tag =
            updatedDirectives

        End Using

        UpdateDirectiveCell(rowIndex)

        _hasChanges = True

    End Sub
    Private Sub UpdateDirectiveCell(
    rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then
            Return
        End If

        Dim cell As DataGridViewCell =
        transcriptionGrid.Rows(rowIndex).
        Cells(GridField.Directive.ToString())

        Dim directives As List(Of RowDirective) =
        TryCast(cell.Tag, List(Of RowDirective))

        If directives Is Nothing OrElse directives.Count = 0 Then

            cell.Value = "+"
            cell.ToolTipText =
            "Click to add or edit directives for this row."

            Return

        End If

        cell.Value =
        $"+{directives.Count}"

        cell.ToolTipText =
        "Click to edit directives for this row." &
        Environment.NewLine &
        Environment.NewLine &
        String.Join(
            Environment.NewLine,
            directives.Select(Function(item) item.ToString()))

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

        Dim gridRow As DataGridViewRow = transcriptionGrid.Rows(e.RowIndex)

        If IsBlankEntryRow(gridRow) Then

            ' An empty value does not turn the blank entry row
            ' into a genuine transcription row.
            If String.IsNullOrEmpty(value) Then
                Return
            End If

            gridRow.Tag = Nothing

        End If

        _hasChanges = True

        EnsureBlankEntryRow()
        UpdateRowNumbers()

    End Sub
    ' Shows the row validation message when the mouse is over
    ' the row-number area containing the validation indicator.
    Private Sub transcriptionGrid_CellMouseEnter(
    sender As Object,
    e As DataGridViewCellEventArgs) _
    Handles transcriptionGrid.CellMouseEnter

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(e.ColumnIndex)

        If column.Name <> "RowNumber" Then
            Return
        End If

        Dim worstResult As ValidationResult = Nothing

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim key = (Row:=e.RowIndex, Column:=columnIndex)

            Dim result As ValidationResult = Nothing

            If Not _cellValidationResults.TryGetValue(key, result) Then
                Continue For
            End If

            If result.IsError Then
                worstResult = result
                Exit For
            End If

            If result.IsWarning AndAlso worstResult Is Nothing Then
                worstResult = result
            End If

        Next

        If worstResult Is Nothing Then
            Return
        End If

        Dim cellRectangle As Rectangle =
        transcriptionGrid.GetCellDisplayRectangle(
            e.ColumnIndex,
            e.RowIndex,
            cutOverflow:=True)

        _validationToolTip.Show(
        worstResult.Message,
        transcriptionGrid,
        cellRectangle.Right,
        cellRectangle.Top,
        5000)

    End Sub
    Private Sub transcriptionGrid_CellMouseLeave(
    sender As Object,
    e As DataGridViewCellEventArgs) _
    Handles transcriptionGrid.CellMouseLeave

        _validationToolTip.Hide(transcriptionGrid)

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
    ' Updates the normal status-bar position text.
    ' If a temporary message is currently being displayed, this
    ' becomes the text that will be restored when that message ends.
    ' Updates the row and column position shown at the
    ' right-hand side of the status bar.
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
    ' Shows the validation message when the mouse is over the
    ' row indicator area at the left of the grid.
    Private Sub transcriptionGrid_CellToolTipTextNeeded(
    sender As Object,
    e As DataGridViewCellToolTipTextNeededEventArgs) _
    Handles transcriptionGrid.CellToolTipTextNeeded

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(e.ColumnIndex)

        If column.Name <> "RowNumber" Then
            Return
        End If

        Dim worstResult As ValidationResult = Nothing

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim key = (Row:=e.RowIndex, Column:=columnIndex)

            Dim result As ValidationResult = Nothing

            If Not _cellValidationResults.TryGetValue(key, result) Then
                Continue For
            End If

            If result.IsError Then
                worstResult = result
                Exit For
            End If

            If result.IsWarning AndAlso worstResult Is Nothing Then
                worstResult = result
            End If

        Next

        If worstResult IsNot Nothing Then
            e.ToolTipText = worstResult.Message
        End If

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

                Case Keys.Up, Keys.Down

                    If _pickListPopup.IsOpen Then

                        Dim e As New KeyEventArgs(keyData)

                        If _navigationManager.HandleKey(editor, e) Then
                            Return True
                        End If

                    End If

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
    Friend Function CopyNextWordFromAbove(
    editor As TextBox) As Boolean

        If editor Is Nothing Then
            Return False
        End If

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

        Dim previousText As String =
        If(
            transcriptionGrid.Rows(currentRow - 1).
                Cells(currentColumn).Value,
            "").ToString()

        If String.IsNullOrWhiteSpace(previousText) Then
            Return False
        End If

        Dim fromWords() As String =
        previousText.Split(
            {" "c, "."c},
            StringSplitOptions.RemoveEmptyEntries)

        If fromWords.Length = 0 Then
            Return False
        End If

        Dim currentText As String =
        editor.Text.Trim()

        If currentText.Length = 0 Then

            editor.Text = fromWords(0)

            If Not editor.Text.EndsWith(" ") AndAlso
           Not editor.Text.EndsWith(".") Then

                editor.Text &= " "

            End If

        Else

            Dim toWords() As String =
            currentText.Split(
                {" "c, "."c},
                StringSplitOptions.RemoveEmptyEntries)

            Dim wordCopied As Boolean = False

            For Each fromWord As String In fromWords

                Dim alreadyPresent As Boolean = False

                For Each toWord As String In toWords

                    If String.Equals(
                    toWord.Trim(),
                    fromWord.Trim(),
                    StringComparison.OrdinalIgnoreCase) Then

                        alreadyPresent = True
                        Exit For

                    End If

                Next

                If Not alreadyPresent Then

                    editor.Text &= fromWord

                    If Not editor.Text.EndsWith(" ") AndAlso
                   Not editor.Text.EndsWith(".") Then

                        editor.Text &= " "

                    End If

                    wordCopied = True
                    Exit For

                End If

            Next

            If Not wordCopied Then
                Return False
            End If

        End If

        editor.SelectionStart = editor.TextLength
        editor.SelectionLength = 0

        Return True

    End Function
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
    Friend ReadOnly Property PickListIsOpen As Boolean
        Get
            Return _pickListPopup.IsOpen
        End Get
    End Property

    Friend Sub MovePickListSelectionUp()
        _pickListPopup.MoveSelectionUp()
    End Sub

    Friend Sub MovePickListSelectionDown()
        _pickListPopup.MoveSelectionDown()
    End Sub

    Friend Function SelectPickListItemByNumber(number As Integer) As Boolean
        Return _pickListPopup.SelectItemByNumber(number)
    End Function
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
    ' Returns the logical GridField represented by the current grid column.
    ' The field itself is stored in the DataGridView column's Tag.
    Friend ReadOnly Property CurrentField As GridField
        Get

            If transcriptionGrid.CurrentCell Is Nothing Then
                Throw New InvalidOperationException(
                "There is no current grid cell.")

            End If

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(
                transcriptionGrid.CurrentCell.ColumnIndex)

            If column.Tag Is Nothing Then
                Throw New InvalidOperationException(
                "The current grid column has no GridField.")

            End If

            Return DirectCast(column.Tag, GridField)

        End Get
    End Property
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

        If currentField = GridField.Forename Then

            ' Replace the partial forename currently being typed with the
            ' selected picklist entry, while preserving earlier forenames.
            Dim existingText As String = editor.Text
            Dim lastSpace As Integer = existingText.LastIndexOf(" "c)

            If lastSpace >= 0 Then

                editor.Text =
                    existingText.Substring(0, lastSpace + 1) &
                    selectedItem.Text

            Else

                editor.Text = selectedItem.Text

            End If

        Else

            editor.Text = selectedItem.Text

        End If

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
    ' Validate a field only after the user has finished editing it.
    ' This avoids reporting temporary errors while a UCF expression
    ' or other multi-character field value is still being entered.
    Private Sub transcriptionGrid_CellEndEdit(
    sender As Object,
    e As DataGridViewCellEventArgs) _
    Handles transcriptionGrid.CellEndEdit

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn =
        transcriptionGrid.Columns(e.ColumnIndex)

        If column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField =
        DirectCast(column.Tag, GridField)

        If Not FieldMetaData.Meta(field).IsDataColumn Then
            Return
        End If

        Dim cell As DataGridViewCell =
        transcriptionGrid.Rows(e.RowIndex).Cells(e.ColumnIndex)

        Dim value As String =
        If(cell.Value, "").ToString()

        Dim result As ValidationResult =
    Validator.Validate(field, value)

        If field = GridField.District OrElse
           field = GridField.Volume OrElse
           field = GridField.DistNum Then

            ' District and its code are interdependent, so changing either
            ' one requires both cells to be validated again.
            ValidateDistrictCodePair(e.RowIndex)

            ' Pick up the final result for the cell which was actually edited.
            result =
        _cellValidationResults((e.RowIndex, e.ColumnIndex))

        Else

            ApplyCellValidationResult(
        e.RowIndex,
        e.ColumnIndex,
        result)

        End If

        ' Errors use the DataGridView's built-in error glyph.
        ' Warnings use the cell tooltip; their yellow indicator is
        ' drawn separately in CellPainting.
        If result.IsError Then

            cell.ErrorText = result.Message
            cell.ToolTipText = ""

        ElseIf result.IsWarning Then

            cell.ErrorText = ""
            cell.ToolTipText = result.Message

        Else

            cell.ErrorText = ""
            cell.ToolTipText = ""

        End If

        ' Recalculate the overall warning/error state of the row.
        UpdateRowValidationState(e.RowIndex)

        If field = GridField.Surname OrElse
            field = GridField.Forename Then

            ' Changing a name can affect both this row and the row below it.
            ValidateSequence(e.RowIndex)

            If e.RowIndex + 1 < transcriptionGrid.Rows.Count Then
                ValidateSequence(e.RowIndex + 1)
                UpdateRowValidationState(e.RowIndex + 1)
            End If

        End If

        If Not result.IsOk Then

            DebugLog.WriteAlways(
                $"[VALIDATION] Row={e.RowIndex + 1}, " &
                $"Field={field}, " &
                $"State={result.State}, " &
                $"Message='{result.Message}'")

            ' Queue the validation message for display in the status bar.
            ShowStatusMessage(result.Message)

        End If

    End Sub
    ' Recalculates the overall validation state of a row.
    ' An Error takes precedence over a Warning.
    ' The row header tooltip shows the message belonging to
    ' the most serious validation result on the row.
    Private Sub UpdateRowValidationState(rowIndex As Integer)

        Dim rowState As ValidationState =
        ValidationState.Ok

        Dim messages As New List(Of String)

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim key = (Row:=rowIndex, Column:=columnIndex)

            Dim result As ValidationResult = Nothing

            If Not _cellValidationResults.TryGetValue(key, result) Then
                Continue For
            End If

            If result.IsOk Then
                Continue For
            End If

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(columnIndex)

            messages.Add(
            $"{column.HeaderText}: {result.Message}")

            If result.IsError Then

                rowState = ValidationState.Error

            ElseIf result.IsWarning AndAlso
               rowState = ValidationState.Ok Then

                rowState = ValidationState.Warning

            End If

        Next

        _rowValidationStates(rowIndex) = rowState

        Dim row As DataGridViewRow =
        transcriptionGrid.Rows(rowIndex)

        row.Cells("RowNumber").ToolTipText =
        String.Join(Environment.NewLine, messages)

        transcriptionGrid.InvalidateRow(rowIndex)

    End Sub
    ' Stores a cell's validation result and updates the visual
    ' error/warning information shown by the DataGridView.
    Private Sub ApplyCellValidationResult(
    rowIndex As Integer,
    columnIndex As Integer,
    result As ValidationResult)

        Dim cell As DataGridViewCell =
        transcriptionGrid.Rows(rowIndex).Cells(columnIndex)

        _cellValidationResults((rowIndex, columnIndex)) = result

        If result.IsError Then

            cell.ErrorText = result.Message
            cell.ToolTipText = ""

        ElseIf result.IsWarning Then

            cell.ErrorText = ""
            cell.ToolTipText = result.Message

        Else

            cell.ErrorText = ""
            cell.ToolTipText = ""

        End If

    End Sub
    ' Checks the alphabetical sequence of Surname and Forename against
    ' the previous populated row. A Surname warning is produced when
    ' surnames go backwards; when surnames are equal, Forename order
    ' is checked instead.
    Private Sub ValidateSequence(rowIndex As Integer)

        If rowIndex <= 0 Then
            Return
        End If

        Dim surnameColumn As Integer = -1
        Dim forenameColumn As Integer = -1

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            If field = GridField.Surname Then
                surnameColumn = columnIndex

            ElseIf field = GridField.Forename Then
                forenameColumn = columnIndex

            End If

        Next

        If surnameColumn < 0 OrElse forenameColumn < 0 Then
            Return
        End If

        Dim row As DataGridViewRow =
        transcriptionGrid.Rows(rowIndex)

        Dim surname As String =
        If(row.Cells(surnameColumn).Value, "").ToString().Trim()

        Dim forename As String =
        If(row.Cells(forenameColumn).Value, "").ToString().Trim()

        ' Restore the ordinary field validation first.
        ' This removes any sequence warning left from an earlier value.
        Dim surnameResult As ValidationResult =
        Validator.Validate(GridField.Surname, surname)

        Dim forenameResult As ValidationResult =
        Validator.Validate(GridField.Forename, forename)

        ApplyCellValidationResult(
        rowIndex,
        surnameColumn,
        surnameResult)

        ApplyCellValidationResult(
        rowIndex,
        forenameColumn,
        forenameResult)

        ' Sequence checking is not meaningful until both names exist.
        If String.IsNullOrWhiteSpace(surname) OrElse
       String.IsNullOrWhiteSpace(forename) Then

            Return
        End If

        ' Look backwards for the previous populated surname row.
        For previousIndex As Integer = rowIndex - 1 To 0 Step -1

            Dim previousRow As DataGridViewRow =
            transcriptionGrid.Rows(previousIndex)

            Dim previousSurname As String =
            If(previousRow.Cells(surnameColumn).Value, "").ToString().Trim()

            ' Ignore blank rows and continue looking backwards.
            If String.IsNullOrWhiteSpace(previousSurname) Then
                Continue For
            End If

            Dim surnameCompare As Integer =
            String.Compare(
                surname,
                previousSurname,
                StringComparison.OrdinalIgnoreCase)

            If surnameCompare < 0 Then

                ApplySequenceWarning(
                rowIndex,
                surnameColumn,
                $"Surname '{surname}' is before previous surname '{previousSurname}'.")

                Return

            End If

            If surnameCompare = 0 Then

                Dim previousForename As String =
                If(previousRow.Cells(forenameColumn).Value, "").ToString().Trim()

                Dim forenameCompare As Integer =
                String.Compare(
                    forename,
                    previousForename,
                    StringComparison.OrdinalIgnoreCase)

                If forenameCompare < 0 Then

                    ApplySequenceWarning(
                    rowIndex,
                    forenameColumn,
                    $"Forename '{forename}' is before previous forename '{previousForename}' for surname '{surname}'.")

                    Return

                End If

            End If

            ' We have found and compared against the nearest previous
            ' populated surname row, so there is nothing further to check.
            Return

        Next

    End Sub
    ' Adds a sequence warning without replacing a more serious
    ' validation result already present on the cell.
    Private Sub ApplySequenceWarning(
    rowIndex As Integer,
    columnIndex As Integer,
    message As String)

        Dim key = (Row:=rowIndex, Column:=columnIndex)

        Dim existing As ValidationResult = Nothing

        If _cellValidationResults.TryGetValue(key, existing) Then

            If existing.IsError OrElse existing.IsWarning Then
                Return
            End If

        End If

        ApplyCellValidationResult(
        rowIndex,
        columnIndex,
        ValidationResult.Warning(message))

    End Sub
    ' Revalidates District together with its associated Volume or DistNum.
    '
    ' In addition to each field's normal validation, a warning is produced
    ' when one half of the District/code pair is present and the other is blank.
    Private Sub ValidateDistrictCodePair(rowIndex As Integer)

        Dim districtColumn As Integer = -1
        Dim codeColumn As Integer = -1
        Dim codeField As GridField

        ' Find the visible District and Volume/DistNum columns from their metadata.
        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim column As DataGridViewColumn =
            transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
            DirectCast(column.Tag, GridField)

            If field = GridField.District Then

                districtColumn = columnIndex

            ElseIf field = GridField.Volume OrElse
               field = GridField.DistNum Then

                codeColumn = columnIndex
                codeField = field

            End If

        Next

        If districtColumn < 0 OrElse codeColumn < 0 Then
            Return
        End If

        Dim row As DataGridViewRow =
        transcriptionGrid.Rows(rowIndex)

        Dim districtValue As String =
        If(row.Cells(districtColumn).Value, "").ToString()

        Dim codeValue As String =
        If(row.Cells(codeColumn).Value, "").ToString()

        ' First perform the normal independent validation of both cells.
        Dim districtResult As ValidationResult =
        Validator.Validate(
            GridField.District,
            districtValue)

        Dim codeResult As ValidationResult =
        Validator.Validate(
            codeField,
            codeValue)

        ' Only add the pair warning where normal validation has succeeded.
        ' A real field error/warning must not be hidden by this check.
        If districtResult.IsOk AndAlso
       String.IsNullOrWhiteSpace(districtValue) AndAlso
       Not String.IsNullOrWhiteSpace(codeValue) Then

            districtResult =
            ValidationResult.Warning(
                "District is blank but Volume/DistNum is present.")

        End If

        If codeResult.IsOk AndAlso
       Not String.IsNullOrWhiteSpace(districtValue) AndAlso
       String.IsNullOrWhiteSpace(codeValue) Then

            codeResult =
            ValidationResult.Warning(
                "Volume/DistNum is blank but District is present.")

        End If

        ApplyCellValidationResult(
        rowIndex,
        districtColumn,
        districtResult)

        ApplyCellValidationResult(
        rowIndex,
        codeColumn,
        codeResult)

    End Sub
    ' Draws the row-level validation indicator in the row header.
    ' Error rows show a red circle; warning rows show a yellow circle.
    Private Sub transcriptionGrid_RowPostPaint(
    sender As Object,
    e As DataGridViewRowPostPaintEventArgs) _
    Handles transcriptionGrid.RowPostPaint

        Dim state As ValidationState

        If Not _rowValidationStates.TryGetValue(e.RowIndex, state) Then
            Return
        End If

        If state = ValidationState.Ok Then
            Return
        End If

        Dim indicatorColor As Color

        If state = ValidationState.Error Then
            indicatorColor = UiColors.ValidationError
        Else
            indicatorColor = UiColors.ValidationWarning
        End If

        Dim diameter As Integer = 10

        Dim x As Integer =
        e.RowBounds.Left + 4

        Dim y As Integer =
        e.RowBounds.Top +
        (e.RowBounds.Height - diameter) \ 2

        Using brush As New SolidBrush(indicatorColor)

            e.Graphics.FillEllipse(
            brush,
            x,
            y,
            diameter,
            diameter)

        End Using

    End Sub
    ' Draws a small warning indicator in cells which contain
    ' a validation warning. Errors continue to use the
    ' DataGridView's built-in ErrorText indicator.
    Private Sub transcriptionGrid_CellPainting(
    sender As Object,
    e As DataGridViewCellPaintingEventArgs) _
    Handles transcriptionGrid.CellPainting

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim key = (Row:=e.RowIndex, Column:=e.ColumnIndex)

        Dim result As ValidationResult = Nothing

        If Not _cellValidationResults.TryGetValue(key, result) Then
            Return
        End If

        If Not result.IsWarning Then
            Return
        End If

        e.Paint(e.ClipBounds, e.PaintParts)

        Dim diameter As Integer = 10

        Dim x As Integer =
        e.CellBounds.Right - diameter - 4

        Dim y As Integer =
        e.CellBounds.Top +
        (e.CellBounds.Height - diameter) \ 2

        Using brush As New SolidBrush(UiColors.ValidationWarning)

            e.Graphics.FillEllipse(
            brush,
            x,
            y,
            diameter,
            diameter)

        End Using

        e.Handled = True

    End Sub
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