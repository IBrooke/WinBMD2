Imports System.Drawing
Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class ScanViewerControl
    Inherits ScrollableControl

    Public Event EnterPressed As EventHandler
    Public Event PanChanged As EventHandler
    Public Event DragFinished As EventHandler
    Public Event ZoomChanged As EventHandler
    Private _image As Image
    Private _displayImage As Bitmap
    Private _displayImageDirty As Boolean = True
    Private _imageOffsetX As Integer
    Private _imageOffsetY As Integer
    Private _panX As Single
    Private _panY As Single

    ' The vertical scrollbar represents the same pan used by dragging and the ruler.
    ' Both scrollbars reflect the image pan; native AutoScroll is disabled.
    Private ReadOnly _imageVerticalScrollBar As New VScrollBar With {
        .Dock = DockStyle.Right,
        .TabStop = False,
        .Enabled = False
    }
    Private ReadOnly _imageHorizontalScrollBar As New HScrollBar With {
        .Dock = DockStyle.Bottom,
        .TabStop = False,
        .Enabled = False
    }
    Private _syncingHorizontalScrollBar As Boolean
    Private _horizontalScrollOrigin As Single
    Private _syncingVerticalScrollBar As Boolean
    Private _verticalScrollOrigin As Single
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
    Private _rulerHeight As Single = 4.0F

    ' Reusable ruler-sized buffers; never modify the cached scan pixels.
    Private _maskedRuler As Bitmap
    Private _rulerRowBytes As Byte()

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
            SyncVerticalScrollBar()
            Invalidate()

        End Set
    End Property
    Public ReadOnly Property RulerTop As Single
        Get
            Return _rulerScreenY - (_rulerHeight / 2.0F)
        End Get
    End Property
    Public Sub SetRulerHeight(height As Single)

        _rulerHeight = Math.Max(4.0F, height)
        SyncVerticalScrollBar()
        SyncHorizontalScrollBar()
        Invalidate()

    End Sub
    Public Sub New()

        DoubleBuffered = True
        AutoScroll = False
        Controls.Add(_imageVerticalScrollBar)
        Controls.Add(_imageHorizontalScrollBar)
        AddHandler _imageHorizontalScrollBar.ValueChanged, AddressOf ImageHorizontalScrollBar_ValueChanged
        AddHandler _imageVerticalScrollBar.ValueChanged, AddressOf ImageVerticalScrollBar_ValueChanged
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

            Dim newZoom As Single = Math.Max(MinZoom, Math.Min(MaxZoom, value))

            If newZoom = _zoom Then Return

            _zoom = newZoom

            UpdateScrollArea()
            InvalidateDisplayImage()

            RaiseEvent ZoomChanged(Me, EventArgs.Empty)

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
    ' Returns the displayed vertical position of the scan, including mouse-wheel scrolling.
    Public Function GetScanPositionY() As Double
        Return CDbl(_imageOffsetY) + CDbl(_panY)
    End Function

    ' Restores the displayed vertical position without moving the ruler or changing X.
    Public Sub SetScanPositionY(value As Double)
        If _image Is Nothing Then Return
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return

        Dim panY As Double = value - CDbl(_imageOffsetY)
        If panY < Single.MinValue OrElse panY > Single.MaxValue Then Return

        _panY = CSng(panY)
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
        DisposeMaskedRuler()
        UpdateScrollArea()
        Invalidate()

    End Sub
    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)

        _imageOffsetX = ImageViewportSize.Width \ 2
        _imageOffsetY = ImageViewportSize.Height \ 2

        RulerScreenY = ImageViewportSize.Height / 2.0F

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

            If ProjectValues.RulerTranslucent Then
                DrawMaskedRuler(e.Graphics, rulerTop, ThemeManager.RulerColour)
            Else
                Using rulerBrush As New SolidBrush(ThemeManager.RulerColour)
                    e.Graphics.FillRectangle(rulerBrush, 0.0F, rulerTop, ClientSize.Width, _rulerHeight)
                End Using
            End If

        End If

    End Sub

    Private Sub DrawMaskedRuler(graphics As Graphics, rulerTop As Single, colour As Color)

        If ClientSize.Width <= 0 OrElse ClientSize.Height <= 0 Then Return

        ' Cover the visible ruler band, including fractional screen positions.
        Dim top As Integer = CInt(Math.Max(0.0, Math.Min(CDbl(ClientSize.Height), Math.Floor(CDbl(rulerTop)))))
        Dim bottom As Integer = CInt(Math.Max(0.0, Math.Min(CDbl(ClientSize.Height), Math.Ceiling(CDbl(rulerTop) + _rulerHeight))))
        If bottom <= top Then Return

        If _displayImage Is Nothing Then
            ' Match the mask operation even when the viewer has no scan.
            Dim maskedBackground As Color = Color.FromArgb(
                CInt(BackColor.R) And CInt(colour.R),
                CInt(BackColor.G) And CInt(colour.G),
                CInt(BackColor.B) And CInt(colour.B))
            Using brush As New SolidBrush(maskedBackground)
                graphics.FillRectangle(brush, 0, top, ClientSize.Width, bottom - top)
            End Using
            Return
        End If

        Dim band As Rectangle = Rectangle.Intersect(
            New Rectangle(0, top, ClientSize.Width, bottom - top),
            New Rectangle(0, 0, _displayImage.Width, _displayImage.Height))
        If band.Width <= 0 OrElse band.Height <= 0 Then Return

        If _maskedRuler Is Nothing OrElse
           _maskedRuler.Width <> band.Width OrElse
           _maskedRuler.Height <> band.Height Then
            DisposeMaskedRuler()
            _maskedRuler = New Bitmap(band.Width, band.Height, PixelFormat.Format32bppArgb)
        End If

        Dim rowLength As Integer = band.Width * 4
        If _rulerRowBytes Is Nothing OrElse _rulerRowBytes.Length <> rowLength Then
            _rulerRowBytes = New Byte(rowLength - 1) {}
        End If

        Dim sourceData As BitmapData = _displayImage.LockBits(
            band, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        Try
            Dim targetData As BitmapData = _maskedRuler.LockBits(
                New Rectangle(0, 0, band.Width, band.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
            Try
                For row As Integer = 0 To band.Height - 1
                    Marshal.Copy(IntPtr.Add(sourceData.Scan0, row * sourceData.Stride),
                                 _rulerRowBytes, 0, rowLength)

                    ' vbMaskPen / R2_MASKPEN: destination RGB AND selected RGB.
                    ' Windows 32-bit bitmap byte order is blue, green, red, alpha.
                    ' Ignore the theme colour's alpha: this is a Boolean mask.
                    For pixel As Integer = 0 To rowLength - 1 Step 4
                        _rulerRowBytes(pixel) = CByte(_rulerRowBytes(pixel) And colour.B)
                        _rulerRowBytes(pixel + 1) = CByte(_rulerRowBytes(pixel + 1) And colour.G)
                        _rulerRowBytes(pixel + 2) = CByte(_rulerRowBytes(pixel + 2) And colour.R)
                        _rulerRowBytes(pixel + 3) = Byte.MaxValue
                    Next

                    Marshal.Copy(_rulerRowBytes, 0,
                                 IntPtr.Add(targetData.Scan0, row * targetData.Stride), rowLength)
                Next
            Finally
                _maskedRuler.UnlockBits(targetData)
            End Try
        Finally
            _displayImage.UnlockBits(sourceData)
        End Try

        graphics.DrawImageUnscaled(_maskedRuler, band.Left, band.Top)

    End Sub

    Private Sub DisposeMaskedRuler()

        If _maskedRuler IsNot Nothing Then
            _maskedRuler.Dispose()
            _maskedRuler = Nothing
        End If
        _rulerRowBytes = Nothing

    End Sub

    Private Sub DrawScan(graphics As Graphics)

        graphics.Clear(BackColor)

        If _image Is Nothing Then Return

        graphics.InterpolationMode = Drawing2D.InterpolationMode.Bilinear
        graphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.Default

        Dim scaledWidth As Single = _image.Width * _zoom
        Dim scaledHeight As Single = _image.Height * _zoom

        ' Drawing and the scrollbars use the same pan position.
        Dim x As Single = _imageOffsetX + _panX
        Dim y As Single = _imageOffsetY + _panY

        ' DebugLog.Write($"[SCAN DRAW] OffsetY={_imageOffsetY:0.###}, PanY={_panY:0.###}, ActualY={y:0.###}")

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
    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)

        ' Do not invoke native vertical scrolling: wheel and ruler share _panY.
        Dim handledArgs As HandledMouseEventArgs = TryCast(e, HandledMouseEventArgs)
        If handledArgs IsNot Nothing Then handledArgs.Handled = True
        If _image Is Nothing OrElse e.Delta = 0 Then Return

        Dim lines As Integer = SystemInformation.MouseWheelScrollLines
        If lines = 0 Then Return

        Dim distance As Single = If(lines < 0,
            CSng(ImageViewportSize.Height),
            CSng(lines * ScaleForCurrentDpi(16)))

        NudgeImage(0.0F, CSng(e.Delta) / 120.0F * distance)
        RaiseEvent PanChanged(Me, EventArgs.Empty)

    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)

        If disposing Then
            DisposeCurrentImage()
            DisposeDisplayImage()
            DisposeMaskedRuler()
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
        SyncVerticalScrollBar()
        SyncHorizontalScrollBar()
        Invalidate()

    End Sub
    ' The visible image area excludes the two fixed scrollbar controls.
    Private ReadOnly Property ImageViewportSize As Size
        Get
            Dim barWidth As Integer = If(_imageVerticalScrollBar Is Nothing, 0, _imageVerticalScrollBar.Width)
            Dim barHeight As Integer = If(_imageHorizontalScrollBar Is Nothing, 0, _imageHorizontalScrollBar.Height)
            Return New Size(Math.Max(1, ClientSize.Width - barWidth), Math.Max(1, ClientSize.Height - barHeight))
        End Get
    End Property

    Private Sub UpdateScrollArea()

        _imageOffsetX = ImageViewportSize.Width \ 2
        _imageOffsetY = ImageViewportSize.Height \ 2
        SyncVerticalScrollBar()
        SyncHorizontalScrollBar()

    End Sub

    ' Synchronises the thumb without rounding or changing the actual image position.
    Private Sub SyncVerticalScrollBar()

        If _imageVerticalScrollBar Is Nothing OrElse _syncingVerticalScrollBar Then Return
        _syncingVerticalScrollBar = True

        Try
            If _image Is Nothing Then
                _imageVerticalScrollBar.Enabled = False
                _imageVerticalScrollBar.Value = 0
                Return
            End If

            ' Allow the top and bottom of the rotated scan to reach the ruler.
            Dim radians As Double = CDbl(_rotation) * Math.PI / 180.0
            Dim rotatedHeight As Single = CSng(
                (Math.Abs(Math.Sin(radians)) * _image.Width +
                 Math.Abs(Math.Cos(radians)) * _image.Height) * _zoom)
            Dim rotatedTop As Single = (_image.Height * _zoom - rotatedHeight) / 2.0F
            Dim currentY As Single = _imageOffsetY + _panY
            Dim upperY As Single = RulerTop - rotatedTop
            Dim lowerY As Single = upperY - rotatedHeight

            ' Manual positioning must never be clamped or undone by synchronisation.
            upperY = CSng(Math.Ceiling(Math.Max(upperY, currentY)))
            lowerY = CSng(Math.Floor(Math.Min(lowerY, currentY)))
            _verticalScrollOrigin = upperY

            Dim travel As Integer = Math.Max(0, CInt(Math.Ceiling(CDbl(upperY) - lowerY)))
            Dim pageSize As Integer = ImageViewportSize.Height
            Dim position As Integer = Math.Max(0, Math.Min(travel, CInt(Math.Round(CDbl(upperY) - currentY))))

            With _imageVerticalScrollBar
                .Minimum = 0
                .Maximum = travel + pageSize - 1
                .LargeChange = pageSize
                .SmallChange = 1
                .Value = position
                .Enabled = travel > 0
            End With
        Finally
            _syncingVerticalScrollBar = False
        End Try

    End Sub

    Private Sub ImageVerticalScrollBar_ValueChanged(sender As Object, e As EventArgs)

        If _syncingVerticalScrollBar OrElse _image Is Nothing Then Return

        ' Keep the range fixed while dragging the thumb; update the shared pan directly.
        _panY = _verticalScrollOrigin - _imageVerticalScrollBar.Value - _imageOffsetY
        _displayImageDirty = True
        Invalidate()
        RaiseEvent PanChanged(Me, EventArgs.Empty)

    End Sub

    ' Synchronises the thumb without rounding or changing the actual image position.
    Private Sub SyncHorizontalScrollBar()

        If _imageHorizontalScrollBar Is Nothing OrElse _syncingHorizontalScrollBar Then Return
        _syncingHorizontalScrollBar = True

        Try
            If _image Is Nothing Then
                _imageHorizontalScrollBar.Enabled = False
                _imageHorizontalScrollBar.Value = 0
                Return
            End If

            ' Allow the left and right of the rotated scan to reach the viewport centre.
            Dim radians As Double = CDbl(_rotation) * Math.PI / 180.0
            Dim rotatedWidth As Single = CSng(
                (Math.Abs(Math.Sin(radians)) * _image.Height +
                 Math.Abs(Math.Cos(radians)) * _image.Width) * _zoom)
            Dim rotatedLeft As Single = (_image.Width * _zoom - rotatedWidth) / 2.0F
            Dim currentX As Single = _imageOffsetX + _panX
            Dim upperX As Single = CSng(ImageViewportSize.Width) / 2.0F - rotatedLeft
            Dim lowerX As Single = upperX - rotatedWidth

            ' Manual positioning must never be clamped or undone by synchronisation.
            upperX = CSng(Math.Ceiling(Math.Max(upperX, currentX)))
            lowerX = CSng(Math.Floor(Math.Min(lowerX, currentX)))
            _horizontalScrollOrigin = upperX

            Dim travel As Integer = Math.Max(0, CInt(Math.Ceiling(CDbl(upperX) - lowerX)))
            Dim pageSize As Integer = ImageViewportSize.Width
            Dim position As Integer = Math.Max(0, Math.Min(travel, CInt(Math.Round(CDbl(upperX) - currentX))))

            With _imageHorizontalScrollBar
                .Minimum = 0
                .Maximum = travel + pageSize - 1
                .LargeChange = pageSize
                .SmallChange = 1
                .Value = position
                .Enabled = travel > 0
            End With
        Finally
            _syncingHorizontalScrollBar = False
        End Try

    End Sub

    Private Sub ImageHorizontalScrollBar_ValueChanged(sender As Object, e As EventArgs)

        If _syncingHorizontalScrollBar OrElse _image Is Nothing Then Return

        ' Keep the range fixed while dragging the thumb; update the shared pan directly.
        _panX = _horizontalScrollOrigin - _imageHorizontalScrollBar.Value - _imageOffsetX
        _displayImageDirty = True
        Invalidate()
        RaiseEvent PanChanged(Me, EventArgs.Empty)

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

        If _image Is Nothing Then Return 0.0F

        Return (screenY - _imageOffsetY - _panY) / _zoom

    End Function

    Public Sub PositionImageYAtScreenY(imageY As Single, screenY As Single)

        If _image Is Nothing Then Return

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

        Dim x As Single = _imageOffsetX + _panX
        Dim y As Single = _imageOffsetY + _panY

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
