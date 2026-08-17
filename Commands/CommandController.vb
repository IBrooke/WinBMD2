Imports System.IO

Public NotInheritable Class CommandController
    Inherits ApplicationContext
    Implements ICommandExecutor

    Private _transcriptionForm As TranscriptionForm
    Private _scanView As ScanView

    Public Sub Start()

        If HeaderDetailsAreRequired() Then
            ShowHeaderForm()
        Else
            ShowTranscriptionForms()
        End If

    End Sub

    Private Function HeaderDetailsAreRequired() As Boolean

        ' Temporary until the real header validation is implemented.
        Return True

    End Function

    Private Sub ShowHeaderForm()

        Dim headerForm As New HeaderForm()

        AddHandler headerForm.FormClosed, AddressOf HeaderForm_FormClosed

        MainForm = headerForm
        headerForm.Show()

    End Sub

    Private Async Sub ShowTranscriptionForms()

        _transcriptionForm = New TranscriptionForm(Me)
        _scanView = New ScanView(Me)

        AddHandler _transcriptionForm.FormClosed, AddressOf TranscriptionForm_FormClosed
        AddHandler _transcriptionForm.CurrentGridRowChanged, AddressOf TranscriptionForm_CurrentGridRowChanged
        AddHandler _scanView.RulerSetupFinished, AddressOf ScanView_RulerSetupFinished

        MainForm = _transcriptionForm

        _transcriptionForm.Show()
        _scanView.Show()

        If ProjectValues.AutoShowScan Then
            Await _scanView.FindScanAsync()
        End If

    End Sub
    Private Sub TranscriptionForm_CurrentGridRowChanged(sender As Object, e As EventArgs)

        If _transcriptionForm Is Nothing OrElse
       _transcriptionForm.CurrentGridCell Is Nothing OrElse
       _scanView Is Nothing Then

            Return

        End If

        Dim rowNumber As Integer =
        _transcriptionForm.CurrentGridCell.RowIndex + 1

        _scanView.MoveRulerToRow(rowNumber)

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
    Private Sub HeaderForm_FormClosed(
    sender As Object,
    e As FormClosedEventArgs)

        Dim headerForm As HeaderForm =
        DirectCast(sender, HeaderForm)

        If headerForm.DialogResult <> DialogResult.OK Then

            DebugLog.WriteAlways(
            "[STARTUP] Header form cancelled.")

            ExitThread()
            Return

        End If

        Dim filePath As String = ""

        If headerForm.Tag IsNot Nothing Then
            filePath = headerForm.Tag.ToString()
        End If

        If Not String.IsNullOrWhiteSpace(filePath) Then

            DebugLog.WriteAlways(
            $"[STARTUP] Existing batch selected: '{filePath}'")

            ShowTranscriptionForms()

            If Not _transcriptionForm.LoadBatchFile(filePath) Then

                MessageBox.Show(
                "The selected batch could not be loaded.",
                "Open Batch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

                ExitThread()
                Return

            End If

            Return

        End If

        Dim currentFilePath As String = ""

        If Not String.IsNullOrWhiteSpace(ProjectValues.BatchName) Then
            currentFilePath = Path.Combine(AppPaths.OutputFolder, ProjectValues.BatchName)
        End If

        If Not String.IsNullOrWhiteSpace(currentFilePath) AndAlso File.Exists(currentFilePath) Then

            DebugLog.WriteAlways(
            $"[STARTUP] Continuing current batch: '{currentFilePath}'")

            ShowTranscriptionForms()

            If Not _transcriptionForm.LoadBatchFile(currentFilePath) Then

                MessageBox.Show(
                "The current batch could not be loaded.",
                "Open Batch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

                ExitThread()
                Return

            End If

            Return

        End If

        LogHeaderDetails()
        LogVisibleGridFields()

        ShowTranscriptionForms()

    End Sub
    Private Sub LogHeaderDetails()

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
    Private Sub LogVisibleGridFields()

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
                    dialog.InitialDirectory = AppPaths.OutputFolder

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
                    AppPaths.OutputFolder,
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