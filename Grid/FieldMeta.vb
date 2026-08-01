Public NotInheritable Class FieldMeta

    Public Property Header As String = ""
    Public Property Align As CellTextAlign = CellTextAlign.Left

    Public Property PreferredWidth As Integer = 80
    Public Property MinWidth As Integer = 50

    Public Property UsesPicklist As Boolean
    Public Property IsVolumeField As Boolean
    Public Property IsDataColumn As Boolean = True

End Class