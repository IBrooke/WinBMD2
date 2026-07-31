Imports System.IO
Imports System.Text

Public Module DebugLog

    Public Sub Clear()

        Try

            AppPaths.EnsureFoldersExist()

            File.WriteAllText(
                AppPaths.DebugLogFilePath,
                $"===== NEW SESSION {DateTime.Now:yyyy-MM-dd HH:mm:ss} ====={Environment.NewLine}")

        Catch
            ' Logging must never prevent the program running.
        End Try

    End Sub

    Public Sub Write(message As String)

        If Not ProjectValues.EnableDiagnosticLogging Then
            Return
        End If

        WriteInternal(message)

    End Sub

    Public Sub WriteAlways(message As String)

        WriteInternal(message)

    End Sub

    Public Sub LogException(
        context As String,
        ex As Exception)

        Try

            WriteInternal(
                $"[EXCEPTION] {context}: {ex.GetType().Name}: {ex.Message}")

            If Not String.IsNullOrWhiteSpace(ex.StackTrace) Then
                WriteInternal(ex.StackTrace)
            End If

            If ex.InnerException IsNot Nothing Then

                WriteInternal(
                    $"[INNER] {ex.InnerException.GetType().Name}: {ex.InnerException.Message}")

                If Not String.IsNullOrWhiteSpace(ex.InnerException.StackTrace) Then
                    WriteInternal(ex.InnerException.StackTrace)
                End If

            End If

        Catch
            ' Logging itself must never crash the application.
        End Try

    End Sub

    Private Sub WriteInternal(message As String)

        Try

            AppPaths.EnsureFoldersExist()

            Dim line As String =
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {message}{Environment.NewLine}"

            File.AppendAllText(
                AppPaths.DebugLogFilePath,
                line,
                Encoding.UTF8)

        Catch
            ' Diagnostic logging must never break the application.
        End Try

    End Sub

End Module
