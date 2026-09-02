Public Class PickListPopup
    Implements IDisposable

    Public Event ItemClicked As EventHandler
    Private ReadOnly _listBox As ListBox
    Private ReadOnly _host As ToolStripControlHost
    Private ReadOnly _dropDown As ToolStripDropDown

    Private _disposed As Boolean

    Public Sub New()

        _listBox = New ListBox With {
            .BorderStyle = BorderStyle.FixedSingle,
            .Font = UiFonts.Normal,
            .IntegralHeight = False,
            .DrawMode = DrawMode.OwnerDrawFixed,
            .ItemHeight = 24
        }
        AddHandler _listBox.DrawItem, AddressOf ListBox_DrawItem
        AddHandler _listBox.MouseClick, AddressOf ListBox_MouseClick

        _host = New ToolStripControlHost(_listBox) With {
            .AutoSize = False,
            .Margin = Padding.Empty,
            .Padding = Padding.Empty
        }

        _dropDown = New ToolStripDropDown With {
            .AutoClose = False,
            .Margin = Padding.Empty,
            .Padding = Padding.Empty
        }

        _dropDown.Items.Add(_host)

    End Sub
    Private Sub ListBox_MouseClick(sender As Object, e As MouseEventArgs)

        Dim index As Integer = _listBox.IndexFromPoint(e.Location)

        If index < 0 OrElse index >= _listBox.Items.Count Then
            Return
        End If

        _listBox.SelectedIndex = index
        RaiseEvent ItemClicked(Me, EventArgs.Empty)

    End Sub
    Private Sub ListBox_DrawItem(
    sender As Object,
    e As DrawItemEventArgs)

        If e.Index < 0 OrElse
       e.Index >= _listBox.Items.Count Then

            Return
        End If

        Dim item As PickListItem =
        TryCast(_listBox.Items(e.Index), PickListItem)

        If item Is Nothing Then
            Return
        End If

        Dim selected As Boolean =
        (e.State And DrawItemState.Selected) =
        DrawItemState.Selected

        Dim backgroundColor As Color =
        If(
            selected,
            UiColors.Selected,
            UiColors.PanelBackground)

        Using backgroundBrush As New SolidBrush(backgroundColor)
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds)
        End Using

        Dim numberRectangle As New Rectangle(
        e.Bounds.Left + 4,
        e.Bounds.Top,
        24,
        e.Bounds.Height)

        TextRenderer.DrawText(
        e.Graphics,
        (e.Index + 1).ToString(),
        _listBox.Font,
        numberRectangle,
        UiColors.TextSecondary,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.NoPrefix)

        Dim textLeft As Integer =
    numberRectangle.Right + 2

        If String.IsNullOrWhiteSpace(item.Volume) Then

            Dim textRectangle As New Rectangle(
        textLeft,
        e.Bounds.Top,
        Math.Max(
            0,
            e.Bounds.Right - textLeft - 4),
        e.Bounds.Height)

            TextRenderer.DrawText(
        e.Graphics,
        item.Text,
        _listBox.Font,
        textRectangle,
        UiColors.TextPrimary,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.EndEllipsis Or
        TextFormatFlags.NoPrefix)

        Else

            Dim secondaryWidth As Integer =
        Math.Max(
            44,
            TextRenderer.MeasureText(
                item.Volume,
                _listBox.Font,
                Size.Empty,
                TextFormatFlags.NoPadding).Width + 12)

            Dim secondaryRectangle As New Rectangle(
        e.Bounds.Right - secondaryWidth - 4,
        e.Bounds.Top,
        secondaryWidth,
        e.Bounds.Height)

            Dim textRectangle As New Rectangle(
        textLeft,
        e.Bounds.Top,
        Math.Max(
            0,
            secondaryRectangle.Left - textLeft - 8),
        e.Bounds.Height)

            TextRenderer.DrawText(
        e.Graphics,
        item.Text,
        _listBox.Font,
        textRectangle,
        UiColors.TextPrimary,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.EndEllipsis Or
        TextFormatFlags.NoPrefix)

            TextRenderer.DrawText(
        e.Graphics,
        item.Volume,
        _listBox.Font,
        secondaryRectangle,
        UiColors.TextSecondary,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.NoPrefix)

        End If

        If selected Then

            Using selectionPen As New Pen(UiColors.AccentBorder)

                Dim selectionRectangle As New Rectangle(
                e.Bounds.Left,
                e.Bounds.Top,
                Math.Max(0, e.Bounds.Width - 1),
                Math.Max(0, e.Bounds.Height - 1))

                e.Graphics.DrawRectangle(
                selectionPen,
                selectionRectangle)

            End Using

        End If
        e.DrawFocusRectangle()
    End Sub
    Public ReadOnly Property IsOpen As Boolean
        Get
            Return _dropDown.Visible
        End Get
    End Property

    Public ReadOnly Property SelectedItem As PickListItem
        Get
            Return TryCast(_listBox.SelectedItem, PickListItem)
        End Get
    End Property
    Public ReadOnly Property ItemCount As Integer
        Get
            Return _listBox.Items.Count
        End Get
    End Property

    Public Sub MoveSelectionUp()

        If _listBox.Items.Count = 0 Then
            Return
        End If

        If _listBox.SelectedIndex > 0 Then
            _listBox.SelectedIndex -= 1
        End If

    End Sub

    Public Sub MoveSelectionDown()

        If _listBox.Items.Count = 0 Then
            Return
        End If

        If _listBox.SelectedIndex < 0 Then
            _listBox.SelectedIndex = 0
        ElseIf _listBox.SelectedIndex < _listBox.Items.Count - 1 Then
            _listBox.SelectedIndex += 1
        End If

    End Sub

    Public Function SelectItemByNumber(number As Integer) As Boolean

        Dim index As Integer = number - 1

        If index < 0 OrElse index >= _listBox.Items.Count Then
            Return False
        End If

        _listBox.SelectedIndex = index

        Return True

    End Function
    Public Sub SetItems(items As IEnumerable(Of PickListItem))

        _listBox.BeginUpdate()

        Try
            _listBox.Items.Clear()

            If items IsNot Nothing Then
                For Each item As PickListItem In items
                    _listBox.Items.Add(item)
                Next
            End If

            If _listBox.Items.Count > 0 Then
                _listBox.SelectedIndex = 0
            Else
                _listBox.SelectedIndex = -1
            End If

        Finally
            _listBox.EndUpdate()
        End Try

    End Sub

    Public Sub Show(owner As Control, cellBounds As Rectangle)

        If owner Is Nothing Then
            Throw New ArgumentNullException(NameOf(owner))
        End If

        If _listBox.Items.Count = 0 Then
            Hide()
            Return
        End If

        Dim width As Integer = Math.Max(180, cellBounds.Width + 40)
        Dim visibleItemCount As Integer = Math.Min(9, _listBox.Items.Count)
        Dim height As Integer = Math.Max(24, visibleItemCount * _listBox.ItemHeight + 4)

        _listBox.Size = New Size(width, height)
        _host.Size = _listBox.Size

        Dim screenBelow As Point = owner.PointToScreen(New Point(cellBounds.Left, cellBounds.Bottom))
        Dim screenAbove As Point = owner.PointToScreen(New Point(cellBounds.Left, cellBounds.Top - height))
        Dim workingArea As Rectangle = Screen.FromControl(owner).WorkingArea

        Dim screenLocation As Point

        If screenBelow.Y + height <= workingArea.Bottom Then
            screenLocation = screenBelow
        ElseIf screenAbove.Y >= workingArea.Top Then
            screenLocation = screenAbove
        Else
            screenLocation = New Point(screenBelow.X, workingArea.Bottom - height)
        End If

        If screenLocation.X + width > workingArea.Right Then
            screenLocation.X = workingArea.Right - width
        End If

        If screenLocation.X < workingArea.Left Then
            screenLocation.X = workingArea.Left
        End If

        Dim ownerLocation As Point = owner.PointToClient(screenLocation)

        _dropDown.Show(owner, ownerLocation)

    End Sub

    Public Sub Hide()

        If _dropDown.Visible Then
            _dropDown.Close(ToolStripDropDownCloseReason.CloseCalled)
        End If

    End Sub

    Public Sub ApplyTheme()

        _listBox.Font = UiFonts.Normal
        _listBox.BackColor = UiColors.PanelBackground
        _listBox.ForeColor = UiColors.TextPrimary
        _listBox.Invalidate()

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        If _disposed Then
            Return
        End If

        _dropDown.Dispose()
        _host.Dispose()
        _listBox.Dispose()

        _disposed = True

        GC.SuppressFinalize(Me)

    End Sub

End Class
