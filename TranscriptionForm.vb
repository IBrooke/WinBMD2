Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _commandExecutor As ICommandExecutor

    Public Sub New(commandExecutor As ICommandExecutor)

        InitializeComponent()

        _commandExecutor = commandExecutor

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2"

        filePanel.Expanded = ProjectValues.FilePanelExpanded

        RestoreFormBounds()

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

        FormBoundsHelper.RestoreForm(
        Me,
        ProjectValues.TranscriptionFormLeft,
        ProjectValues.TranscriptionFormTop,
        ProjectValues.TranscriptionFormWidth,
        ProjectValues.TranscriptionFormHeight,
        ProjectValues.TranscriptionFormMaximized)

    End Sub

    Private Sub SaveFormBounds()

        Dim boundsToSave As Rectangle =
        FormBoundsHelper.GetBoundsToSave(Me)

        ProjectValues.TranscriptionFormLeft = boundsToSave.Left
        ProjectValues.TranscriptionFormTop = boundsToSave.Top
        ProjectValues.TranscriptionFormWidth = boundsToSave.Width
        ProjectValues.TranscriptionFormHeight = boundsToSave.Height

        ProjectValues.TranscriptionFormMaximized =
        FormBoundsHelper.ShouldRestoreMaximized(Me)

        ProjectValuesStore.Save()

        DebugLog.Write(
        "[FORM] Transcription bounds saved: " &
        "Left=" & boundsToSave.Left.ToString() &
        ", Top=" & boundsToSave.Top.ToString() &
        ", Width=" & boundsToSave.Width.ToString() &
        ", Height=" & boundsToSave.Height.ToString() &
        ", Maximized=" &
        ProjectValues.TranscriptionFormMaximized.ToString())

    End Sub

    Private Sub TranscriptionForm_FormClosing(
        sender As Object,
        e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub

    Private Sub filePanel_ExpandedChanged(
        sender As Object,
        e As EventArgs) Handles filePanel.ExpandedChanged

        ProjectValues.FilePanelExpanded = filePanel.Expanded
        ProjectValuesStore.Save()

    End Sub

End Class