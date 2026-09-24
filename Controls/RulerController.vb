Public Class RulerController

    Private ReadOnly _viewer As ScanViewerControl

    Private _stage As RulerSetupStage = RulerSetupStage.None
    Private _row1ImageY As Single
    Private _row10ImageY As Single
    Private _rowStepImageY As Single
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

    Public ReadOnly Property Row1ImageY As Single
        Get
            Return _row1ImageY
        End Get
    End Property

    Public ReadOnly Property RowStepImageY As Single
        Get
            Return _rowStepImageY
        End Get
    End Property

    Public Sub New(viewer As ScanViewerControl)

        _viewer = viewer

    End Sub

    Public Sub StartSetup()

        _row1ImageY = 0.0F
        _row10ImageY = 0.0F
        _rowStepImageY = 0.0F
        _currentRow = 0
        _stage = RulerSetupStage.AwaitingRow1

        _viewer.ShowRuler = True

        DebugLog.Write("[RULER] Setup started.")

    End Sub

    Public Sub LoadCalibration(row1ImageY As Single, rowStepImageY As Single)

        _row1ImageY = row1ImageY
        _rowStepImageY = rowStepImageY

        _row10ImageY =
            _row1ImageY + (9.0F * _rowStepImageY)

        _currentRow = 1
        _stage = RulerSetupStage.Complete

        DebugLog.Write(
            $"[RULER] Calibration loaded. " &
            $"Row1ImageY={_row1ImageY:0.###}, " &
            $"Row10ImageY={_row10ImageY:0.###}, " &
            $"RowStepImageY={_rowStepImageY:0.###}")

    End Sub

    Public Function ConfirmCurrentPosition() As Boolean

        Select Case _stage

            Case RulerSetupStage.AwaitingRow1

                _panX = _viewer.GetImagePanX()

                _row1ImageY =
                    _viewer.GetImageYAtScreenY(
                        _viewer.RulerScreenY)

                _stage = RulerSetupStage.AwaitingRow10

                DebugLog.Write(
                    $"[RULER] Row 1 captured. ImageY={_row1ImageY:0.###}")

                Return True

            Case RulerSetupStage.AwaitingRow10

                _row10ImageY =
                    _viewer.GetImageYAtScreenY(
                        _viewer.RulerScreenY)

                _rowStepImageY =
                    (_row10ImageY - _row1ImageY) / 9.0F

                _currentRow = 10
                _stage = RulerSetupStage.Complete

                DebugLog.Write(
                    $"[RULER] Row 10 captured. " &
                    $"ImageY={_row10ImageY:0.###}, " &
                    $"RowStepImageY={_rowStepImageY:0.###}")

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

        Dim previousRow As Integer = _currentRow

        If rowNumber = 1 Then

            _viewer.PositionImageYAtScreenY(Row1ImageY, _viewer.RulerScreenY)

            _currentRow = 1

            DebugLog.Write(
            $"[RULER] Grid moved from row {previousRow} to row 1 using absolute position. Row1ImageY={Row1ImageY:0.###}")

            Return

        End If

        Dim deltaRows As Integer = _currentRow - rowNumber

        If deltaRows = 0 Then
            Return
        End If

        MoveByRows(deltaRows)

        _currentRow = rowNumber

        DebugLog.Write(
        $"[RULER] Grid moved from row {previousRow} to row {rowNumber}. DeltaRows={deltaRows}, RowStepImageY={_rowStepImageY:0.###}")

    End Sub
    ' Updates the current grid row without moving the ruler.
    ' Used when the grid moves onto a directive row.
    Public Sub SetCurrentRow(rowNumber As Integer)

        If rowNumber < 1 Then Return

        _currentRow = rowNumber

    End Sub
    Public Sub MoveScanToRow1()
        ' Moves the scan to the saved row 1 position without changing the current grid row.

        If _stage <> RulerSetupStage.Complete Then
            Return
        End If

        _viewer.PositionImageYAtScreenY(_row1ImageY, _viewer.RulerScreenY)

        DebugLog.Write(
        $"[RULER] Scan moved to row 1 absolute position. CurrentGridRow={_currentRow}, Row1ImageY={_row1ImageY:0.###}")

    End Sub
    ' Moves the ruler vertically by a number of row spacings.
    ' deltaRows may be fractional; for example, 1.0 moves one row and 0.1 moves one tenth of a row.
    Public Sub MoveByRows(deltaRows As Single)

        If _stage <> RulerSetupStage.Complete Then Return
        If deltaRows = 0 Then Return

        Dim rulerMovement As Single = -deltaRows * _rowStepImageY * _viewer.Zoom
        Dim newRulerY As Single = _viewer.RulerScreenY + rulerMovement

        Dim currentImageY As Single = _viewer.GetImageYAtScreenY(_viewer.RulerScreenY)
        Dim targetImageY As Single = currentImageY - (deltaRows * _rowStepImageY)

        If targetImageY < 0.0F OrElse targetImageY > _viewer.ImageHeight Then Return

        Dim proposedImageY As Single = _viewer.GetImageYAtScreenY(newRulerY)

        If proposedImageY >= _viewer.ImageHeight Then Return

        Dim topLimit As Single = _viewer.ClientSize.Height * 0.1F
        Dim bottomLimit As Single = _viewer.ClientSize.Height * 0.9F

        If newRulerY > bottomLimit Then

            _viewer.RulerScreenY = topLimit
            _viewer.PositionImageYAtScreenY(currentImageY, topLimit)

            DebugLog.Write($"[RULER] Bottom rollover. CurrentImageY={currentImageY:0.###}, RulerScreenY={topLimit:0.###}")

        ElseIf newRulerY < topLimit Then

            _viewer.RulerScreenY = bottomLimit
            _viewer.PositionImageYAtScreenY(currentImageY, bottomLimit)

            DebugLog.Write($"[RULER] Top rollover. CurrentImageY={currentImageY:0.###}, RulerScreenY={bottomLimit:0.###}")

        Else

            _viewer.RulerScreenY = newRulerY
            DebugLog.Write($"[RULER] Moved by {deltaRows} row(s). RulerScreenY={_viewer.RulerScreenY:0.###}, ImageY={_viewer.GetImageYAtScreenY(_viewer.RulerScreenY):0.###}, ImageHeight={_viewer.ImageHeight}")

        End If

    End Sub
End Class