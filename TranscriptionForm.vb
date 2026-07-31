Imports WinBMD2.My.Resources

Public Class TranscriptionForm

    Private ReadOnly _commandExecutor As ICommandExecutor

    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()

        _commandExecutor = commandExecutor

        filePanel.Expanded = ProjectValues.FilePanelExpanded

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2"

        DebugLog.WriteAlways("======= TRANSCRIPTION FORM OPENED =======")
        DebugLog.WriteAlways($"Batch Type  : '{ProjectValues.BatchType}'")
        DebugLog.WriteAlways($"Year        : {ProjectValues.Year}")
        DebugLog.WriteAlways($"Quarter     : {ProjectValues.Quarter}")
        DebugLog.WriteAlways($"Page        : {ProjectValues.Page}")
        DebugLog.WriteAlways($"Page Letter : '{ProjectValues.PageLetter}'")
        DebugLog.WriteAlways($"Source Ref  : '{ProjectValues.SourceRef}'")
        DebugLog.WriteAlways("=========================================")
    End Sub

    Private Sub filePanel_ExpandedChanged(sender As Object, e As EventArgs) Handles filePanel.ExpandedChanged
        ProjectValues.FilePanelExpanded = filePanel.Expanded
        ProjectValuesStore.Save()
    End Sub

    Private Sub filePanel_Paint(sender As Object, e As PaintEventArgs) Handles filePanel.Paint

    End Sub
End Class