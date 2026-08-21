Public NotInheritable Class RowDirective

    Public Property RowIndex As Integer

    Public Property DirectiveType As String = ""

    Public Property Text As String = ""

    ' Lines is used by #COMMENT directives.
    ' Nothing means no line count was supplied.
    Public Property Lines As Integer?

    Public Overrides Function ToString() As String

        If String.IsNullOrWhiteSpace(Text) Then
            Return DirectiveType
        End If

        Return $"{DirectiveType},{Text}"

    End Function

End Class
