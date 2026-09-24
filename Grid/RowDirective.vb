Public NotInheritable Class RowDirective

    Public Property RowIndex As Integer

    Public Property DirectiveType As String = ""

    Public Property Text As String = ""

    Public ReadOnly Property ShownWarnings As New HashSet(Of DirectiveWarningType)

    ' Lines is used by #COMMENT directives.
    ' Nothing means no line count was supplied.
    Public Property Lines As Integer?
    Public Shared Function FromGridRow(row As DataGridViewRow) As RowDirective

        If row Is Nothing Then Return Nothing

        Return TryCast(row.Tag, RowDirective)

    End Function
    Public Overrides Function ToString() As String

        If String.IsNullOrWhiteSpace(Text) Then
            Return DirectiveType
        End If

        Return $"{DirectiveType},{Text}"

    End Function

End Class
