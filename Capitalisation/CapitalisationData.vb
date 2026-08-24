Imports System.IO

Friend NotInheritable Class CapitalisationData

    Private Shared ReadOnly _settings As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

    Private Sub New()
    End Sub

    Private Shared ReadOnly Property FilePath As String
        Get
            Return Path.Combine(AppPaths.FilesFolder, "CapitalisationData.txt")
        End Get
    End Property

    Public Shared Sub Load()

        _settings.Clear()

        If Not File.Exists(FilePath) Then
            Return
        End If

        For Each line As String In File.ReadLines(FilePath)

            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            Dim parts() As String = line.Split("|"c)

            If parts.Length <> 3 Then
                Continue For
            End If

            Dim key As String = parts(0).Trim() & "|" & parts(1).Trim()
            Dim values As String = parts(2).Trim()

            If values.Length = 0 Then
                Continue For
            End If

            _settings(key) = values

        Next

    End Sub

    Public Shared Function GetSettings(batchType As String, year As Integer) As String

        Dim key As String = batchType & "|" & year.ToString()
        Dim values As String = ""

        If _settings.TryGetValue(key, values) Then
            Return values
        End If

        Return ""

    End Function

    Public Shared Sub SetSettings(batchType As String, year As Integer, values As String)

        Dim key As String = batchType & "|" & year.ToString()

        If String.IsNullOrWhiteSpace(values) Then
            _settings.Remove(key)
        Else
            _settings(key) = values
        End If

    End Sub

    Public Shared Sub Save()

        Dim lines As New List(Of String)

        For Each pair In _settings.OrderBy(Function(item) item.Key)
            lines.Add(pair.Key & "|" & pair.Value)
        Next

        File.WriteAllLines(FilePath, lines)

    End Sub
    Public Shared Function GetMode(batchType As String, year As Integer, columnIndex As Integer) As CapitalisationMode

        Dim values As String = GetSettings(batchType, year)

        If columnIndex < 0 OrElse columnIndex >= values.Length Then
            Return CapitalisationMode.AsTyped
        End If

        Dim enumValue As Integer

        If Not Integer.TryParse(values(columnIndex).ToString(), enumValue) Then
            Return CapitalisationMode.AsTyped
        End If

        If Not [Enum].IsDefined(GetType(CapitalisationMode), enumValue) Then
            Return CapitalisationMode.AsTyped
        End If

        Return CType(enumValue, CapitalisationMode)

    End Function
    Public Shared Sub SetMode(batchType As String, year As Integer, columnIndex As Integer, mode As CapitalisationMode)

        If columnIndex < 0 Then
            Return
        End If

        Dim values As String = GetSettings(batchType, year)

        If values.Length <= columnIndex Then
            values = values.PadRight(columnIndex + 1, CChar(CInt(CapitalisationMode.AsTyped).ToString()))
        End If

        Dim chars() As Char = values.ToCharArray()
        chars(columnIndex) = CChar(CInt(mode).ToString())

        SetSettings(batchType, year, New String(chars))

    End Sub
End Class
