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
        AddHandler _viewer.ZoomChanged, AddressOf Viewer_ZoomChanged
        UpdateRulerHeight()

    End Sub
    Private Sub Viewer_ZoomChanged(sender As Object, e As EventArgs)

        UpdateRulerHeight()

    End Sub
    ' During ruler setup the ruler remains at a fixed screen position and the
    ' scan is moved underneath it to align scan rows 1 and 10 with the ruler.
    ' This avoids ruler rollover and makes the two calibration rows easier to position.
    '
    ' ConfirmCurrentPosition records the IMAGE Y coordinate beneath the ruler,
    ' so the saved calibration is independent of the ruler's screen position.
    ' Once setup is complete, normal operation continues to move the scan
    ' beneath the stationary ruler.
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
        UpdateRulerHeight()

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

                _row1ImageY = _viewer.GetImageYAtScreenY(_viewer.RulerTop)

                _stage = RulerSetupStage.AwaitingRow10

                DebugLog.Write($"[RULER] Row 1 captured. ImageY={_row1ImageY:0.###}")

                Return True

            Case RulerSetupStage.AwaitingRow10

                _row10ImageY = _viewer.GetImageYAtScreenY(_viewer.RulerTop)

                _rowStepImageY = (_row10ImageY - _row1ImageY) / 9.0F

                _currentRow = 10
                _stage = RulerSetupStage.Complete
                UpdateRulerHeight()

                DebugLog.Write(
                    $"[RULER] Row 10 captured. " &
                    $"ImageY={_row10ImageY:0.###}, " &
                    $"RowStepImageY={_rowStepImageY:0.###}")

                Return True

        End Select

        Return False

    End Function
    Public ReadOnly Property MaximumRulerHeight As Integer
        Get
            If _stage <> RulerSetupStage.Complete OrElse Math.Abs(_rowStepImageY) < 0.001F Then
                Return Math.Max(30, CInt(Math.Round(30.0F * _viewer.DeviceDpi / 96.0F)))
            End If

            Return Math.Max(4, CInt(Math.Floor(Math.Abs(_rowStepImageY))))
        End Get
    End Property
    Public Sub CancelSetup()

        _stage = RulerSetupStage.None

        DebugLog.Write("[RULER] Setup cancelled.")

    End Sub
    Public Sub MoveScanToRow1()
        ' Moves the scan to the saved row 1 position without changing the current grid row.

        If _stage <> RulerSetupStage.Complete Then
            Return
        End If

        _viewer.PositionImageYAtScreenY(_row1ImageY, _viewer.RulerTop)

        DebugLog.Write(
        $"[RULER] Scan moved to row 1 absolute position. CurrentGridRow={_currentRow}, Row1ImageY={_row1ImageY:0.###}")

    End Sub
    ' Moves the scan vertically by a number of calibrated row spacings.
    ' Positive values advance through the grid, moving the scan upwards.
    ' Negative values move backwards, moving the scan downwards.
    Public Sub MoveByRows(deltaRows As Single)

        If _stage <> RulerSetupStage.Complete Then Return
        If deltaRows = 0 Then Return
        If Not _viewer.HasImage Then Return

        Dim scanMovement As Single = -deltaRows * _rowStepImageY * _viewer.Zoom

        _viewer.NudgeImage(0.0F, scanMovement)

        DebugLog.Write($"[RULER] Scan moved by {deltaRows:0.###} row(s). Movement={scanMovement:0.###}, PanY={_viewer.GetImagePanY():0.###}")

    End Sub
    Public Sub UpdateRulerHeight()

        Dim height As Single = ProjectValues.RulerHeight * _viewer.Zoom

        If _stage = RulerSetupStage.Complete AndAlso Math.Abs(_rowStepImageY) > 0.001F Then

            Dim maximumHeight As Single = Math.Abs(_rowStepImageY * _viewer.Zoom)

            height = Math.Min(height, maximumHeight)

        End If

        _viewer.SetRulerHeight(Math.Max(4.0F, height))

    End Sub
End Class