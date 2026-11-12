Imports System.IO
Imports System.Runtime.InteropServices

Public NotInheritable Class CommandController
    Inherits ApplicationContext
    Implements ICommandExecutor

    Private _transcriptionForm As TranscriptionForm
    Private _scanView As ScanView
    Private _startingNewFile As Boolean
    Private _previousGridRow As Integer = 0
    ' Prevents startup positions from overwriting the position being restored.
    Private _restoringPosition As Boolean
    Private _bringingFormsForward As Boolean

    Private Const SWP_NOSIZE As UInteger = &H1UI
    Private Const SWP_NOMOVE As UInteger = &H2UI
    Private Const SWP_NOACTIVATE As UInteger = &H10UI
    Private Const SWP_NOOWNERZORDER As UInteger = &H200UI

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowPos(hWnd As IntPtr, hWndInsertAfter As IntPtr, x As Integer, y As Integer, cx As Integer, cy As Integer, flags As UInteger) As Boolean
    End Function

    ' Identifies the batch belonging to the image currently displayed.
    Private _scanPositionEntry As RecentFileEntry

    ' Retains a startup scan position if the scan is opened later.
    Private _pendingScanEntry As RecentFileEntry
    Private _pendingScanY As Double?

    ' Copies the current position into the MRU immediately before JSON is written.
    ' This routine does not request another save.
    Private Sub CaptureCurrentPosition()
        If _restoringPosition Then Return
        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return

        Dim entry As RecentFileEntry = _transcriptionForm.PositionEntry
        Dim rowIndex As Integer? = _transcriptionForm.CurrentPositionRow

        If entry Is Nothing OrElse Not rowIndex.HasValue Then Return

        entry.GridRow = rowIndex.Value

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return
        If Not Object.ReferenceEquals(entry, _scanPositionEntry) Then Return

        Dim scanY As Double? = _scanView.GetScanPositionY()
        If scanY.HasValue Then entry.ScanY = scanY.Value
    End Sub

    ' Associates the loaded image with the current batch.
    ' A pending startup position is applied when a previously unavailable scan opens.
    Private Sub ScanView_ScanPositionReady(sender As Object, e As EventArgs)
        If Not Object.ReferenceEquals(sender, _scanView) Then Return
        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return

        _scanPositionEntry = _transcriptionForm.PositionEntry

        If _pendingScanY.HasValue AndAlso Object.ReferenceEquals(_pendingScanEntry, _scanPositionEntry) Then
            _scanView.RestoreScanPositionY(_pendingScanY.Value)
            _pendingScanY = Nothing
            _pendingScanEntry = Nothing
        End If
    End Sub

    ' Shared by batch resume and workfile recovery.
    ' Select the grid row first because that selection can move the scan.
    Private Sub RestoreLastPosition()
        Dim entry As RecentFileEntry = _transcriptionForm.PositionEntry

        If entry Is Nothing Then
            _transcriptionForm.FocusGridRow(0)
            Return
        End If

        _transcriptionForm.RestorePositionRow(entry.GridRow.GetValueOrDefault(0))

        If _transcriptionForm.CurrentGridCell IsNot Nothing Then
            _previousGridRow = _transcriptionForm.CurrentGridCell.RowIndex + 1
        End If

        If Not entry.ScanY.HasValue Then Return

        If _scanView IsNot Nothing AndAlso Not _scanView.IsDisposed AndAlso _scanView.HasScan Then
            _scanView.RestoreScanPositionY(entry.ScanY.Value)
        Else
            _pendingScanEntry = entry
            _pendingScanY = entry.ScanY.Value
        End If
    End Sub
    Public Sub Start(Optional startupFile As String = "")

        DebugLog.WriteAlways("[STARTUP] CommandController.Start entered. startupFile='" & startupFile & "'")

        RemoveHandler ProjectValuesStore.SavingSettings, AddressOf CaptureCurrentPosition
        AddHandler ProjectValuesStore.SavingSettings, AddressOf CaptureCurrentPosition

        If Not String.IsNullOrWhiteSpace(startupFile) Then
            DebugLog.WriteAlways($"[STARTUP] Opening command-line file: '{startupFile}'")
            ShowTranscriptionForms(startupFile)
            Return
        End If

        ' A surviving workfile means the previous session did not close normally.
        If File.Exists(AppPaths.WorkFilePath) Then

            Dim answer As DialogResult = MessageBox.Show(
        "WinBMD2 found a workfile from a previous session." & Environment.NewLine & Environment.NewLine &
        "Do you want to recover it?",
        "Recover Previous Work",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question)

            If answer = DialogResult.Yes Then
                ShowTranscriptionForms(recoveringWorkfile:=True, restorePosition:=True)
                Return
            End If

        End If

        ' If the previous batch still exists, offer to continue working on it.
        If Not String.IsNullOrWhiteSpace(ProjectValues.BatchName) Then

            Dim filePath As String = ProjectValues.GetBatchFilePath(ProjectValues.BatchName)

            If File.Exists(filePath) Then

                Dim answer As DialogResult = MessageBox.Show(
                    "Do you want to resume the previous batch?" & Environment.NewLine & Environment.NewLine &
                    ProjectValues.BatchName,
                    "Resume Previous Batch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question)

                If answer = DialogResult.Yes Then
                    ShowTranscriptionForms(filePath, restorePosition:=True)
                    Return
                End If

            End If

        End If

        ShowHeaderForm()

    End Sub
    Public ReadOnly Property MaximumRulerHeight As Integer Implements ICommandExecutor.MaximumRulerHeight
        Get
            If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return 30
            Return _scanView.MaximumRulerHeight
        End Get
    End Property
    Public Sub NudgeScan(deltaX As Single, deltaY As Single) Implements ICommandExecutor.NudgeScan

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.NudgeViewer(deltaX, deltaY)

    End Sub
    Public Sub MoveScanOneRow(direction As Integer) Implements ICommandExecutor.MoveScanOneRow

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.MoveScanByRows(direction)

    End Sub
    Public Sub MoveScanByRows(deltaRows As Single) Implements ICommandExecutor.MoveScanByRows

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return

        _scanView.MoveScanByRows(deltaRows)

    End Sub
    Public Sub MoveScanForInsertedGridRow(rowNumber As Integer) Implements ICommandExecutor.MoveScanForInsertedGridRow

        DebugLog.Write($"[SCAN] Inserted grid row {rowNumber}. ScanView available: {_scanView IsNot Nothing AndAlso Not _scanView.IsDisposed}")

        _previousGridRow = rowNumber

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return

        _scanView.MoveScanByRows(-1.0F)

    End Sub
    Public Sub ResetPreviousGridRow() Implements ICommandExecutor.ResetPreviousGridRow

        _previousGridRow = 0

    End Sub
    Public Sub ScanRowInserted(rowNumber As Integer)

        _previousGridRow = rowNumber

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return

        _scanView.MoveScanByRows(-1.0F)

    End Sub
    Public Sub MoveScanToRow1() Implements ICommandExecutor.MoveScanToRow1

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.MoveScanToRow1()

    End Sub
    Public Sub RestoreGridFocus() Implements ICommandExecutor.RestoreGridFocus

        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return
        If _transcriptionForm.CurrentGridCell Is Nothing Then Return

        _transcriptionForm.FocusGridRow(_transcriptionForm.CurrentGridCell.RowIndex)

    End Sub
    Public Sub ApplyScanViewColourScheme() Implements ICommandExecutor.ApplyScanViewColourScheme

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.ApplyColourScheme()

    End Sub
    Public Sub RefreshVerifyFont() Implements ICommandExecutor.RefreshVerifyFont

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.RefreshVerifyFont()

    End Sub
    Private Sub ShowHeaderForm()

        Dim headerForm As New HeaderForm()

        AddHandler headerForm.FormClosed, AddressOf HeaderForm_FormClosed

        MainForm = headerForm
        headerForm.Show()

    End Sub

    Private Async Sub ShowTranscriptionForms(Optional filePath As String = "", Optional recoveringWorkfile As Boolean = False, Optional restorePosition As Boolean = False)

        _restoringPosition = True
        _scanPositionEntry = Nothing
        _pendingScanEntry = Nothing
        _pendingScanY = Nothing
        _previousGridRow = 0

        _transcriptionForm = New TranscriptionForm(Me)
        _scanView = New ScanView(Me)

        AddHandler _transcriptionForm.Activated, AddressOf WorkingForm_Activated
        AddHandler _scanView.Activated, AddressOf WorkingForm_Activated
        AddHandler _transcriptionForm.FormClosed, AddressOf TranscriptionForm_FormClosed
        AddHandler _transcriptionForm.Resize, AddressOf TranscriptionForm_Resize
        AddHandler _transcriptionForm.CurrentGridRowChanged, AddressOf TranscriptionForm_CurrentGridRowChanged
        AddHandler _transcriptionForm.VerifyValuesChanged, AddressOf TranscriptionForm_VerifyValuesChanged
        AddHandler _scanView.RulerSetupFinished, AddressOf ScanView_RulerSetupFinished
        AddHandler _scanView.VerifyArrangeModeChanged, AddressOf ScanView_VerifyArrangeModeChanged
        AddHandler _scanView.Resize, AddressOf ScanView_Resize
        AddHandler _scanView.VerifyFieldFocused, AddressOf ScanView_VerifyFieldFocused
        AddHandler _scanView.ScanPositionReady, AddressOf ScanView_ScanPositionReady

        MainForm = _transcriptionForm

        _transcriptionForm.Show()

        If recoveringWorkfile Then

            If Not _transcriptionForm.RecoverWorkfile() Then

                MessageBox.Show(
                    "The previous workfile could not be recovered.",
                    "Recover Previous Work",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

                ExitThread()
                Return

            End If

        End If

        If Not String.IsNullOrWhiteSpace(filePath) Then

            If Not _transcriptionForm.LoadBatchFile(filePath) Then

                MessageBox.Show(
                    "The selected batch could not be loaded.",
                    "Open Batch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

                ExitThread()
                Return

            End If

        ElseIf Not recoveringWorkfile Then

            _transcriptionForm.CreateWorkfile()

        End If

        If ProjectValues.AutoShowScan Then
            _scanView.Show()
            Await _scanView.FindScanAsync()
        End If

        If _transcriptionForm.IsDisposed OrElse _scanView.IsDisposed Then Return

        Try
            If restorePosition Then
                RestoreLastPosition()
            Else
                _transcriptionForm.FocusGridRow(0)
            End If
        Finally
            _restoringPosition = False
        End Try

    End Sub
    ' Brings the companion window directly behind the activated window.
    ' The activated window retains keyboard focus.
    Private Sub WorkingForm_Activated(sender As Object, e As EventArgs)

        If _bringingFormsForward Then Return

        Dim activeWindow As Form = TryCast(sender, Form)
        Dim otherWindow As Form

        If activeWindow Is Nothing Then Return

        If activeWindow Is _transcriptionForm Then
            otherWindow = _scanView
        ElseIf activeWindow Is _scanView Then
            otherWindow = _transcriptionForm
        Else
            Return
        End If

        If activeWindow.IsDisposed OrElse activeWindow.Disposing Then Return
        If otherWindow Is Nothing OrElse otherWindow.IsDisposed OrElse otherWindow.Disposing Then Return

        If Not activeWindow.Visible OrElse Not otherWindow.Visible Then Return
        If Not activeWindow.Enabled OrElse Not otherWindow.Enabled Then Return
        If activeWindow.WindowState = FormWindowState.Minimized OrElse otherWindow.WindowState = FormWindowState.Minimized Then Return
        If Not activeWindow.IsHandleCreated OrElse Not otherWindow.IsHandleCreated Then Return

        _bringingFormsForward = True

        Try

            Dim flags As UInteger = SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE Or SWP_NOOWNERZORDER

            If Not SetWindowPos(otherWindow.Handle, activeWindow.Handle, 0, 0, 0, 0, flags) Then
                DebugLog.Write($"[WINDOWS] Could not bring companion window forward. Windows error={Marshal.GetLastWin32Error()}")
            End If

        Finally

            _bringingFormsForward = False

        End Try

    End Sub
    Public Sub ToggleVerify() Implements ICommandExecutor.ToggleVerify

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.ToggleVerify()

    End Sub
    Private Sub ScanView_VerifyFieldFocused(field As GridField)

        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return

        _transcriptionForm.HighlightVerifyField(field)

    End Sub
    Public Async Function ToggleScanViewAsync() As Task Implements ICommandExecutor.ToggleScanViewAsync

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        If _scanView.Visible AndAlso _scanView.HasScan Then
            _scanView.Hide()
            Return
        End If

        CaptureCurrentPosition()
        _scanPositionEntry = Nothing

        _scanView.ClearScan()
        _scanView.Show()

        Await _scanView.FindScanAsync()

    End Function

    Public Sub SetVerifyVisible(visible As Boolean) Implements ICommandExecutor.SetVerifyVisible

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.SetVerifyVisible(visible)

        If Not visible OrElse
       _transcriptionForm Is Nothing OrElse
       _transcriptionForm.IsDisposed Then

            Return

        End If

        Dim values As Dictionary(Of GridField, String) =
        _transcriptionForm.StartVerify()

        If values Is Nothing Then

            If _transcriptionForm.CurrentVerifyRowHasError Then
                MessageBox.Show(_transcriptionForm, "Verify cannot continue until the error on this row is corrected.", "Verify", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                SetVerifyVisible(False)
            End If

            Return

        End If

        _transcriptionForm.EndGridEdit()
        _scanView.LoadVerifyValues(values, _transcriptionForm.GetPreviousVerifySurname())

        _scanView.BeginInvoke(Sub() _scanView.FocusFirstVerifyBox())

    End Sub
    Public Sub CompleteVerifyRow() Implements ICommandExecutor.CompleteVerifyRow

        If _transcriptionForm Is Nothing OrElse _scanView Is Nothing Then
            Return
        End If

        ' Commit any grid correction before verifying the current verification row.
        _transcriptionForm.EndGridEdit()

        Dim nextRow As Integer = _transcriptionForm.CompleteCurrentVerify()

        If nextRow = -2 Then

            MessageBox.Show(_transcriptionForm, "Verify cannot continue until the error on this row is corrected.", "Verify", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            SetVerifyVisible(False)
            Return

        End If

        If nextRow < 0 Then
            Return
        End If

        _scanView.LoadVerifyValues(_transcriptionForm.GetCurrentVerifyValues(), _transcriptionForm.GetPreviousVerifySurname())
        _scanView.BeginInvoke(Sub() _scanView.FocusFirstVerifyBox())

    End Sub

    Private Sub TranscriptionForm_CurrentGridRowChanged(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse
       _transcriptionForm.CurrentGridCell Is Nothing Then Return

        Dim rowNumber As Integer = _transcriptionForm.CurrentGridCell.RowIndex + 1

        DebugLog.WriteAlways($"[VERIFY POSITION] GridRow={rowNumber}, PreviousRow={_previousGridRow}, VerifyVisible={_scanView IsNot Nothing AndAlso _scanView.VerifyVisible}")

        If _previousGridRow = 0 Then
            _previousGridRow = 1
        End If

        Dim deltaRows As Integer = _transcriptionForm.CountScanRowsBetween(_previousGridRow - 1, rowNumber - 1)

        _previousGridRow = rowNumber

        If deltaRows = 0 Then Return
        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return
        If Not _scanView.RulerVisible Then Return

        _scanView.MoveScanByRows(deltaRows)

    End Sub
    Private Sub TranscriptionForm_VerifyValuesChanged(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return
        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return
        If Not _scanView.VerifyVisible Then Return

        ' Refresh the fixed verification row without moving the scan, grid or focus.
        ' Reload the previous surname too, because a correction may change the hyphens.
        Dim values As Dictionary(Of GridField, String) = _transcriptionForm.GetCurrentVerifyValues()

        If values Is Nothing Then Return

        _scanView.LoadVerifyValues(values, _transcriptionForm.GetPreviousVerifySurname())

    End Sub
    Private Sub ScanView_RulerSetupFinished(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse
       _transcriptionForm.CurrentGridCell Is Nothing OrElse
       _scanView Is Nothing Then Return

        Dim rowIndex As Integer = _transcriptionForm.CurrentGridCell.RowIndex
        Dim rowNumber As Integer = rowIndex + 1

        _scanView.MoveScanToRow1()

        If rowNumber > 1 Then
            _scanView.MoveScanByRows(rowNumber - 1)
        End If

        _transcriptionForm.FocusGridRow(rowIndex)
        _previousGridRow = rowNumber

        DebugLog.Write($"[RULER] Setup finished. Scan aligned with grid row {rowNumber}.")

    End Sub
    Private Sub ScanView_VerifyArrangeModeChanged(active As Boolean)

        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return

        _transcriptionForm.Enabled = Not active

        If Not active Then
            _transcriptionForm.Activate()
        End If

    End Sub
    Private Sub ScanView_Resize(sender As Object, e As EventArgs)

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return
        If _transcriptionForm Is Nothing OrElse _transcriptionForm.IsDisposed Then Return

        If _scanView.WindowState = FormWindowState.Minimized Then
            _transcriptionForm.HidePickList()
        End If

    End Sub
    Private Sub HeaderForm_FormClosed(sender As Object, e As FormClosedEventArgs)

        Dim headerForm As HeaderForm = DirectCast(sender, HeaderForm)

        If headerForm.DialogResult <> DialogResult.OK Then

            DebugLog.WriteAlways("[STARTUP] Header form cancelled.")

            ExitThread()
            Return

        End If

        Dim filePath As String = ""

        If headerForm.Tag IsNot Nothing Then
            filePath = headerForm.Tag.ToString()
        End If

        If Not String.IsNullOrWhiteSpace(filePath) Then

            DebugLog.WriteAlways($"[STARTUP] Existing batch selected: '{filePath}'")

            ShowTranscriptionForms(filePath)
            Return

        End If

        Dim currentFilePath As String = ""

        If Not String.IsNullOrWhiteSpace(ProjectValues.BatchName) Then
            currentFilePath = Path.Combine(AppPaths.SaveFolder, ProjectValues.BatchName)
        End If

        If Not String.IsNullOrWhiteSpace(currentFilePath) AndAlso File.Exists(currentFilePath) Then

            DebugLog.WriteAlways($"[STARTUP] Continuing current batch: '{currentFilePath}'")

            ShowTranscriptionForms(currentFilePath)
            Return

        End If

        If Not DistrictData.LoadAliveDistricts() Then
            DebugLog.WriteAlways("[STARTUP] New batch cancelled because the district information could not be loaded.")
            ExitThread()
            Return
        End If

        LogHeaderDetails()
        LogVisibleGridFields()

        ShowTranscriptionForms()

    End Sub
    Private Shared Sub LogHeaderDetails()

        DebugLog.WriteAlways("========== HEADER ACCEPTED ==========")

        DebugLog.WriteAlways($"Batch Type    : '{ProjectValues.BatchType}'")
        DebugLog.WriteAlways($"Year          : {ProjectValues.Year}")
        DebugLog.WriteAlways($"Quarter       : {ProjectValues.Quarter}")
        DebugLog.WriteAlways($"Month         : {ProjectValues.Month}")

        DebugLog.WriteAlways($"Page          : {ProjectValues.Page}")
        DebugLog.WriteAlways($"Page Source   : {ProjectValues.PageSource}")
        DebugLog.WriteAlways($"Page Letter   : '{ProjectValues.PageLetter}'")
        DebugLog.WriteAlways($"Page Suffix   : '{ProjectValues.PageSuffix}'")

        DebugLog.WriteAlways($"VNF           : '{ProjectValues.VNF}'")
        DebugLog.WriteAlways($"Source Ref    : '{ProjectValues.SourceRef}'")

        DebugLog.WriteAlways($"Creator       : '{ProjectValues.Creator}'")
        DebugLog.WriteAlways($"Creator Email : '{ProjectValues.CreatorEmail}'")
        DebugLog.WriteAlways($"Syndicate     : '{ProjectValues.Syndicate}'")

        DebugLog.WriteAlways($"User Name     : '{ProjectValues.UserName}'")
        DebugLog.WriteAlways($"Comments      : '{ProjectValues.Comments}'")

        DebugLog.WriteAlways("=====================================")

    End Sub
    Private Shared Sub LogVisibleGridFields()

        Dim fields() As GridField = GridLayout.GetVisibleFields()

        DebugLog.WriteAlways("========== VISIBLE GRID FIELDS ==========")

        For index As Integer = 0 To fields.Length - 1

            Dim field As GridField = fields(index)
            Dim fieldInformation As FieldMeta = FieldMetaData.Meta(field)

            DebugLog.WriteAlways(
            $"Column {index}: " &
            $"Field={field}, " &
            $"Header='{fieldInformation.Header}', " &
            $"PreferredWidth={fieldInformation.PreferredWidth}, " &
            $"MinWidth={fieldInformation.MinWidth}, " &
            $"Align={fieldInformation.Align}, " &
            $"Picklist={fieldInformation.UsesPicklist}, " &
            $"VolumeField={fieldInformation.IsVolumeField}, " &
            $"DataColumn={fieldInformation.IsDataColumn}")

        Next

        DebugLog.WriteAlways(
        $"Last data column index: {FieldMetaData.GetLastDataColumn()}")

        DebugLog.WriteAlways("=========================================")

    End Sub
    Public Sub StartNewFile() Implements ICommandExecutor.StartNewFile

        _startingNewFile = True
        _transcriptionForm.Close()

        If Not _transcriptionForm.IsDisposed Then
            _startingNewFile = False
        End If

    End Sub
 
    Public Sub UpdateRulerHeight() Implements ICommandExecutor.UpdateRulerHeight

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return

        _scanView.UpdateRulerHeight()

    End Sub
    Private Sub TranscriptionForm_FormClosed(sender As Object, e As FormClosedEventArgs)

        If _scanView IsNot Nothing AndAlso Not _scanView.IsDisposed Then
            _scanView.Close()
        End If

        If _startingNewFile Then
            _startingNewFile = False
            If ProjectValues.Page > 0 Then
                ProjectValues.Page += 1
            End If
            ShowHeaderForm()
            Return
        End If

        ExitThread()

    End Sub
    Private Sub TranscriptionForm_Resize(sender As Object, e As EventArgs)

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return
        If Not _scanView.Visible Then Return

        If _transcriptionForm.WindowState = FormWindowState.Minimized Then

            _scanView.WindowState = FormWindowState.Minimized

        ElseIf _scanView.WindowState = FormWindowState.Minimized Then

            _scanView.WindowState = FormWindowState.Normal

        End If

    End Sub
    Public Async Sub Execute(command As AppCommand) Implements ICommandExecutor.Execute

        Select Case command

            Case AppCommand.OpenFile

                If _transcriptionForm Is Nothing Then
                    Return
                End If

                If Not _transcriptionForm.ConfirmSaveChangesIfNeeded() Then
                    Return
                End If

                Using dialog As New OpenFileDialog()

                    dialog.Filter = "BMD Files (*.BMD)|*.BMD|All Files (*.*)|*.*"
                    dialog.Title = "Open Batch"
                    dialog.InitialDirectory = AppPaths.SaveFolder

                    If dialog.ShowDialog(_transcriptionForm) <> DialogResult.OK Then
                        Return
                    End If

                    _previousGridRow = 0

                    If Not _transcriptionForm.LoadBatchFile(dialog.FileName) Then

                        MessageBox.Show(
                            "The selected batch could not be loaded.",
                            "Open Batch",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

                        Return

                    End If

                    Await RefreshScanAsync()

                End Using

            Case AppCommand.SaveFile

                If _transcriptionForm Is Nothing Then
                    Return
                End If

                If String.IsNullOrWhiteSpace(ProjectValues.BatchName) Then

                    MessageBox.Show(
                        "The batch name is not set.", "Save Batch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning)

                    Return

                End If

                Dim filePath As String = Path.Combine(AppPaths.SaveFolder, ProjectValues.BatchName)

                If _transcriptionForm.PositionEntry IsNot Nothing AndAlso String.Equals(Path.GetFileName(_transcriptionForm.PositionEntry.FilePath), ProjectValues.BatchName, StringComparison.OrdinalIgnoreCase) Then
                    filePath = _transcriptionForm.PositionEntry.FilePath
                End If

                If _transcriptionForm.SaveCurrentBatch(filePath) Then

                    RememberSavedBatch(filePath)

                    DebugLog.WriteAlways($"[SAVE] File saved successfully: '{filePath}'")

                    MessageBox.Show(
                        $"{ProjectValues.BatchName} has been saved.",
                        "Save Batch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                Else

                    MessageBox.Show(
                        $"{ProjectValues.BatchName} could not be saved.",
                        "Save Batch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)

                End If


            Case AppCommand.SaveFileAs

                If _transcriptionForm Is Nothing Then Return

                Using dialog As New SaveFileDialog()

                    dialog.Title = "Save Batch As"
                    dialog.Filter = "BMD files (*.BMD)|*.BMD|All files (*.*)|*.*"
                    dialog.DefaultExt = "BMD"
                    dialog.AddExtension = True
                    dialog.FileName = ProjectValues.BatchName

                    If dialog.ShowDialog(_transcriptionForm) <> DialogResult.OK Then Return

                    Dim originalBatchName As String = ProjectValues.BatchName
                    Dim saved As Boolean = False

                    Try
                        ' The file header must contain the destination filename.
                        ProjectValues.BatchName = Path.GetFileName(dialog.FileName)
                        saved = _transcriptionForm.SaveCurrentBatch(dialog.FileName)
                    Finally
                        ' A failed save must not rename the current batch.
                        If Not saved Then ProjectValues.BatchName = originalBatchName
                    End Try

                    If saved Then

                        RememberSavedBatch(dialog.FileName)
                        _transcriptionForm.UpdateStatusPosition()

                        MessageBox.Show($"{ProjectValues.BatchName} has been saved.", "Save Batch As", MessageBoxButtons.OK, MessageBoxIcon.Information)

                    Else

                        MessageBox.Show("The batch could not be saved.", "Save Batch As", MessageBoxButtons.OK, MessageBoxIcon.Error)

                    End If

                End Using

            Case AppCommand.ExitApplication
                ExitThread()

            Case AppCommand.ZoomIn
                ' _scanView.ZoomIn()

            Case AppCommand.ZoomOut
                ' _scanView.ZoomOut()

            Case AppCommand.FitScanWidth
                ' _scanView.FitWidth()

            Case AppCommand.ResetScanView
                ' _scanView.ResetView()

            Case AppCommand.RotateScanLeft
                ' _scanView.RotateLeft()

            Case AppCommand.RotateScanRight
                ' _scanView.RotateRight()

            Case AppCommand.ToggleMagnifier
                ' _scanView.ToggleMagnifier()

            Case AppCommand.ToggleRuler
                ' _scanView.ToggleRuler()

        End Select

    End Sub
    ' Associates a successfully saved batch with its MRU entry.
    ' Save As keeps the original entry and gives the destination its own position.
    Private Sub RememberSavedBatch(filePath As String)
        CaptureCurrentPosition()

        Dim previousEntry As RecentFileEntry = _transcriptionForm.PositionEntry
        Dim scanY As Double? = Nothing

        If previousEntry IsNot Nothing Then scanY = previousEntry.ScanY

        ' Only read the displayed image if it belongs to the batch being saved.
        Dim scanBelongsToBatch As Boolean = Object.ReferenceEquals(previousEntry, _scanPositionEntry)

        If scanBelongsToBatch AndAlso _scanView IsNot Nothing AndAlso Not _scanView.IsDisposed Then
            Dim displayedY As Double? = _scanView.GetScanPositionY()
            If displayedY.HasValue Then scanY = displayedY.Value
        End If

        Dim entry As RecentFileEntry = ProjectValues.RecentFiles.Find(Function(item) String.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase))

        If entry Is Nothing Then
            entry = New RecentFileEntry With {.FilePath = filePath}
        End If

        entry.FilePath = filePath
        entry.GridRow = _transcriptionForm.CurrentPositionRow
        entry.ScanY = scanY

        ProjectValues.RecentFiles.RemoveAll(Function(item) String.Equals(item.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
        ProjectValues.RecentFiles.Insert(0, entry)

        While ProjectValues.RecentFiles.Count > 10
            ProjectValues.RecentFiles.RemoveAt(ProjectValues.RecentFiles.Count - 1)
        End While

        _transcriptionForm.SelectPositionEntry(filePath)

        If scanBelongsToBatch Then _scanPositionEntry = entry

        If _pendingScanY.HasValue AndAlso Object.ReferenceEquals(_pendingScanEntry, previousEntry) Then
            _pendingScanEntry = entry
        End If

        ProjectValuesStore.Save()
    End Sub
    Public Async Function RefreshScanAsync() As Task Implements ICommandExecutor.RefreshScanAsync

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then Return

        ' The old scan must not be recorded against a newly loaded batch.
        _scanPositionEntry = Nothing
        _pendingScanEntry = Nothing
        _pendingScanY = Nothing

        _scanView.ClearScan()

        If Not ProjectValues.AutoShowScan Then
            _scanView.Hide()
            Return
        End If

        _scanView.Show()
        Await _scanView.FindScanAsync()

    End Function
End Class