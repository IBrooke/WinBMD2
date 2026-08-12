Imports System.Drawing
Imports System.Windows.Forms

Public Class ScanViewerControl
    Inherits ScrollableControl

    Private _image As Image
    Private _imageOffsetX As Integer
    Private _imageOffsetY As Integer
    Private _isDragging As Boolean
    Private _dragStartMouse As Point
    Private _dragStartScroll As Point

    Public ReadOnly Property HasImage As Boolean
        Get
            Return _image IsNot Nothing
        End Get
    End Property

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

        UpdateScrollArea()
        Invalidate()

    End Sub

    Public Sub ClearImage()

        DisposeCurrentImage()
        UpdateScrollArea()
        Invalidate()

    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        e.Graphics.Clear(BackColor)

        If _image Is Nothing Then
            Return
        End If

        Dim scroll As Point = AutoScrollPosition

        e.Graphics.DrawImage(
            _image,
            scroll.X + _imageOffsetX,
            scroll.Y + _imageOffsetY,
            _image.Width,
            _image.Height)

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

        AutoScrollMinSize = New Size(
        _image.Width + (_imageOffsetX * 2),
        _image.Height + (_imageOffsetY * 2))

    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)

        MyBase.OnMouseDown(e)

        If _image Is Nothing Then
            Return
        End If

        If e.Button = MouseButtons.Left Then

            _isDragging = True
            _dragStartMouse = e.Location

            _dragStartScroll = New Point(
                -AutoScrollPosition.X,
                -AutoScrollPosition.Y)

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

        AutoScrollPosition = New Point(
            _dragStartScroll.X - dx,
            _dragStartScroll.Y - dy)

        Invalidate()

    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)

        MyBase.OnMouseUp(e)

        If e.Button = MouseButtons.Left Then

            _isDragging = False
            Cursor = Cursors.Default

        End If

    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)

        MyBase.OnMouseLeave(e)

        If _isDragging Then

            _isDragging = False
            Cursor = Cursors.Default

        End If

    End Sub
End Class
