'------------------------------------------------------------------------------
' FormBoundsData
'
' Stores the persisted position, size and maximized state of a form.
'
' Keeping these values together avoids passing or storing five separate
' properties whenever form bounds are restored or saved.
'------------------------------------------------------------------------------
Public NotInheritable Class FormBoundsData

    Public Property Left As Integer = -1
    Public Property Top As Integer = -1

    Public Property Width As Integer = 900
    Public Property Height As Integer = 600

    Public Property Maximized As Boolean

End Class
