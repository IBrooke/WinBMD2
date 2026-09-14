Public Interface ICommandExecutor

    Sub Execute(command As AppCommand)
    Sub ApplyScanViewColourScheme()
    Sub ToggleVerify()
    Sub SetVerifyVisible(visible As Boolean)
    Sub CompleteVerifyRow()
    Sub NudgeScan(deltaX As Single, deltaY As Single)
    Sub MoveScanOneRow(direction As Integer)
    Sub MoveScanToRow1()
    Function ToggleScanViewAsync() As Task
End Interface