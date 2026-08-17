Imports System.ComponentModel
Imports System.Drawing.Drawing2D

Public Class ScanCommandButton
    Inherits Button

    Private Const CornerRadius As Integer = 8
    Private _commandIcon As ScanCommandIcon = ScanCommandIcon.None

    <DefaultValue(ScanCommandIcon.None)>
    Public Property CommandIcon As ScanCommandIcon
        Get
            Return _commandIcon
        End Get
        Set(value As ScanCommandIcon)

            If _commandIcon = value Then
                Return
            End If

            _commandIcon = value

            Select Case value

                Case ScanCommandIcon.ZoomOut
                    Text = "－"

                Case ScanCommandIcon.ZoomIn
                    Text = "＋"

                Case ScanCommandIcon.RotateLeft
                    Text = "⟲"

                Case ScanCommandIcon.RotateRight
                    Text = "⟳"

                Case Else
                    Text = ""

            End Select

            Invalidate()

        End Set
    End Property

    Public Sub New()

        Size = New Size(36, 36)
        FlatStyle = FlatStyle.Flat
        FlatAppearance.BorderSize = 1
        Cursor = Cursors.Hand
        Text = ""
        TextAlign = ContentAlignment.MiddleCenter
        UseVisualStyleBackColor = False
        TabStop = False

        Font = New Font("Segoe UI Symbol", 14.0F, FontStyle.Regular)

        ApplyColours()

    End Sub

    Public Sub ApplyColours()

        BackColor = UiColors.PanelBackground
        ForeColor = UiColors.TextPrimary

        FlatAppearance.BorderColor = UiColors.Border
        FlatAppearance.MouseOverBackColor = UiColors.ThemeSoft
        FlatAppearance.MouseDownBackColor = UiColors.Selected

        Invalidate()

    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)
        UpdateRegion()

    End Sub

    Protected Overrides Sub OnHandleCreated(e As EventArgs)

        MyBase.OnHandleCreated(e)
        UpdateRegion()

    End Sub

    Private Sub UpdateRegion()

        If Width <= 0 OrElse Height <= 0 Then
            Return
        End If

        Using path As GraphicsPath =
            CreateRoundedRectangle(
                New Rectangle(0, 0, Width, Height),
                CornerRadius)

            Region = New Region(path)

        End Using

    End Sub

    Private Shared Function CreateRoundedRectangle(
        bounds As Rectangle,
        radius As Integer) As GraphicsPath

        Dim diameter As Integer = radius * 2
        Dim path As New GraphicsPath()

        path.AddArc(
            bounds.Left,
            bounds.Top,
            diameter,
            diameter,
            180,
            90)

        path.AddArc(
            bounds.Right - diameter,
            bounds.Top,
            diameter,
            diameter,
            270,
            90)

        path.AddArc(
            bounds.Right - diameter,
            bounds.Bottom - diameter,
            diameter,
            diameter,
            0,
            90)

        path.AddArc(
            bounds.Left,
            bounds.Bottom - diameter,
            diameter,
            diameter,
            90,
            90)

        path.CloseFigure()

        Return path

    End Function

End Class
