Public Class VerifyBar
    Inherits Panel

    Private ReadOnly _verifiedButton As New Button()
    Private ReadOnly _fieldBoxes As New List(Of VerifyFieldBox)
    Private ReadOnly _arrangeButton As New Button()
    Private _arrangeMode As Boolean
    Public Event VerifiedClicked As EventHandler
    Public Event FieldFocused(field As GridField)
    Public Event ArrangeModeChanged(active As Boolean)
    Private ReadOnly _verifyToolTip As New ToolTip()
    Public Sub New()

        DoubleBuffered = True
        ResizeRedraw = True

        Dock = DockStyle.Bottom
        Height = 52

        Visible = False

        ThemeManager.ApplyInformationPanel(Me)

        _verifiedButton.Text = "✓ VERIFIED"
        _verifiedButton.Width = 110
        _verifiedButton.Height = 30
        _verifiedButton.Top = 10

        _arrangeButton.Text = "Arrange"
        _arrangeButton.Width = 80
        _arrangeButton.Height = 30
        _arrangeButton.Top = 10

        ThemeManager.ApplyLozengeButton(_arrangeButton)

        AddHandler _arrangeButton.Click, AddressOf ArrangeButton_Click

        Controls.Add(_arrangeButton)
        _verifyToolTip.SetToolTip(_arrangeButton, "Click Arrange to move or resize the Verify Bar fields.")
        _verifyToolTip.SetToolTip(Me, "Click Arrange to move or resize the Verify Bar fields.")

        ThemeManager.ApplyLozengeButton(_verifiedButton)

        AddHandler _verifiedButton.Click,
            Sub()
                RaiseEvent VerifiedClicked(Me, EventArgs.Empty)
            End Sub
        AddHandler _verifiedButton.KeyDown, AddressOf VerifiedButton_KeyDown

        Controls.Add(_verifiedButton)

    End Sub
    Public Sub CancelArrangeMode()

        If Not _arrangeMode Then Return

        _arrangeMode = False

        For Each box As VerifyFieldBox In _fieldBoxes
            box.ArrangeMode = False
        Next

        _verifiedButton.Enabled = True
        ThemeManager.ApplyLozengeButton(_arrangeButton)

        RaiseEvent ArrangeModeChanged(False)

    End Sub
    Private Sub VerifiedButton_KeyDown(sender As Object, e As KeyEventArgs)

        If e.KeyCode = Keys.Tab Then

            e.Handled = True
            e.SuppressKeyPress = True

            System.Media.SystemSounds.Beep.Play()

            Return

        End If

        If e.KeyCode = Keys.Enter Then

            e.Handled = True
            e.SuppressKeyPress = True

            RaiseEvent VerifiedClicked(Me, EventArgs.Empty)

        End If

    End Sub
    Private Sub ArrangeButton_Click(sender As Object, e As EventArgs)

        _arrangeMode = Not _arrangeMode

        For Each box As VerifyFieldBox In _fieldBoxes
            box.ArrangeMode = _arrangeMode
        Next

        _verifiedButton.Enabled = Not _arrangeMode

        If _arrangeMode Then

            _arrangeButton.BackColor = Color.Orange
            _arrangeButton.ForeColor = Color.Black

        Else

            ThemeManager.ApplyLozengeButton(_arrangeButton)

        End If
        RaiseEvent ArrangeModeChanged(_arrangeMode)
    End Sub
    Public Sub ConfigureFields(fields As IEnumerable(Of GridField))

        For Each box As VerifyFieldBox In _fieldBoxes
            Controls.Remove(box)
            box.Dispose()
        Next

        _fieldBoxes.Clear()

        Dim left As Integer = 20
        Dim layoutKey As String = GetLayoutKey()

        Dim savedLayout As Dictionary(Of String, VerifyFieldLayoutData) = Nothing

        ProjectValues.VerifyFieldLayouts.TryGetValue(layoutKey, savedLayout)

        Dim verifyFont As Font

        If String.IsNullOrWhiteSpace(ProjectValues.VerifyFontName) Then

            verifyFont = UiFonts.Normal

        Else

            verifyFont = New Font(
                ProjectValues.VerifyFontName,
                ProjectValues.VerifyFontSize,
                ProjectValues.VerifyFontStyle)

        End If
        Dim verifyBoxHeight As Integer = Math.Max(30, verifyFont.Height + 10)
        Height = verifyBoxHeight + 20

        For Each field As GridField In fields

            Dim box As New VerifyFieldBox With {
                .Field = field,
                .Left = left,
                .Top = 10,
                .Width = 120,
                .Height = verifyBoxHeight,
                .ArrangeMode = _arrangeMode,
                .Font = verifyFont
            }

            If savedLayout IsNot Nothing Then

                Dim savedField As VerifyFieldLayoutData = Nothing

                If savedLayout.TryGetValue(field.ToString(), savedField) Then

                    box.Left = savedField.Left
                    box.Width = savedField.Width
                    box.TextAlignment = savedField.TextAlignment

                End If

            End If

            AddHandler box.LayoutChanged,
                Sub()
                    PositionVerifiedButton()
                End Sub

            AddHandler box.TabPressed, AddressOf VerifyBox_TabPressed
            AddHandler box.FieldFocused, AddressOf VerifyBox_FieldFocused

            AddHandler box.LayoutFinished,
                Sub()
                    SaveLayout()
                End Sub

            _fieldBoxes.Add(box)
            Controls.Add(box)

            left = box.Right + 10

        Next

        PositionVerifiedButton()
        RefreshFont()

    End Sub
    Public Sub RefreshFont()

        Dim verifyFont As Font
        Dim verifyColour As Color

        If String.IsNullOrWhiteSpace(ProjectValues.VerifyFontName) Then

            verifyFont = New Font(ProjectValues.UiFontName, ProjectValues.UiFontSize, ProjectValues.UiFontStyle)
            verifyColour = Color.FromArgb(ProjectValues.UiFontColourArgb)

        Else

            verifyFont = New Font(ProjectValues.VerifyFontName, ProjectValues.VerifyFontSize, ProjectValues.VerifyFontStyle)
            verifyColour = Color.FromArgb(ProjectValues.VerifyFontColor)

        End If

        Dim verifyBoxHeight As Integer = Math.Max(30, verifyFont.Height + 10)

        For Each box As VerifyFieldBox In _fieldBoxes

            box.Font = verifyFont
            box.ForeColor = verifyColour
            box.Height = verifyBoxHeight

        Next

        Height = verifyBoxHeight + 20

    End Sub
    Private Sub VerifyBox_TabPressed(sender As Object, e As EventArgs)

        Dim box As VerifyFieldBox = DirectCast(sender, VerifyFieldBox)
        Dim index As Integer = _fieldBoxes.IndexOf(box)

        If index < 0 Then
            Return
        End If

        If index < _fieldBoxes.Count - 1 Then
            _fieldBoxes(index + 1).FocusEditor()
        Else
            _verifiedButton.Focus()
        End If

    End Sub
    Private Sub VerifyBox_FieldFocused(sender As Object, e As EventArgs)

        Dim box As VerifyFieldBox = DirectCast(sender, VerifyFieldBox)

        RaiseEvent FieldFocused(box.Field)

    End Sub
    Private Function GetLayoutKey() As String

        Return ProjectValues.BatchType & "|" &
           ProjectValues.Year.ToString() & "|" &
           ProjectValues.Quarter.ToString()

    End Function
    Private Sub VerifyFieldBox_LayoutChanged(sender As Object, e As EventArgs)

        PositionVerifiedButton()
        SaveLayout()

    End Sub
    Private Sub SaveLayout()

        Dim layoutKey As String = GetLayoutKey()

        Dim layout As Dictionary(Of String, VerifyFieldLayoutData) = Nothing

        If Not ProjectValues.VerifyFieldLayouts.TryGetValue(layoutKey, layout) Then

            layout = New Dictionary(Of String, VerifyFieldLayoutData)

            ProjectValues.VerifyFieldLayouts(layoutKey) =
            layout

        End If

        For Each box As VerifyFieldBox In _fieldBoxes

            layout(box.Field.ToString()) =
            New VerifyFieldLayoutData With {
                .Left = box.Left,
                .Width = box.Width,
                .TextAlignment = box.TextAlignment
            }

        Next

        ProjectValuesStore.Save()

    End Sub
    Private Sub PositionVerifiedButton()

        Dim rightEdge As Integer = 0

        For Each box As VerifyFieldBox In _fieldBoxes
            rightEdge = Math.Max(rightEdge, box.Right)
        Next

        _verifiedButton.Left = rightEdge + 20
        _arrangeButton.Left = _verifiedButton.Right + 10

    End Sub
    Public Sub LoadValues(values As Dictionary(Of GridField, String), previousSurname As String)

        For Each box As VerifyFieldBox In _fieldBoxes

            Dim value As String = ""

            If values.TryGetValue(box.Field, value) Then
                box.Value = value
            Else
                box.Value = ""
            End If

            If box.Field = GridField.Surname Then
                box.PreviousSurname = previousSurname
            End If

            box.ArrangeMode = False

        Next

    End Sub
    Public Function GetValues() As Dictionary(Of GridField, String)

        Dim values As New Dictionary(Of GridField, String)

        For Each box As VerifyFieldBox In _fieldBoxes
            values(box.Field) = box.Value
        Next

        Return values

    End Function
    Public Sub FocusFirstBox()

        _verifiedButton.Focus()

    End Sub
End Class
