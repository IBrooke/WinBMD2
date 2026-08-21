Imports System.IO
Imports WinBMD2.My.Resources

Public Class ScanView

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private ReadOnly _viewer As New ScanViewerControl()
    Private ReadOnly _rulerController As RulerController
    Public Event RulerSetupStarted As EventHandler
    Public Event RulerSetupFinished As EventHandler
    Private _rulerInstructionForm As RulerInstructionForm
    Private ReadOnly _verifyBar As New VerifyBar()
    Private _rulerWasVisibleBeforeVerify As Boolean
    Private _rulerPanYBeforeVerify As Single

    Public Sub LoadVerifyValues(values As Dictionary(Of GridField, String))

        _verifyBar.LoadValues(values)

    End Sub

    Public Function GetVerifyValues() As Dictionary(Of GridField, String)

        Return _verifyBar.GetValues()

    End Function

    Public Sub FocusFirstVerifyBox()

        _verifyBar.FocusFirstBox()

    End Sub
    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()

        lblZoomValue.BackColor = Color.Transparent
        _viewer.Dock = DockStyle.Fill
        _rulerController = New RulerController(_viewer)
        Controls.Add(_viewer)
        _viewer.BringToFront()
        scanTopPanel.BringToFront()
        RestoreFormBounds()
        _commandExecutor = commandExecutor
        ApplyColourScheme()
        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2 Scan"
        Controls.Add(_verifyBar)
        _verifyBar.BringToFront()
        AddHandler _verifyBar.VerifiedClicked, AddressOf VerifyBar_VerifiedClicked
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

        _rulerController.StartSetup()

        Dim key As String = GetScanViewKey()
        Dim settings As RulerData = Nothing

        If ProjectValues.RulerSettings.TryGetValue(key, settings) Then

            _viewer.SetImagePanX(settings.PanX)

            _viewer.PositionImageYAtScreenY(
            settings.Row1ImageY,
            _viewer.RulerScreenY)

        End If

        RaiseEvent RulerSetupStarted(Me, EventArgs.Empty)

        _viewer.Focus()

        ShowRulerInstruction(
        "Ruler Setup — Row 1",
        "Position the scan so the ruler is centred on transcription row 1.",
        "Drag the scan or use the arrow keys for fine adjustment. Press Enter when ready.",
        My.Resources.WinBMDResources.Row1)

    End Sub
    Private Function GetSavedRulerImageY(rowNumber As Integer) As Single

        If rowNumber <> 1 AndAlso rowNumber <> 10 Then
            Return Single.NaN
        End If

        Dim key As String = GetScanViewKey()
        Dim settings As RulerData = Nothing

        If Not ProjectValues.RulerSettings.TryGetValue(key, settings) Then
            Return Single.NaN
        End If

        If rowNumber = 1 Then
            Return settings.Row1ImageY
        End If

        Return settings.Row1ImageY + (9.0F * settings.RowStepImageY)

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

        If ProjectValues.AutoShowRuler Then
            AutoShowRuler()
        End If
        UpdateZoomDisplay()
    End Sub
    Public Sub ApplyColourScheme()

        ThemeManager.Apply(Me)
        ThemeManager.ApplyVerifyBar(_verifyBar)

        ' The scan toolbar is deliberately given the stronger theme shade.
        scanTopPanel.BackColor = UiColors.ThemeSoft

        ' Keep the actual scan canvas neutral.
        _viewer.BackColor = Color.DimGray

        ' Repaint so a visible ruler uses the current colour scheme.
        _viewer.Invalidate()

        If _rulerInstructionForm IsNot Nothing AndAlso
       Not _rulerInstructionForm.IsDisposed Then

            _rulerInstructionForm.ApplyColours()

        End If

        Invalidate(True)

    End Sub
    Private Sub btnZoomIn_Click(sender As Object, e As EventArgs) Handles btnZoomIn.Click
        _viewer.ZoomIn()
        UpdateZoomDisplay()
        SaveScanViewSettings()
    End Sub
    Private Sub UpdateZoomDisplay()
        lblZoomValue.Text = CInt(Math.Round(_viewer.Zoom * 100.0F)).ToString() & "%"
    End Sub
    Private Sub btnZoomOut_Click(sender As Object, e As EventArgs) Handles btnZoomOut.Click
        _viewer.ZoomOut()
        UpdateZoomDisplay()
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
                    Dim imageY As Single = GetSavedRulerImageY(10)

                    If Not Single.IsNaN(imageY) Then
                        _viewer.PositionImageYAtScreenY(
        imageY,
        _viewer.RulerScreenY)
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
   $"The ruler has been calibrated. Row spacing is {Math.Abs(_rulerController.RowStepImageY):0.##} image pixels.",
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
    Private Sub AutoShowRuler()

        Dim key As String = GetScanViewKey()
        Dim settings As RulerData = Nothing

        If Not ProjectValues.RulerSettings.TryGetValue(key, settings) Then

            DebugLog.Write(
            $"[RULER] Auto-show skipped. No saved settings for {key}.")

            Return

        End If

        _rulerController.LoadCalibration(
        settings.Row1ImageY,
        settings.RowStepImageY)

        _viewer.SetImagePanX(settings.PanX)

        _viewer.PositionImageYAtScreenY(
        settings.Row1ImageY,
        _viewer.RulerScreenY)

        _viewer.ShowRuler = True

        DebugLog.Write(
        $"[RULER] Auto-show. Key={key}, " &
        $"PanX={settings.PanX:0.###}, " &
        $"Row1ImageY={settings.Row1ImageY:0.###}, " &
        $"RowStepImageY={settings.RowStepImageY:0.###}")

    End Sub
    Public Sub ToggleVerify()

        _verifyBar.Visible = Not _verifyBar.Visible

    End Sub
    Public Sub SetVerifyVisible(visible As Boolean)

        DebugLog.WriteAlways($"[VERIFY] SetVerifyVisible called. Visible={visible}")

        If visible Then

            Dim fields() As GridField =
        GridLayout.GetVisibleFields().
        Where(Function(field) FieldMetaData.Meta(field).IsDataColumn).
        ToArray()

            _verifyBar.ConfigureFields(fields)

            _rulerWasVisibleBeforeVerify = _viewer.ShowRuler
            _rulerPanYBeforeVerify = _viewer.GetImagePanY()

            _viewer.ShowRuler = False

        Else

            _viewer.SetImagePanY(_rulerPanYBeforeVerify)
            _viewer.ShowRuler = _rulerWasVisibleBeforeVerify

        End If

        _verifyBar.Visible = visible

    End Sub
    Private Sub VerifyBar_VerifiedClicked(
    sender As Object,
    e As EventArgs)

        _commandExecutor.CompleteVerifyRow()

    End Sub
    Public Sub MoveScanToVerifyRow(rowNumber As Integer)

        If Not _verifyBar.Visible Then
            Return
        End If

        Dim key As String = GetScanViewKey()
        Dim settings As RulerData = Nothing

        If Not ProjectValues.RulerSettings.TryGetValue(key, settings) Then
            Return
        End If

        _viewer.ShowRuler = False

        Dim rowImageY As Single =
        settings.Row1ImageY +
        ((rowNumber - 1) * settings.RowStepImageY)

        Dim rowHeightScreen As Single =
        Math.Abs(settings.RowStepImageY) * _viewer.Zoom

        Dim targetScreenY As Single =
        _viewer.ClientSize.Height - rowHeightScreen

        _viewer.PositionImageYAtScreenY(
        rowImageY,
        targetScreenY)

    End Sub
    Public Sub MoveRulerToRow(rowNumber As Integer)

        If Not _viewer.ShowRuler Then
            Return
        End If

        _rulerController.MoveToRow(rowNumber)

    End Sub
    Public ReadOnly Property VerifyVisible As Boolean
        Get
            Return _verifyBar.Visible
        End Get
    End Property
    Private Sub SaveRulerSettings()

        If _rulerController.Stage <> RulerSetupStage.Complete Then
            Return
        End If

        Dim key As String = GetScanViewKey()

        ProjectValues.RulerSettings(key) =
            New RulerData With {
                .PanX = _rulerController.PanX,
                .Row1ImageY = _rulerController.Row1ImageY,
                .RowStepImageY = _rulerController.RowStepImageY
            }

        ProjectValuesStore.Save()

        DebugLog.Write(
            $"[RULER] Settings saved. Key={key}, " &
            $"Row1ImageY={_rulerController.Row1ImageY:0.###}, " &
            $"RowStepImageY={_rulerController.RowStepImageY:0.###}")

    End Sub
End Class