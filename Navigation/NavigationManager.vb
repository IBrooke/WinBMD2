Public Class NavigationManager

    Private ReadOnly _owner As TranscriptionForm

    Public Sub New(owner As TranscriptionForm)

        _owner = owner

    End Sub

    Public Function HandleKey(sender As Object, e As KeyEventArgs) As Boolean

        Dim editor As TextBox = TryCast(sender, TextBox)

        If _owner.CurrentGridCell Is Nothing Then Return False

        If _owner.PickListIsActive Then

            Dim number As Integer = -1

            If Not e.Shift AndAlso e.KeyCode >= Keys.D1 AndAlso e.KeyCode <= Keys.D9 Then
                number = e.KeyCode - Keys.D0
            ElseIf e.KeyCode >= Keys.NumPad1 AndAlso e.KeyCode <= Keys.NumPad9 Then
                number = e.KeyCode - Keys.NumPad0
            End If

            If number > 0 Then

                If _owner.SelectPickListItemByNumber(number) Then

                    _owner.AcceptCurrentPickListSelection(editor, True)

                    ' A Forename field can contain several names. After selecting
                    ' one from the picklist, add a space and remain in the same
                    ' cell ready to select or type the next forename.
                    If _owner.CurrentField = GridField.Forename Then

                        If editor IsNot Nothing Then

                            If Not editor.Text.EndsWith(" ") Then editor.Text &= " "

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

            If e.KeyCode = Keys.D0 OrElse e.KeyCode = Keys.NumPad0 Then
                _owner.CopyNextWordFromAbove(editor)
                Return True
            End If

        End If

        Select Case e.KeyCode

            Case Keys.Up

                If e.Alt AndAlso _owner.PickListIsActive Then
                    _owner.MovePickListSelectionUp()
                    Return True
                End If

            Case Keys.Down

                If e.Alt AndAlso _owner.PickListIsActive Then
                    _owner.MovePickListSelectionDown()
                    Return True
                End If

            Case Keys.Left, Keys.Back

                If editor IsNot Nothing Then
                    If editor.SelectionStart <> 0 OrElse editor.SelectionLength <> 0 Then Return False
                End If

                _owner.MoveToPreviousDataCell()
                Return True

            Case Keys.Right

                If editor IsNot Nothing Then
                    If editor.SelectionStart <> editor.TextLength OrElse editor.SelectionLength <> 0 Then Return False
                End If

                _owner.MoveToNextDataCell(moveToStart:=True)
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
                If _owner.PickListIsActive AndAlso
               _owner.CurrentField = GridField.Forename AndAlso
               Not ShouldIgnorePickList(Keys.Enter) Then

                    _owner.AcceptCurrentPickListSelection(editor)

                    If editor IsNot Nothing Then

                        If Not editor.Text.EndsWith(" ") Then editor.Text &= " "

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
                Return key = Keys.Tab OrElse key = Keys.Enter

            Case IgnoreAutoCompleteKey.Tab
                Return key = Keys.Tab

            Case IgnoreAutoCompleteKey.Return
                Return key = Keys.Enter

            Case Else
                Return False

        End Select

    End Function
    Private Sub PrepareForForwardMove(editor As TextBox, key As Keys)

        ' District is a special case. If it is completely blank, there is no
        ' typed value or picklist selection to accept. In that case Tab or
        ' Return should copy the District from the previous data row regardless
        ' of the Ignore AutoComplete setting.
        '
        ' CopyFromAboveIfBlank also copies the associated Volume/DistNum.
        If _owner.CurrentField = GridField.District AndAlso editor IsNot Nothing AndAlso String.IsNullOrWhiteSpace(editor.Text) Then
            _owner.CopyFromAboveIfBlank(editor, allowPickList:=True)
            Return
        End If

        ' A blank Forename should copy the complete Forename from the preceding
        ' data row rather than accepting the current picklist selection.
        If _owner.CurrentField = GridField.Forename AndAlso editor IsNot Nothing AndAlso String.IsNullOrWhiteSpace(editor.Text) Then

            If _owner.CopyFromAboveIfBlank(editor, allowPickList:=True) Then Return

        End If

        ' For fields which use a picklist, the Ignore AutoComplete setting
        ' determines whether Tab/Return accepts the selected picklist entry
        ' or performs the normal copy-from-above behaviour.
        If _owner.PickListIsActive Then

            If ShouldIgnorePickList(key) Then
                _owner.CopyFromAboveIfBlank(editor, allowPickList:=True)
            Else
                _owner.AcceptCurrentPickListSelection(editor)
            End If

        Else

            ' Ordinary fields do not have a picklist, so use the normal
            ' copy-from-above behaviour when the field is blank.
            _owner.CopyFromAboveIfBlank(editor)

        End If

    End Sub
End Class
