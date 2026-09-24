Imports System.IO
Imports System.Text
Imports System.Threading

Friend Module Program
    Public AbnormalShutdown As Boolean = False
    <STAThread>
    Public Sub Main(args() As String)

        Dim createdNew As Boolean

        Using singleInstanceMutex As New Mutex(initiallyOwned:=True, name:="Local\WinBMD2_SingleInstance", createdNew:=createdNew)

            If Not createdNew Then
                Return
            End If

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
            AddHandler Application.ThreadException, AddressOf Application_ThreadException
            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf CurrentDomain_UnhandledException
            AddHandler TaskScheduler.UnobservedTaskException, AddressOf TaskScheduler_UnobservedTaskException

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)

            DebugLog.Clear()
            DebugLog.WriteAlways("WinBMD2 starting.")

            Dim startupFile As String = Nothing

            If args.Length > 0 Then

                startupFile = args(0)

                DebugLog.WriteAlways("[STARTUP] Command-line file argument: " & startupFile)

                If Not File.Exists(startupFile) Then
                    DebugLog.WriteAlways("[STARTUP] Command-line file argument ignored: file does not exist")
                    startupFile = Nothing
                End If

            End If

            Try

                ProjectValuesStore.Initialise()
                CapitalisationData.Load()
                DebugLog.WriteAlways("======= CAPITALISATION =======")

                Dim capitalisationSettings As String = CapitalisationData.GetSettings(BatchType, Year)

                DebugLog.WriteAlways($"Settings string         : {capitalisationSettings}")

                For columnIndex As Integer = 0 To capitalisationSettings.Length - 1
                    DebugLog.WriteAlways($"Column {columnIndex,-2}               : {CapitalisationData.GetMode(BatchType, Year, columnIndex)}")
                Next

                DebugLog.WriteAlways("==============================")

                If Not ForenameData.Load() Then
                    DebugLog.WriteAlways("Startup cancelled because the forenames file could not be loaded.")
                    Return
                End If

                If Not DistrictData.LoadBaseDistricts() Then
                    DebugLog.WriteAlways("Startup cancelled because the districts file could not be loaded.")
                    Return
                End If

                Dim controller As New CommandController()

                controller.Start(startupFile)

                Application.Run(controller)

            Catch ex As Exception

                ShowUnhandledException("A program error occurred during startup.", ex)

            End Try

        End Using

    End Sub
    Private Sub Application_ThreadException(sender As Object, e As ThreadExceptionEventArgs)

        AbnormalShutdown = True
        ShowUnhandledException("An unexpected program error occurred. WinBMD2 must now close.", e.Exception)

        If Application.OpenForms.Count > 0 Then
            Application.OpenForms(0).BeginInvoke(Sub() Application.Exit())
        Else
            Application.Exit()
        End If

    End Sub

    Private Sub CurrentDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)

        AbnormalShutdown = True

        Dim ex As Exception = TryCast(e.ExceptionObject, Exception)

        If ex IsNot Nothing Then
            ShowUnhandledException("A serious program error occurred. WinBMD2 must now close.", ex)
        Else
            Try
                DebugLog.WriteAlways("[FATAL] Unknown unhandled exception: " & e.ExceptionObject.ToString())
            Catch
            End Try
        End If

    End Sub

    Private Sub TaskScheduler_UnobservedTaskException(sender As Object, e As UnobservedTaskExceptionEventArgs)

        ShowUnhandledException("A background task encountered an unexpected error.", e.Exception)
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
