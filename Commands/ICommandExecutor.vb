Public Interface ICommandExecutor

    Sub Execute(command As AppCommand)
    Sub ApplyScanViewColourScheme()
    Sub ToggleVerify()
    Sub SetVerifyVisible(visible As Boolean)
    Sub CompleteVerifyRow()
    Sub NudgeScan(deltaX As Single, deltaY As Single)
    Sub MoveScanOneRow(direction As Integer)
    Sub MoveScanByRows(deltaRows As Single)
    Sub MoveScanToRow1()
    Function ToggleScanViewAsync() As Task
    Function RefreshScanAsync() As Task
    Sub StartNewFile()
    Sub RestoreGridFocus()
End Interface