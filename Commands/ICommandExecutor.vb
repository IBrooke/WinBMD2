Public Interface ICommandExecutor

    Sub Execute(command As AppCommand)
    Sub ApplyScanViewColourScheme()
    Sub ToggleVerify()
    Sub SetVerifyVisible(visible As Boolean)
    Sub CompleteVerifyRow()
End Interface