Imports System.IO
Imports System.Threading
Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _workfile As New Workfile()
    Private ReadOnly _changeState As New ChangeState()
    Public Event CurrentGridRowChanged As EventHandler
    Private ReadOnly _commandExecutor As ICommandExecutor
    Private _configuringGrid As Boolean
    Private _refreshingGrid As Boolean
    Private ReadOnly _pickListPopup As New PickListPopup()
    Private ReadOnly _navigationManager As NavigationManager
    Private Class StatusItem
        Public Property Message As String
        Public Property Expires As DateTime
    End Class

    Private ReadOnly _statusItems As New List(Of StatusItem)
    Private ReadOnly _statusQueue As New Queue(Of (Message As String, Duration As Integer))
    Private ReadOnly _statusTimer As New System.Windows.Forms.Timer()
    Private ReadOnly _openFileMenu As New ContextMenuStrip()
    Private _baseStatusText As String = "Ready"

    Private ReadOnly _gridRowMenu As New ContextMenuStrip()
    Private ReadOnly _gridInsertRowItem As New ToolStripMenuItem("Insert Row")
    Private ReadOnly _gridDeleteRowItem As New ToolStripMenuItem("Delete Row")
    Private _gridContextRowIndex As Integer = -1

    ' Stores the latest validation result for each grid cell.
    Private ReadOnly _cellValidationResults As New Dictionary(Of (Row As Integer, Column As Integer), ValidationResult)

    ' Stores the overall validation state for each row.
    Private ReadOnly _rowValidationStates As New Dictionary(Of Integer, ValidationState)
    Private ReadOnly _validationToolTip As New ToolTip()
    Private _selectedCommandCategory As String = ""
    Private _lastGridRowIndex As Integer = -1
    Private _uploadTooltipShowing As Boolean
    Private _verifyRowIndex As Integer = -1
    Private Shared ReadOnly WordSeparators() As Char = {" "c, "."c}
    Private _settingCompletionCharacters As Boolean
    Private _suppressNextAutoComplete As Boolean
    Private _pickListActive As Boolean
    Private _startingCellEdit As Boolean

    Public Sub New(commandExecutor As ICommandExecutor)

        InitializeComponent()

        _gridRowMenu.Items.Add(_gridInsertRowItem)
        _gridRowMenu.Items.Add(_gridDeleteRowItem)

        AddHandler _gridInsertRowItem.Click, AddressOf GridInsertRow_Click
        AddHandler _gridDeleteRowItem.Click, AddressOf GridDeleteRow_Click

        uploadToolTip.SetToolTip(btnSpecialCharacters, "Show the special character form")
        _statusTimer.Interval = 250
        AddHandler _statusTimer.Tick, AddressOf StatusTimer_Tick
        AddHandler ProjectValues.StatusMessageRequested, AddressOf ProjectValues_StatusMessageRequested
        AddHandler _pickListPopup.ItemClicked, AddressOf PickListPopup_ItemClicked
        _navigationManager = New NavigationManager(Me)
        ApplyTranscriptionAppearance()
        UpdateCommandCategoryAppearance()
        _commandExecutor = commandExecutor
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2"
        filePanel.Expanded = ProjectValues.FilePanelExpanded

        InitialiseTranscriptionForm()

        DebugLog.WriteAlways("======= TRANSCRIPTION FORM OPENED =======")
        DebugLog.WriteAlways($"Batch Type  : '{ProjectValues.BatchType}'")
        DebugLog.WriteAlways($"Year        : {ProjectValues.Year}")
        DebugLog.WriteAlways($"Quarter     : {ProjectValues.Quarter}")
        DebugLog.WriteAlways($"Page        : {ProjectValues.Page}")
        DebugLog.WriteAlways($"Page Letter : '{ProjectValues.PageLetter}'")
        DebugLog.WriteAlways($"Source Ref  : '{ProjectValues.SourceRef}'")
        DebugLog.WriteAlways("=========================================")

    End Sub
    Private Sub GridInsertRow_Click(sender As Object, e As EventArgs)

        Throw New Exception("Test of WinBMD2 global exception handler.")

        If _gridContextRowIndex < 0 OrElse _gridContextRowIndex >= transcriptionGrid.Rows.Count Then Return

        _pickListPopup.Hide()

        If transcriptionGrid.IsCurrentCellInEditMode Then transcriptionGrid.EndEdit()

        Dim insertIndex As Integer = _gridContextRowIndex

        ' A +PAGE belongs after its row. Inserting immediately after such a row
        ' may therefore leave the page break in the wrong place.
        If insertIndex > 0 Then

            Dim previousDirectiveCell As DataGridViewCell = transcriptionGrid.Rows(insertIndex - 1).Cells(GridField.Directive.ToString())
            Dim previousDirectives As List(Of RowDirective) = TryCast(previousDirectiveCell.Tag, List(Of RowDirective))
            Dim hasPageDirective As Boolean = False

            If previousDirectives IsNot Nothing Then
                For Each directive As RowDirective In previousDirectives
                    If directive.DirectiveType.Equals("+PAGE", StringComparison.OrdinalIgnoreCase) Then
                        hasPageDirective = True
                        Exit For
                    End If
                Next
            End If

            If hasPageDirective Then
                Dim result As DialogResult = MessageBox.Show(Me,
                "The row immediately before this position has a +PAGE directive." & Environment.NewLine & Environment.NewLine &
                "Inserting a row here may leave the +PAGE in the wrong position." & Environment.NewLine & Environment.NewLine &
                "Do you want to insert the row anyway?",
                "Insert Row",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)

                If result <> DialogResult.Yes Then Return
            End If

        End If

        _refreshingGrid = True

        Try

            transcriptionGrid.Rows.Insert(insertIndex, 1)

            Dim newRow As DataGridViewRow = transcriptionGrid.Rows(insertIndex)
            newRow.Tag = Nothing

            For Each column As DataGridViewColumn In transcriptionGrid.Columns

                If column.Name = "RowNumber" Then Continue For

                If column.Tag Is Nothing Then Continue For

                Dim field As GridField = DirectCast(column.Tag, GridField)

                If field = GridField.Directive Then
                    newRow.Cells(column.Index).Tag = New List(Of RowDirective)()
                    newRow.Cells(column.Index).Value = "+"
                ElseIf field = GridField.Verified Then
                    newRow.Cells(column.Index).Tag = False
                    newRow.Cells(column.Index).Value = ""
                Else
                    newRow.Cells(column.Index).Value = ""
                End If

            Next

            ' The RowDirective objects move with their rows, but their stored
            ' RowIndex values must also be brought up to date.
            For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

                Dim directiveCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Directive.ToString())
                Dim directives As List(Of RowDirective) = TryCast(directiveCell.Tag, List(Of RowDirective))

                If directives Is Nothing Then Continue For

                For Each directive As RowDirective In directives
                    directive.RowIndex = rowIndex
                Next

            Next

            UpdateRowNumbers()

        Finally
            _refreshingGrid = False
        End Try

        _changeState.SetFileChanged()

        ' Row numbers have changed, so rebuild validation and verification state.
        ValidateLoadedRows()

        If _workfile.CreateOrReplace(transcriptionGrid) Then _changeState.WorkfileSaved()

        _lastGridRowIndex = -1

        transcriptionGrid.CurrentCell = transcriptionGrid.Rows(insertIndex).Cells(GetFirstDataColumn())
        transcriptionGrid.Focus()

    End Sub

    Private Sub GridDeleteRow_Click(sender As Object, e As EventArgs)

        If _gridContextRowIndex < 0 OrElse _gridContextRowIndex >= transcriptionGrid.Rows.Count Then Return

        _pickListPopup.Hide()

        If transcriptionGrid.IsCurrentCellInEditMode Then transcriptionGrid.EndEdit()

        Dim deleteIndex As Integer = _gridContextRowIndex
        Dim deleteRow As DataGridViewRow = transcriptionGrid.Rows(deleteIndex)
        Dim hasData As Boolean = False
        Dim hasDirectives As Boolean = False

        ' Determine whether the row contains any transcription data.
        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then Continue For

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If FieldMetaData.Meta(field).IsDataColumn Then
                Dim value As String = If(deleteRow.Cells(column.Index).Value, "").ToString()
                If Not String.IsNullOrWhiteSpace(value) Then
                    hasData = True
                    Exit For
                End If
            End If

        Next

        ' Directives belong to the row and will also disappear if it is deleted.
        Dim directiveCell As DataGridViewCell = deleteRow.Cells(GridField.Directive.ToString())
        Dim directives As List(Of RowDirective) = TryCast(directiveCell.Tag, List(Of RowDirective))

        If directives IsNot Nothing AndAlso directives.Count > 0 Then hasDirectives = True

        If hasData OrElse hasDirectives Then

            Dim message As String

            If hasData AndAlso hasDirectives Then
                message = "This row contains transcription data and directives." & Environment.NewLine & Environment.NewLine & "Deleting the row will permanently remove both." & Environment.NewLine & Environment.NewLine & "Do you want to delete the row?"
            ElseIf hasDirectives Then
                message = "This row contains directives." & Environment.NewLine & Environment.NewLine & "Deleting the row will permanently remove them." & Environment.NewLine & Environment.NewLine & "Do you want to delete the row?"
            Else
                message = "This row contains transcription data." & Environment.NewLine & Environment.NewLine & "Do you want to delete the row?"
            End If

            Dim result As DialogResult = MessageBox.Show(Me, message, "Delete Row", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If result <> DialogResult.Yes Then Return

        End If

        _refreshingGrid = True

        Try

            transcriptionGrid.Rows.RemoveAt(deleteIndex)

            EnsureBlankEntryRow()

            ' The directives move with their rows, but their stored RowIndex
            ' values must be corrected after the deletion.
            For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

                Dim cell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Directive.ToString())
                Dim rowDirectives As List(Of RowDirective) = TryCast(cell.Tag, List(Of RowDirective))

                If rowDirectives Is Nothing Then Continue For

                For Each directive As RowDirective In rowDirectives
                    directive.RowIndex = rowIndex
                Next

            Next

            UpdateRowNumbers()

        Finally
            _refreshingGrid = False
        End Try

        _changeState.SetFileChanged()

        ' Row indexes have changed, so rebuild validation and verification state.
        ValidateLoadedRows()

        If _workfile.CreateOrReplace(transcriptionGrid) Then _changeState.WorkfileSaved()

        _lastGridRowIndex = -1

        ' Leave the cursor on the row which replaced the deleted row.
        Dim targetRow As Integer = Math.Min(deleteIndex, transcriptionGrid.Rows.Count - 1)

        If targetRow >= 0 Then
            transcriptionGrid.CurrentCell = transcriptionGrid.Rows(targetRow).Cells(GetFirstDataColumn())
        End If

        transcriptionGrid.Focus()

    End Sub
    Friend ReadOnly Property CurrentVerifyRowIndex As Integer
        Get
            Return _verifyRowIndex
        End Get
    End Property
    Private Sub ProjectValues_StatusMessageRequested(message As String, duration As Integer)

        ShowStatusMessage(message, duration)

    End Sub
    Private Sub PickListPopup_ItemClicked(sender As Object, e As EventArgs)

        Dim editor As TextBox = TryCast(transcriptionGrid.EditingControl, TextBox)

        If editor Is Nothing Then
            Return
        End If

        If Not AcceptCurrentPickListSelection(editor) Then
            Return
        End If

        If CurrentField = GridField.Forename Then

            If Not editor.Text.EndsWith(" ") Then
                editor.Text &= " "
            End If

            editor.SelectionStart = editor.TextLength
            editor.SelectionLength = 0

        Else

            EndGridEdit()
            MoveToNextDataCell()

        End If

    End Sub
    Private Sub transcriptionGrid_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles transcriptionGrid.CellMouseDown

        If e.Button <> MouseButtons.Right OrElse e.RowIndex < 0 Then Return

        _pickListPopup.Hide()

        _gridContextRowIndex = e.RowIndex

        If e.ColumnIndex >= 0 Then transcriptionGrid.CurrentCell = transcriptionGrid.Rows(e.RowIndex).Cells(e.ColumnIndex)

        _gridRowMenu.Show(transcriptionGrid, transcriptionGrid.PointToClient(Cursor.Position))

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
        btnFileExit.Visible = False
        btnFileEditHeader.Visible = False

        If String.IsNullOrWhiteSpace(_selectedCommandCategory) Then

            commandStrip.Visible = False
            filePanel.Height = 53

            Return

        End If

        commandStrip.Visible = True
        filePanel.Height = 92

        Select Case _selectedCommandCategory

            Case "File"

                btnFileOpen.Visible = True
                btnFileSave.Visible = True
                btnFileSaveAs.Visible = True
                btnFileExit.Visible = True
                btnFileEditHeader.Visible = True

        End Select

    End Sub
    Private Sub categoryStrip_MouseMove(sender As Object, e As MouseEventArgs) Handles categoryStrip.MouseMove

        If btnCategoryUpload.Enabled Then
            Return
        End If

        If btnCategoryUpload.Bounds.Contains(e.Location) Then

            If Not _uploadTooltipShowing Then
                uploadToolTip.Show("Upload is not available until all rows are verified.", categoryStrip, e.X + 10, e.Y + 20, 5000)
                _uploadTooltipShowing = True
            End If

        Else

            If _uploadTooltipShowing Then
                uploadToolTip.Hide(categoryStrip)
                _uploadTooltipShowing = False
            End If

        End If

    End Sub

    Private Sub categoryStrip_MouseLeave(sender As Object, e As EventArgs) Handles categoryStrip.MouseLeave

        uploadToolTip.Hide(categoryStrip)
        _uploadTooltipShowing = False

    End Sub
    Public Function LoadBatchFile(filePath As String) As Boolean

        SaveFormBounds()

        _refreshingGrid = True

        Try

            If Not LoadSaveFiles.Load(filePath, transcriptionGrid, AddressOf ConfigureGridColumns) Then
                Return False
            End If

            FinishLoadingBatch()

            _changeState.FileSaved()
            ProjectValuesStore.Save()

            _workfile.CreateOrReplace(transcriptionGrid)

            UpdateStatusPosition()
            Return True

        Finally
            _refreshingGrid = False
        End Try

    End Function
    Private Sub FinishLoadingBatch()

        Dim verificationState As String = VerificationData.Load(ProjectValues.BatchName)

        ApplyVerificationState(verificationState)
        ValidateLoadedRows()

        UpdateUploadEnabled()

        RestoreFormBounds()

        Dim fields() As GridField = GridLayout.GetVisibleFields()

        AddBlankEntryRow(fields)

        ValidateLoadedRows()

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1
            UpdateDirectiveCell(rowIndex)
        Next

    End Sub
    Friend Function SaveCurrentBatch(filePath As String) As Boolean

        _workfile.FlushCurrentRow(transcriptionGrid, _changeState)

        If Not LoadSaveFiles.Save(filePath, transcriptionGrid) Then
            Return False
        End If

        _changeState.FileSaved()

        Return True

    End Function
    Friend Function ConfirmSaveChangesIfNeeded() As Boolean

        If Not _changeState.FileChanged() Then
            Return True
        End If

        _pickListPopup.Hide()

        Dim result As DialogResult =
        MessageBox.Show(
            Me,
            $"{ProjectValues.BatchName} has unsaved changes." &
            Environment.NewLine &
            Environment.NewLine &
            "Do you want to save them?",
            "Unsaved Changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning)

        Select Case result

            Case DialogResult.Yes
                _commandExecutor.Execute(AppCommand.SaveFile)
                Return Not _changeState.FileChanged()

            Case DialogResult.No
                Return True

            Case Else
                Return False

        End Select

    End Function
    ' Sets the normal status text shown whenever there are no
    ' temporary messages being displayed.
    Private Sub SetBaseStatus(message As String)

        _baseStatusText = message

        If _statusItems.Count = 0 Then
            statusMessageLabel.Text = _baseStatusText
            statusMessageLabel.Visible = True
        End If

    End Sub

    ' Adds a temporary status message. Up to three messages can be
    ' displayed simultaneously. Further messages wait in the queue.
    Private Sub ShowStatusMessage(message As String, Optional duration As Integer = 5000)

        DebugLog.WriteAlways($"[STATUS] ShowStatusMessage called: '{message}'")

        If String.IsNullOrWhiteSpace(message) Then
            Return
        End If

        If _statusItems.Count < 3 Then
            _statusItems.Add(New StatusItem With {.Message = message, .Expires = DateTime.Now.AddMilliseconds(duration)})
        Else
            _statusQueue.Enqueue((message, duration))
        End If

        UpdateStatusMessages()

        If Not _statusTimer.Enabled Then
            _statusTimer.Start()
        End If

    End Sub

    ' Removes expired messages, fills any available positions from the
    ' waiting queue, then updates the three status-message labels.
    Private Sub UpdateStatusMessages()

        Dim now As DateTime = DateTime.Now

        _statusItems.RemoveAll(Function(item) item.Expires <= now)

        While _statusItems.Count < 3 AndAlso _statusQueue.Count > 0
            Dim queuedItem = _statusQueue.Dequeue()
            _statusItems.Add(New StatusItem With {.Message = queuedItem.Message, .Expires = now.AddMilliseconds(queuedItem.Duration)})
        End While

        If _statusItems.Count = 0 Then

            statusMessageLabel.Text = _baseStatusText
            statusMessageLabel.Visible = True
            statusMessageLabel2.Visible = False
            statusMessageLabel3.Visible = False

            If _statusQueue.Count = 0 Then
                _statusTimer.Stop()
            End If

            Return

        End If

        statusMessageLabel.Text = _statusItems(0).Message
        statusMessageLabel.Visible = True

        If _statusItems.Count >= 2 Then
            statusMessageLabel2.Text = _statusItems(1).Message
            statusMessageLabel2.Visible = True
        Else
            statusMessageLabel2.Visible = False
        End If

        If _statusItems.Count >= 3 Then
            statusMessageLabel3.Text = _statusItems(2).Message
            statusMessageLabel3.Visible = True
        Else
            statusMessageLabel3.Visible = False
        End If

    End Sub
    Private Sub StatusTimer_Tick(sender As Object, e As EventArgs)

        UpdateStatusMessages()

    End Sub
    Private Sub InitialiseTranscriptionForm()

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

        _changeState.FileSaved()

        UpdateStatusPosition()

        UpdateUploadEnabled()

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
    Private Async Sub CommandCategory_Click(sender As Object, e As EventArgs) Handles btnCategoryFile.Click, btnCategoryScan.Click, btnCategoryVerify.Click, btnCategoryUpload.Click, btnCategoryOptions.Click, btnCategoryHelp.Click
        Dim button As Button = TryCast(sender, Button)

        If button Is Nothing Then
            Return
        End If

        If button Is btnCategoryOptions Then

            _pickListPopup.Hide()

            Using form As New OptionsForm(True)

                If form.ShowDialog(Me) = DialogResult.OK Then
                    ApplyCapitalisationChanges(form.CapitalisationChanges)
                    ApplyTranscriptionAppearance()
                    UpdateCommandCategoryAppearance()
                    UpdateCommandStrip()
                    _commandExecutor.ApplyScanViewColourScheme()
                End If

            End Using

            Return

        End If

        If button Is btnCategoryUpload Then

            Await UploadCurrentBatchAsync()
            Return

        End If

        If button Is btnCategoryHelp Then

            Using form As New HelpForm()
                form.ShowDialog(Me)
            End Using

            Return

        End If

        If button Is btnCategoryVerify Then

            Dim verifyVisible As Boolean = _selectedCommandCategory <> button.Text

            ' Do not open the Verify strip when there is nothing left to verify.
            If verifyVisible AndAlso FindNextUnverifiedRow(0) < 0 Then
                ShowStatusMessage("All rows have already been verified.")
                Return
            End If

            If verifyVisible Then
                _selectedCommandCategory = button.Text
            Else
                _selectedCommandCategory = ""
            End If

            _commandExecutor.SetVerifyVisible(verifyVisible)

            UpdateCommandCategoryAppearance()
            UpdateCommandStrip()

            Return

        End If

        If _selectedCommandCategory = button.Text Then
            _selectedCommandCategory = ""
        Else
            _selectedCommandCategory = button.Text
        End If

        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

    End Sub

    Private Sub ApplyCapitalisationChanges(changes As IEnumerable(Of OptionsForm.CapitalisationChange))

        For Each change As OptionsForm.CapitalisationChange In changes

            ' AsTyped cannot meaningfully transform existing data.
            If change.NewMode = CapitalisationMode.AsTyped Then
                Continue For
            End If

            If Not CapitalisationColumnHasData(change.Field) Then
                Continue For
            End If

            Dim fieldName As String = FieldMetaData.Meta(change.Field).Header

            Dim result As DialogResult = MessageBox.Show(
            Me,
            $"Capitalisation for {fieldName} has changed from {change.OldMode} to {change.NewMode}." &
            Environment.NewLine &
            Environment.NewLine &
            $"Do you want WinBMD2 to change all existing values in the {fieldName} column?",
            "Capitalisation Changed",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                ApplyCapitalisationToColumn(change.Field, change.NewMode)
            End If

        Next

        If _workfile.CreateOrReplace(transcriptionGrid) Then _changeState.WorkfileSaved()
    End Sub
    Private Function CapitalisationColumnHasData(field As GridField) As Boolean

        If Not transcriptionGrid.Columns.Contains(field.ToString()) Then
            Return False
        End If

        Dim columnIndex As Integer = transcriptionGrid.Columns(field.ToString()).Index

        For Each row As DataGridViewRow In transcriptionGrid.Rows

            If IsBlankEntryRow(row) Then
                Continue For
            End If

            Dim value As String = If(row.Cells(columnIndex).Value, "").ToString()

            If Not String.IsNullOrWhiteSpace(value) Then
                Return True
            End If

        Next

        Return False

    End Function
    Private Sub ApplyCapitalisationToColumn(field As GridField, mode As CapitalisationMode)

        If Not transcriptionGrid.Columns.Contains(field.ToString()) Then
            Return
        End If

        Dim columnIndex As Integer = transcriptionGrid.Columns(field.ToString()).Index

        For Each row As DataGridViewRow In transcriptionGrid.Rows

            If IsBlankEntryRow(row) Then
                Continue For
            End If

            Dim cell As DataGridViewCell = row.Cells(columnIndex)
            Dim original As String = If(cell.Value, "").ToString()

            If String.IsNullOrWhiteSpace(original) Then
                Continue For
            End If

            Dim updated As String = CapitalisationHelper.Apply(original, mode)

            If field = GridField.Forename Then
                updated = CapitalisationHelper.NormaliseReservedForename(updated)
            End If

            If Not String.Equals(original, updated, StringComparison.Ordinal) Then
                cell.Value = updated
            End If

        Next

    End Sub
    Private Async Function UploadCurrentBatchAsync() As Task

        _pickListPopup.Hide()

        If transcriptionGrid.IsCurrentCellInEditMode Then
            transcriptionGrid.EndEdit()
        End If

        ' Find the last actual transcription row, ignoring the blank entry row.
        Dim lastDataRowIndex As Integer = -1

        For rowIndex As Integer = transcriptionGrid.Rows.Count - 1 To 0 Step -1

            If Not IsBlankEntryRow(transcriptionGrid.Rows(rowIndex)) Then
                lastDataRowIndex = rowIndex
                Exit For
            End If

        Next

        ' Check that the last transcription row has a +PAGE directive.
        If lastDataRowIndex >= 0 Then

            Dim directiveCell As DataGridViewCell = transcriptionGrid.Rows(lastDataRowIndex).Cells(GridField.Directive.ToString())
            Dim directives As List(Of RowDirective) = TryCast(directiveCell.Tag, List(Of RowDirective))
            Dim hasPageDirective As Boolean = False

            If directives IsNot Nothing Then

                For Each directive As RowDirective In directives

                    If directive.DirectiveType.Equals("+PAGE", StringComparison.OrdinalIgnoreCase) Then
                        hasPageDirective = True
                        Exit For
                    End If

                Next

            End If

            If Not hasPageDirective Then

                Dim answer As DialogResult = MessageBox.Show(Me,
                "The last transcription row does not have a +PAGE directive." & Environment.NewLine & Environment.NewLine &
                "Do you want WinBMD2 to add one?",
                "Missing PAGE Directive",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

                If answer = DialogResult.Yes Then

                    ' Find the most recent preceding +PAGE and increment its page number.
                    Dim previousPageNumber As Integer = ProjectValues.Page

                    For previousRowIndex As Integer = 0 To lastDataRowIndex - 1

                        Dim previousCell As DataGridViewCell = transcriptionGrid.Rows(previousRowIndex).Cells(GridField.Directive.ToString())
                        Dim previousDirectives As List(Of RowDirective) = TryCast(previousCell.Tag, List(Of RowDirective))

                        If previousDirectives Is Nothing Then
                            Continue For
                        End If

                        For Each directive As RowDirective In previousDirectives

                            If Not directive.DirectiveType.Equals("+PAGE", StringComparison.OrdinalIgnoreCase) Then
                                Continue For
                            End If

                            Dim pageNumber As Integer

                            If Integer.TryParse(directive.Text, pageNumber) Then
                                previousPageNumber = pageNumber
                            End If

                        Next

                    Next

                    Dim nextPageNumber As Integer = previousPageNumber + 1

                    If directives Is Nothing Then
                        directives = New List(Of RowDirective)()
                    End If

                    directives.Add(New RowDirective With {
                    .RowIndex = lastDataRowIndex,
                    .DirectiveType = "+PAGE",
                    .Lines = Nothing,
                    .Text = nextPageNumber.ToString()
                })

                    directiveCell.Tag = directives
                    UpdateDirectiveCell(lastDataRowIndex)

                    _changeState.SetFileChanged()

                    If _workfile.CreateOrReplace(transcriptionGrid) Then
                        _changeState.WorkfileSaved()
                    End If

                End If

            End If

        End If

        Dim filePath As String = Path.Combine(AppPaths.SaveFolder, ProjectValues.BatchName)

        ShowStatusMessage("Saving transcription before upload...")

        DebugLog.WriteAlways("===== UPLOAD STARTED =====")
        DebugLog.WriteAlways($"[UPLOAD] Saving current transcription before upload: '{filePath}'")

        If Not SaveCurrentBatch(filePath) Then

            DebugLog.WriteAlways("[UPLOAD] Save failed. Upload cancelled.")
            ShowStatusMessage("Upload cancelled because the transcription could not be saved.")

            MessageBox.Show(Me, "The transcription could not be saved, so it has not been uploaded.", "Upload", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Return

        End If

        ShowStatusMessage("Connecting to FreeBMD...")

        Dim uploader As New FreeBmdUploader(
        Sub(message) ShowStatusMessage(message),
        Sub(message) DebugLog.WriteAlways($"[UPLOAD] {message}"),
        Function(message, title) MessageBox.Show(Me, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes)

        Dim request As New FreeBmdUploader.UploadRequest With {
        .BatchFilePath = filePath,
        .UploadName = Path.GetFileNameWithoutExtension(ProjectValues.BatchName),
        .Username = ProjectValues.UserName,
        .Password = ProjectValues.UserPW,
        .Site = ProjectValues.UploadServerUrl,
        .Port = 443,
        .CurrentDistrictVersion = ProjectValues.DistrictVersion,
        .DistrictFolderPath = AppPaths.FilesFolder,
        .Year = ProjectValues.Year,
        .Quarter = ProjectValues.Quarter,
        .AskBeforeReplace = True
    }

        Try

            btnCategoryUpload.Enabled = False

            Dim result As FreeBmdUploader.UploadResult = Await uploader.UploadFileAsync(request, CancellationToken.None)

            DebugLog.WriteAlways($"[UPLOAD] Success: {result.Success}")
            DebugLog.WriteAlways($"[UPLOAD] Message: {result.Message}")

            If result.Success Then

                ShowStatusMessage("Upload completed successfully.")
                MessageBox.Show(Me, result.Message, "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Else

                ShowStatusMessage("Upload failed.")
                MessageBox.Show(Me, result.Message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)

            End If

        Catch ex As Exception

            DebugLog.LogException("Uploading transcription", ex)
            ShowStatusMessage("Upload failed.")
            MessageBox.Show(Me, "The upload could not be completed." & Environment.NewLine & Environment.NewLine & ex.Message, "Upload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally

            UpdateUploadEnabled()

        End Try

    End Function
    Private Sub btnFileOpen_Click(sender As Object, e As EventArgs) Handles btnFileOpen.Click

        BuildOpenFileMenu()

        _openFileMenu.Show(
        btnFileOpen, New Point(0, btnFileOpen.Height))

        _selectedCommandCategory = ""
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

    End Sub
    Private Sub BrowseFile_Click(sender As Object, e As EventArgs)

        _commandExecutor.Execute(AppCommand.OpenFile)

    End Sub
    Private Sub btnFileSave_Click(sender As Object, e As EventArgs) Handles btnFileSave.Click

        _selectedCommandCategory = ""
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

        _commandExecutor.Execute(AppCommand.SaveFile)

    End Sub
    Private Sub btnFileSaveAs_Click(sender As Object, e As EventArgs) Handles btnFileSaveAs.Click

        _selectedCommandCategory = ""
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

        _commandExecutor.Execute(AppCommand.SaveFileAs)

    End Sub
    Private Sub btnFileEditHeader_Click(sender As Object, e As EventArgs) Handles btnFileEditHeader.Click

        _selectedCommandCategory = ""
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

        Dim oldBatchName As String = ProjectValues.BatchName

        Using form As New HeaderForm(True)

            If form.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

        End Using

        Dim newFilePath As String =
        Path.Combine(AppPaths.SaveFolder, ProjectValues.BatchName)

        If Not SaveCurrentBatch(newFilePath) Then

            MessageBox.Show(
            Me,
            "The transcription could not be saved with the edited header.",
            "Edit Header",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

            Return

        End If

        If Not LoadBatchFile(newFilePath) Then

            MessageBox.Show(
            Me,
            "The edited transcription was saved, but could not be reloaded.",
            "Edit Header",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

            Return

        End If

        DebugLog.Write(
        $"[HEADER EDIT] Header updated. OldBatchName='{oldBatchName}', NewBatchName='{ProjectValues.BatchName}'.")

    End Sub
    Friend Sub CreateWorkfile()
        _workfile.CreateOrReplace(transcriptionGrid)
    End Sub

    Private Sub btnFileExit_Click(sender As Object, e As EventArgs) Handles btnFileExit.Click

        Close

    End Sub

    Private Sub BuildOpenFileMenu()

        _openFileMenu.Items.Clear()

        For Each filePath As String In ProjectValues.RecentFiles

            If String.IsNullOrWhiteSpace(filePath) Then
                Continue For
            End If

            Dim item As New ToolStripMenuItem(Path.GetFileName(filePath)) With {.Tag = filePath}

            AddHandler item.Click, AddressOf RecentFile_Click

            _openFileMenu.Items.Add(item)

        Next

        If _openFileMenu.Items.Count > 0 Then
            _openFileMenu.Items.Add(New ToolStripSeparator())
        End If

        Dim browseItem As New ToolStripMenuItem("Browse...")

        AddHandler browseItem.Click, AddressOf BrowseFile_Click

        _openFileMenu.Items.Add(browseItem)

    End Sub
    Private Sub RecentFile_Click(sender As Object, e As EventArgs)

        Dim item As ToolStripMenuItem =
        DirectCast(sender, ToolStripMenuItem)

        Dim filePath As String =
        DirectCast(item.Tag, String)

        If Not File.Exists(filePath) Then

            ProjectValues.RecentFiles.Remove(filePath)
            ProjectValuesStore.Save()

            MessageBox.Show(
            Me,
            "That file no longer exists and has been removed from the recent files list.",
            "Open Batch",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)

            Return

        End If

        If Not ConfirmSaveChangesIfNeeded() Then
            Return
        End If

        LoadBatchFile(filePath)

    End Sub
    Private Sub btnSpecialCharacters_Click(sender As Object, e As EventArgs) Handles btnSpecialCharacters.Click

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        transcriptionGrid.Focus()

        If Not transcriptionGrid.IsCurrentCellInEditMode Then
            transcriptionGrid.BeginEdit(selectAll:=False)
        End If

        Dim editor As TextBox = TryCast(transcriptionGrid.EditingControl, TextBox)

        If editor Is Nothing Then
            Return
        End If

        ShowSpecialCharacters(editor)

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

        Dim directiveCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Directive.ToString())
        Dim directives As List(Of RowDirective) = TryCast(directiveCell.Tag, List(Of RowDirective))

        directives = If(directives, New List(Of RowDirective)())

        Dim suggestedPageNumber As Integer = ProjectValues.Page

        For previousRowIndex As Integer = 0 To rowIndex

            Dim previousCell As DataGridViewCell = transcriptionGrid.Rows(previousRowIndex).Cells(GridField.Directive.ToString())
            Dim previousDirectives As List(Of RowDirective) = TryCast(previousCell.Tag, List(Of RowDirective))

            If previousDirectives Is Nothing Then
                Continue For
            End If

            For Each directive As RowDirective In previousDirectives

                If Not directive.DirectiveType.Equals("+PAGE", StringComparison.OrdinalIgnoreCase) Then
                    Continue For
                End If

                Dim pageNumber As Integer

                If Integer.TryParse(directive.Text, pageNumber) Then
                    suggestedPageNumber = pageNumber
                End If

            Next

        Next

        suggestedPageNumber += 1

        Using form As New DirectiveEditorForm(rowIndex + 1, directives, suggestedPageNumber)

            If form.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Dim updatedDirectives As New List(Of RowDirective)

            For Each directive As RowDirective In form.Directives
                directive.RowIndex = rowIndex
                updatedDirectives.Add(directive)
            Next

            directiveCell.Tag = updatedDirectives

        End Using

        UpdateDirectiveCell(rowIndex)
        _changeState.FileSaved()
        If _workfile.CreateOrReplace(transcriptionGrid) Then _changeState.WorkfileSaved()

    End Sub
    Private Sub UpdateDirectiveCell(
    rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then
            Return
        End If

        Dim cell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Directive.ToString())

        Dim directives As List(Of RowDirective) = TryCast(cell.Tag, List(Of RowDirective))

        If directives Is Nothing OrElse directives.Count = 0 Then

            cell.Value = "+"
            cell.ToolTipText = "Click to add or edit directives to be inserted after this row."

            Return

        End If

        cell.Value =
        $"+{directives.Count}"

        cell.ToolTipText =
        "Click to edit directives for this row." &
        Environment.NewLine & Environment.NewLine &
        String.Join(
            Environment.NewLine,
            directives.Select(Function(item) item.ToString()))

    End Sub
    Private Sub UpdateUploadEnabled()

        Dim dataRowCount As Integer = 0
        Dim allVerified As Boolean = True

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

            DebugLog.Write("Upload check: Row=" & rowIndex & ", Last=" & (transcriptionGrid.Rows.Count - 1) & ", IsNewRow=" & transcriptionGrid.Rows(rowIndex).IsNewRow & ", Blank=" & IsBlankEntryRow(transcriptionGrid.Rows(rowIndex)) & ", Verified=" & IsRowVerified(rowIndex))

            ' Ignore only the final blank row, which is provided for entering the next record.
            If rowIndex = transcriptionGrid.Rows.Count - 1 AndAlso IsBlankEntryRow(transcriptionGrid.Rows(rowIndex)) Then Continue For

            dataRowCount += 1

            If Not IsRowVerified(rowIndex) Then
                allVerified = False
                Exit For
            End If

        Next

        btnCategoryUpload.Enabled = dataRowCount > 0 AndAlso allVerified

        If btnCategoryUpload.Enabled Then
            uploadToolTip.SetToolTip(btnCategoryUpload, "Upload the completed transcription.")
        Else
            uploadToolTip.SetToolTip(btnCategoryUpload, "Upload is not available until all rows are verified.")
        End If

    End Sub
    Friend Function VerifyRowHasError(rowIndex As Integer) As Boolean

        Dim state As ValidationState

        If Not _rowValidationStates.TryGetValue(rowIndex, state) Then
            Return False
        End If

        Return state = ValidationState.Error

    End Function
    Private Sub SaveFormBounds()

        If WindowState = FormWindowState.Minimized Then
            Return
        End If

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

        Try
            transcriptionGrid.Columns.Clear()
            transcriptionGrid.AutoGenerateColumns = False
            transcriptionGrid.AllowUserToAddRows = False

            AddRowNumberColumn()

            Dim fields() As GridField = GridLayout.GetVisibleFields()
            Dim layoutKey As String = GetGridLayoutKey()
            Dim savedWidths As List(Of Integer) = Nothing

            ProjectValues.GridColumnWidths.TryGetValue(layoutKey, savedWidths)

            If savedWidths IsNot Nothing AndAlso savedWidths.Count <> fields.Length Then
                ProjectValues.GridColumnWidths.Remove(layoutKey)
                savedWidths = Nothing
                DebugLog.WriteAlways("[GRID] Saved column widths for " & layoutKey & " do not match the current layout and have been discarded.")
            End If

            For fieldIndex As Integer = 0 To fields.Length - 1

                Dim field As GridField = fields(fieldIndex)
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
                If field = GridField.Directive OrElse field = GridField.Verified Then
                    column.MinimumWidth = 30
                    column.Width = 60
                End If
                If field = GridField.Directive OrElse field = GridField.Verified Then column.MinimumWidth = 30

                If savedWidths IsNot Nothing Then column.Width = Math.Max(column.MinimumWidth, savedWidths(fieldIndex))

                column.DefaultCellStyle.Alignment = GetGridAlignment(fieldInformation.Align)
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter

                If field = GridField.Directive Then
                    column.HeaderText = "+"
                    column.HeaderCell.ToolTipText = "Directives — click to add or edit directives to be inserted after this row."
                ElseIf field = GridField.Verified Then
                    column.HeaderText = "✓"
                    column.HeaderCell.ToolTipText = "Verified — indicates that this transcription row has been verified."
                End If

                If field = GridField.Directive OrElse field = GridField.Verified Then column.MinimumWidth = 30
                If field = GridField.Verified Then
                    column.DefaultCellStyle.ForeColor = Color.Green
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                    column.DefaultCellStyle.Font = New Font(transcriptionGrid.Font, FontStyle.Bold)
                End If

                transcriptionGrid.Columns.Add(column)

            Next

            ThemeManager.ApplyDataGrid(transcriptionGrid)

            DebugLog.WriteAlways("[GRID] Created " & transcriptionGrid.Columns.Count.ToString() & " columns.")

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
        Return GridLayout.GetLayoutStartYear().ToString() & "|" & ProjectValues.BatchType
    End Function
    Private Sub transcriptionGrid_ColumnWidthChanged(sender As Object, e As DataGridViewColumnEventArgs) Handles transcriptionGrid.ColumnWidthChanged

        If _configuringGrid Then Return
        If e.Column Is Nothing Then Return
        If e.Column.Name = "RowNumber" Then Return
        If e.Column.Tag Is Nothing Then Return

        Dim layoutKey As String = GetGridLayoutKey()
        Dim savedWidths As New List(Of Integer)

        For Each column As DataGridViewColumn In transcriptionGrid.Columns
            If column.Name <> "RowNumber" Then savedWidths.Add(column.Width)
        Next

        ProjectValues.GridColumnWidths(layoutKey) = savedWidths
        ProjectValuesStore.Save()

    End Sub
    Private Sub transcriptionGrid_CurrentCellChanged(sender As Object, e As EventArgs) Handles transcriptionGrid.CurrentCellChanged

        _pickListPopup.Hide()
        UpdateStatusPosition()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim rowIndex As Integer =
        transcriptionGrid.CurrentCell.RowIndex

        If rowIndex = _lastGridRowIndex Then
            Return
        End If

        _lastGridRowIndex = rowIndex

        _workfile.SetCurrentRow(rowIndex, transcriptionGrid, _changeState)

        RaiseEvent CurrentGridRowChanged(Me, EventArgs.Empty)

    End Sub
    Private Sub transcriptionGrid_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles transcriptionGrid.CellValueChanged

        If _refreshingGrid Then
            Return
        End If

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then
            Return
        End If

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(e.ColumnIndex)

        If column.Name = "RowNumber" OrElse column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField = DirectCast(column.Tag, GridField)
        Dim cell As DataGridViewCell = transcriptionGrid.Rows(e.RowIndex).Cells(e.ColumnIndex)
        Dim isDataColumn As Boolean = FieldMetaData.Meta(field).IsDataColumn

        If isDataColumn Then
            ClearRowVerified(e.RowIndex)
        End If

        _changeState.SetCellChanged()

        EnsureBlankEntryRow()
        UpdateRowNumbers()

    End Sub
    ' Shows the row validation message when the mouse is over
    ' the row-number area containing the validation indicator.
    Private Sub transcriptionGrid_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) _
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
    Friend Shared Function IsBlankEntryRow(gridRow As DataGridViewRow) As Boolean

        For Each cell As DataGridViewCell In gridRow.Cells

            Dim column As DataGridViewColumn = cell.OwningColumn

            If column.Tag Is Nothing Then Continue For

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If Not FieldMetaData.Meta(field).IsDataColumn Then Continue For

            Dim value As String = If(cell.Value, "").ToString()

            If Not String.IsNullOrWhiteSpace(value) Then Return False

        Next

        Return True

    End Function
    ' Returns the index of the visible Volume or DistNum column.
    ' Returns -1 if the current layout has no volume-equivalent column.
    Public Function GetVolumeColumn() As Integer

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField = DirectCast(column.Tag, GridField)
            Dim fieldInformation As FieldMeta = FieldMetaData.Meta(field)

            If fieldInformation.IsVolumeField Then
                Return column.Index
            End If

        Next

        Return -1

    End Function
    ' Updates the normal status-bar position text.
    ' If a temporary message is currently being displayed, this
    ' becomes the text that will be restored when that message ends.
    ' Updates the row and column position shown at the
    ' right-hand side of the status bar.
    Friend Sub UpdateStatusPosition()

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
                $"{ProjectValues.BatchName}    Row {rowNumber}"
            Return
        End If

        statusPositionLabel.Text =
$"{ProjectValues.BatchName}    Row {rowNumber}, {column.HeaderText}"

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

        Dim keyCode As Keys = keyData And Keys.KeyCode

        Dim modifiers As Keys = keyData And Keys.Modifiers

        ' Shift+Up/Down nudges the scan by the same small amount
        ' used by the arrow keys on the ScanView form.
        If modifiers = Keys.Shift Then

            Select Case keyCode

                Case Keys.Left
                    _commandExecutor.NudgeScan(-2.0F, 0)
                    Return True

                Case Keys.Right
                    _commandExecutor.NudgeScan(2.0F, 0)
                    Return True

                Case Keys.Up
                    _commandExecutor.NudgeScan(0, -2.0F)
                    Return True

                Case Keys.Down
                    _commandExecutor.NudgeScan(0, 2.0F)
                    Return True

            End Select

        End If

        ' Ctrl+Up/Down moves the scan by one complete transcription row
        ' using the row spacing established during ruler calibration.
        If modifiers = Keys.Control Then

            Select Case keyCode

                Case Keys.Up
                    _commandExecutor.MoveScanOneRow(-1)
                    Return True

                Case Keys.Down
                    _commandExecutor.MoveScanOneRow(1)
                    Return True

            End Select

        End If

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

        RemoveHandler editor.KeyPress, AddressOf Capitalisation_KeyPress
        AddHandler editor.KeyPress, AddressOf Capitalisation_KeyPress

        RemoveHandler editor.TextChanged, AddressOf Editor_TextChanged
        AddHandler editor.TextChanged, AddressOf Editor_TextChanged

        RemoveHandler editor.MouseDown, AddressOf GridEditor_MouseDown
        AddHandler editor.MouseDown, AddressOf GridEditor_MouseDown

        editor.BackColor = UiColors.EditBackground
        editor.ForeColor = UiColors.UserText

        If editor.Parent IsNot Nothing Then
            editor.Parent.BackColor = UiColors.EditBackground
        End If

        RefreshPickList(editor)

    End Sub
    Private Sub GridEditor_MouseDown(sender As Object, e As MouseEventArgs)

        If e.Button <> MouseButtons.Right OrElse transcriptionGrid.CurrentCell Is Nothing Then Return

        _pickListPopup.Hide()

        _gridContextRowIndex = transcriptionGrid.CurrentCell.RowIndex

        Dim editor As TextBox = DirectCast(sender, TextBox)
        editor.ContextMenuStrip = _gridRowMenu

    End Sub
    Private Sub Capitalisation_KeyPress(sender As Object, e As KeyPressEventArgs)

        Dim editor As TextBox = TryCast(sender, TextBox)

        If editor Is Nothing OrElse transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        ' # and + at the beginning of a transcription field are reserved for
        ' FreeBMD Directives. Directives must be entered through the Directives
        ' column rather than typed directly into an ordinary data cell.
        If (e.KeyChar = "#"c OrElse e.KeyChar = "+"c) AndAlso editor.SelectionStart = 0 Then

            e.Handled = True

            MessageBox.Show(
                Me,
                "The characters '#' and '+' cannot be entered as the first character of a transcription field." & Environment.NewLine & Environment.NewLine &
                "They are reserved for FreeBMD Directives and must not be typed directly into the transcription grid." & Environment.NewLine & Environment.NewLine &
                "To enter a Directive at this position, click the box in the Directives column on the preceding row and select the required Directive there." & Environment.NewLine & Environment.NewLine &
                "WinBMD2 will then place the Directive after that row and before your current row and save it in the correct FreeBMD format.",
                "Directive Character",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

            Return
        End If

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(transcriptionGrid.CurrentCell.ColumnIndex)

        If column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField = DirectCast(column.Tag, GridField)

        If Not FieldMetaData.Meta(field).IsDataColumn Then
            Return
        End If

        Dim fields() As GridField = GridLayout.GetVisibleFields().Where(Function(item) FieldMetaData.Meta(item).IsDataColumn).ToArray()
        Dim fieldIndex As Integer = Array.IndexOf(fields, field)

        If fieldIndex < 0 Then
            Return
        End If

        Dim mode As CapitalisationMode = CapitalisationData.GetMode(ProjectValues.BatchType, ProjectValues.Year, fieldIndex)
        Dim shiftPressed As Boolean = (Control.ModifierKeys And Keys.Shift) = Keys.Shift

        Dim typedText As String = editor.Text
        Dim typedPosition As Integer = editor.SelectionStart

        If editor.SelectionLength > 0 AndAlso editor.SelectionStart + editor.SelectionLength = editor.TextLength Then
            typedText = editor.Text.Substring(0, editor.SelectionStart)
            typedPosition = typedText.Length
        End If

        e.KeyChar = CapitalisationHelper.ApplyTypedCharacter(e.KeyChar, mode, shiftPressed, typedText, typedPosition)

    End Sub
    Private Sub transcriptionGrid_CellEnter(sender As Object, e As DataGridViewCellEventArgs) Handles transcriptionGrid.CellEnter

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

            _startingCellEdit = True

            ' Beginning an edit populates the editing TextBox from the cell value, which raises
            ' TextChanged even though the user has not changed anything. Suppress actions which
            ' should only occur when the user actually changes the text.
            _startingCellEdit = True

            Try
                transcriptionGrid.BeginEdit(selectAll:=False)
            Finally
                _startingCellEdit = False
            End Try

        End Sub)

    End Sub
    Private Sub Editor_TextChanged(sender As Object, e As EventArgs)

        If _settingCompletionCharacters Then
            Return
        End If

        Dim editor As TextBox = TryCast(sender, TextBox)

        If editor Is Nothing OrElse transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(transcriptionGrid.CurrentCell.ColumnIndex)

        If Not _startingCellEdit AndAlso column.Tag IsNot Nothing Then

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If FieldMetaData.Meta(field).IsDataColumn Then
                ClearRowVerified(transcriptionGrid.CurrentCell.RowIndex)
            End If

            If field = GridField.District Then
                ClearLinkedDistrictCodeCell()
            End If

        End If

        RefreshPickList(editor)

    End Sub
    Private Sub ClearLinkedDistrictCodeCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim volumeColumn As Integer = GetVolumeColumn()

        If volumeColumn < 0 Then
            Return
        End If

        transcriptionGrid.Rows(transcriptionGrid.CurrentCell.RowIndex).Cells(volumeColumn).Value = ""

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

        Dim fromWords() As String = previousText.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)

        If fromWords.Length = 0 Then
            Return False
        End If

        Dim currentText As String =
        editor.Text.Trim()

        If currentText.Length = 0 Then

            editor.Text = fromWords(0)

            If Not editor.Text.EndsWith(" "c) AndAlso Not editor.Text.EndsWith("."c) Then
                editor.Text &= " "
            End If

        Else

            Dim toWords() As String = currentText.Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)

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

                    If Not editor.Text.EndsWith(" "c) AndAlso Not editor.Text.EndsWith("."c) Then
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
    Private Sub RefreshPickList(editor As TextBox)

        If _gridRowMenu.Visible Then
            _pickListPopup.Hide()
            Return
        End If

        _pickListActive = False

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

        Dim typedText As String = editor.Text

        If editor.SelectionLength > 0 AndAlso
       editor.SelectionStart + editor.SelectionLength = editor.TextLength Then

            typedText = editor.Text.Substring(0, editor.SelectionStart)

        End If

        Dim items As List(Of PickListItem)

        Select Case field

            Case GridField.Forename

                items =
                ForenameData.
                GetMatches(typedText, 9).
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
                GetMatches(typedText, 9).
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

        _pickListActive = True

        If _suppressNextAutoComplete Then

            _suppressNextAutoComplete = False

        ElseIf ProjectValues.PickListCompletion AndAlso Not String.IsNullOrEmpty(typedText) Then

            Dim wordStart As Integer = typedText.LastIndexOf(" "c) + 1
            Dim currentWord As String = typedText.Substring(wordStart)
            Dim completion As String = items(0).Text

            Dim fields() As GridField = GridLayout.GetVisibleFields().Where(Function(item) FieldMetaData.Meta(item).IsDataColumn).ToArray()
            Dim fieldIndex As Integer = Array.IndexOf(fields, field)

            If fieldIndex >= 0 Then
                Dim mode As CapitalisationMode = CapitalisationData.GetMode(ProjectValues.BatchType, ProjectValues.Year, fieldIndex)
                completion = CapitalisationHelper.Apply(completion, mode)
            End If

            If Not String.IsNullOrEmpty(currentWord) AndAlso completion.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase) AndAlso completion.Length > currentWord.Length Then

                _settingCompletionCharacters = True

                Try
                    editor.Text = typedText & completion.Substring(currentWord.Length)
                    editor.SelectionStart = typedText.Length
                    editor.SelectionLength = completion.Length - currentWord.Length

                Finally
                    _settingCompletionCharacters = False
                End Try

            End If

        End If

        Dim showPickList As Boolean =
        (field = GridField.Forename AndAlso ProjectValues.ShowForenamePickList) OrElse
        (field = GridField.District AndAlso ProjectValues.ShowDistrictPickList)

        If Not showPickList Then
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
    Friend ReadOnly Property PickListIsActive As Boolean
        Get
            Return _pickListActive
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
    Friend Function CopyFromAboveIfBlank(editor As TextBox, Optional allowPickList As Boolean = False) As Boolean

        ' There must be a current grid cell before anything can be copied.
        If transcriptionGrid.CurrentCell Is Nothing Then
            Return False
        End If

        Dim currentRow As Integer = transcriptionGrid.CurrentCell.RowIndex
        Dim currentColumn As Integer = transcriptionGrid.CurrentCell.ColumnIndex

        ' There is no row above the first row.
        If currentRow <= 0 Then
            Return False
        End If

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(currentColumn)

        If column.Tag Is Nothing Then
            Return False
        End If

        Dim field As GridField = DirectCast(column.Tag, GridField)

        ' Picklist fields are normally handled by the picklist navigation
        ' logic. allowPickList=True is used when that logic deliberately
        ' wants the normal copy-from-above behaviour instead.
        If FieldMetaData.Meta(field).UsesPicklist AndAlso Not allowPickList Then
            Return False
        End If

        ' Obtain the current value from the editing TextBox when the cell is
        ' being edited; otherwise obtain it directly from the grid cell.
        Dim currentText As String

        If editor IsNot Nothing Then
            currentText = editor.Text
        Else
            currentText = If(transcriptionGrid.CurrentCell.Value, "").ToString()
        End If

        ' Never overwrite something the user has already entered.
        If Not String.IsNullOrWhiteSpace(currentText) Then
            Return False
        End If

        ' Get the value from the same column in the previous row.
        Dim previousCell As DataGridViewCell = transcriptionGrid.Rows(currentRow - 1).Cells(currentColumn)
        Dim previousText As String = If(previousCell.Value, "").ToString().Trim()

        If String.IsNullOrWhiteSpace(previousText) Then
            Return False
        End If

        ' Put the copied value into the active editor if there is one.
        ' Otherwise write it directly to the grid cell.
        If editor IsNot Nothing Then
            editor.Text = previousText
            editor.SelectionStart = editor.TextLength
            editor.SelectionLength = 0
        Else
            transcriptionGrid.CurrentCell.Value = previousText
        End If

        ' District and Volume/DistNum belong together. Selecting a District
        ' from the picklist also supplies its Volume/DistNum, so copying a
        ' blank District from the previous row should behave in the same way.
        If field = GridField.District Then

            Dim volumeColumn As Integer = GetVolumeColumn()

            If volumeColumn >= 0 Then
                Dim previousVolume As String = If(transcriptionGrid.Rows(currentRow - 1).Cells(volumeColumn).Value, "").ToString()
                transcriptionGrid.Rows(currentRow).Cells(volumeColumn).Value = previousVolume
            End If

        End If

        Return True

    End Function
    Private Sub ApplyTranscriptionAppearance()

        ThemeManager.Apply(Me)

    End Sub
    Private Sub NavigationKeyDown(sender As Object, e As KeyEventArgs) Handles transcriptionGrid.KeyDown
        _suppressNextAutoComplete = e.KeyCode = Keys.Back OrElse e.KeyCode = Keys.Delete

        If e.KeyCode = Keys.F4 Then

            ShowSpecialCharacters(TryCast(sender, TextBox))

            e.Handled = True
            e.SuppressKeyPress = True
            Return

        End If
        If _navigationManager.HandleKey(sender, e) Then
            e.Handled = True
            e.SuppressKeyPress = True
        End If

    End Sub
    Private Sub ShowSpecialCharacters(editor As TextBox)

        If editor Is Nothing OrElse transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(transcriptionGrid.CurrentCell.ColumnIndex)

        If column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField = DirectCast(column.Tag, GridField)

        If field <> GridField.Surname AndAlso
       field <> GridField.Forename AndAlso
       field <> GridField.Spouse AndAlso
       field <> GridField.Mother Then

            Return

        End If

        Using form As New SpecialCharactersForm()

            If form.ShowDialog(Me) <> DialogResult.OK Then
                editor.Focus()
                Return
            End If

            Dim character As String = form.SelectedCharacter
            Dim selectionStart As Integer = editor.SelectionStart

            editor.SelectedText = character
            editor.SelectionStart = selectionStart + character.Length
            editor.SelectionLength = 0
            editor.Focus()

        End Using

    End Sub
    Friend ReadOnly Property CurrentGridCell As DataGridViewCell
        Get
            Return transcriptionGrid.CurrentCell
        End Get
    End Property
    Friend Sub FocusGridRow(rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then
            Return
        End If

        Dim columnIndex As Integer = GetFirstDataColumn()

        If transcriptionGrid.CurrentCell IsNot Nothing AndAlso
       IsDataColumn(transcriptionGrid.CurrentCell.ColumnIndex) Then

            columnIndex = transcriptionGrid.CurrentCell.ColumnIndex

        End If

        transcriptionGrid.CurrentCell =
        transcriptionGrid.Rows(rowIndex).Cells(columnIndex)

        transcriptionGrid.Focus()
        transcriptionGrid.BeginEdit(selectAll:=False)

        If Not IsHandleCreated OrElse IsDisposed OrElse Disposing Then
            Return
        End If

        BeginInvoke(
        Sub()

            Dim editor As TextBox =
                TryCast(transcriptionGrid.EditingControl, TextBox)

            If editor Is Nothing Then
                Return
            End If

            editor.Focus()
            editor.Select(editor.TextLength, 0)

        End Sub)

    End Sub
    Friend Sub EndGridEdit()

        transcriptionGrid.EndEdit()

    End Sub
    Friend Sub MoveToNextDataCell()

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return
        End If

        Dim currentRow As Integer = transcriptionGrid.CurrentCell.RowIndex
        Dim currentColumn As Integer = transcriptionGrid.CurrentCell.ColumnIndex
        Dim targetRow As Integer = currentRow
        Dim targetColumn As Integer = -1

        If ProjectValues.EntryMode = EntryMode.Vertical Then

            ' In vertical entry mode, forward movement stays in the
            ' same column and moves down one row.
            If currentRow >= transcriptionGrid.Rows.Count - 1 Then
                Return
            End If

            targetRow = currentRow + 1
            targetColumn = currentColumn

        Else

            ' Horizontal entry mode retains the existing District handling.
            Dim currentField As GridField = DirectCast(transcriptionGrid.Columns(currentColumn).Tag, GridField)

            If currentField = GridField.District Then

                Dim nextColumn As Integer = GetNextColumnAfterDistrict(currentRow, currentColumn)

                If nextColumn >= 0 Then

                    transcriptionGrid.EndEdit()
                    transcriptionGrid.CurrentCell = transcriptionGrid.Rows(currentRow).Cells(nextColumn)
                    transcriptionGrid.BeginEdit(selectAll:=False)

                    If DirectCast(transcriptionGrid.Columns(nextColumn).Tag, GridField) = GridField.DistNum Then

                        BeginInvoke(
                    Sub()

                        Dim editor As TextBox = TryCast(transcriptionGrid.EditingControl, TextBox)

                        If editor Is Nothing Then
                            Return
                        End If

                        editor.Focus()
                        editor.SelectionStart = editor.TextLength
                        editor.SelectionLength = 0

                    End Sub)

                    End If

                    Return

                End If

            End If

            ' Normal horizontal movement goes to the next data column.
            For columnIndex As Integer = currentColumn + 1 To transcriptionGrid.Columns.Count - 1

                If IsDataColumn(columnIndex) Then
                    targetColumn = columnIndex
                    Exit For
                End If

            Next

            ' At the end of a row, continue at the first data column of the following row.
            ' This also implements Skip Surname, which copies the surname from the previous row if the next row's surname is blank and jumps to column 2
            If targetColumn < 0 AndAlso currentRow < transcriptionGrid.Rows.Count - 1 Then

                targetRow = currentRow + 1
                targetColumn = GetFirstDataColumn()

                If ProjectValues.SkipSurname Then

                    Dim firstColumn As Integer = GetFirstDataColumn()
                    Dim nextColumn As Integer = -1

                    Dim currentSurname As String = If(transcriptionGrid.Rows(currentRow).Cells(firstColumn).Value, "").ToString()
                    Dim nextSurname As String = If(transcriptionGrid.Rows(targetRow).Cells(firstColumn).Value, "").ToString()

                    If String.IsNullOrWhiteSpace(nextSurname) AndAlso Not String.IsNullOrWhiteSpace(currentSurname) Then
                        transcriptionGrid.Rows(targetRow).Cells(firstColumn).Value = currentSurname
                    End If

                    For columnIndex As Integer = firstColumn + 1 To transcriptionGrid.Columns.Count - 1

                        If IsDataColumn(columnIndex) Then
                            nextColumn = columnIndex
                            Exit For
                        End If

                    Next

                    If nextColumn >= 0 Then
                        targetColumn = nextColumn
                    End If

                End If

            End If

        End If

        If targetColumn < 0 Then
            Return
        End If

        transcriptionGrid.EndEdit()
        transcriptionGrid.CurrentCell = transcriptionGrid.Rows(targetRow).Cells(targetColumn)

        If Not IsHandleCreated OrElse IsDisposed OrElse Disposing Then
            Return
        End If

        BeginInvoke(
            Sub()

                If transcriptionGrid.CurrentCell Is Nothing Then
                    Return
                End If

                If transcriptionGrid.CurrentCell.RowIndex <> targetRow OrElse transcriptionGrid.CurrentCell.ColumnIndex <> targetColumn Then
                    Return
                End If

                If Not transcriptionGrid.IsCurrentCellInEditMode Then
                    transcriptionGrid.BeginEdit(selectAll:=False)
                End If

                Dim editor As TextBox = TryCast(transcriptionGrid.EditingControl, TextBox)

                If editor Is Nothing Then
                    Return
                End If

                editor.Focus()
                editor.Select(editor.TextLength, 0)

            End Sub)

    End Sub
    Private Function GetNextColumnAfterDistrict(currentRow As Integer, districtColumn As Integer) As Integer

        Dim codeColumn As Integer = GetVolumeColumn()

        If codeColumn < 0 Then
            Return -1
        End If

        Dim codeField As GridField = DirectCast(transcriptionGrid.Columns(codeColumn).Tag, GridField)

        ' If something lies between District and the code field,
        ' visit that field normally.
        If codeColumn > districtColumn + 1 Then
            Return districtColumn + 1
        End If

        Dim code As String = If(transcriptionGrid.Rows(currentRow).Cells(codeColumn).Value, "").ToString()

        If String.IsNullOrWhiteSpace(code) Then
            Return codeColumn
        End If

        ' DistNum may contain only the three-character district
        ' code supplied by the picklist. The user must then add
        ' the final two characters manually.
        If codeField = GridField.DistNum AndAlso code.Length = 3 Then
            Return codeColumn
        End If

        ' The linked code is complete, so skip over it.
        For columnIndex As Integer = codeColumn + 1 To transcriptionGrid.Columns.Count - 1

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

        Dim currentRow As Integer = transcriptionGrid.CurrentCell.RowIndex
        Dim currentColumn As Integer = transcriptionGrid.CurrentCell.ColumnIndex
        Dim targetRow As Integer = currentRow
        Dim targetColumn As Integer = -1

        If ProjectValues.EntryMode = EntryMode.Vertical Then

            ' In vertical entry mode, reverse movement stays in the
            ' same column and moves up one row.
            If currentRow <= 0 Then
                Return
            End If

            targetRow = currentRow - 1
            targetColumn = currentColumn

        Else

            ' Normal horizontal reverse movement goes to the
            ' previous data column.
            For columnIndex As Integer = currentColumn - 1 To 0 Step -1

                If IsDataColumn(columnIndex) Then
                    targetColumn = columnIndex
                    Exit For
                End If

            Next

            ' At the beginning of a row, continue at the last data
            ' column of the previous row.
            If targetColumn < 0 AndAlso currentRow > 0 Then
                targetRow = currentRow - 1
                targetColumn = GetLastDataColumn()
            End If

        End If

        If targetColumn < 0 Then
            Return
        End If

        transcriptionGrid.CurrentCell = transcriptionGrid.Rows(targetRow).Cells(targetColumn)
        transcriptionGrid.BeginEdit(selectAll:=False)

        Dim editor As TextBox = TryCast(transcriptionGrid.EditingControl, TextBox)

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
    Friend Function AcceptCurrentPickListSelection(editor As TextBox) As Boolean

        If editor Is Nothing Then
            Return False
        End If

        If transcriptionGrid.CurrentCell Is Nothing Then
            Return False
        End If

        Dim selectedItem As PickListItem = _pickListPopup.SelectedItem

        If selectedItem Is Nothing Then
            Return False
        End If

        Dim currentColumn As DataGridViewColumn = transcriptionGrid.Columns(transcriptionGrid.CurrentCell.ColumnIndex)

        If currentColumn.Tag Is Nothing Then
            Return False
        End If

        Dim currentField As GridField = DirectCast(currentColumn.Tag, GridField)
        Dim selectedText As String = selectedItem.Text

        If ProjectValues.FormatPicklistSelections Then

            Dim fields() As GridField = GridLayout.GetVisibleFields().Where(Function(field) FieldMetaData.Meta(field).IsDataColumn).ToArray()
            Dim fieldIndex As Integer = Array.IndexOf(fields, currentField)

            If fieldIndex >= 0 Then
                Dim mode As CapitalisationMode = CapitalisationData.GetMode(ProjectValues.BatchType, ProjectValues.Year, fieldIndex)
                selectedText = CapitalisationHelper.Apply(selectedText, mode)
            End If

        End If

        If currentField = GridField.Forename Then

            Dim existingText As String = editor.Text
            Dim lastSpace As Integer = existingText.LastIndexOf(" "c)
            Dim currentWord As String = If(lastSpace >= 0, existingText.Substring(lastSpace + 1), existingText)

            ' If the current word already matches the selected picklist entry,
            ' preserve exactly what the user typed, including Shift overrides.
            If String.Equals(currentWord, selectedText, StringComparison.OrdinalIgnoreCase) Then

                editor.SelectionStart = editor.TextLength
                editor.SelectionLength = 0

            ElseIf editor.SelectionLength > 0 AndAlso editor.SelectionStart + editor.SelectionLength = editor.TextLength Then

                ' Inline completion is already present, so accept it exactly as shown.
                editor.SelectionStart = editor.TextLength
                editor.SelectionLength = 0

            Else

                ' Replace the partial forename currently being typed with the
                ' selected picklist entry, while preserving earlier forenames.
                If lastSpace >= 0 Then
                    editor.Text = existingText.Substring(0, lastSpace + 1) & selectedText
                Else
                    editor.Text = selectedText
                End If

            End If

        Else

            editor.Text = selectedText

        End If

        editor.SelectionStart = editor.TextLength
        editor.SelectionLength = 0

        If currentField = GridField.District AndAlso Not String.IsNullOrWhiteSpace(selectedItem.Volume) Then

            Dim volumeColumn As Integer = GetVolumeColumn()

            If volumeColumn >= 0 Then
                transcriptionGrid.Rows(transcriptionGrid.CurrentCell.RowIndex).Cells(volumeColumn).Value = selectedItem.Volume
            End If

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

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(e.ColumnIndex)

        If column.Tag Is Nothing Then
            Return
        End If

        Dim field As GridField = DirectCast(column.Tag, GridField)

        If Not FieldMetaData.Meta(field).IsDataColumn Then
            Return
        End If

        Dim cell As DataGridViewCell =
        transcriptionGrid.Rows(e.RowIndex).Cells(e.ColumnIndex)

        Dim value As String = If(cell.Value, "").ToString()

        If field = GridField.Forename Then

            Dim normalisedValue As String = CapitalisationHelper.NormaliseReservedForename(value)

            If Not String.Equals(value, normalisedValue, StringComparison.Ordinal) Then
                value = normalisedValue
                cell.Value = normalisedValue
            End If

        End If

        Dim result As ValidationResult = Validator.Validate(field, value)

        If field = GridField.District OrElse
           field = GridField.Volume OrElse
           field = GridField.DistNum Then

            ' District and its code are interdependent, so changing either
            ' one requires both cells to be validated again.
            ValidateDistrictCodePair(e.RowIndex, False)

            ' Pick up the final result for the cell which was actually edited.
            Dim pairResult As ValidationResult = Nothing
            If _cellValidationResults.TryGetValue((e.RowIndex, e.ColumnIndex), pairResult) Then result = pairResult

        Else

            ApplyCellValidationResult(e.RowIndex, e.ColumnIndex, result)

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

        If field = GridField.Surname OrElse field = GridField.Forename Then

            ValidateNamePair(e.RowIndex)
            ValidateSequence(e.RowIndex)

            If e.RowIndex + 1 < transcriptionGrid.Rows.Count Then
                ValidateSequence(e.RowIndex + 1)
                UpdateRowValidationState(e.RowIndex + 1)
            End If

            If Not _cellValidationResults.TryGetValue((e.RowIndex, e.ColumnIndex), result) Then Return

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
    ' Revalidates Surname and Forename as a pair.
    ' A completely blank name pair is allowed, but if either name is present
    ' the other field is validated normally and may therefore report blank.
    Private Sub ValidateNamePair(rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then Return

        Dim surnameColumn As Integer = -1
        Dim forenameColumn As Integer = -1

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim column As DataGridViewColumn = transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then Continue For

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If field = GridField.Surname Then
                surnameColumn = columnIndex
            ElseIf field = GridField.Forename Then
                forenameColumn = columnIndex
            End If

        Next

        If surnameColumn < 0 OrElse forenameColumn < 0 Then Return

        Dim row As DataGridViewRow = transcriptionGrid.Rows(rowIndex)
        Dim surname As String = If(row.Cells(surnameColumn).Value, "").ToString().Trim()
        Dim forename As String = If(row.Cells(forenameColumn).Value, "").ToString().Trim()

        If String.IsNullOrWhiteSpace(surname) AndAlso String.IsNullOrWhiteSpace(forename) Then

            _cellValidationResults.Remove((rowIndex, surnameColumn))
            _cellValidationResults.Remove((rowIndex, forenameColumn))

            row.Cells(surnameColumn).ErrorText = ""
            row.Cells(surnameColumn).ToolTipText = ""
            row.Cells(forenameColumn).ErrorText = ""
            row.Cells(forenameColumn).ToolTipText = ""

        Else

            ApplyCellValidationResult(rowIndex, surnameColumn, Validator.Validate(GridField.Surname, surname))
            ApplyCellValidationResult(rowIndex, forenameColumn, Validator.Validate(GridField.Forename, forename))

        End If

        UpdateRowValidationState(rowIndex)

    End Sub
    ' Checks the alphabetical sequence of Surname and Forename against
    ' the previous populated row. This routine performs sequence checking only;
    ' ordinary Surname/Forename validation is handled by ValidateNamePair.
    Private Sub ValidateSequence(rowIndex As Integer)

        If rowIndex <= 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then Return

        Dim surnameColumn As Integer = -1
        Dim forenameColumn As Integer = -1

        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim column As DataGridViewColumn = transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then Continue For

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If field = GridField.Surname Then
                surnameColumn = columnIndex
            ElseIf field = GridField.Forename Then
                forenameColumn = columnIndex
            End If

        Next

        If surnameColumn < 0 OrElse forenameColumn < 0 Then Return

        Dim row As DataGridViewRow = transcriptionGrid.Rows(rowIndex)
        Dim surname As String = If(row.Cells(surnameColumn).Value, "").ToString().Trim()
        Dim forename As String = If(row.Cells(forenameColumn).Value, "").ToString().Trim()

        ' Sequence checking requires both names.
        If String.IsNullOrWhiteSpace(surname) OrElse String.IsNullOrWhiteSpace(forename) Then Return

        For previousIndex As Integer = rowIndex - 1 To 0 Step -1

            Dim previousRow As DataGridViewRow = transcriptionGrid.Rows(previousIndex)
            Dim previousSurname As String = If(previousRow.Cells(surnameColumn).Value, "").ToString().Trim()

            If String.IsNullOrWhiteSpace(previousSurname) Then Continue For

            Dim surnameCompare As Integer = String.Compare(surname, previousSurname, StringComparison.OrdinalIgnoreCase)

            If surnameCompare < 0 Then
                ApplySequenceWarning(rowIndex, surnameColumn, $"Surname '{surname}' is before previous surname '{previousSurname}'.")
                UpdateRowValidationState(rowIndex)
                Return
            End If

            If surnameCompare = 0 Then

                Dim previousForename As String = If(previousRow.Cells(forenameColumn).Value, "").ToString().Trim()
                Dim forenameCompare As Integer = String.Compare(forename, previousForename, StringComparison.OrdinalIgnoreCase)

                If forenameCompare < 0 Then
                    ApplySequenceWarning(rowIndex, forenameColumn, $"Forename '{forename}' is before previous forename '{previousForename}' for surname '{surname}'.")
                    UpdateRowValidationState(rowIndex)
                    Return
                End If

            End If

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
    Private Sub ValidateDistrictCodePair(rowIndex As Integer, Optional promptForNewDistrict As Boolean = True)

        Dim districtColumn As Integer = -1
        Dim codeColumn As Integer = GetVolumeColumn()

        ' Find the visible District column.
        For columnIndex As Integer = 0 To transcriptionGrid.Columns.Count - 1

            Dim column As DataGridViewColumn = transcriptionGrid.Columns(columnIndex)

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If field = GridField.District Then
                districtColumn = columnIndex
                Exit For
            End If

        Next

        If districtColumn < 0 OrElse codeColumn < 0 Then
            Return
        End If

        If Not promptForNewDistrict Then
            Return
        End If

        Dim codeField As GridField = DirectCast(transcriptionGrid.Columns(codeColumn).Tag, GridField)
        Dim row As DataGridViewRow = transcriptionGrid.Rows(rowIndex)

        Dim districtValue As String = If(row.Cells(districtColumn).Value, "").ToString()
        Dim codeValue As String = If(row.Cells(codeColumn).Value, "").ToString()

        ' First perform the normal independent validation of both cells.
        Dim districtResult As ValidationResult = Validator.Validate(GridField.District, districtValue)
        Dim codeResult As ValidationResult = Validator.Validate(codeField, codeValue)

        ' Only add the pair warning where normal validation has succeeded.
        ' A real field error/warning must not be hidden by this check.
        If districtResult.IsOk AndAlso String.IsNullOrWhiteSpace(districtValue) AndAlso Not String.IsNullOrWhiteSpace(codeValue) Then
            districtResult = ValidationResult.Warning("District is blank but Volume/DistNum is present.")
        End If

        If codeResult.IsOk AndAlso Not String.IsNullOrWhiteSpace(districtValue) AndAlso String.IsNullOrWhiteSpace(codeValue) Then
            codeResult = ValidationResult.Warning("Volume/DistNum is blank but District is present.")
        End If

        ApplyCellValidationResult(rowIndex, districtColumn, districtResult)
        ApplyCellValidationResult(rowIndex, codeColumn, codeResult)

        If Not districtResult.IsOk OrElse Not codeResult.IsOk Then
            Return
        End If

        If String.IsNullOrWhiteSpace(districtValue) OrElse String.IsNullOrWhiteSpace(codeValue) Then
            Return
        End If

        If districtValue.IndexOfAny({"*"c, "?"c, "_"c}) >= 0 OrElse codeValue.IndexOfAny({"*"c, "?"c, "_"c}) >= 0 Then
            Return
        End If

        If DistrictData.ContainsDistrictAndCode(districtValue, codeValue) Then
            Return
        End If

        Dim answer As DialogResult = MessageBox.Show(
        $"The District and Volume/DistNum combination '{districtValue}, {codeValue}' is not in the district list." &
        Environment.NewLine & Environment.NewLine &
        "Do you want to add it as a supplementary district?",
        "New District",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question)

        If answer = DialogResult.Yes Then
            DistrictData.AddSupplementaryDistrict(districtValue, codeValue)
        End If

    End Sub
    ' Draws the row-level validation indicator in the row header.
    ' Error rows show a red circle; warning rows show a yellow circle.
    Private Sub transcriptionGrid_RowPostPaint(sender As Object, e As DataGridViewRowPostPaintEventArgs) Handles transcriptionGrid.RowPostPaint

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

        Dim x As Integer = e.RowBounds.Left + 4

        Dim y As Integer = e.RowBounds.Top + (e.RowBounds.Height - diameter) \ 2

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
    Private Sub TranscriptionForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        If AbnormalShutdown Then

            DebugLog.WriteAlways("[RECOVERY] Abnormal shutdown - workfile retained for recovery.")
            SaveFormBounds()
            _pickListPopup.Dispose()
            Return

        End If

        If Not ConfirmSaveChangesIfNeeded() Then
            e.Cancel = True
            Return
        End If

        _workfile.Delete()
        SaveFormBounds()
        _pickListPopup.Dispose()

    End Sub

    Private Sub filePanel_ExpandedChanged(sender As Object, e As EventArgs) Handles filePanel.ExpandedChanged

        ProjectValues.FilePanelExpanded = filePanel.Expanded
        ProjectValuesStore.Save()

    End Sub
    Friend Function FindNextUnverifiedRow(startRow As Integer) As Integer

        For rowIndex As Integer = Math.Max(0, startRow) To transcriptionGrid.Rows.Count - 1

            Dim row As DataGridViewRow =
                transcriptionGrid.Rows(rowIndex)

            If IsBlankEntryRow(row) Then
                Continue For
            End If

            If Not IsRowVerified(rowIndex) Then
                Return rowIndex
            End If

        Next

        Return -1

    End Function
    Friend Function GetVerifyValues(rowIndex As Integer) As Dictionary(Of GridField, String)

        Dim values As New Dictionary(Of GridField, String)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then
            Return values
        End If

        Dim row As DataGridViewRow = transcriptionGrid.Rows(rowIndex)

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField =
                DirectCast(column.Tag, GridField)

            If Not FieldMetaData.Meta(field).IsDataColumn Then
                Continue For
            End If

            values(field) = If(row.Cells(column.Index).Value, "").ToString()

        Next

        Return values

    End Function
    Friend ReadOnly Property CurrentVerifyRowHasError As Boolean
        Get
            Return _verifyRowIndex >= 0 AndAlso VerifyRowHasError(_verifyRowIndex)
        End Get
    End Property
    Friend Function StartVerify() As Dictionary(Of GridField, String)

        _verifyRowIndex = FindNextUnverifiedRow(0)

        If _verifyRowIndex < 0 Then
            Return Nothing
        End If

        If VerifyRowHasError(_verifyRowIndex) Then
            FocusGridRow(_verifyRowIndex)
            Return Nothing
        End If

        transcriptionGrid.CurrentCell = transcriptionGrid.Rows(_verifyRowIndex).Cells(GetFirstDataColumn())

        Return GetVerifyValues(_verifyRowIndex)

    End Function
    Friend Function IsRowVerified(rowIndex As Integer) As Boolean

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then Return False

        Dim verifiedCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Verified.ToString())

        Return verifiedCell.Tag IsNot Nothing AndAlso DirectCast(verifiedCell.Tag, Boolean)

    End Function

    Friend Sub MarkRowVerified(rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then Return

        Dim verifiedCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Verified.ToString())

        verifiedCell.Tag = True
        verifiedCell.Value = "✓"

        VerificationData.Save(ProjectValues.BatchName, BuildVerificationState())
        UpdateUploadEnabled()

    End Sub

    Friend Sub ClearRowVerified(rowIndex As Integer)

        If rowIndex < 0 OrElse rowIndex >= transcriptionGrid.Rows.Count Then Return

        Dim verifiedCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Verified.ToString())

        verifiedCell.Tag = False
        verifiedCell.Value = ""

        VerificationData.Save(ProjectValues.BatchName, BuildVerificationState())
        UpdateUploadEnabled()

    End Sub
    Friend Function CompleteCurrentVerify(values As Dictionary(Of GridField, String)) As Integer

        If _verifyRowIndex < 0 OrElse _verifyRowIndex >= transcriptionGrid.Rows.Count Then
            Return -1
        End If

        Dim row As DataGridViewRow = transcriptionGrid.Rows(_verifyRowIndex)

        For Each column As DataGridViewColumn In transcriptionGrid.Columns

            If column.Tag Is Nothing Then
                Continue For
            End If

            Dim field As GridField = DirectCast(column.Tag, GridField)

            If Not FieldMetaData.Meta(field).IsDataColumn Then
                Continue For
            End If

            Dim value As String = ""
            values.TryGetValue(field, value)

            row.Cells(column.Index).Value = value

        Next

        MarkRowVerified(_verifyRowIndex)

        _changeState.SetFileChanged()

        Dim nextRow As Integer = FindNextUnverifiedRow(_verifyRowIndex + 1)

        _verifyRowIndex = nextRow

        If nextRow >= 0 AndAlso VerifyRowHasError(nextRow) Then
            FocusGridRow(nextRow)
            Return -2
        End If

        If nextRow >= 0 Then
            transcriptionGrid.CurrentCell = transcriptionGrid.Rows(nextRow).Cells(GetFirstDataColumn())
            Return nextRow
        End If

        ' All transcription rows have now been verified.
        _selectedCommandCategory = ""
        _commandExecutor.SetVerifyVisible(False)
        UpdateCommandCategoryAppearance()
        UpdateCommandStrip()

        ShowStatusMessage("All rows have been verified.")

        transcriptionGrid.Focus()

        Return -1

    End Function
    Friend Function GetCurrentVerifyValues() As Dictionary(Of GridField, String)

        If _verifyRowIndex < 0 Then
            Return Nothing
        End If

        Return GetVerifyValues(_verifyRowIndex)

    End Function
    Private Function BuildVerificationState() As String

        Dim result As New Text.StringBuilder

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

            If IsBlankEntryRow(transcriptionGrid.Rows(rowIndex)) Then Continue For

            If IsRowVerified(rowIndex) Then
                result.Append("1"c)
            Else
                result.Append("0"c)
            End If

        Next

        Return result.ToString()

    End Function
    Private Sub ApplyVerificationState(state As String)

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

            Dim verifiedCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Verified.ToString())

            verifiedCell.Tag = False
            verifiedCell.Value = ""

        Next

        If String.IsNullOrEmpty(state) Then Return

        Dim rowCount As Integer = Math.Min(state.Length, transcriptionGrid.Rows.Count)

        For rowIndex As Integer = 0 To rowCount - 1

            If state(rowIndex) <> "1"c Then Continue For

            Dim verifiedCell As DataGridViewCell = transcriptionGrid.Rows(rowIndex).Cells(GridField.Verified.ToString())

            verifiedCell.Tag = True
            verifiedCell.Value = "✓"

        Next

    End Sub
    Private Sub ValidateLoadedRows()

        _cellValidationResults.Clear()
        _rowValidationStates.Clear()

        For rowIndex As Integer = 0 To transcriptionGrid.Rows.Count - 1

            Dim row As DataGridViewRow = transcriptionGrid.Rows(rowIndex)

            If IsBlankEntryRow(row) Then
                Continue For
            End If

            For Each column As DataGridViewColumn In transcriptionGrid.Columns

                If column.Tag Is Nothing Then
                    Continue For
                End If

                Dim field As GridField = DirectCast(column.Tag, GridField)

                If Not FieldMetaData.Meta(field).IsDataColumn Then
                    Continue For
                End If

                If field = GridField.District OrElse field = GridField.Volume OrElse field = GridField.DistNum Then
                    Continue For
                End If

                Dim value As String = If(row.Cells(column.Index).Value, "").ToString()
                Dim result As ValidationResult = Validator.Validate(field, value)

                ApplyCellValidationResult(rowIndex, column.Index, result)

            Next

            ValidateDistrictCodePair(rowIndex)
            ValidateNamePair(rowIndex)
            ValidateSequence(rowIndex)
            UpdateRowValidationState(rowIndex)
            Dim rowState As ValidationState

            If _rowValidationStates.TryGetValue(rowIndex, rowState) AndAlso rowState = ValidationState.Error Then
                row.Cells(GridField.Verified.ToString()).Tag = False
                row.Cells(GridField.Verified.ToString()).Value = ""
            End If

        Next

        VerificationData.Save(ProjectValues.BatchName, BuildVerificationState())
        UpdateUploadEnabled()
    End Sub
    Private Sub TranscriptionForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed

        RemoveHandler ProjectValues.StatusMessageRequested, AddressOf ProjectValues_StatusMessageRequested

    End Sub
    Private Sub transcriptionGrid_CellLeave(sender As Object, e As DataGridViewCellEventArgs) Handles transcriptionGrid.CellLeave

        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return

        Dim column As DataGridViewColumn = transcriptionGrid.Columns(e.ColumnIndex)

        If column.Tag Is Nothing Then Return

        Dim field As GridField = DirectCast(column.Tag, GridField)

        If field = GridField.District OrElse field = GridField.Volume OrElse field = GridField.DistNum Then
            ValidateDistrictCodePair(e.RowIndex)
        End If

    End Sub
    Friend Function RecoverWorkfile() As Boolean

        Dim result As Workfile.LoadResult = _workfile.TryLoad()

        If Not result.Success OrElse result.Lines Is Nothing Then
            DebugLog.WriteAlways("[WORKFILE] Recovery failed: " & result.ErrorMessage)
            Return False
        End If

        _refreshingGrid = True

        Try

            If Not LoadSaveFiles.LoadLines(result.Lines, transcriptionGrid, AddressOf ConfigureGridColumns) Then
                DebugLog.WriteAlways("[WORKFILE] Recovered workfile could not be loaded into the transcription form.")
                Return False
            End If

            FinishLoadingBatch()

            _changeState.SetFileChanged()

            UpdateStatusPosition()

            DebugLog.WriteAlways("[WORKFILE] Recovery completed successfully.")

            Return True

        Finally
            _refreshingGrid = False
        End Try

    End Function
End Class