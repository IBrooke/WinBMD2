Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class ToggleSwitch
    Inherits Control

    Private _checked As Boolean

    <DefaultValue(False)>
    Public Property Checked As Boolean
        Get
            Return _checked
        End Get
        Set(value As Boolean)

            If _checked = value Then
                Return
            End If

            _checked = value
            Invalidate()
            RaiseEvent CheckedChanged(Me, EventArgs.Empty)

        End Set
    End Property

    Public Event CheckedChanged As EventHandler

    Public Sub New()

        DoubleBuffered = True
        Cursor = Cursors.Hand
        Size = New Size(48, 24)

    End Sub

    Protected Overrides Sub OnClick(e As EventArgs)

        Checked = Not Checked

        MyBase.OnClick(e)

    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        MyBase.OnPaint(e)

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim track As Rectangle =
            New Rectangle(0, 0, Width - 1, Height - 1)

        Using trackPath As GraphicsPath =
            CreateRoundedRectangle(track, Height \ 2)

            Using brush As New SolidBrush(
    If(
        Checked,
        UiColors.ToggleOn,
        UiColors.ToggleOff))

                e.Graphics.FillPath(brush, trackPath)

            End Using

        End Using

        Dim knobSize As Integer =
            Height - 6

        Dim knobLeft As Integer =
            If(
                Checked,
                Width - knobSize - 3,
                3)

        Dim knob As New Rectangle(
            knobLeft,
            3,
            knobSize,
            knobSize)

        Using brush As New SolidBrush(Color.White)
            e.Graphics.FillEllipse(brush, knob)
        End Using

    End Sub

    Private Shared Function CreateRoundedRectangle(
        rectangle As Rectangle,
        radius As Integer) As GraphicsPath

        Dim diameter As Integer =
            radius * 2

        Dim path As New GraphicsPath()

        path.AddArc(
            rectangle.Left,
            rectangle.Top,
            diameter,
            diameter,
            180,
            90)

        path.AddArc(
            rectangle.Right - diameter,
            rectangle.Top,
            diameter,
            diameter,
            270,
            90)

        path.AddArc(
            rectangle.Right - diameter,
            rectangle.Bottom - diameter,
            diameter,
            diameter,
            0,
            90)

        path.AddArc(
            rectangle.Left,
            rectangle.Bottom - diameter,
            diameter,
            diameter,
            90,
            90)

        path.CloseFigure()

        Return path

    End Function

End Class