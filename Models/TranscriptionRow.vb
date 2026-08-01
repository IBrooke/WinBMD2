Imports System.Collections.Generic
'------------------------------------------------------------------------------
' TranscriptionRow
'
' Represents a single row of transcription data within the current document.
'
' Each row stores the text entered for every GridField that is applicable to
' the current batch layout (Births, Marriages or Deaths). Rather than having a
' separate property for every possible field, values are stored in a dictionary
' indexed by GridField. This allows the same class to support every historical
' layout simply by using the fields selected by GridLayout.
'
' The row also stores information that is not part of the transcription text,
' such as whether the row has been verified.
'
' This class is purely a data container. It contains no user interface code
' and no validation logic; those responsibilities belong elsewhere.
'------------------------------------------------------------------------------
Public NotInheritable Class TranscriptionRow

    Private ReadOnly _values As New Dictionary(Of GridField, String)

    Public Property IsVerified As Boolean

    Default Public Property Item(field As GridField) As String
        Get
            Return GetValue(field)
        End Get

        Set(value As String)
            SetValue(field, value)
        End Set
    End Property

    Public Function GetValue(field As GridField) As String

        If field = GridField.Verified Then
            Return If(IsVerified, "True", "")
        End If

        Dim storedValue As String = Nothing

        If _values.TryGetValue(field, storedValue) Then
            Return storedValue
        End If

        Return ""

    End Function

    Public Sub SetValue(field As GridField, value As String)

        If field = GridField.Verified Then
            IsVerified =
                String.Equals(
                    value,
                    "True",
                    StringComparison.OrdinalIgnoreCase)

            Return
        End If

        _values(field) = If(value, "")

    End Sub

    Public Sub ClearValue(field As GridField)

        If field = GridField.Verified Then
            IsVerified = False
            Return
        End If

        _values.Remove(field)

    End Sub

    Public Sub Clear()

        _values.Clear()
        IsVerified = False

    End Sub

    Public Function HasValue(field As GridField) As Boolean

        If field = GridField.Verified Then
            Return IsVerified
        End If

        Return Not String.IsNullOrWhiteSpace(GetValue(field))

    End Function

    Public Function HasAnyData() As Boolean

        For Each item As KeyValuePair(Of GridField, String) In _values

            If FieldMetaData.Meta(item.Key).IsDataColumn AndAlso
               Not String.IsNullOrWhiteSpace(item.Value) Then

                Return True
            End If

        Next

        Return False

    End Function

End Class
