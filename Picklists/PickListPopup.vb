Public Class PickListPopup
    Implements IDisposable

    Public Event ItemClicked As EventHandler

    Private ReadOnly _listBox As ListBox
    Private ReadOnly _floatingForm As Form

    Private _disposed As Boolean
    Private _fixedOwner As Control
    Private _fixedCellBounds As Rectangle

    Public Sub New()

        _listBox = New ListBox With {
            .Dock = DockStyle.Fill,
            .BorderStyle = BorderStyle.None,
            .Font = UiFonts.Normal,
            .IntegralHeight = False,
            .DrawMode = DrawMode.OwnerDrawFixed,
            .ItemHeight = 24
        }

        AddHandler _listBox.DrawItem, AddressOf ListBox_DrawItem
        AddHandler _listBox.MouseClick, AddressOf ListBox_MouseClick

        _floatingForm = New FloatingPickListForm With {
            .Text = "Picklist",
            .Icon = My.Resources.WinBMDResources.WinBMD2Icon,
            .FormBorderStyle = FormBorderStyle.Sizable,
            .StartPosition = FormStartPosition.Manual,
            .ShowInTaskbar = False,
            .MinimizeBox = False,
            .MaximizeBox = False,
            .MinimumSize = New Size(180, 120),
            .Size = New Size(300, 250)
        }

        _floatingForm.Controls.Add(_listBox)

        AddHandler _floatingForm.Move, AddressOf FloatingForm_BoundsChanged
        AddHandler _floatingForm.Resize, AddressOf FloatingForm_BoundsChanged
        AddHandler _floatingForm.FormClosing, AddressOf FloatingForm_FormClosing
        AddHandler _floatingForm.Shown, AddressOf FloatingForm_Shown
    End Sub

    Private Sub FloatingForm_FormClosing(sender As Object, e As FormClosingEventArgs)

        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            _floatingForm.Hide()
        End If

    End Sub
    Private Sub FloatingForm_Shown(sender As Object, e As EventArgs)

        If _floatingForm.Owner Is Nothing Then
            Return
        End If

        AddHandler _floatingForm.Owner.Move, AddressOf OwnerForm_Move

    End Sub

    Private Sub OwnerForm_Move(sender As Object, e As EventArgs)

        If ProjectValues.FloatingPickList OrElse Not _floatingForm.Visible OrElse _fixedOwner Is Nothing Then
            Return
        End If

        Dim height As Integer = _floatingForm.Height
        Dim width As Integer = _floatingForm.Width
        Dim screenBelow As Point = _fixedOwner.PointToScreen(New Point(_fixedCellBounds.Left, _fixedCellBounds.Bottom))
        Dim screenAbove As Point = _fixedOwner.PointToScreen(New Point(_fixedCellBounds.Left, _fixedCellBounds.Top - height))
        Dim workingArea As Rectangle = Screen.FromControl(_fixedOwner).WorkingArea
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

        _floatingForm.Location = screenLocation

    End Sub
    Public Sub ClearSelection()

        _listBox.SelectedIndex = -1

    End Sub

    Private Sub ListBox_MouseClick(sender As Object, e As MouseEventArgs)

        Dim listBox As ListBox = DirectCast(sender, ListBox)
        Dim index As Integer = listBox.IndexFromPoint(e.Location)

        If index < 0 OrElse index >= listBox.Items.Count Then
            Return
        End If

        listBox.SelectedIndex = index
        RaiseEvent ItemClicked(Me, EventArgs.Empty)

    End Sub

    Private Sub ListBox_DrawItem(sender As Object, e As DrawItemEventArgs)

        Dim listBox As ListBox = DirectCast(sender, ListBox)

        If e.Index < 0 OrElse e.Index >= listBox.Items.Count Then
            Return
        End If

        Dim item As PickListItem = TryCast(listBox.Items(e.Index), PickListItem)

        If item Is Nothing Then
            Return
        End If

        Dim selected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected
        Dim backgroundColor As Color = If(selected, UiColors.Selected, UiColors.PanelBackground)

        Using backgroundBrush As New SolidBrush(backgroundColor)
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds)
        End Using

        Dim numberRectangle As New Rectangle(e.Bounds.Left + 4, e.Bounds.Top, 24, e.Bounds.Height)

        TextRenderer.DrawText(e.Graphics, (e.Index + 1).ToString(), listBox.Font, numberRectangle, UiColors.TextSecondary, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)

        Dim textLeft As Integer = numberRectangle.Right + 2

        If String.IsNullOrWhiteSpace(item.Volume) Then

            Dim textRectangle As New Rectangle(textLeft, e.Bounds.Top, Math.Max(0, e.Bounds.Right - textLeft - 4), e.Bounds.Height)

            TextRenderer.DrawText(e.Graphics, item.Text, listBox.Font, textRectangle, listBox.ForeColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)

        Else

            Dim secondaryWidth As Integer = Math.Max(44, TextRenderer.MeasureText(item.Volume, listBox.Font, Size.Empty, TextFormatFlags.NoPadding).Width + 12)
            Dim secondaryRectangle As New Rectangle(e.Bounds.Right - secondaryWidth - 4, e.Bounds.Top, secondaryWidth, e.Bounds.Height)
            Dim textRectangle As New Rectangle(textLeft, e.Bounds.Top, Math.Max(0, secondaryRectangle.Left - textLeft - 8), e.Bounds.Height)

            TextRenderer.DrawText(e.Graphics, item.Text, listBox.Font, textRectangle, listBox.ForeColor, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            TextRenderer.DrawText(e.Graphics, item.Volume, listBox.Font, secondaryRectangle, UiColors.TextSecondary, TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.NoPrefix)

        End If

        If selected Then

            Using selectionPen As New Pen(UiColors.AccentBorder)

                Dim selectionRectangle As New Rectangle(e.Bounds.Left, e.Bounds.Top, Math.Max(0, e.Bounds.Width - 1), Math.Max(0, e.Bounds.Height - 1))

                e.Graphics.DrawRectangle(selectionPen, selectionRectangle)

            End Using

        End If

        e.DrawFocusRectangle()

    End Sub

    Public ReadOnly Property IsOpen As Boolean
        Get
            Return _floatingForm.Visible
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
        DebugLog.WriteAlways($"[PICKLIST] SetItems complete. Count={_listBox.Items.Count}, SelectedIndex={_listBox.SelectedIndex}")
    End Sub

    Public Sub Show(owner As Control, cellBounds As Rectangle)

        If owner Is Nothing Then
            Throw New ArgumentNullException(NameOf(owner))
        End If

        If _listBox.Items.Count = 0 Then
            Hide()
            Return
        End If

        If ProjectValues.FloatingPickList Then

            _floatingForm.FormBorderStyle = FormBorderStyle.Sizable
            _floatingForm.MinimumSize = New Size(180, 120)

            If Not _floatingForm.Visible Then

                Dim bounds As FormBoundsData = ProjectValues.PickListBounds

                If bounds.Left >= 0 AndAlso bounds.Top >= 0 Then
                    _floatingForm.SetBounds(bounds.Left, bounds.Top, bounds.Width, bounds.Height)
                Else
                    _floatingForm.Size = New Size(bounds.Width, bounds.Height)
                End If

                _floatingForm.Show(owner.FindForm())

            End If

            Return

        End If

        _fixedOwner = owner
        _fixedCellBounds = cellBounds

        _floatingForm.FormBorderStyle = FormBorderStyle.None
        _floatingForm.MinimumSize = Size.Empty

        Dim width As Integer = Math.Max(180, cellBounds.Width + 40)
        Dim visibleItemCount As Integer = Math.Min(9, _listBox.Items.Count)
        Dim height As Integer = Math.Max(24, visibleItemCount * _listBox.ItemHeight)

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

        _floatingForm.SetBounds(screenLocation.X, screenLocation.Y, width, height)

        Dim ownerForm As Form = owner.FindForm()

        If _floatingForm.Owner IsNot ownerForm Then
            _floatingForm.Owner = ownerForm
        End If

        If Not _floatingForm.Visible Then
            _floatingForm.Show(ownerForm)
        End If

    End Sub

    Private Sub FloatingForm_BoundsChanged(sender As Object, e As EventArgs)

        If Not ProjectValues.FloatingPickList Then
            Return
        End If

        If Not _floatingForm.Visible OrElse _floatingForm.WindowState <> FormWindowState.Normal Then
            Return
        End If

        ProjectValues.PickListBounds.Left = _floatingForm.Left
        ProjectValues.PickListBounds.Top = _floatingForm.Top
        ProjectValues.PickListBounds.Width = _floatingForm.Width
        ProjectValues.PickListBounds.Height = _floatingForm.Height

        ProjectValuesStore.Save()

    End Sub

    Public Sub Hide()

        If _floatingForm.Visible Then
            _floatingForm.Hide()
        End If

    End Sub

    Public Sub ApplyTheme()

        Dim pickListFont As New Font(ProjectValues.UiFontName, ProjectValues.UiFontSize, ProjectValues.UiFontStyle)
        Dim pickListColour As Color = Color.FromArgb(ProjectValues.UiFontColourArgb)
        Dim itemHeight As Integer = Math.Max(24, CInt(Math.Ceiling(pickListFont.GetHeight())) + 8)

        _listBox.Font = pickListFont
        _listBox.BackColor = UiColors.PanelBackground
        _listBox.ForeColor = pickListColour
        _listBox.ItemHeight = itemHeight
        _listBox.Invalidate()

        _floatingForm.BackColor = UiColors.PanelBackground

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose

        If _disposed Then
            Return
        End If

        _floatingForm.Dispose()

        _disposed = True

        GC.SuppressFinalize(Me)

    End Sub

    Private NotInheritable Class FloatingPickListForm
        Inherits Form

        Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
            Get
                Return True
            End Get
        End Property

    End Class

End Class