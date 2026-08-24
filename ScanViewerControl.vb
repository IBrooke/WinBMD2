Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel

Public Class ScanViewerControl
    Inherits ScrollableControl

    Public Event EnterPressed As EventHandler
    Public Event PanChanged As EventHandler
    Private _image As Image
    Private _imageOffsetX As Integer
    Private _imageOffsetY As Integer
    Private _panX As Single
    Private _panY As Single
    Private _isDragging As Boolean
    Private _dragStartMouse As Point
    Private _dragStartPan As PointF
    Private _zoom As Single = 1.0F
    Private _rotation As Single = 0.0F

    Private Const MinZoom As Single = 0.05F
    Private Const MaxZoom As Single = 8.0F

    Private _showRuler As Boolean
    Private _rulerScreenY As Single
    Private _rulerBandHeight As Single = 12.0F
    <DefaultValue(False)>
    Public Property ShowRuler As Boolean
        Get
            Return _showRuler
        End Get
        Set(value As Boolean)

            If _showRuler = value Then
                Return
            End If

            _showRuler = value
            Invalidate()

        End Set
    End Property

    <DefaultValue(0.0F)>
    Public Property RulerScreenY As Single
        Get
            Return _rulerScreenY
        End Get
        Set(value As Single)

            _rulerScreenY = value
            Invalidate()

        End Set
    End Property
    Public ReadOnly Property HasImage As Boolean
        Get
            Return _image IsNot Nothing
        End Get
    End Property
    <DefaultValue(1.0F)>
    Public Property Zoom As Single
        Get
            Return _zoom
        End Get
        Set(value As Single)

            _zoom = Math.Max(MinZoom, Math.Min(MaxZoom, value))

            UpdateScrollArea()
            Invalidate()

        End Set
    End Property
    <DefaultValue(0.0F)>
    Public Property Rotation As Single
        Get
            Return _rotation
        End Get
        Set(value As Single)

            _rotation = value

            UpdateScrollArea()
            Invalidate()

        End Set
    End Property

    Public Sub ZoomIn()
        Zoom *= 1.05F
    End Sub

    Public Sub ZoomOut()
        Zoom /= 1.05F
    End Sub

    Public Sub RotateLeft()
        Rotation -= 0.25F
    End Sub

    Public Sub RotateRight()
        Rotation += 0.25F
    End Sub
    Public Sub NudgeImage(deltaX As Single, deltaY As Single)

        _panX += deltaX
        _panY += deltaY
        Invalidate()

    End Sub
    Public Function GetImagePanX() As Single

        Return _panX

    End Function
    Public Sub SetImagePanX(value As Single)

        _panX = value
        Invalidate()

    End Sub
    Public Function GetImagePanY() As Single

        Return _panY

    End Function

    Public Sub SetImagePanY(value As Single)

        _panY = value
        Invalidate()

    End Sub

    Public Sub New()

        DoubleBuffered = True
        AutoScroll = True
        BackColor = Color.DimGray

    End Sub
    Public Sub LoadImage(fileName As String)

        DisposeCurrentImage()

        Using temp As Image = Image.FromFile(fileName)
            _image = New Bitmap(temp)
        End Using
        _panX = 0.0F
        _panY = 0.0F
        UpdateScrollArea()
        Invalidate()

    End Sub

    Public Sub ClearImage()

        DisposeCurrentImage()
        UpdateScrollArea()
        Invalidate()

    End Sub
    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)

        _imageOffsetX = ClientSize.Width \ 2
        _imageOffsetY = ClientSize.Height \ 2

        _rulerScreenY = ClientSize.Height / 2.0F

        UpdateScrollArea()
        Invalidate()

    End Sub
    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        e.Graphics.Clear(BackColor)

        If _image Is Nothing Then
            Return
        End If

        e.Graphics.InterpolationMode =
        Drawing2D.InterpolationMode.HighQualityBicubic

        e.Graphics.PixelOffsetMode =
        Drawing2D.PixelOffsetMode.HighQuality

        e.Graphics.SmoothingMode =
        Drawing2D.SmoothingMode.HighQuality

        Dim scaledWidth As Single = _image.Width * _zoom

        Dim scaledHeight As Single = _image.Height * _zoom

        Dim x As Single = _imageOffsetX + _panX

        Dim y As Single = _imageOffsetY + _panY

        e.Graphics.TranslateTransform(
        x + scaledWidth / 2.0F,
        y + scaledHeight / 2.0F)

        e.Graphics.RotateTransform(_rotation)
        e.Graphics.ScaleTransform(_zoom, _zoom)

        e.Graphics.TranslateTransform(
        -_image.Width / 2.0F,
        -_image.Height / 2.0F)

        e.Graphics.DrawImage(
        _image,
        0,
        0,
        _image.Width,
        _image.Height)

        e.Graphics.ResetTransform()

        If _showRuler Then

            Using rulerPen As New Pen(ThemeManager.RulerColour, 15.0F)
                e.Graphics.DrawLine(rulerPen, 0, _rulerScreenY, ClientSize.Width, _rulerScreenY)
            End Using

        End If

    End Sub
    Protected Overrides Sub OnScroll(se As ScrollEventArgs)

        MyBase.OnScroll(se)
        Invalidate()

    End Sub
    Protected Overrides Sub Dispose(disposing As Boolean)

        If disposing Then
            DisposeCurrentImage()
        End If

        MyBase.Dispose(disposing)

    End Sub

    Private Sub DisposeCurrentImage()

        If _image Is Nothing Then
            Return
        End If

        _image.Dispose()
        _image = Nothing

    End Sub

    Private Sub UpdateScrollArea()

        If _image Is Nothing Then
            AutoScrollMinSize = Size.Empty
            Return
        End If

        _imageOffsetX = ClientSize.Width \ 2
        _imageOffsetY = ClientSize.Height \ 2

        Dim radians As Single =
        Math.Abs(_rotation) * CSng(Math.PI) / 180.0F

        Dim cosValue As Single =
        Math.Abs(CSng(Math.Cos(radians)))

        Dim sinValue As Single =
        Math.Abs(CSng(Math.Sin(radians)))

        Dim width As Integer =
        CInt(Math.Ceiling(
            (_image.Width * cosValue +
             _image.Height * sinValue) * _zoom))

        Dim height As Integer =
        CInt(Math.Ceiling(
            (_image.Width * sinValue +
             _image.Height * cosValue) * _zoom))

        AutoScrollMinSize =
        New Size(
            width + (_imageOffsetX * 2),
            height + (_imageOffsetY * 2))

    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)

        MyBase.OnMouseDown(e)

        If _image Is Nothing Then
            Return
        End If

        If e.Button = MouseButtons.Left Then

            _isDragging = True
            _dragStartMouse = e.Location

            _dragStartPan =
                New PointF(
                    _panX,
                    _panY)

            Cursor = Cursors.SizeAll

        End If

    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)

        MyBase.OnMouseMove(e)

        If Not _isDragging Then
            Return
        End If

        Dim dx As Integer = e.X - _dragStartMouse.X
        Dim dy As Integer = e.Y - _dragStartMouse.Y

        _panX =
    _dragStartPan.X + dx

        _panY =
    _dragStartPan.Y + dy

        Invalidate()

    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)

        MyBase.OnMouseUp(e)

        If e.Button = MouseButtons.Left AndAlso _isDragging Then
            _isDragging = False
            Cursor = Cursors.Default
            RaiseEvent PanChanged(Me, EventArgs.Empty)
        End If

    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)

        MyBase.OnMouseLeave(e)

        If _isDragging Then

            _isDragging = False
            Cursor = Cursors.Default

        End If

    End Sub
    Public Function GetImageYAtScreenY(screenY As Single) As Single

        If _image Is Nothing Then
            Return 0.0F
        End If

        Return (screenY - _imageOffsetY - _panY) / _zoom

    End Function
    Public Sub PositionImageYAtScreenY(
    imageY As Single,
    screenY As Single)

        If _image Is Nothing Then
            Return
        End If

        _panY =
            screenY -
            _imageOffsetY -
            (imageY * _zoom)

        Invalidate()

    End Sub
End Class
