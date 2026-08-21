Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class CollapsiblePanel
    Inherits Panel

    Private Const DefaultHeaderHeight As Integer = 20

    Private _headerText As String = "Panel"
    Private _headerHeight As Integer = DefaultHeaderHeight
    Private _headerHovered As Boolean
    Private _expanded As Boolean = True
    Private _expandedHeight As Integer
    Public Event ExpandedChanged As EventHandler

    <Category("Appearance")>
    <DefaultValue("Panel")>
    Public Property HeaderText As String
        Get
            Return _headerText
        End Get
        Set(value As String)
            If _headerText = value Then
                Return
            End If

            _headerText = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    <DefaultValue(DefaultHeaderHeight)>
    Public Property HeaderHeight As Integer
        Get
            Return _headerHeight
        End Get
        Set(value As Integer)
            Dim newValue As Integer = Math.Max(18, value)

            If _headerHeight = newValue Then
                Return
            End If

            _headerHeight = newValue

            Padding = New Padding(1, _headerHeight, 1, 1)

            If Not _expanded Then
                Height = _headerHeight
            End If

            Invalidate()
            PerformLayout()
        End Set
    End Property

    <Category("Behavior")>
    <DefaultValue(True)>
    Public Property Expanded As Boolean
        Get
            Return _expanded
        End Get
        Set(value As Boolean)

            If _expanded = value Then
                Return
            End If

            _expanded = value

            If _expanded Then
                Height = Math.Max(_expandedHeight, _headerHeight + 1)

                For Each control As Control In Controls
                    control.Visible = True
                Next
            Else
                _expandedHeight = Height
                Height = _headerHeight

                For Each control As Control In Controls
                    control.Visible = False
                Next
            End If

            Invalidate()
            RaiseEvent ExpandedChanged(Me, EventArgs.Empty)
        End Set
    End Property

    Public Sub New()
        DoubleBuffered = True
        ResizeRedraw = True

        Padding = New Padding(1, _headerHeight, 1, 1)
        MinimumSize = New Size(0, _headerHeight)

        _expandedHeight = Height
    End Sub
    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)

        If _expanded Then
            _expandedHeight = Height
        End If

    End Sub
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        If Width < 2 OrElse Height < 2 Then
            Return
        End If

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        DrawBorder(e.Graphics)
        DrawHeader(e.Graphics)
        DrawExpandArrow(e.Graphics)
        DrawHeaderText(e.Graphics)
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        Dim isOverHeader As Boolean =
            e.Y >= 0 AndAlso e.Y < _headerHeight

        If _headerHovered <> isOverHeader Then
            _headerHovered = isOverHeader
            InvalidateHeader()
        End If

        Cursor =
            If(isOverHeader,
               Cursors.Hand,
               Cursors.Default)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        MyBase.OnMouseLeave(e)

        If _headerHovered Then
            _headerHovered = False
            InvalidateHeader()
        End If

        Cursor = Cursors.Default
    End Sub

    Protected Overrides Sub OnMouseClick(e As MouseEventArgs)
        MyBase.OnMouseClick(e)

        If e.Button <> MouseButtons.Left Then
            Return
        End If

        If e.Y < 0 OrElse e.Y >= _headerHeight Then
            Return
        End If

        Expanded = Not Expanded
    End Sub

    Private Sub InvalidateHeader()
        Invalidate(
            New Rectangle(
                x:=0,
                y:=0,
                width:=Width,
                height:=_headerHeight))
    End Sub

    Private Sub DrawBorder(graphics As Graphics)
        Using borderPen As New Pen(ThemeManager.PanelBorderColour)
            graphics.DrawLine(borderPen, 0, 0, Width - 1, 0)
            graphics.DrawLine(borderPen, 0, 0, 0, Height - 1)
            graphics.DrawLine(borderPen, Width - 1, 0, Width - 1, Height - 1)

            If _expanded Then
                graphics.DrawLine(borderPen, 0, Height - 1, Width - 1, Height - 1)
            End If
        End Using
    End Sub

    Private Sub DrawHeader(graphics As Graphics)

        Dim headerRectangle As New Rectangle(
        x:=1,
        y:=1,
        width:=Math.Max(0, ClientSize.Width - 2),
        height:=Math.Max(0, _headerHeight - 3))

        Dim backgroundColor As Color =
    If(
        _headerHovered,
        ThemeManager.CollapsibleHeaderHoverColour,
        ThemeManager.CollapsibleHeaderColour)


        Using headerBrush As New SolidBrush(backgroundColor)
            graphics.FillRectangle(headerBrush, headerRectangle)
        End Using

        ' Draw a 1-pixel accent line along the bottom of the header.
        Using accentPen As New Pen(ThemeManager.CollapsibleHeaderAccentColour)
            graphics.DrawLine(
            accentPen,
            1,
            _headerHeight - 2,
            ClientSize.Width - 2,
            _headerHeight - 2)
        End Using

    End Sub

    Private Sub DrawExpandArrow(graphics As Graphics)
        Dim centreX As Integer = 14
        Dim centreY As Integer = _headerHeight \ 2
        Dim points() As Point

        If _expanded Then
            points = {
                New Point(centreX - 4, centreY - 2),
                New Point(centreX + 4, centreY - 2),
                New Point(centreX, centreY + 3)
            }
        Else
            points = {
                New Point(centreX - 2, centreY - 4),
                New Point(centreX - 2, centreY + 4),
                New Point(centreX + 3, centreY)
            }
        End If

        Using arrowBrush As New SolidBrush(ThemeManager.CollapsibleHeaderTextColour)
            graphics.FillPolygon(
                arrowBrush,
                points)
        End Using
    End Sub

    Private Sub DrawHeaderText(graphics As Graphics)
        Dim textRectangle As New Rectangle(26, 0, Math.Max(0, Width - 36), _headerHeight)

        TextRenderer.DrawText(
        graphics,
        _headerText,
        Theme.HeaderFont,
        textRectangle,
        ThemeManager.CollapsibleHeaderTextColour,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.EndEllipsis)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class