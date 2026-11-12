' Historical defaults for displaying names in the verification bar.  These layouts are from BMDVerify, Thanks David Lang.
' These select layout and reference measurements, not the user's display font.
Friend NotInheritable Class VerifyNameLayout

    Public ReadOnly FromDate As Integer
    Public ReadOnly ReferenceFontName As String
    Public ReadOnly ReferenceHyphens As String
    Private ReadOnly _birthForenamePosition As Integer
    Private ReadOnly _otherForenamePosition As Integer

    Private Sub New(startDate As Integer, fontName As String, hyphens As String, birthPosition As Integer, otherPosition As Integer)

        FromDate = startDate
        ReferenceFontName = fontName
        ReferenceHyphens = hyphens
        _birthForenamePosition = birthPosition
        _otherForenamePosition = otherPosition

    End Sub

    ' Each entry remains applicable until the next date.
    Private Shared ReadOnly Layouts As VerifyNameLayout() = {
        New VerifyNameLayout(0, "Times New Roman", "------", 0, 0),
        New VerifyNameLayout(18373, "Courier New", "--", 17, 12),
        New VerifyNameLayout(18661, "Times New Roman", "------", 0, 0),
        New VerifyNameLayout(19103, "Courier New", "--", 0, 0)
    }

    Public Shared Function GetCurrent() As VerifyNameLayout

        Dim batchType As String = ProjectValues.BatchType.ToString().ToUpperInvariant()

        If batchType <> "B" AndAlso batchType <> "M" AndAlso batchType <> "D" Then Return Layouts(0)
        If ProjectValues.Year < 1837 OrElse ProjectValues.Quarter < 0 Then Return Layouts(0)

        Dim dateKey As Integer = ProjectValues.Year * 10 + ProjectValues.Quarter
        Dim selected As VerifyNameLayout = Layouts(0)

        For Each layout As VerifyNameLayout In Layouts
            If layout.FromDate > dateKey Then Exit For
            selected = layout
        Next

        Return selected

    End Function

    Public Function GetForenamePosition() As Integer

        If String.Equals(ProjectValues.BatchType.ToString(), "B", StringComparison.OrdinalIgnoreCase) Then
            Return _birthForenamePosition
        End If

        Return _otherForenamePosition

    End Function

    Public Function GetForenameOffset(displayFont As Font) As Integer

        Dim position As Integer = GetForenamePosition()
        If position = 0 Then Return 0

        ' The original display starts with two spaces before the surname.
        ' Our boxes omit those spaces, so subtract them from the reference offset.
        Using referenceFont As New Font(ReferenceFontName, displayFont.Size, displayFont.Style, displayFont.Unit)
            Dim flags As TextFormatFlags = TextFormatFlags.NoPadding Or TextFormatFlags.SingleLine Or TextFormatFlags.NoPrefix
            Return TextRenderer.MeasureText(New String("0"c, position - 2), referenceFont, Size.Empty, flags).Width
        End Using

    End Function

End Class