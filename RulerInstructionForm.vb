Imports System.Drawing.Drawing2D

Public Class RulerInstructionForm
    Inherits Form

    Private ReadOnly _layout As New TableLayoutPanel()
    Private ReadOnly _stepLabel As New Label()
    Private ReadOnly _instructionPicture As New PictureBox()
    Private ReadOnly _messageLabel As New Label()
    Private ReadOnly _hintLabel As New Label()

    Public Sub New()

        FormBorderStyle = FormBorderStyle.SizableToolWindow
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual

        Size = New Size(460, 360)
        MinimumSize = New Size(430, 330)

        BackColor = UiColors.PanelBackground
        DoubleBuffered = True

        _layout.Dock = DockStyle.Fill
        _layout.Padding = New Padding(18)
        _layout.ColumnCount = 1
        _layout.RowCount = 4
        _layout.BackColor = UiColors.PanelBackground

        _layout.ColumnStyles.Add(
            New ColumnStyle(SizeType.Percent, 100.0F))

        _layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 36.0F))

        _layout.RowStyles.Add(
            New RowStyle(SizeType.Percent, 100.0F))

        _layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 62.0F))

        _layout.RowStyles.Add(
            New RowStyle(SizeType.Absolute, 46.0F))

        _stepLabel.Dock = DockStyle.Fill
        _stepLabel.TextAlign = ContentAlignment.MiddleLeft
        _stepLabel.Font = New Font(
            UiFonts.Normal.FontFamily,
            12.0F,
            FontStyle.Bold)

        _stepLabel.ForeColor = UiColors.ThemeSolid

        _instructionPicture.Dock = DockStyle.Fill
        _instructionPicture.SizeMode = PictureBoxSizeMode.Zoom
        _instructionPicture.BorderStyle = BorderStyle.None
        _instructionPicture.BackColor = UiColors.PanelBackground
        _instructionPicture.Margin = New Padding(0, 4, 0, 8)

        _messageLabel.Dock = DockStyle.Fill
        _messageLabel.TextAlign = ContentAlignment.MiddleLeft
        _messageLabel.Font = New Font(
            UiFonts.Normal.FontFamily,
            10.0F,
            FontStyle.Regular)

        _messageLabel.ForeColor = UiColors.TextPrimary

        _hintLabel.Dock = DockStyle.Fill
        _hintLabel.TextAlign = ContentAlignment.MiddleLeft
        _hintLabel.Font = New Font(
            UiFonts.Normal.FontFamily,
            9.0F,
            FontStyle.Regular)

        _hintLabel.ForeColor = UiColors.TextSecondary

        _layout.Controls.Add(_stepLabel, 0, 0)
        _layout.Controls.Add(_instructionPicture, 0, 1)
        _layout.Controls.Add(_messageLabel, 0, 2)
        _layout.Controls.Add(_hintLabel, 0, 3)

        Controls.Add(_layout)

    End Sub

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get

            Dim parameters As CreateParams = MyBase.CreateParams

            Const WS_EX_NOACTIVATE As Integer = &H8000000
            Const WS_EX_TOOLWINDOW As Integer = &H80

            parameters.ExStyle =
                parameters.ExStyle Or
                WS_EX_NOACTIVATE Or
                WS_EX_TOOLWINDOW

            Return parameters

        End Get
    End Property

    Public Sub ShowStep(
        stepText As String,
        message As String,
        hint As String,
        instructionImage As Image)

        _stepLabel.Text = stepText
        _messageLabel.Text = message
        _hintLabel.Text = hint

        _instructionPicture.Image = instructionImage
        _instructionPicture.Visible = instructionImage IsNot Nothing

        ApplyColours()
        Invalidate()

    End Sub

    Public Sub ApplyColours()

        BackColor = UiColors.PanelBackground
        _layout.BackColor = UiColors.PanelBackground

        _stepLabel.ForeColor = UiColors.ThemeSolid
        _messageLabel.ForeColor = UiColors.TextPrimary
        _hintLabel.ForeColor = UiColors.TextSecondary

        _instructionPicture.BackColor =
            UiColors.PanelBackground

        Invalidate()

    End Sub

End Class