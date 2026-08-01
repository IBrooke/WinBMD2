'------------------------------------------------------------------------------
' TranscriptionDocument
'
' Represents the complete transcription currently being edited.
'
' A TranscriptionDocument owns the collection of TranscriptionRow objects
' together with document-level state such as the current row and whether the
' document has been modified.
'
' The document is independent of the user interface. It does not know how the
' rows are displayed or edited; it simply manages the transcription data in
' memory.
'
' Loading and saving will later be handled separately, so this class can remain
' focused on the document data and document-wide operations.
'------------------------------------------------------------------------------
Public NotInheritable Class TranscriptionDocument

    Private ReadOnly _rows As New List(Of TranscriptionRow)

    Public ReadOnly Property Rows As IReadOnlyList(Of TranscriptionRow)
        Get
            Return _rows
        End Get
    End Property

    Public Property CurrentRow As Integer

    Public Property HasChanges As Boolean

    Public Sub Clear()

        _rows.Clear()

        CurrentRow = 0
        HasChanges = False

    End Sub

    Public Function AddRow() As TranscriptionRow

        Dim row As New TranscriptionRow()

        _rows.Add(row)

        HasChanges = True

        Return row

    End Function

    Public Sub RemoveRow(index As Integer)

        If index < 0 OrElse index >= _rows.Count Then
            Throw New ArgumentOutOfRangeException(NameOf(index))
        End If

        _rows.RemoveAt(index)

        If CurrentRow >= _rows.Count Then
            CurrentRow = Math.Max(0, _rows.Count - 1)
        End If

        HasChanges = True

    End Sub

End Class