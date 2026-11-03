Imports System.ComponentModel
Imports System.Runtime.InteropServices
Public Class VerifyFieldBox
    Inherits UserControl

    Private ReadOnly _textBox As New TextBox()
    Private ReadOnly _alignmentMenu As New ContextMenuStrip()
    Private ReadOnly _alignLeftItem As New ToolStripMenuItem("Left aligned")
    Private ReadOnly _alignRightItem As New ToolStripMenuItem("Right aligned")
    Private _field As GridField
    Private _arrangeMode As Boolean
    Private _actualValue As String = ""
    Private _previousSurname As String = ""

    Public Event LayoutChanged As EventHandler
    Public Event LayoutFinished As EventHandler
    Public Event TabPressed As EventHandler
    Public Event FieldFocused As EventHandler
    Private _dragging As Boolean
    Private _resizing As Boolean
    Private _dragStartMouseScreenX As Integer
    Private _dragStartLeft As Integer
    Private _resizeStartWidth As Integer

    Private Const ResizeGripWidth As Integer = 6
    Private Const EM_SETMARGINS As Integer = &HD3
    Private Const EC_LEFTMARGIN As Integer = &H1
    Private Const EC_RIGHTMARGIN As Integer = &H2

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function
    Private Function GetMinimumBoxWidth() As Integer

        Dim characterWidth As Integer = TextRenderer.MeasureText("W", _textBox.Font, Size.Empty, TextFormatFlags.NoPadding).Width

        Return characterWidth + 4

    End Function
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Field As GridField
        Get
            Return _field
        End Get
        Set(value As GridField)

            _field = value

            _textBox.PlaceholderText =
                FieldMetaData.Meta(value).Header

        End Set
    End Property

    <DefaultValue(False)>
    Public Property ArrangeMode As Boolean
        Get
            Return _arrangeMode
        End Get
        Set(value As Boolean)

            _arrangeMode = value

            Cursor =
            If(value, Cursors.SizeAll, Cursors.Default)

            _textBox.Cursor =
            If(value, Cursors.SizeAll, Cursors.IBeam)

        End Set
    End Property

    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Value As String
        Get
            If _textBox.Focused Then Return _textBox.Text
            Return _actualValue
        End Get
        Set(value As String)
            _actualValue = If(value, "")
            UpdateDisplayedValue()
        End Set
    End Property
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property PreviousSurname As String
        Get
            Return _previousSurname
        End Get
        Set(value As String)
            _previousSurname = If(value, "")
            UpdateDisplayedValue()
        End Set
    End Property
    Private Sub RemoveTextMargins()

        If Not _textBox.IsHandleCreated Then Return

        SendMessage(_textBox.Handle, EM_SETMARGINS, New IntPtr(EC_LEFTMARGIN Or EC_RIGHTMARGIN), IntPtr.Zero)

    End Sub
    Private Sub UpdateDisplayedValue()

        If _textBox.Focused Then Return

        If _field = GridField.Surname AndAlso
       Not String.IsNullOrWhiteSpace(_previousSurname) AndAlso
       String.Equals(_actualValue.Trim(), _previousSurname.Trim(), StringComparison.OrdinalIgnoreCase) Then

            _textBox.Text = "------"

        Else

            _textBox.Text = _actualValue

        End If

    End Sub
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property TextAlignment As HorizontalAlignment
        Get
            Return _textBox.TextAlign
        End Get
        Set(value As HorizontalAlignment)
            _textBox.TextAlign = value
        End Set
    End Property
    Public Sub New()

        Height = 30
        Width = 120

        _textBox.Multiline = True
        _textBox.Dock = DockStyle.Fill
        _textBox.BorderStyle = BorderStyle.FixedSingle

        Controls.Add(_textBox)

        _alignmentMenu.Items.Add(_alignLeftItem)
        _alignmentMenu.Items.Add(_alignRightItem)

        _textBox.ContextMenuStrip = _alignmentMenu
        Me.ContextMenuStrip = _alignmentMenu

        AddHandler _alignmentMenu.Opening, AddressOf AlignmentMenu_Opening
        AddHandler _alignLeftItem.Click, AddressOf AlignLeft_Click
        AddHandler _alignRightItem.Click, AddressOf AlignRight_Click
        AddHandler _textBox.MouseDown, AddressOf TextBox_MouseDown
        AddHandler _textBox.MouseMove, AddressOf TextBox_MouseMove
        AddHandler _textBox.MouseUp, AddressOf TextBox_MouseUp
        AddHandler _textBox.Enter, AddressOf TextBox_Enter
        AddHandler _textBox.Leave, AddressOf TextBox_Leave
        AddHandler _textBox.HandleCreated,
            Sub()
                RemoveTextMargins()
            End Sub
    End Sub
    Private Sub TextBox_Enter(sender As Object, e As EventArgs)

        If _arrangeMode Then Return

        _textBox.Text = _actualValue

        RaiseEvent FieldFocused(Me, EventArgs.Empty)

    End Sub
    Private Sub TextBox_Leave(sender As Object, e As EventArgs)

        If _arrangeMode Then Return

        _actualValue = _textBox.Text
        UpdateDisplayedValue()

    End Sub
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean

        If keyData = Keys.Tab Then
            RaiseEvent TabPressed(Me, EventArgs.Empty)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)

    End Function
    Protected Overrides Sub OnFontChanged(e As EventArgs)

        MyBase.OnFontChanged(e)

        If _textBox IsNot Nothing Then _textBox.Font = Font

    End Sub

    Protected Overrides Sub OnForeColorChanged(e As EventArgs)

        MyBase.OnForeColorChanged(e)

        If _textBox IsNot Nothing Then _textBox.ForeColor = ForeColor

    End Sub
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)

        MyBase.OnMouseDown(e)

        If Not _arrangeMode OrElse e.Button <> MouseButtons.Left Then Return

        _dragStartMouseScreenX = MousePosition.X

        If e.X >= Width - ResizeGripWidth Then
            _resizing = True
            _resizeStartWidth = Width
        Else
            _dragging = True
            _dragStartLeft = Left
        End If

        Capture = True

    End Sub
    Private Sub AlignmentMenu_Opening(sender As Object, e As CancelEventArgs)

        _alignLeftItem.Checked = _textBox.TextAlign = HorizontalAlignment.Left
        _alignRightItem.Checked = _textBox.TextAlign = HorizontalAlignment.Right

    End Sub

    Private Sub AlignLeft_Click(sender As Object, e As EventArgs)

        TextAlignment = HorizontalAlignment.Left
        RaiseEvent LayoutFinished(Me, EventArgs.Empty)

    End Sub

    Private Sub AlignRight_Click(sender As Object, e As EventArgs)

        TextAlignment = HorizontalAlignment.Right
        RaiseEvent LayoutFinished(Me, EventArgs.Empty)

    End Sub
    Private Sub TextBox_MouseDown(sender As Object, e As MouseEventArgs)

        If Not _arrangeMode Then Return

        Dim point As Point = PointToClient(_textBox.PointToScreen(e.Location))

        OnMouseDown(New MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta))

    End Sub

    Private Sub TextBox_MouseUp(sender As Object, e As MouseEventArgs)

        If Not _arrangeMode Then Return

        Dim point As Point = PointToClient(_textBox.PointToScreen(e.Location))

        OnMouseUp(New MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta))

    End Sub
    Private Sub TextBox_MouseMove(sender As Object, e As MouseEventArgs)

        If Not _arrangeMode Then
            Return
        End If

        Dim point As Point = PointToClient(_textBox.PointToScreen(e.Location))

        OnMouseMove(
        New MouseEventArgs(
            e.Button,
            e.Clicks,
            point.X,
            point.Y,
            e.Delta))

    End Sub
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)

        MyBase.OnMouseMove(e)

        If Not _arrangeMode Then
            Return
        End If

        Dim deltaX As Integer = MousePosition.X - _dragStartMouseScreenX

        If _resizing Then

            Width = Math.Max(GetMinimumBoxWidth(), _resizeStartWidth + deltaX)

            RaiseEvent LayoutChanged(Me, EventArgs.Empty)
            Return

        End If

        If _dragging Then

            Left = Math.Max(0, _dragStartLeft + deltaX)

            RaiseEvent LayoutChanged(Me, EventArgs.Empty)
            Return

        End If

        Cursor = If(e.X >= Width - ResizeGripWidth,
            Cursors.SizeWE,
            Cursors.SizeAll)

    End Sub
    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)

        MyBase.OnMouseUp(e)

        If e.Button <> MouseButtons.Left Then
            Return
        End If

        Dim layoutWasChanged As Boolean =
        _dragging OrElse _resizing

        _dragging = False
        _resizing = False
        Capture = False

        If layoutWasChanged Then
            RaiseEvent LayoutFinished(Me, EventArgs.Empty)
        End If

    End Sub
    Public Sub FocusEditor()

        Dim focused As Boolean = _textBox.Focus()
        _textBox.SelectAll()

        DebugLog.WriteAlways($"[VERIFY] FocusEditor. Field={_field}, FocusResult={focused}, TextBoxFocused={_textBox.Focused}, CanFocus={_textBox.CanFocus}, Enabled={_textBox.Enabled}, Visible={_textBox.Visible}")

    End Sub
End Class