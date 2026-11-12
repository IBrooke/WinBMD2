' One entry in the recent-files list, stored in the existing settings JSON.
' Keeps the batch filename and its last scan Y position and grid row.
' The position is retained when the entry moves to the top of the list,
' and removed when the entry drops out of the list.
Public Class RecentFileEntry

    ' Full path to the batch file.
    Public Property FilePath As String = ""

    ' Vertical position of the scan. The ruler remains fixed.
    ' Nothing means no scan position has been recorded yet.
    Public Property ScanY As Double?

    ' Zero-based grid row index.
    ' Nothing means no grid position has been recorded yet.
    Public Property GridRow As Integer?

End Class