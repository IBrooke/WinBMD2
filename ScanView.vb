Imports System.IO
Imports System.Windows.Forms.AxHost
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Window
Imports WinBMD2.My.Resources

Public Class ScanView

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private ReadOnly _viewer As New ScanViewerControl()
    Private ReadOnly _rulerController As RulerController
    Public Event RulerSetupStarted As EventHandler
    Public Event RulerSetupFinished As EventHandler
    Private _rulerInstructionForm As RulerInstructionForm

    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()

        _viewer.Dock = DockStyle.Fill
        _rulerController = New RulerController(_viewer)
        Controls.Add(_viewer)
        _viewer.BringToFront()
        scanTopPanel.BringToFront()
        RestoreFormBounds()
        _commandExecutor = commandExecutor

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2 Scan"
    End Sub
    Private Sub RestoreFormBounds()

        FormBoundsHelper.RestoreForm(
        Me,
        ProjectValues.ScanViewLeft,
        ProjectValues.ScanViewTop,
        ProjectValues.ScanViewWidth,
        ProjectValues.ScanViewHeight,
        ProjectValues.ScanViewMaximized)

    End Sub

    Private Sub SaveFormBounds()

        Dim boundsToSave As Rectangle =
        FormBoundsHelper.GetBoundsToSave(Me)

        ProjectValues.ScanViewLeft = boundsToSave.Left
        ProjectValues.ScanViewTop = boundsToSave.Top
        ProjectValues.ScanViewWidth = boundsToSave.Width
        ProjectValues.ScanViewHeight = boundsToSave.Height

        ProjectValues.ScanViewMaximized =
        FormBoundsHelper.ShouldRestoreMaximized(Me)

        ProjectValuesStore.Save()

        DebugLog.Write(
        "[FORM] ScanView bounds saved: " &
        "Left=" & boundsToSave.Left.ToString() &
        ", Top=" & boundsToSave.Top.ToString() &
        ", Width=" & boundsToSave.Width.ToString() &
        ", Height=" & boundsToSave.Height.ToString() &
        ", Maximized=" &
        ProjectValues.ScanViewMaximized.ToString())

    End Sub
    Private Sub btnOpenScan_Click(sender As Object, e As EventArgs) Handles btnOpenScan.Click

        Using dialog As New OpenFileDialog()

            dialog.Title = "Open Scan"
            dialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.tif;*.tiff)|*.jpg;*.jpeg;*.png;*.tif;*.tiff|All files (*.*)|*.*"
            dialog.InitialDirectory = AppPaths.DownloadedScansFolder

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            _viewer.LoadImage(dialog.FileName)
            Text = "WinBMD2 Scan - " & Path.GetFileName(dialog.FileName)
            RestoreScanViewSettings()
        End Using

    End Sub
    Private Function GetScanViewKey() As String

        Return ProjectValues.BatchType & "|" &
           ProjectValues.Year.ToString() & "|" &
           ProjectValues.Quarter.ToString()

    End Function
    Private Sub SaveScanViewSettings()

        If Not _viewer.HasImage Then
            Return
        End If

        Dim key As String = GetScanViewKey()

        ProjectValues.ScanViewSettings(key) =
        New ScanViewData With {
            .Zoom = _viewer.Zoom,
            .Rotation = _viewer.Rotation
        }

        ProjectValuesStore.Save()

    End Sub
    Private Sub RestoreScanViewSettings()

        Dim key As String = GetScanViewKey()
        Dim settings As ScanViewData = Nothing

        If ProjectValues.ScanViewSettings.TryGetValue(key, settings) Then

            _viewer.Zoom = settings.Zoom
            _viewer.Rotation = settings.Rotation

        Else

            _viewer.Zoom = 1.0F
            _viewer.Rotation = 0.0F

        End If

    End Sub
    Private Function GetSavedRulerPanY(rowNumber As Integer) As Single

        If rowNumber <> 1 AndAlso rowNumber <> 10 Then
            Return -1.0F
        End If

        Dim key As String = GetScanViewKey()
        Dim settings As RulerData = Nothing

        If Not ProjectValues.RulerSettings.TryGetValue(key, settings) Then
            Return Single.NaN
        End If

        If rowNumber = 1 Then
            Return settings.Row1PanY
        End If

        Return settings.Row1PanY + (9.0F * settings.RowStepPanY)

    End Function
    Private Sub ScanView_FormClosing(
        sender As Object,
        e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub

    Private Async Sub btnFindScan_Click(sender As Object, e As EventArgs) Handles btnFindScan.Click

        Await FindScanAsync()

    End Sub
    Public Async Function FindScanAsync() As Task

        btnFindScan.Enabled = False
        btnOpenScan.Enabled = False

        scanStatusLabel.Text = "Locating scan..."
        scanStatusLabel.Visible = True
        ScanProgressBar.Visible = True

        Try

            Dim locator As New ScanLocator()

            Dim result As ScanSearchResult =
                Await locator.LocateScanAsync()

            If result.Success AndAlso
               Not String.IsNullOrWhiteSpace(result.LocalPath) Then

                _viewer.LoadImage(result.LocalPath)
                Text = "WinBMD2 Scan - " & Path.GetFileName(result.LocalPath)
                RestoreScanViewSettings()
                scanStatusLabel.Text =
                    If(
                        result.Message,
                        "Scan loaded.")

                Return

            End If

            If result.AlternativeFound AndAlso
               Not String.IsNullOrWhiteSpace(result.AlternativeSourceRef) Then

                Dim useAlternative As DialogResult =
                    MessageBox.Show(
                        Me,
                        result.Message &
                        Environment.NewLine &
                        Environment.NewLine &
                        $"A matching scan was found under Source Reference '{result.AlternativeSourceRef}'." &
                        Environment.NewLine &
                        Environment.NewLine &
                        $"File: {result.AlternativeFileName}" &
                        Environment.NewLine &
                        Environment.NewLine &
                        "Do you want to use this alternative scan?",
                        "Alternative Scan Found",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question)

                If useAlternative = DialogResult.Yes Then

                    DebugLog.Write(
                        $"[SCAN SOURCE] User accepted alternative SourceRef '{result.AlternativeSourceRef}'.")

                    Dim alternativeResult As ScanSearchResult =
                        Await locator.DownloadAlternativeScanAsync(result)

                    If alternativeResult.Success AndAlso
                       Not String.IsNullOrWhiteSpace(alternativeResult.LocalPath) Then

                        _viewer.LoadImage(alternativeResult.LocalPath)
                        Text = "Scan: " & Path.GetFileName(result.LocalPath)
                        RestoreScanViewSettings()
                        scanStatusLabel.Text = "Scan loaded."

                        Return

                    End If

                    MessageBox.Show(
                        Me,
                        alternativeResult.Message,
                        "Find Scan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information)

                    Return

                End If

                Return

            End If

            MessageBox.Show(
                Me,
                result.Message,
                "Find Scan",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        Finally

            ScanProgressBar.Visible = False
            scanStatusLabel.Visible = False

            btnFindScan.Enabled = True
            btnOpenScan.Enabled = True

        End Try

    End Function
    Private Sub btnZoomIn_Click(sender As Object, e As EventArgs) Handles btnZoomIn.Click
        _viewer.ZoomIn()
        SaveScanViewSettings()
    End Sub

    Private Sub btnZoomOut_Click(sender As Object, e As EventArgs) Handles btnZoomOut.Click
        _viewer.ZoomOut()
        SaveScanViewSettings()
    End Sub

    Private Sub btnRotateLeft_Click(sender As Object, e As EventArgs) Handles btnRotateLeft.Click
        _viewer.RotateLeft()
        SaveScanViewSettings()
    End Sub

    Private Sub btnRotateRight_Click(sender As Object, e As EventArgs) Handles btnRotateRight.Click
        _viewer.RotateRight()
        SaveScanViewSettings()
    End Sub
    Private Sub btnRuler_Click(sender As Object, e As EventArgs) Handles btnRuler.Click

        If _viewer.ShowRuler Then

            _viewer.ShowRuler = False

            If _rulerController.Stage = RulerSetupStage.AwaitingRow1 OrElse
           _rulerController.Stage = RulerSetupStage.AwaitingRow10 Then

                _rulerController.CancelSetup()

            End If

            CloseRulerInstruction()
            Return

        End If

        _viewer.ShowRuler = True

        If _rulerController.Stage <> RulerSetupStage.Complete Then

            _rulerController.StartSetup()
            Dim key As String = GetScanViewKey()
            Dim settings As RulerData = Nothing

            If ProjectValues.RulerSettings.TryGetValue(key, settings) Then

                _viewer.SetImagePanX(settings.PanX)
                _viewer.SetImagePanY(settings.Row1PanY)

            End If
            RaiseEvent RulerSetupStarted(Me, EventArgs.Empty)
            _viewer.Focus()

            ShowRulerInstruction(
        "Ruler Setup — Row 1",
        "Position the scan so the ruler is centred on transcription row 1.",
        "Drag the scan or use the arrow keys for fine adjustment. Press Enter when ready.", My.Resources.WinBMDResources.Row1)

        End If

    End Sub
    Private Sub ShowRulerInstruction(
    stepText As String,
    message As String,
    hint As String,
    instructionImage As Image)

        If _rulerInstructionForm Is Nothing OrElse
       _rulerInstructionForm.IsDisposed Then

            _rulerInstructionForm = New RulerInstructionForm()

        End If

        _rulerInstructionForm.ShowStep(
    stepText,
    message,
    hint,
    instructionImage)

        _rulerInstructionForm.Location =
    New Point(
        Right - _rulerInstructionForm.Width - 30,
        Top + 70)

        If Not _rulerInstructionForm.Visible Then
            _rulerInstructionForm.Show(Me)
        End If

        _rulerInstructionForm.BringToFront()

        _viewer.Focus()

    End Sub
    Private Sub CloseRulerInstruction()

        If _rulerInstructionForm Is Nothing Then
            Return
        End If

        _rulerInstructionForm.Close()
        _rulerInstructionForm.Dispose()
        _rulerInstructionForm = Nothing

    End Sub
    Protected Overrides Function ProcessCmdKey(
    ByRef msg As Message,
    keyData As Keys) As Boolean

        Const nudge As Single = 2.0F

        Select Case keyData And Keys.KeyCode

            Case Keys.Left
                _viewer.NudgeImage(-nudge, 0)
                Return True

            Case Keys.Right
                _viewer.NudgeImage(nudge, 0)
                Return True

            Case Keys.Up
                _viewer.NudgeImage(0, -nudge)
                Return True

            Case Keys.Down
                _viewer.NudgeImage(0, nudge)
                Return True

            Case Keys.Enter

                If _rulerController.Stage = RulerSetupStage.AwaitingRow1 Then

                    _rulerController.ConfirmCurrentPosition()
                    Dim panY As Single = GetSavedRulerPanY(10)

                    If Not Single.IsNaN(panY) Then
                        _viewer.SetImagePanY(panY)
                    End If

                    ShowRulerInstruction(
        "Ruler Setup — Row 10",
        "Now position the scan so the ruler is centred on transcription row 10.",
        "Use the mouse or arrow keys to adjust the scan. Press Enter when ready.", My.Resources.WinBMDResources.Row10)

                    Return True

                End If

                If _rulerController.Stage = RulerSetupStage.AwaitingRow10 Then

                    _rulerController.ConfirmCurrentPosition()
                    SaveRulerSettings()
                    ShowRulerInstruction(
    "Ruler Setup Complete",
   $"The ruler has been calibrated. Row spacing is {Math.Abs(_rulerController.RowStepPanY):0.##} pixels.",
    "Press Enter to return to the transcription grid.", Nothing)

                    DebugLog.Write(
                    $"[RULER] Setup complete. ShowRuler={_viewer.ShowRuler}")

                    Return True

                End If

                If _rulerController.Stage = RulerSetupStage.Complete AndAlso
   _rulerInstructionForm IsNot Nothing AndAlso
   _rulerInstructionForm.Visible Then

                    CloseRulerInstruction()

                    RaiseEvent RulerSetupFinished(Me, EventArgs.Empty)

                    Return True

                End If

        End Select

        Return MyBase.ProcessCmdKey(msg, keyData)

    End Function
    Public Sub MoveRulerToRow(rowNumber As Integer)

        _rulerController.MoveToRow(rowNumber)

    End Sub
    Private Sub SaveRulerSettings()

        If _rulerController.Stage <> RulerSetupStage.Complete Then
            Return
        End If

        Dim key As String = GetScanViewKey()

        ProjectValues.RulerSettings(key) =
    New RulerData With {
        .PanX = _rulerController.PanX,
        .Row1PanY = _rulerController.Row1PanY,
        .RowStepPanY = _rulerController.RowStepPanY
    }

        ProjectValuesStore.Save()

        DebugLog.Write(
            $"[RULER] Settings saved. Key={key}, " &
            $"Row1PanY={_rulerController.Row1PanY:0.###}, " &
            $"RowStepPanY={_rulerController.RowStepPanY:0.###}")

    End Sub
End Class