Public Class VerifyBar
    Inherits Panel

    Private ReadOnly _verifiedButton As New Button()
    Private ReadOnly _fieldBoxes As New List(Of VerifyFieldBox)
    Private ReadOnly _arrangeButton As New Button()
    Private _arrangeMode As Boolean
    Public Event VerifiedClicked As EventHandler
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

        ThemeManager.ApplyLozengeButton(_verifiedButton)

        AddHandler _verifiedButton.Click,
            Sub()
                RaiseEvent VerifiedClicked(Me, EventArgs.Empty)
            End Sub

        Controls.Add(_verifiedButton)

    End Sub
    Private Sub ArrangeButton_Click(
    sender As Object,
    e As EventArgs)

        _arrangeMode = Not _arrangeMode

        For Each box As VerifyFieldBox In _fieldBoxes
            box.ArrangeMode = _arrangeMode
        Next

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

        ProjectValues.VerifyFieldLayouts.TryGetValue(
        layoutKey,
        savedLayout)

        For Each field As GridField In fields

            Dim box As New VerifyFieldBox With {
            .Field = field,
            .Left = left,
            .Top = 10,
            .Width = 120,
            .ArrangeMode = True,
            .Font = New Font(UiFonts.Normal.FontFamily, ProjectValues.VerifyFontSize)
        }

            If savedLayout IsNot Nothing Then

                Dim savedField As VerifyFieldLayoutData = Nothing

                If savedLayout.TryGetValue(field.ToString(), savedField) Then

                    box.Left = savedField.Left
                    box.Width = savedField.Width

                End If

            End If

            AddHandler box.LayoutChanged,
                Sub()
                    PositionVerifiedButton()
                End Sub

            AddHandler box.LayoutFinished,
                Sub()
                    SaveLayout()
                End Sub

            _fieldBoxes.Add(box)
            Controls.Add(box)

            left = box.Right + 10

        Next

        PositionVerifiedButton()

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
                .Width = box.Width
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
    Public Sub LoadValues(values As Dictionary(Of GridField, String))

        For Each box As VerifyFieldBox In _fieldBoxes

            Dim value As String = ""

            If values.TryGetValue(box.Field, value) Then
                box.Value = value
            Else
                box.Value = ""
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

        If _fieldBoxes.Count = 0 Then
            Return
        End If

        _fieldBoxes(0).FocusEditor()

    End Sub
End Class
