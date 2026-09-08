Public Class ChangeState

    Private _cellChanged As Boolean
    Private _rowChanged As Boolean
    Private _fileChanged As Boolean

    Public Sub SetCellChanged()
        _cellChanged = True
        _rowChanged = True
        _fileChanged = True
    End Sub

    Public Function CellChanged() As Boolean
        Return _cellChanged
    End Function
    Public Sub SetFileChanged()
        _fileChanged = True
    End Sub
    Public Function RowChanged() As Boolean
        Return _rowChanged
    End Function

    Public Function FileChanged() As Boolean
        Return _fileChanged
    End Function

    Public Sub WorkfileSaved()
        _cellChanged = False
        _rowChanged = False
    End Sub

    Public Sub FileSaved()
        _cellChanged = False
        _rowChanged = False
        _fileChanged = False
    End Sub

End Class
