Imports System.ComponentModel
Public Class VerifyFieldBox
    Inherits UserControl

    Private ReadOnly _textBox As New TextBox()
    Private _field As GridField
    Private _arrangeMode As Boolean

    Public Event LayoutChanged As EventHandler
    Public Event LayoutFinished As EventHandler
    Public Event TabPressed As EventHandler
    Private _dragging As Boolean
    Private _resizing As Boolean
    Private _dragStartMouseScreenX As Integer
    Private _dragStartLeft As Integer
    Private _resizeStartWidth As Integer

    Private Const ResizeGripWidth As Integer = 6

    Private Function GetMinimumBoxWidth() As Integer

        Select Case _field

            Case GridField.Volume,
             GridField.DistNum

                Return 35

            Case Else

                Return 50

        End Select

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
            Return _textBox.Text
        End Get
        Set(value As String)
            _textBox.Text = If(value, "")
        End Set
    End Property

    Public Sub New()

        Height = 30
        Width = 120

        _textBox.Dock = DockStyle.Fill
        _textBox.BorderStyle = BorderStyle.FixedSingle

        Controls.Add(_textBox)

        AddHandler _textBox.MouseDown, AddressOf TextBox_MouseDown
        AddHandler _textBox.MouseMove, AddressOf TextBox_MouseMove
        AddHandler _textBox.MouseUp, AddressOf TextBox_MouseUp
    End Sub
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean

        If keyData = Keys.Tab Then
            RaiseEvent TabPressed(Me, EventArgs.Empty)
            Return True
        End If

        Return MyBase.ProcessCmdKey(msg, keyData)

    End Function
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)

        MyBase.OnMouseDown(e)

        If Not _arrangeMode OrElse e.Button <> MouseButtons.Left Then
            Return
        End If

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
    Private Sub TextBox_MouseDown(
    sender As Object,
    e As MouseEventArgs)

        If Not _arrangeMode Then
            Return
        End If

        Dim point As Point =
        PointToClient(
            _textBox.PointToScreen(e.Location))

        OnMouseDown(
        New MouseEventArgs(
            e.Button,
            e.Clicks,
            point.X,
            point.Y,
            e.Delta))

    End Sub
    Private Sub TextBox_MouseUp(
    sender As Object,
    e As MouseEventArgs)

        If Not _arrangeMode Then
            Return
        End If

        Dim point As Point =
        PointToClient(
            _textBox.PointToScreen(e.Location))

        OnMouseUp(
        New MouseEventArgs(
            e.Button,
            e.Clicks,
            point.X,
            point.Y,
            e.Delta))

    End Sub
    Private Sub TextBox_MouseMove(
    sender As Object,
    e As MouseEventArgs)

        If Not _arrangeMode Then
            Return
        End If

        Dim point As Point =
        PointToClient(
            _textBox.PointToScreen(e.Location))

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

        Dim deltaX As Integer =
        MousePosition.X - _dragStartMouseScreenX

        If _resizing Then

            Width = Math.Max(GetMinimumBoxWidth(), _resizeStartWidth + deltaX)

            RaiseEvent LayoutChanged(Me, EventArgs.Empty)
            Return

        End If

        If _dragging Then

            Left = Math.Max(
            0,
            _dragStartLeft + deltaX)

            RaiseEvent LayoutChanged(Me, EventArgs.Empty)
            Return

        End If

        Cursor =
        If(
            e.X >= Width - ResizeGripWidth,
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

        _textBox.Focus()
        _textBox.SelectAll()

    End Sub
End Class