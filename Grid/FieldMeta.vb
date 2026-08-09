Public NotInheritable Class FieldMeta

    Public Property Header As String = ""
    Public Property Align As CellTextAlign = CellTextAlign.Left

    Public Property PreferredWidth As Integer = 80
    Public Property MinWidth As Integer = 50

    Public Property UsesPicklist As Boolean
    Public Property IsVolumeField As Boolean
    Public Property IsDataColumn As Boolean = True

    ' Field-specific validation routine.
    ' Nothing means that this field has no additional validation rules.
    Public Property Validator As Func(Of String, ValidationResult)
End Class