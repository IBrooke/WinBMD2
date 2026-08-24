Imports System.IO

Public NotInheritable Class VerificationData

    Private Sub New()
    End Sub

    Private Shared ReadOnly Property FilePath As String
        Get
            Return Path.Combine(AppPaths.FilesFolder, "VerificationData.txt")
        End Get
    End Property

    Public Shared Function Load(batchName As String) As String

        If String.IsNullOrWhiteSpace(batchName) Then
            Return ""
        End If

        If Not File.Exists(FilePath) Then
            Return ""
        End If

        For Each line As String In File.ReadLines(FilePath)

            Dim parts() As String = line.Split("|"c)

            If parts.Length <> 3 Then
                Continue For
            End If

            If String.Equals(
                parts(0),
                batchName,
                StringComparison.OrdinalIgnoreCase) Then

                Return parts(2)

            End If

        Next

        Return ""

    End Function

    Public Shared Sub Save(batchName As String, state As String)

        If String.IsNullOrWhiteSpace(batchName) Then
            Return
        End If

        state = If(state, "")

        Dim lines As New List(Of String)

        If File.Exists(FilePath) Then
            lines.AddRange(File.ReadAllLines(FilePath))
        End If

        Dim newLine As String =
            $"{batchName}|{state.Length}|{state}"

        Dim found As Boolean = False

        For index As Integer = 0 To lines.Count - 1

            Dim parts() As String = lines(index).Split("|"c)

            If parts.Length < 1 Then
                Continue For
            End If

            If String.Equals(
                parts(0),
                batchName,
                StringComparison.OrdinalIgnoreCase) Then

                lines(index) = newLine
                found = True
                Exit For

            End If

        Next

        If Not found Then
            lines.Add(newLine)
        End If

        File.WriteAllLines(FilePath, lines)

    End Sub

End Class