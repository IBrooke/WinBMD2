Friend Module Program

    <STAThread>
    Public Sub Main()

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        DebugLog.Clear()
        DebugLog.WriteAlways("WinBMD2 starting.")

        ProjectValuesStore.Initialise()

        If Not ForenameData.Load() Then
            DebugLog.WriteAlways("Startup cancelled because the forenames file could not be loaded.")
            Return
        End If

        If Not DistrictData.LoadBaseDistricts() Then
            DebugLog.WriteAlways("Startup cancelled because the districts file could not be loaded.")
            Return
        End If

        Dim controller As New CommandController()

        controller.Start()

        Application.Run(controller)

    End Sub

End Module
