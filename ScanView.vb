Imports System.Net.Mime.MediaTypeNames
Imports WinBMD2.My.Resources

Public Class ScanView

    Private ReadOnly _commandExecutor As ICommandExecutor
    Private ReadOnly _viewer As New ScanViewerControl()

    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()

        _viewer.Dock = DockStyle.Fill
        Controls.Add(_viewer)
        _viewer.BringToFront()
        scanTopPanel.BringToFront()
        RestoreFormBounds()
        _commandExecutor = commandExecutor

        Icon = WinBMDResources.WinBMD2Icon
        Text = "ScanView"
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

        End Using

    End Sub

    Private Sub ScanView_FormClosing(
        sender As Object,
        e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub

    Private Sub btnOpenScan_Click_1(sender As Object, e As EventArgs) Handles btnOpenScan.Click

    End Sub

    Private Async Sub btnFindScan_Click(sender As Object, e As EventArgs) Handles btnFindScan.Click

        btnFindScan.Enabled = False
        btnOpenScan.Enabled = False

        scanStatusLabel.Text = "Locating scan..."
        scanStatusLabel.Visible = True
        scanProgressBar.Visible = True

        Try

            Dim locator As New ScanLocator()

            Dim result As ScanSearchResult =
            Await locator.LocateScanAsync()

            If result.Success AndAlso
           Not String.IsNullOrWhiteSpace(result.LocalPath) Then

                _viewer.LoadImage(result.LocalPath)

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

            scanProgressBar.Visible = False
            scanStatusLabel.Visible = False

            btnFindScan.Enabled = True
            btnOpenScan.Enabled = True

        End Try

    End Sub
End Class