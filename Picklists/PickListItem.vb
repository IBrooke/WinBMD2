Public Class PickListItem

    Public Property Text As String = ""
    Public Property Volume As String = ""

    Public Overrides Function ToString() As String

        Return Text

    End Function

End Class
Public Class PickListItemAcceptedEventArgs
    Inherits EventArgs

    Public Sub New(item As PickListItem)

        Me.Item = item

    End Sub

    Public ReadOnly Property Item As PickListItem

End Class