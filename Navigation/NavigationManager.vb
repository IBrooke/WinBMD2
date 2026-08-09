Public Class NavigationManager

    Private ReadOnly _owner As TranscriptionForm

    Public Sub New(owner As TranscriptionForm)

        _owner = owner

    End Sub

    Public Function HandleKey(
        sender As Object,
        e As KeyEventArgs) As Boolean

        Dim editor As TextBox =
            TryCast(sender, TextBox)

        If _owner.CurrentGridCell Is Nothing Then
            Return False
        End If

        Dim currentRow As Integer =
            _owner.CurrentGridCell.RowIndex

        Dim currentColumn As Integer =
            _owner.CurrentGridCell.ColumnIndex

        If _owner.PickListIsOpen Then

            Dim number As Integer = -1

            If e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D9 Then
                number = e.KeyCode - Keys.D0
            ElseIf e.KeyCode >= Keys.NumPad1 AndAlso e.KeyCode <= Keys.NumPad9 Then
                number = e.KeyCode - Keys.NumPad0
            End If

            If number > 0 Then

                If _owner.SelectPickListItemByNumber(number) Then

                    _owner.AcceptCurrentPickListSelection(editor)

                    ' A Forename field can contain several names. After selecting
                    ' one from the picklist, add a space and remain in the same
                    ' cell ready to select or type the next forename.
                    If _owner.CurrentField = GridField.Forename Then

                        If editor IsNot Nothing Then

                            If Not editor.Text.EndsWith(" ") Then
                                editor.Text &= " "
                            End If

                            editor.SelectionStart = editor.TextLength
                            editor.SelectionLength = 0

                        End If

                    Else

                        ' Other picklists, such as District, represent a complete
                        ' field value, so accept the item and move on.
                        _owner.EndGridEdit()
                        _owner.MoveToNextDataCell()

                    End If

                End If

                Return True

            End If

        End If

        If _owner.PickListIsOpen Then

            If e.KeyCode = Keys.D0 OrElse
       e.KeyCode = Keys.NumPad0 Then

                _owner.CopyNextWordFromAbove(editor)
                Return True

            End If

        End If

        Select Case e.KeyCode
            Case Keys.Up

                If _owner.PickListIsOpen Then
                    _owner.MovePickListSelectionUp()
                    Return True
                End If

            Case Keys.Down

                If _owner.PickListIsOpen Then
                    _owner.MovePickListSelectionDown()
                    Return True
                End If

            Case Keys.Left

                If editor IsNot Nothing Then

                    If editor.SelectionStart <> 0 OrElse
           editor.SelectionLength <> 0 Then

                        Return False

                    End If

                End If

                _owner.MoveToPreviousDataCell()
                Return True

            Case Keys.Right

                If editor IsNot Nothing Then

                    If editor.SelectionStart <> editor.TextLength OrElse
           editor.SelectionLength <> 0 Then

                        Return False

                    End If

                End If

                _owner.MoveToNextDataCell()
                Return True

            Case Keys.Tab

                If e.Shift Then

                    _owner.EndGridEdit()
                    _owner.MoveToPreviousDataCell()

                Else

                    PrepareForForwardMove(editor, Keys.Tab)
                    _owner.EndGridEdit()
                    _owner.MoveToNextDataCell()

                End If

                Return True

            Case Keys.Enter

                ' A Forename field can contain several names. If Enter is
                ' accepting a picklist entry, append it and remain in the
                ' Forename cell ready for another name.
                If _owner.PickListIsOpen AndAlso
                   _owner.CurrentField = GridField.Forename AndAlso
                   Not ShouldIgnorePickList(Keys.Enter) Then

                    _owner.AcceptCurrentPickListSelection(editor)

                    If editor IsNot Nothing Then

                        If Not editor.Text.EndsWith(" ") Then
                            editor.Text &= " "
                        End If

                        editor.SelectionStart = editor.TextLength
                        editor.SelectionLength = 0

                    End If

                    Return True

                End If

                ' All other Enter behaviour moves forward normally.
                PrepareForForwardMove(editor, Keys.Enter)
                _owner.EndGridEdit()
                _owner.MoveToNextDataCell()

                Return True
        End Select
        Return False

    End Function

    Private Function ShouldIgnorePickList(
    key As Keys) As Boolean

        Select Case ProjectValues.IgnoreAutoComplete

            Case IgnoreAutoCompleteKey.All
                Return key = Keys.Tab OrElse
                       key = Keys.Enter

            Case IgnoreAutoCompleteKey.Tab
                Return key = Keys.Tab

            Case IgnoreAutoCompleteKey.Return
                Return key = Keys.Enter

            Case Else
                Return False

        End Select

    End Function
    Private Sub PrepareForForwardMove(
    editor As TextBox,
    key As Keys)

        If _owner.CurrentFieldUsesPickList Then

            If ShouldIgnorePickList(key) Then

                _owner.CopyFromAboveIfBlank(
                    editor,
                    allowPickList:=True)

            Else

                _owner.AcceptCurrentPickListSelection(editor)

            End If

        Else

            _owner.CopyFromAboveIfBlank(editor)

        End If

    End Sub
End Class
