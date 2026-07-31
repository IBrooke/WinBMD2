Imports System.IO
Imports System.Text
Public Module ForenameData

    Private Const ForenamesFileName As String = "WinBMD_Name3.txt"

    Private ReadOnly _names As New List(Of String)
    Private ReadOnly _topNames As New List(Of String)

    Public ReadOnly Property Names As IReadOnlyList(Of String)
        Get
            Return _names
        End Get
    End Property

    Public ReadOnly Property TopNames As IReadOnlyList(Of String)
        Get
            Return _topNames
        End Get
    End Property

    Public ReadOnly Property NameCount As Integer
        Get
            Return _names.Count
        End Get
    End Property

    Public ReadOnly Property TopNameCount As Integer
        Get
            Return _topNames.Count
        End Get
    End Property

    Public Function Load() As Boolean

        _names.Clear()
        _topNames.Clear()

        Dim filePath As String =
            Path.Combine(AppPaths.FilesFolder, ForenamesFileName)

        If Not File.Exists(filePath) Then

            Dim result As DialogResult =
                MessageBox.Show(
                    "The forenames file could not be found." &
                    Environment.NewLine &
                    Environment.NewLine &
                    filePath &
                    Environment.NewLine &
                    Environment.NewLine &
                    "Do you want to continue without forename picklists?",
                    "Forenames File Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning)

            Return result = DialogResult.Yes

        End If

        Try

            For Each rawLine As String In File.ReadLines(filePath)

                If String.IsNullOrWhiteSpace(rawLine) Then
                    Continue For
                End If

                If Char.IsWhiteSpace(rawLine(0)) Then
                    Continue For
                End If

                Dim line As String = rawLine.Trim()

                If line.StartsWith("@") Then

                    Dim topName As String = line.Substring(1).Trim()

                    If topName.Length > 0 Then
                        _topNames.Add(topName)
                    End If

                Else

                    _names.Add(line)

                End If

            Next

            _names.Sort(StringComparer.OrdinalIgnoreCase)

            DebugLog.Write($"[NAMES] Loaded {TopNameCount} top names and {NameCount} normal names.")

            Return True

        Catch ex As Exception

            MessageBox.Show(
                "Error reading the forenames file." &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Forenames",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            _names.Clear()
            _topNames.Clear()

            Return False

        End Try

    End Function

    Public Function GetMatches(
        text As String,
        Optional maximumResults As Integer = 9) As List(Of String)

        If text Is Nothing Then
            text = ""
        End If

        ' User has just typed a space, so suggest common first names.
        If text.Length > 0 AndAlso Char.IsWhiteSpace(text(text.Length - 1)) Then
            Return _topNames.Take(maximumResults).ToList()
        End If

        Dim prefix As String = text.Trim()

        ' Match only the current forename being typed.
        Dim lastSpace As Integer = prefix.LastIndexOf(" "c)

        If lastSpace >= 0 Then
            prefix = prefix.Substring(lastSpace + 1)
        End If

        If prefix.Length = 0 Then
            Return _topNames.Take(maximumResults).ToList()
        End If

        Return _names.
            Where(Function(name)
                      Return name.StartsWith(
                          prefix,
                          StringComparison.OrdinalIgnoreCase)
                  End Function).
            Take(maximumResults).
            ToList()

    End Function
    Public Sub AddName(name As String)

        If String.IsNullOrWhiteSpace(name) Then
            Return
        End If

        name = name.Trim()

        If _names.Any(Function(existingName) String.Equals(existingName, name, StringComparison.OrdinalIgnoreCase)) Then
            Return
        End If

        _names.Add(name)
        _names.Sort(StringComparer.OrdinalIgnoreCase)

    End Sub
    Public Function Save() As Boolean

        Dim filePath As String = Path.Combine(AppPaths.FilesFolder, ForenamesFileName)

        Try
            AppPaths.EnsureFoldersExist()

            Using writer As New StreamWriter(filePath, append:=False, encoding:=Encoding.UTF8)

                For Each topName As String In _topNames
                    writer.WriteLine("@" & topName)
                Next

                For Each name As String In _names
                    writer.WriteLine(name)
                Next

            End Using

            Return True

        Catch ex As Exception

            MessageBox.Show(
                "The forenames file could not be saved." &
                Environment.NewLine &
                Environment.NewLine &
                filePath &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Forenames File Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False

        End Try

    End Function
End Module
