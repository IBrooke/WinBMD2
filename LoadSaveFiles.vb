Imports System.Globalization
Imports System.IO
Imports System.Text

Public NotInheritable Class LoadSaveFiles

    Private Sub New()
    End Sub

    ' Loads a BMD file into ProjectValues and the transcription grid.
    ' Directives are not yet handled; they will be added later.
    Public Shared Function Load(
        filePath As String,
        grid As DataGridView) As Boolean

        If String.IsNullOrWhiteSpace(filePath) Then
            Return False
        End If

        If Not File.Exists(filePath) Then
            Return False
        End If

        Try

            Dim lines As List(Of String) =
                ReadAllLines(filePath)

            If lines.Count < 5 Then
                Throw New InvalidDataException(
                    "The file does not contain the required BMD header lines.")
            End If

            ValidateHeaderLines(lines)

            ParseInfoLine(lines(0))
            ParseHeaderLine1(lines(1))
            ParseHeaderLine2(lines(2))
            ParseSourceLine(lines(3))
            ParseOpeningPage(lines(4))

            LoadGridRows(
                lines,
                grid,
                firstDataLine:=5)

            Return True

        Catch ex As Exception

            DebugLog.WriteAlways(
        $"[SAVE] Failed to save '{filePath}': {ex}")

            Return False

        End Try

    End Function


    ' Saves the current transcription grid as a BMD file.
    ' Directives are not yet handled; they will be added later.
    Public Shared Function Save(
        filePath As String,
        grid As DataGridView) As Boolean

        If String.IsNullOrWhiteSpace(filePath) Then
            Return False
        End If

        Try

            Dim encoding As Encoding =
                GetSaveEncoding()

            Dim lines As New List(Of String)

            lines.Add(BuildInfoLine())
            lines.Add(BuildHeaderLine1())
            lines.Add(BuildHeaderLine2())
            lines.Add(BuildSourceLine())
            lines.Add($"+PAGE,{ProjectValues.Page}")

            Dim fields() As GridField =
                GetDataFields()

            Dim lastDataRow As Integer =
                FindLastNonEmptyRow(
                    grid,
                    fields)

            For rowIndex As Integer = 0 To lastDataRow

                lines.Add(
                    BuildDataLine(
                        grid.Rows(rowIndex),
                        fields))

            Next

            Dim folder As String =
                Path.GetDirectoryName(filePath)

            If Not String.IsNullOrWhiteSpace(folder) Then
                Directory.CreateDirectory(folder)
            End If

            File.WriteAllLines(
                filePath,
                lines,
                encoding)

            DebugLog.Write(
                $"[SAVE] File saved: '{filePath}', Rows={lastDataRow + 1}")

            Return True

        Catch ex As Exception

            DebugLog.WriteAlways(
        $"[SAVE] Failed to save '{filePath}': {ex}")

            Return False

        End Try

    End Function
    ' Returns only genuine transcription-data fields.
    ' Directive and Verified are grid-only columns and are not
    ' written as comma-separated data fields.
    Private Shared Function GetDataFields() As GridField()

        Return GridLayout.GetVisibleFields().
            Where(
                Function(field)
                    Return FieldMetaData.Meta(field).IsDataColumn
                End Function).
            ToArray()

    End Function
    Private Shared Function FindGridColumn(
        grid As DataGridView,
        field As GridField) As Integer

        For columnIndex As Integer = 0 To grid.Columns.Count - 1

            Dim column As DataGridViewColumn =
                grid.Columns(columnIndex)

            If column.Tag Is Nothing Then
                Continue For
            End If

            If DirectCast(column.Tag, GridField) = field Then
                Return columnIndex
            End If

        Next

        Return -1

    End Function
    Private Shared Function SplitCsv(line As String) As List(Of String)

        Dim fields As New List(Of String)
        Dim current As New StringBuilder
        Dim inQuotes As Boolean = False

        For index As Integer = 0 To line.Length - 1

            Dim ch As Char =
                line(index)

            If ch = """"c Then

                If inQuotes AndAlso
                   index + 1 < line.Length AndAlso
                   line(index + 1) = """"c Then

                    current.Append(""""c)
                    index += 1

                Else

                    inQuotes =
                        Not inQuotes

                End If

            ElseIf ch = ","c AndAlso Not inQuotes Then

                fields.Add(current.ToString())
                current.Clear()

            Else

                current.Append(ch)

            End If

        Next

        fields.Add(current.ToString())

        Return fields

    End Function


    Private Shared Function ReQuote(value As String) As String

        If String.IsNullOrEmpty(value) Then
            Return ""
        End If

        If value.Contains(","c) OrElse
           value.Contains(""""c) Then

            Return """" &
                   value.Replace("""", """""") &
                   """"

        End If

        Return value

    End Function
    Private Shared Sub LoadGridRows(
        lines As List(Of String),
        grid As DataGridView,
        firstDataLine As Integer)

        Dim fields() As GridField =
            GetDataFields()

        grid.Rows.Clear()

        For lineIndex As Integer = firstDataLine To lines.Count - 1

            Dim line As String =
                If(lines(lineIndex), "").TrimEnd()

            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            ' Directives will be handled separately later.
            If line.StartsWith("+") OrElse
               line.StartsWith("#") Then

                Continue For
            End If

            Dim values As List(Of String) =
                SplitCsv(line)

            Dim rowIndex As Integer =
                grid.Rows.Add()

            For fieldIndex As Integer = 0 To fields.Length - 1

                Dim columnIndex As Integer =
                    FindGridColumn(
                        grid,
                        fields(fieldIndex))

                If columnIndex < 0 Then
                    Continue For
                End If

                Dim value As String =
                    If(
                        fieldIndex < values.Count,
                        values(fieldIndex),
                        "")

                grid.Rows(rowIndex).
                    Cells(columnIndex).
                    Value = value

            Next

        Next

        If grid.Rows.Count = 0 Then
            grid.Rows.Add()
        End If

    End Sub


    Private Shared Function BuildDataLine(
        row As DataGridViewRow,
        fields() As GridField) As String

        Dim values As New List(Of String)

        For Each field As GridField In fields

            Dim columnIndex As Integer =
                FindGridColumn(
                    row.DataGridView,
                    field)

            Dim value As String = ""

            If columnIndex >= 0 Then

                value =
                    If(
                        row.Cells(columnIndex).Value,
                        "").
                    ToString()

            End If

            values.Add(
                ReQuote(value))

        Next

        Return String.Join(
            ",",
            values)

    End Function


    Private Shared Function FindLastNonEmptyRow(
        grid As DataGridView,
        fields() As GridField) As Integer

        For rowIndex As Integer = grid.Rows.Count - 1 To 0 Step -1

            Dim row As DataGridViewRow =
                grid.Rows(rowIndex)

            If row.IsNewRow Then
                Continue For
            End If

            For Each field As GridField In fields

                Dim columnIndex As Integer =
                    FindGridColumn(
                        grid,
                        field)

                If columnIndex < 0 Then
                    Continue For
                End If

                Dim value As String =
                    If(
                        row.Cells(columnIndex).Value,
                        "").
                    ToString()

                If Not String.IsNullOrWhiteSpace(value) Then
                    Return rowIndex
                End If

            Next

        Next

        Return -1

    End Function
    Private Shared Sub ValidateHeaderLines(
        lines As List(Of String))

        If Not lines(0).StartsWith(
            "+INFO",
            StringComparison.OrdinalIgnoreCase) Then

            Throw New InvalidDataException(
                "Line 1 must start with +INFO.")
        End If

        If Not lines(1).StartsWith("#") Then
            Throw New InvalidDataException(
                "Line 2 must start with #.")
        End If

        If Not lines(2).StartsWith("#") Then
            Throw New InvalidDataException(
                "Line 3 must start with #.")
        End If

        If Not lines(3).StartsWith(
            "+S",
            StringComparison.OrdinalIgnoreCase) Then

            Throw New InvalidDataException(
                "Line 4 must start with +S.")
        End If

        If Not lines(4).StartsWith(
            "+PAGE",
            StringComparison.OrdinalIgnoreCase) Then

            Throw New InvalidDataException(
                "Line 5 must start with +PAGE.")
        End If

    End Sub
    Private Shared Sub ParseInfoLine(line As String)

        Dim parts As List(Of String) =
            SplitCsv(line)

        If parts.Count < 5 Then
            Return
        End If

        ProjectValues.CreatorEmail =
            parts(1)

        ProjectValues.SequenceType =
            parts(3)

        Select Case parts(4).Trim().ToUpperInvariant()

            Case "BIRTHS"
                ProjectValues.BatchType = "B"

            Case "MARRIAGES"
                ProjectValues.BatchType = "M"

            Case "DEATHS"
                ProjectValues.BatchType = "D"

        End Select

    End Sub


    Private Shared Sub ParseHeaderLine1(line As String)

        Dim parts As List(Of String) =
            SplitCsv(line)

        If parts.Count < 10 Then
            Return
        End If

        ProjectValues.VNF =
            parts(1)

        ProjectValues.Creator =
            parts(2)

        ProjectValues.Syndicate =
            parts(3)

        ProjectValues.BatchName =
            parts(4)

        ProjectValues.Created =
            parts(5)

        ProjectValues.PageLetter =
            parts(9)

    End Sub


    Private Shared Sub ParseHeaderLine2(line As String)

        If line.StartsWith("#,") Then
            ProjectValues.Comments = line.Substring(2)

        ElseIf line.StartsWith("#") Then
            ProjectValues.Comments = line.Substring(1)

        Else
            ProjectValues.Comments = line
        End If

    End Sub


    Private Shared Sub ParseSourceLine(line As String)

        Dim parts As List(Of String) =
        SplitCsv(line)

        If parts.Count < 5 Then
            Return
        End If

        Dim year As Integer

        If Integer.TryParse(parts(1), year) Then
            ProjectValues.Year = year
        End If

        Select Case parts(2).Trim().ToUpperInvariant()

            Case "MAR"
                ProjectValues.Quarter = 1

            Case "JUN"
                ProjectValues.Quarter = 2

            Case "SEP"
                ProjectValues.Quarter = 3

            Case "DEC"
                ProjectValues.Quarter = 4

            Case Else
                ProjectValues.Quarter = 0

        End Select

        ProjectValues.SourceRef =
        parts(3)

        Dim modifiedDate As Date

        If Date.TryParseExact(
        parts(4),
        "d-MMM-yyyy",
        Globalization.CultureInfo.InvariantCulture,
        Globalization.DateTimeStyles.None,
        modifiedDate) Then

            ProjectValues.DateModified =
            modifiedDate

        End If

    End Sub


    Private Shared Sub ParseOpeningPage(line As String)

        Dim parts As List(Of String) =
            SplitCsv(line)

        If parts.Count < 2 Then
            Return
        End If

        Dim page As Integer

        If Integer.TryParse(
            parts(1),
            page) Then

            ProjectValues.Page =
                page

        End If

    End Sub
    Private Shared Function BuildInfoLine() As String

        Dim recordType As String

        Select Case ProjectValues.BatchType

            Case "B"
                recordType = "BIRTHS"

            Case "M"
                recordType = "MARRIAGES"

            Case "D"
                recordType = "DEATHS"

            Case Else
                recordType = "BIRTHS"

        End Select

        Return String.Join(
            ",",
            "+INFO",
            ReQuote(ProjectValues.CreatorEmail),
            "Password",
            ReQuote(ProjectValues.SequenceType),
            recordType)

    End Function


    Private Shared Function BuildHeaderLine1() As String

        Return String.Join(
            ",",
            "#",
            ReQuote(ProjectValues.VNF),
            ReQuote(ProjectValues.Creator),
            ReQuote(ProjectValues.Syndicate),
            ReQuote(ProjectValues.BatchName),
            ReQuote(ProjectValues.Created),
            "Y",
            "N",
            "N",
            ReQuote(ProjectValues.PageLetter),
            "0",
            ReQuote(AppIdentity.VersionString))

    End Function


    Private Shared Function BuildHeaderLine2() As String

        Return "#," &
               ReQuote(ProjectValues.Comments)

    End Function


    Private Shared Function BuildSourceLine() As String

        Dim quarter As String

        Select Case ProjectValues.Quarter

            Case 1
                quarter = "MAR"

            Case 2
                quarter = "JUN"

            Case 3
                quarter = "SEP"

            Case 4
                quarter = "DEC"

            Case Else
                quarter = ""

        End Select

        Return String.Join(
            ",",
            "+S",
            ProjectValues.Year.ToString(
                CultureInfo.InvariantCulture),
            quarter,
            ReQuote(ProjectValues.SourceRef),
            Date.Today.ToString(
                "d-MMM-yyyy",
                CultureInfo.InvariantCulture))

    End Function
    Private Shared Function ReadAllLines(
    filePath As String) As List(Of String)

        Encoding.RegisterProvider(
        CodePagesEncodingProvider.Instance)

        Dim firstLine As String

        Using reader As New StreamReader(
        filePath,
        Encoding.Default,
        detectEncodingFromByteOrderMarks:=True)

            firstLine =
            If(reader.ReadLine(), "")

        End Using

        Dim fileEncoding As Encoding =
        DetectEncoding(firstLine)

        Return File.ReadAllLines(
        filePath,
        fileEncoding).
        ToList()

    End Function


    Private Shared Function DetectEncoding(
        firstLine As String) As Encoding

        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance)

        If firstLine.StartsWith(
            "+INFO",
            StringComparison.OrdinalIgnoreCase) Then

            Dim parts As List(Of String) =
                SplitCsv(firstLine)

            If parts.Count >= 6 AndAlso
               Not String.IsNullOrWhiteSpace(parts(5)) Then

                Return ResolveEncoding(
                    parts(5))

            End If

        End If

        Return Encoding.GetEncoding(
            "ISO-8859-1")

    End Function


    Private Shared Function GetSaveEncoding() As Encoding

        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance)

        Return Encoding.GetEncoding(
            "ISO-8859-1")

    End Function


    Private Shared Function ResolveEncoding(
        charset As String) As Encoding

        Select Case charset.Trim().ToLowerInvariant()

            Case "iso-8859-1",
                 "latin1",
                 "latin-1"

                Return Encoding.GetEncoding(
                    "ISO-8859-1")

            Case "macintosh"
                Return Encoding.GetEncoding(10000)

            Case "cp850",
                 "ibm850"

                Return Encoding.GetEncoding(850)

            Case "utf-8"
                Return New UTF8Encoding(False)

            Case Else
                Return Encoding.GetEncoding(
                    charset)

        End Select

    End Function

End Class