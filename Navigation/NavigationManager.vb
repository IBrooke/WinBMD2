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

        Select Case e.KeyCode

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
