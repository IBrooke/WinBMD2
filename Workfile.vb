Imports System.IO
Imports System.Text

Public Class Workfile

    Private _currentRowIndex As Integer = -1

    Private Const RecordSize As Integer = 512
    Private Const HeaderRecordStart As Integer = 1
    Private Const HeaderRecordCount As Integer = 6
    Private Const FirstRowRecord As Integer = HeaderRecordStart + HeaderRecordCount

    Private Shared ReadOnly WorkfileEncoding As Encoding = Encoding.GetEncoding(1252)

    Private Const Signature As String = "W00000|WINBMD2_WORK_V3"
    Private Const SignatureRecord As Integer = 0

    Friend NotInheritable Class LoadResult

        Friend Property Success As Boolean
        Friend Property ErrorMessage As String = ""
        Friend Property Lines As List(Of String)

    End Class
    Friend Function Exists() As Boolean
        Return File.Exists(AppPaths.WorkFilePath)
    End Function

    Friend Sub Reset()
        _currentRowIndex = -1
    End Sub

    Friend Sub Delete()

        Try
            Dim workFilePath As String = AppPaths.WorkFilePath

            If File.Exists(workFilePath) Then
                File.Delete(workFilePath)
                DebugLog.Write("[WORKFILE] Deleted workfile")
            End If

        Catch ex As Exception
            DebugLog.Write("[WORKFILE] Failed to delete workfile: " & ex.Message)
        End Try

    End Sub

    ' Creates a new workfile containing the current header, directives and all grid rows,
    ' replacing any existing workfile.
    Friend Function CreateOrReplace(grid As DataGridView) As Boolean

        If grid Is Nothing Then Return False

        Try
            Dim workFilePath As String = AppPaths.WorkFilePath
            Dim folder As String = Path.GetDirectoryName(workFilePath)

            If Not String.IsNullOrWhiteSpace(folder) Then Directory.CreateDirectory(folder)

            Using stream As New FileStream(workFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)

                WriteRecord(stream, SignatureRecord, Signature)

                Dim headerLines() As String = {
                    LoadSaveFiles.BuildInfoLine(),
                    LoadSaveFiles.BuildHeaderLine1(),
                    LoadSaveFiles.BuildHeaderLine2(),
                    LoadSaveFiles.BuildSourceLine(),
                   "+PAGE," & ProjectValues.Page.ToString(),
                    ""
                }

                For i As Integer = 0 To HeaderRecordCount - 1
                    WriteRecord(stream, HeaderRecordStart + i, "H" & (i + 1).ToString("00000") & "|" & headerLines(i))
                Next

                Dim fields() As GridField = LoadSaveFiles.GetDataFields()

                For rowIndex As Integer = 0 To grid.Rows.Count - 1
                    Dim row As DataGridViewRow = grid.Rows(rowIndex)
                    If row.IsNewRow Then Continue For
                    If rowIndex = grid.Rows.Count - 1 AndAlso TranscriptionForm.IsBlankEntryRow(row) Then Continue For

                    Dim line As String
                    Dim directive As RowDirective = RowDirective.FromGridRow(row)

                    If directive IsNot Nothing Then
                        line = LoadSaveFiles.BuildDirectiveLine(directive)
                    Else
                        line = LoadSaveFiles.BuildDataLine(row, fields)
                    End If

                    WriteRecord(stream, FirstRowRecord + rowIndex, "L" & rowIndex.ToString("00000") & "|" & line)
                Next

                stream.Flush()

            End Using

            Reset()

            If grid.Rows.Count > 0 Then _currentRowIndex = 0

            DebugLog.Write("[WORKFILE] Created workfile. Rows=" & grid.Rows.Count.ToString())

        Catch ex As Exception
            DebugLog.Write("[WORKFILE] Failed to create workfile: " & ex.Message)
            Return False
        End Try
        Return True
    End Function

    Friend Sub SetCurrentRow(newRowIndex As Integer, grid As DataGridView, changeState As ChangeState)

        If newRowIndex < 0 Then Return

        If grid Is Nothing Then
            _currentRowIndex = newRowIndex
            Return
        End If

        If _currentRowIndex >= 0 AndAlso newRowIndex <> _currentRowIndex AndAlso changeState.RowChanged() Then
            If SaveRowIfPossible(_currentRowIndex, grid) Then changeState.WorkfileSaved()
        End If

        _currentRowIndex = newRowIndex

    End Sub

    Friend Sub FlushCurrentRow(grid As DataGridView, changeState As ChangeState)

        If grid Is Nothing Then Return
        If Not changeState.RowChanged() Then Return
        If _currentRowIndex < 0 Then Return

        If SaveRowIfPossible(_currentRowIndex, grid) Then changeState.WorkfileSaved()

    End Sub

    Private Function SaveRowIfPossible(rowIndex As Integer, grid As DataGridView) As Boolean

        If rowIndex < 0 Then Return False
        If rowIndex >= grid.Rows.Count Then Return False
        If grid.Rows(rowIndex).IsNewRow Then Return False
        If rowIndex = grid.Rows.Count - 1 AndAlso TranscriptionForm.IsBlankEntryRow(grid.Rows(rowIndex)) Then
            RemoveFinalRowRecord(rowIndex)
            Return False
        End If

        Return SaveRow(rowIndex, grid.Rows(rowIndex))

    End Function
    Private Sub RemoveFinalRowRecord(rowIndex As Integer)

        Try
            Dim workFilePath As String = AppPaths.WorkFilePath

            If Not File.Exists(workFilePath) Then Return

            Dim newLength As Long = CLng(FirstRowRecord + rowIndex) * RecordSize

            Using stream As New FileStream(workFilePath, FileMode.Open, FileAccess.Write, FileShare.Read)
                If stream.Length > newLength Then
                    stream.SetLength(newLength)
                    stream.Flush()
                End If
            End Using

            DebugLog.Write("[WORKFILE] Removed final blank row " & (rowIndex + 1).ToString())

        Catch ex As Exception
            DebugLog.Write("[WORKFILE] Failed to remove final blank row " & (rowIndex + 1).ToString() & ": " & ex.Message)
        End Try

    End Sub
    Private Function SaveRow(rowIndex As Integer, row As DataGridViewRow) As Boolean

        Try
            Dim workFilePath As String = AppPaths.WorkFilePath
            Dim folder As String = Path.GetDirectoryName(workFilePath)

            If Not String.IsNullOrWhiteSpace(folder) Then Directory.CreateDirectory(folder)

            Dim line As String
            Dim directive As RowDirective = RowDirective.FromGridRow(row)

            If directive IsNot Nothing Then
                line = LoadSaveFiles.BuildDirectiveLine(directive)
            Else
                Dim fields() As GridField = LoadSaveFiles.GetDataFields()
                line = LoadSaveFiles.BuildDataLine(row, fields)
            End If

            Dim recordText As String = "L" & rowIndex.ToString("00000") & "|" & line
            Dim record() As Byte = MakeRecord(recordText)
            Dim offset As Long = CLng(FirstRowRecord + rowIndex) * RecordSize

            Using stream As New FileStream(workFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)
                stream.Seek(offset, SeekOrigin.Begin)
                stream.Write(record, 0, record.Length)
                stream.Flush()
            End Using

            DebugLog.Write("[WORKFILE] Saved row " & (rowIndex + 1).ToString())

            Return True

        Catch ex As Exception
            DebugLog.Write("[WORKFILE] Failed to save row " & (rowIndex + 1).ToString() & ": " & ex.Message)
            Return False
        End Try

    End Function

    Private Shared Function ReadRecord(stream As FileStream, recordNumber As Integer) As String

        Dim buffer(RecordSize - 1) As Byte
        Dim offset As Long = CLng(recordNumber) * RecordSize

        If offset >= stream.Length Then Return ""

        stream.Seek(offset, SeekOrigin.Begin)

        Dim bytesRead As Integer = stream.Read(buffer, 0, buffer.Length)

        If bytesRead <= 0 Then Return ""

        Return WorkfileEncoding.GetString(buffer, 0, bytesRead).TrimEnd(ChrW(0), ControlChars.Cr, ControlChars.Lf, " "c)

    End Function

    Private Shared Function MakeRecord(text As String) As Byte()

        Dim buffer(RecordSize - 1) As Byte
        Dim bytes() As Byte = WorkfileEncoding.GetBytes(text)

        If bytes.Length > RecordSize Then Throw New InvalidOperationException("Workfile record is too long: " & bytes.Length.ToString() & " bytes.")

        Array.Copy(bytes, buffer, bytes.Length)

        Return buffer

    End Function

    Private Shared Sub WriteRecord(stream As FileStream, recordNumber As Integer, text As String)

        Dim record() As Byte = MakeRecord(text)
        Dim offset As Long = CLng(recordNumber) * RecordSize

        stream.Seek(offset, SeekOrigin.Begin)
        stream.Write(record, 0, record.Length)

    End Sub
    Friend Function TryLoad() As LoadResult

        Try
            Dim workFilePath As String = AppPaths.WorkFilePath

            If Not File.Exists(workFilePath) Then Return New LoadResult With {.Success = False, .ErrorMessage = "No workfile exists."}

            Using stream As New FileStream(workFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)

                If stream.Length < RecordSize Then Return New LoadResult With {.Success = False, .ErrorMessage = "The workfile is too short."}

                Dim signatureText As String = ReadRecord(stream, SignatureRecord)

                If Not String.Equals(signatureText, Signature, StringComparison.Ordinal) Then
                    Return New LoadResult With {.Success = False, .ErrorMessage = "The workfile signature is invalid."}
                End If

                Dim headerLines As New List(Of String)
                Dim rowLines As New SortedDictionary(Of Integer, String)

                For i As Integer = 0 To HeaderRecordCount - 1
                    Dim record As String = ReadRecord(stream, HeaderRecordStart + i)
                    If String.IsNullOrWhiteSpace(record) Then Continue For

                    Dim pipe As Integer = record.IndexOf("|"c)
                    If pipe < 0 Then Continue For

                    Dim content As String = record.Substring(pipe + 1)
                    If Not String.IsNullOrWhiteSpace(content) Then headerLines.Add(content)
                Next

                Dim recordCount As Integer = CInt(stream.Length \ RecordSize)

                For recordNumber As Integer = FirstRowRecord To recordCount - 1
                    Dim record As String = ReadRecord(stream, recordNumber)
                    If String.IsNullOrWhiteSpace(record) Then Continue For
                    If Not record.StartsWith("L", StringComparison.OrdinalIgnoreCase) Then Continue For

                    Dim pipe As Integer = record.IndexOf("|"c)
                    If pipe < 0 Then Continue For

                    Dim recordId As String = record.Substring(0, pipe)
                    If recordId.Length < 2 Then Continue For

                    Dim rowIndex As Integer
                    If Not Integer.TryParse(recordId.Substring(1), rowIndex) Then Continue For

                    Dim content As String = record.Substring(pipe + 1)
                    If Not String.IsNullOrWhiteSpace(content) Then rowLines(rowIndex) = content
                Next

                Dim batchLines As New List(Of String)

                batchLines.AddRange(headerLines)

                For Each pair As KeyValuePair(Of Integer, String) In rowLines
                    batchLines.Add(pair.Value)
                Next

                DebugLog.Write("[WORKFILE] Read workfile. Headers=" & headerLines.Count.ToString() & ", Rows=" & rowLines.Count.ToString() & ", Lines=" & batchLines.Count.ToString())

                Return New LoadResult With {.Success = True, .Lines = batchLines}

            End Using

        Catch ex As Exception
            DebugLog.Write("[WORKFILE] Failed to load workfile: " & ex.Message)
            Return New LoadResult With {.Success = False, .ErrorMessage = ex.Message}
        End Try

    End Function
End Class