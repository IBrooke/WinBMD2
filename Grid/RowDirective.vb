Public NotInheritable Class RowDirective

    Public Property RowIndex As Integer

    Public Property DirectiveType As String = ""

    Public Property Text As String = ""

    Public Overrides Function ToString() As String

        If String.IsNullOrWhiteSpace(Text) Then
            Return DirectiveType
        End If

        Return $"{DirectiveType},{Text}"

    End Function

End Class
