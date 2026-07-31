Public NotInheritable Class CommandController
    Inherits ApplicationContext
    Implements ICommandExecutor

    Private _transcriptionForm As TranscriptionForm
    Private _scanView As ScanView

    Public Sub Start()

        If HeaderDetailsAreRequired() Then
            ShowHeaderForm()
        Else
            ShowTranscriptionForms()
        End If

    End Sub

    Private Function HeaderDetailsAreRequired() As Boolean

        ' Temporary until the real header validation is implemented.
        Return False

    End Function

    Private Sub ShowHeaderForm()

        Dim headerForm As New HeaderForm()

        AddHandler headerForm.FormClosed, AddressOf HeaderForm_FormClosed

        MainForm = headerForm
        headerForm.Show()

    End Sub

    Private Sub ShowTranscriptionForms()

        _transcriptionForm = New TranscriptionForm(Me)
        _scanView = New ScanView(Me)

        AddHandler _transcriptionForm.FormClosed, AddressOf TranscriptionForm_FormClosed

        MainForm = _transcriptionForm

        _transcriptionForm.Show()
        _scanView.Show()

    End Sub

    Private Sub HeaderForm_FormClosed(sender As Object, e As FormClosedEventArgs)

        ExitThread()

    End Sub

    Private Sub TranscriptionForm_FormClosed(sender As Object, e As FormClosedEventArgs)

        If _scanView IsNot Nothing AndAlso Not _scanView.IsDisposed Then
            _scanView.Close()
        End If

        ExitThread()

    End Sub

    Public Sub Execute(command As AppCommand) Implements ICommandExecutor.Execute

        Select Case command

            Case AppCommand.OpenFile
                ' To be connected to the real Open routine.

            Case AppCommand.SaveFile
                ' To be connected to the real Save routine.

            Case AppCommand.SaveFileAs
                ' To be connected to the real Save As routine.

            Case AppCommand.CloseFile
                ' To be connected later.

            Case AppCommand.ExitApplication
                ExitThread()

            Case AppCommand.ZoomIn
                ' _scanView.ZoomIn()

            Case AppCommand.ZoomOut
                ' _scanView.ZoomOut()

            Case AppCommand.FitScanWidth
                ' _scanView.FitWidth()

            Case AppCommand.ResetScanView
                ' _scanView.ResetView()

            Case AppCommand.RotateScanLeft
                ' _scanView.RotateLeft()

            Case AppCommand.RotateScanRight
                ' _scanView.RotateRight()

            Case AppCommand.ToggleMagnifier
                ' _scanView.ToggleMagnifier()

            Case AppCommand.ToggleRuler
                ' _scanView.ToggleRuler()

        End Select

    End Sub

End Class