Imports WinBMD2.My.Resources

Public Class ScanView

    Private ReadOnly _commandExecutor As ICommandExecutor

    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()
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

    Private Sub ScanView_FormClosing(
        sender As Object,
        e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub
End Class