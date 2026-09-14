Imports System.IO

Public NotInheritable Class CommandController
    Inherits ApplicationContext
    Implements ICommandExecutor

    Private _transcriptionForm As TranscriptionForm
    Private _scanView As ScanView

    Public Sub Start(Optional startupFile As String = "")

        DebugLog.WriteAlways("[STARTUP] CommandController.Start entered. startupFile='" & startupFile & "'")

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
                ShowTranscriptionForms(recoveringWorkfile:=True)
                Return
            End If

        End If

        ' If the previous batch still exists, offer to continue working on it.
        If Not String.IsNullOrWhiteSpace(ProjectValues.BatchName) Then

            Dim filePath As String = Path.Combine(AppPaths.SaveFolder, ProjectValues.BatchName)

            If File.Exists(filePath) Then

                Dim answer As DialogResult = MessageBox.Show(
            "Do you want to resume the previous batch?" & Environment.NewLine & Environment.NewLine &
            ProjectValues.BatchName,
            "Resume Previous Batch",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

                If answer = DialogResult.Yes Then
                    ShowTranscriptionForms(filePath)
                    Return
                End If

            End If

        End If

        ShowHeaderForm()

    End Sub
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
    Public Sub MoveScanToRow1() Implements ICommandExecutor.MoveScanToRow1

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.MoveRulerToRow(1)

    End Sub
    Public Sub ApplyScanViewColourScheme() Implements ICommandExecutor.ApplyScanViewColourScheme

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.ApplyColourScheme()

    End Sub
    Private Sub ShowHeaderForm()

        Dim headerForm As New HeaderForm()

        AddHandler headerForm.FormClosed, AddressOf HeaderForm_FormClosed

        MainForm = headerForm
        headerForm.Show()

    End Sub

    Private Async Sub ShowTranscriptionForms(Optional filePath As String = "", Optional recoveringWorkfile As Boolean = False)

        _transcriptionForm = New TranscriptionForm(Me)
        _scanView = New ScanView(Me)

        AddHandler _transcriptionForm.FormClosed, AddressOf TranscriptionForm_FormClosed
        AddHandler _transcriptionForm.CurrentGridRowChanged, AddressOf TranscriptionForm_CurrentGridRowChanged
        AddHandler _scanView.RulerSetupFinished, AddressOf ScanView_RulerSetupFinished

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

    End Sub
    Public Sub ToggleVerify() Implements ICommandExecutor.ToggleVerify

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        _scanView.ToggleVerify()

    End Sub
    Public Async Function ToggleScanViewAsync() As Task Implements ICommandExecutor.ToggleScanViewAsync

        If _scanView Is Nothing OrElse _scanView.IsDisposed Then
            Return
        End If

        If _scanView.Visible AndAlso _scanView.HasScan Then
            _scanView.Hide()
            Return
        End If

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

        If values Is Nothing Then
            Return
        End If

        _scanView.LoadVerifyValues(values)

        Dim rowIndex As Integer =
        _transcriptionForm.CurrentVerifyRowIndex

        If rowIndex >= 0 Then
            _scanView.MoveRulerToRow(rowIndex + 1)
        End If
        _scanView.MoveScanToVerifyRow(rowIndex + 1)
        _scanView.FocusFirstVerifyBox()

    End Sub
    Public Sub CompleteVerifyRow() Implements ICommandExecutor.CompleteVerifyRow

        If _transcriptionForm Is Nothing OrElse _scanView Is Nothing Then
            Return
        End If

        Dim values As Dictionary(Of GridField, String) = _scanView.GetVerifyValues()
        Dim nextRow As Integer = _transcriptionForm.CompleteCurrentVerify(values)

        If nextRow = -2 Then

            MessageBox.Show(_transcriptionForm, "Verify cannot continue until the error on this row is corrected.", "Verify", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            SetVerifyVisible(False)
            Return

        End If

        If nextRow < 0 Then
            Return
        End If

        _scanView.LoadVerifyValues(_transcriptionForm.GetCurrentVerifyValues())
        _scanView.MoveRulerToRow(nextRow + 1)
        _scanView.FocusFirstVerifyBox()

    End Sub

    Private Sub TranscriptionForm_CurrentGridRowChanged(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse
       _transcriptionForm.CurrentGridCell Is Nothing OrElse
       _scanView Is Nothing Then

            Return

        End If

        Dim rowNumber As Integer =
    _transcriptionForm.CurrentGridCell.RowIndex + 1

        If _scanView.VerifyVisible Then

            _scanView.MoveScanToVerifyRow(rowNumber)

        Else

            _scanView.MoveRulerToRow(rowNumber)

        End If

    End Sub
    Private Sub ScanView_RulerSetupFinished(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse
       _transcriptionForm.CurrentGridCell Is Nothing Then

            Return

        End If

        Dim rowIndex As Integer =
        _transcriptionForm.CurrentGridCell.RowIndex

        Dim rowNumber As Integer =
        rowIndex + 1

        _scanView.MoveRulerToRow(rowNumber)

        _transcriptionForm.FocusGridRow(rowIndex)

        DebugLog.Write(
        $"[RULER] Setup finished. Returned to grid row {rowNumber}.")

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
    Private Sub TranscriptionForm_FormClosed(sender As Object, e As FormClosedEventArgs)

        If _scanView IsNot Nothing AndAlso Not _scanView.IsDisposed Then
            _scanView.Close()
        End If

        ExitThread()

    End Sub

    Public Sub Execute(command As AppCommand) Implements ICommandExecutor.Execute

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

                    If Not _transcriptionForm.LoadBatchFile(dialog.FileName) Then

                        MessageBox.Show(
                            "The selected batch could not be loaded.",
                            "Open Batch",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

                        Return

                    End If

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

                Dim filePath As String =
                    Path.Combine(
                    AppPaths.SaveFolder,
                    ProjectValues.BatchName)

                If _transcriptionForm.SaveCurrentBatch(filePath) Then

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

                If _transcriptionForm Is Nothing Then
                    Return
                End If

                Using dialog As New SaveFileDialog()

                    dialog.Title = "Save Batch As"
                    dialog.Filter = "BMD files (*.BMD)|*.BMD|All files (*.*)|*.*"
                    dialog.DefaultExt = "BMD"
                    dialog.AddExtension = True
                    dialog.FileName = ProjectValues.BatchName

                    If dialog.ShowDialog(_transcriptionForm) <> DialogResult.OK Then
                        Return
                    End If

                    If _transcriptionForm.SaveCurrentBatch(dialog.FileName) Then

                        ProjectValues.BatchName = Path.GetFileName(dialog.FileName)
                        ProjectValuesStore.Save()

                        _transcriptionForm.UpdateStatusPosition()

                        MessageBox.Show(
                            $"{ProjectValues.BatchName} has been saved.",
                            "Save Batch As",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information)

                    Else

                        MessageBox.Show(
                            "The batch could not be saved.",
                            "Save Batch As",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)

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

End Class