Imports System.Text
Imports System.Threading

Friend Module Program

    <STAThread>
    Public Sub Main()

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf Application_ThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf CurrentDomain_UnhandledException
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf TaskScheduler_UnobservedTaskException

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

        DebugLog.Clear()
        DebugLog.WriteAlways("WinBMD2 starting.")

        Try

            ProjectValuesStore.Initialise()
            CapitalisationData.Load()

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

        Catch ex As Exception

            ShowUnhandledException("A program error occurred during startup.", ex)

        End Try

    End Sub
    Private Sub Application_ThreadException(sender As Object, e As ThreadExceptionEventArgs)

        ShowUnhandledException("An unexpected program error occurred.", e.Exception)

    End Sub

    Private Sub CurrentDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)

        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)

        If ex IsNot Nothing Then
            ShowUnhandledException("A serious program error occurred.", ex)
        Else
            DebugLog.WriteAlways("[FATAL] Unknown unhandled exception: " & e.ExceptionObject.ToString())
        End If

    End Sub

    Private Sub TaskScheduler_UnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)

        ShowUnhandledException("An unexpected background task error occurred.", e.Exception)
        e.SetObserved()

    End Sub

    Private Sub ShowUnhandledException(message As String, ex As Exception)

        Try
            DebugLog.WriteAlways("========== UNHANDLED EXCEPTION ==========")
            DebugLog.WriteAlways(ex.ToString())
            DebugLog.WriteAlways("=========================================")
        Catch
            ' There is nothing useful we can do if even the debug log fails.
        End Try

        Try
            MessageBox.Show(message & Environment.NewLine & Environment.NewLine & "Details have been written to the WinBMD2 debug log." & Environment.NewLine & Environment.NewLine & ex.Message, "WinBMD2", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch
            ' Avoid another exception while reporting the original exception.
        End Try

    End Sub
End Module
