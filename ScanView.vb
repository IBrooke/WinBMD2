Imports WinBMD2.My.Resources

Public Class ScanView

    Private ReadOnly _commandExecutor As ICommandExecutor

    Public Sub New(commandExecutor As ICommandExecutor)
        InitializeComponent()

        _commandExecutor = commandExecutor

        Icon = WinBMDResources.WinBMD2Icon
        Text = "ScanView"
    End Sub

End Class