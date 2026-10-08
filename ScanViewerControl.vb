Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel

Public Class ScanViewerControl
    Inherits ScrollableControl

    Public Event EnterPressed As EventHandler
    Public Event PanChanged As EventHandler
    Public Event DragFinished As EventHandler
    Private _image As Image
    Private _displayImage As Bitmap
    Private _displayImageDirty As Boolean = True
    Private _imageOffsetX As Integer
    Private _imageOffsetY As Integer
    Private _panX As Single
    Private _panY As Single
    Private _isDragging As Boolean
    Private _dragStartMouse As Point
    Private _dragStartPan As PointF
    Private _zoom As Single = 1.0F
    Private _rotation As Single = 0.0F

    Private _magnifierForm As Form
    Private _magnifierBox As PictureBox
    Private _magnifierActive As Boolean

    Private Const MagnifierLogicalSize As Integer = 120
    Private Const MagnifierLogicalOffset As Integer = 20
    Private Const StandardDpi As Integer = 96

    Private Const MinZoom As Single = 0.05F
    Private Const MaxZoom As Single = 8.0F

    Private _showRuler As Boolean
    Private _rulerScreenY As Single
    Private _rulerHeight As Single = 1.0F

    <DefaultValue(False)>
    Public Property ShowRuler As Boolean
        Get
            Return _showRuler
        End Get
        Set(value As Boolean)

            If _showRuler = value Then Return

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
    Public Sub SetRulerHeight(height As Single)

        _rulerHeight = Math.Max(1.0F, height)
        Invalidate()

    End Sub
    Public Sub New()

        DoubleBuffered = True
        AutoScroll = True
        BackColor = Color.DimGray

    End Sub
    Public ReadOnly Property HasImage As Boolean
        Get
            Return _image IsNot Nothing
        End Get
    End Property
    Public ReadOnly Property ImageHeight As Integer
        Get
            If _image Is Nothing Then Return 0
            Return _image.Height
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
            InvalidateDisplayImage()

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
            InvalidateDisplayImage()

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
        InvalidateDisplayImage()

    End Sub
    Public Function GetImagePanX() As Single

        Return _panX

    End Function
    Public Sub SetImagePanX(value As Single)

        _panX = value
        InvalidateDisplayImage()

    End Sub
    Public Function GetImagePanY() As Single

        Return _panY

    End Function

    Public Sub SetImagePanY(value As Single)

        _panY = value
        InvalidateDisplayImage()

    End Sub
    Public Sub LoadImage(fileName As String)

        DisposeCurrentImage()

        Using temp As Image = Image.FromFile(fileName)
            _image = New Bitmap(temp)
        End Using
        _panX = 0.0F
        _panY = 0.0F
        UpdateScrollArea()
        CreateDisplayImage()
        Invalidate()

    End Sub

    Public Sub ClearImage()

        DisposeCurrentImage()
        DisposeDisplayImage()
        UpdateScrollArea()
        Invalidate()

    End Sub
    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)

        _imageOffsetX = ClientSize.Width \ 2
        _imageOffsetY = ClientSize.Height \ 2

        RulerScreenY = ClientSize.Height / 2.0F

        UpdateScrollArea()
        InvalidateDisplayImage()

    End Sub
    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        'Dim paintStarted As Long = Stopwatch.GetTimestamp()
        'Dim scanRedrawn As Boolean = _displayImageDirty

        MyBase.OnPaint(e)

        If _displayImageDirty Then CreateDisplayImage()

        If _displayImage IsNot Nothing Then
            e.Graphics.DrawImageUnscaled(_displayImage, 0, 0)
        Else
            e.Graphics.Clear(BackColor)
        End If

        If _showRuler Then

            Dim rulerTop As Single = _rulerScreenY - (_rulerHeight / 2.0F)

            Using rulerBrush As New SolidBrush(ThemeManager.RulerColour)
                e.Graphics.FillRectangle(rulerBrush, 0.0F, rulerTop, ClientSize.Width, _rulerHeight)
            End Using

        End If

        'Dim paintMilliseconds As Double = Stopwatch.GetElapsedTime(paintStarted).TotalMilliseconds
        'DebugLog.Write($"[SCAN PAINT] Redrawn={scanRedrawn}, Time={paintMilliseconds:0.0} ms")
    End Sub

    Private Sub DrawScan(graphics As Graphics)

        graphics.Clear(BackColor)

        If _image Is Nothing Then Return

        graphics.InterpolationMode = Drawing2D.InterpolationMode.Bilinear
        graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Default

        Dim scaledWidth As Single = _image.Width * _zoom
        Dim scaledHeight As Single = _image.Height * _zoom

        ' Include the ScrollableControl position so mouse-wheel scrolling and repainting
        ' use exactly the same image position.
        Dim x As Single = _imageOffsetX + _panX + AutoScrollPosition.X
        Dim y As Single = _imageOffsetY + _panY + AutoScrollPosition.Y

        ' Make sure that at least part of the image remains visible.
        If x >= ClientSize.Width OrElse x + scaledWidth <= 0 Then
            _panX = 0.0F
            x = _imageOffsetX + AutoScrollPosition.X
            DebugLog.WriteAlways("[SCAN] Horizontal image position was outside the viewer and has been reset.")
            RaiseEvent PanChanged(Me, EventArgs.Empty)
        End If

        If y >= ClientSize.Height OrElse y + scaledHeight <= 0 Then
            _panY = 0.0F
            y = _imageOffsetY + AutoScrollPosition.Y
            DebugLog.WriteAlways("[SCAN] Vertical image position was outside the viewer and has been reset.")
            RaiseEvent PanChanged(Me, EventArgs.Empty)
        End If

        graphics.TranslateTransform(x + scaledWidth / 2.0F, y + scaledHeight / 2.0F)
        graphics.RotateTransform(_rotation)
        graphics.ScaleTransform(_zoom, _zoom)
        graphics.TranslateTransform(-_image.Width / 2.0F, -_image.Height / 2.0F)

        ' Draw only the part of the scan that can currently be seen in the viewer.
        ' Convert the viewer corners back into image coordinates and add a small
        ' margin to allow for rotation, rounding and interpolation.
        Dim visiblePoints() As PointF = {
        New PointF(0, 0),
        New PointF(ClientSize.Width, 0),
        New PointF(ClientSize.Width, ClientSize.Height),
        New PointF(0, ClientSize.Height)
    }

        Using inverseTransform As Drawing2D.Matrix = graphics.Transform.Clone()
            inverseTransform.Invert()
            inverseTransform.TransformPoints(visiblePoints)
        End Using

        Dim left As Single = visiblePoints.Min(Function(p) p.X)
        Dim top As Single = visiblePoints.Min(Function(p) p.Y)
        Dim right As Single = visiblePoints.Max(Function(p) p.X)
        Dim bottom As Single = visiblePoints.Max(Function(p) p.Y)

        Const CropMargin As Single = 20.0F

        left = Math.Max(0.0F, left - CropMargin)
        top = Math.Max(0.0F, top - CropMargin)
        right = Math.Min(_image.Width, right + CropMargin)
        bottom = Math.Min(_image.Height, bottom + CropMargin)

        If right > left AndAlso bottom > top Then
            Dim sourceRectangle As New RectangleF(left, top, right - left, bottom - top)
            graphics.DrawImage(_image, sourceRectangle, sourceRectangle, GraphicsUnit.Pixel)
        End If

        graphics.ResetTransform()

    End Sub
    Protected Overrides Sub OnScroll(se As ScrollEventArgs)

        MyBase.OnScroll(se)
        InvalidateDisplayImage()

    End Sub
    Protected Overrides Sub Dispose(disposing As Boolean)

        If disposing Then
            DisposeCurrentImage()
            DisposeDisplayImage()
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
    Private Sub DisposeDisplayImage()

        If _displayImage Is Nothing Then Return

        _displayImage.Dispose()
        _displayImage = Nothing

    End Sub
    Private Sub CreateDisplayImage()

        DisposeDisplayImage()

        If _image Is Nothing Then Return
        If ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then Return

        _displayImage = New Bitmap(ClientSize.Width, ClientSize.Height)

        Using graphics As Graphics = Graphics.FromImage(_displayImage)
            DrawScan(graphics)
        End Using

        _displayImageDirty = False

    End Sub
    Private Sub InvalidateDisplayImage()

        _displayImageDirty = True
        Invalidate()

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

        If e.Button = MouseButtons.Right Then

            _magnifierActive = True
            ShowMagnifier(e.Location)
            Return

        End If

        If e.Button = MouseButtons.Left Then

            _isDragging = True
            _dragStartMouse = e.Location

            _dragStartPan = New PointF(_panX, _panY)

            Cursor = Cursors.SizeAll

        End If

    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)

        MyBase.OnMouseMove(e)

        If _magnifierActive Then

            UpdateMagnifier(e.Location)
            Return

        End If

        If Not _isDragging Then
            Return
        End If

        Dim dx As Integer = e.X - _dragStartMouse.X
        Dim dy As Integer = e.Y - _dragStartMouse.Y

        _panX = _dragStartPan.X + dx

        _panY = _dragStartPan.Y + dy

        InvalidateDisplayImage()

    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)

        MyBase.OnMouseUp(e)

        If e.Button = MouseButtons.Left AndAlso _isDragging Then
            _isDragging = False
            Cursor = Cursors.Default
            RaiseEvent PanChanged(Me, EventArgs.Empty)
            RaiseEvent DragFinished(Me, EventArgs.Empty)
        End If

        If e.Button = MouseButtons.Right Then

            HideMagnifier()
            RaiseEvent DragFinished(Me, EventArgs.Empty)
            Return

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
    Public Sub PositionImageYAtScreenY(imageY As Single, screenY As Single)

        If _image Is Nothing Then
            Return
        End If

        _panY = screenY - _imageOffsetY - (imageY * _zoom)

        InvalidateDisplayImage()

    End Sub
    Public Function ScaleForCurrentDpi(logicalPixels As Integer) As Integer

        Dim dpi As Integer = If(DeviceDpi > 0, DeviceDpi, StandardDpi)

        Return CInt(Math.Round(logicalPixels * dpi / CDbl(StandardDpi)))

    End Function

    Private Sub ShowMagnifier(mousePoint As Point)

        If _magnifierForm Is Nothing Then

            _magnifierForm = New Form With {
                .FormBorderStyle = FormBorderStyle.None,
                .ShowInTaskbar = False,
                .TopMost = True,
                .StartPosition = FormStartPosition.Manual,
                .AutoScaleMode = AutoScaleMode.None,
                .BackColor = Color.Black
            }

        End If

        If _magnifierBox Is Nothing Then

            _magnifierBox = New PictureBox With {
                .Dock = DockStyle.Fill,
                .SizeMode = PictureBoxSizeMode.Zoom,
                .BackColor = Color.Black
            }

        End If

        If Not _magnifierForm.Controls.Contains(_magnifierBox) Then
            _magnifierForm.Controls.Add(_magnifierBox)
        End If

        Dim magnifierSize As Integer = ScaleForCurrentDpi(MagnifierLogicalSize)

        _magnifierForm.ClientSize = New Size(magnifierSize, magnifierSize)

        _magnifierForm.Show(Me)

        UpdateMagnifier(mousePoint)

    End Sub

    Private Sub UpdateMagnifier(mousePoint As Point)

        If _magnifierForm Is Nothing OrElse _magnifierBox Is Nothing OrElse _image Is Nothing Then
            Return
        End If

        Dim offset As Integer = ScaleForCurrentDpi(MagnifierLogicalOffset)

        _magnifierForm.Location = PointToScreen(New Point(mousePoint.X + offset, mousePoint.Y + offset))

        Dim scaledWidth As Single = _image.Width * _zoom
        Dim scaledHeight As Single = _image.Height * _zoom

        Dim x As Single = _imageOffsetX + _panX + AutoScrollPosition.X
        Dim y As Single = _imageOffsetY + _panY + AutoScrollPosition.Y

        Using matrix As New Drawing2D.Matrix()

            matrix.Translate(x + scaledWidth / 2.0F, y + scaledHeight / 2.0F)
            matrix.Rotate(_rotation)
            matrix.Scale(_zoom, _zoom)
            matrix.Translate(-_image.Width / 2.0F, -_image.Height / 2.0F)

            matrix.Invert()

            Dim points() As PointF = {mousePoint}
            matrix.TransformPoints(points)

            Dim imageX As Integer = CInt(Math.Round(points(0).X))
            Dim imageY As Integer = CInt(Math.Round(points(0).Y))

            Dim cropSize As Integer = Math.Max(1, CInt(Math.Round(MagnifierLogicalSize / (3.0F * _zoom))))

            Dim sourceRectangle As New Rectangle(
                imageX - cropSize \ 2,
                imageY - cropSize \ 2,
                cropSize,
                cropSize)

            sourceRectangle.Intersect(New Rectangle(0, 0, _image.Width, _image.Height))

            If sourceRectangle.Width <= 0 OrElse sourceRectangle.Height <= 0 Then
                Return
            End If

            Dim crop As New Bitmap(sourceRectangle.Width, sourceRectangle.Height)

            Using graphics As Graphics = Graphics.FromImage(crop)
                graphics.DrawImage(_image, New Rectangle(0, 0, crop.Width, crop.Height), sourceRectangle, GraphicsUnit.Pixel)
            End Using

            Dim oldImage As Image = _magnifierBox.Image

            _magnifierBox.Image = crop

            If oldImage IsNot Nothing Then
                oldImage.Dispose()
            End If

        End Using

    End Sub

    Private Sub HideMagnifier()

        _magnifierActive = False

        If _magnifierForm IsNot Nothing Then
            _magnifierForm.Hide()
        End If

    End Sub
End Class
