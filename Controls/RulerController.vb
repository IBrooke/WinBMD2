Public Class RulerController

    Private ReadOnly _viewer As ScanViewerControl

    Private _stage As RulerSetupStage = RulerSetupStage.None
    Private _row1PanY As Single
    Private _row10PanY As Single
    Private _rowStepPanY As Single
    Private _currentRow As Integer
    Private _panX As Single

    Public ReadOnly Property PanX As Single
        Get
            Return _panX
        End Get
    End Property

    Public ReadOnly Property Stage As RulerSetupStage
        Get
            Return _stage
        End Get
    End Property

    Public ReadOnly Property Row1PanY As Single
        Get
            Return _row1PanY
        End Get
    End Property

    Public ReadOnly Property RowStepPanY As Single
        Get
            Return _rowStepPanY
        End Get
    End Property

    Public Sub New(viewer As ScanViewerControl)

        _viewer = viewer

    End Sub

    Public Sub StartSetup()

        _row1PanY = 0.0F
        _row10PanY = 0.0F
        _rowStepPanY = 0.0F
        _currentRow = 0
        _stage = RulerSetupStage.AwaitingRow1

        _viewer.ShowRuler = True

        DebugLog.Write("[RULER] Setup started.")

    End Sub

    Public Function ConfirmCurrentPosition() As Boolean

        Select Case _stage

            Case RulerSetupStage.AwaitingRow1

                _panX = _viewer.GetImagePanX()
                _row1PanY = _viewer.GetImagePanY()

                _stage = RulerSetupStage.AwaitingRow10

                DebugLog.Write(
                    $"[RULER] Row 1 captured. PanY={_row1PanY:0.###}")

                Return True

            Case RulerSetupStage.AwaitingRow10

                _row10PanY = _viewer.GetImagePanY()

                _rowStepPanY =
                    (_row10PanY - _row1PanY) / 9.0F
                _currentRow = 10

                _stage = RulerSetupStage.Complete

                DebugLog.Write(
                    $"[RULER] Row 10 captured. " &
                    $"PanY={_row10PanY:0.###}, " &
                    $"RowStep={_rowStepPanY:0.###}")

                Return True

        End Select

        Return False

    End Function
    Public Sub CancelSetup()

        _stage = RulerSetupStage.None

        DebugLog.Write("[RULER] Setup cancelled.")

    End Sub
    Public Sub MoveToRow(rowNumber As Integer)

        If _stage <> RulerSetupStage.Complete Then
            Return
        End If

        If rowNumber < 1 Then
            Return
        End If

        If rowNumber = 1 Then

            _viewer.SetImagePanY(_row1PanY)
            _currentRow = 1

        Else

            Dim rowDifference As Integer = rowNumber - _currentRow

            _viewer.MoveImageByPanY(
            rowDifference * _rowStepPanY)

            _currentRow = rowNumber

        End If

        DebugLog.Write(
        $"[RULER] Moved to row {rowNumber}. PanY={_viewer.GetImagePanY():0.###}")

    End Sub
End Class