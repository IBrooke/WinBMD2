Imports System.IO

Public Module DistrictData

    Private Const DistrictsFileName As String = "WinBMD_Districts.txt"

    Private ReadOnly _districts As New List(Of DistrictRecord)

    Public ReadOnly Property DistrictCount As Integer
        Get
            Return _districts.Count
        End Get
    End Property

    Public Function LoadBaseDistricts() As Boolean

        _districts.Clear()

        Dim filePath As String = Path.Combine(AppPaths.FilesFolder, DistrictsFileName)

        DebugLog.Write($"[DISTRICTS] Loading: {filePath}")

        If Not File.Exists(filePath) Then
            DebugLog.WriteAlways($"[DISTRICTS] File not found: {filePath}")

            MessageBox.Show(
                "The districts file could not be found." &
                Environment.NewLine &
                Environment.NewLine &
                filePath,
                "Districts File Not Found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False
        End If

        Try
            Dim lineNumber As Integer
            Dim invalidLines As Integer

            For Each rawLine As String In File.ReadLines(filePath)

                lineNumber += 1

                If String.IsNullOrWhiteSpace(rawLine) Then
                    Continue For
                End If

                Dim fields() As String = rawLine.Split("|"c)

                If fields.Length <> 11 Then
                    invalidLines += 1
                    DebugLog.Write($"[DISTRICTS] Line {lineNumber}: expected 11 fields, found {fields.Length}.")
                    Continue For
                End If

                Dim startYear As Integer
                Dim startQuarter As Integer
                Dim endYear As Integer
                Dim endQuarter As Integer

                If Not Integer.TryParse(fields(1).Trim(), startYear) OrElse
                   Not Integer.TryParse(fields(2).Trim(), startQuarter) OrElse
                   Not Integer.TryParse(fields(3).Trim(), endYear) OrElse
                   Not Integer.TryParse(fields(4).Trim(), endQuarter) Then

                    invalidLines += 1
                    DebugLog.Write($"[DISTRICTS] Line {lineNumber}: invalid year or quarter value.")
                    Continue For
                End If

                Dim districtName As String = fields(0).Trim()

                If districtName.Length = 0 Then
                    invalidLines += 1
                    DebugLog.Write($"[DISTRICTS] Line {lineNumber}: district name is blank.")
                    Continue For
                End If

                _districts.Add(
                    New DistrictRecord With {
                        .Name = districtName,
                        .StartYear = startYear,
                        .StartQuarter = startQuarter,
                        .EndYear = endYear,
                        .EndQuarter = endQuarter,
                        .VolumeTo1851 = fields(5).Trim(),
                        .VolumeTo1946 = fields(6).Trim(),
                        .VolumeTo1965 = fields(7).Trim(),
                        .VolumeTo1974 = fields(8).Trim(),
                        .VolumeTo1993 = fields(9).Trim(),
                        .VolumeAfter1993 = fields(10).Trim()
                    })

            Next

            DebugLog.Write($"[DISTRICTS] Loaded {_districts.Count} base district records. Invalid lines: {invalidLines}.")

            Return True

        Catch ex As Exception
            _districts.Clear()

            DebugLog.LogException("Loading base districts", ex)

            MessageBox.Show(
                "The districts file could not be read." &
                Environment.NewLine &
                Environment.NewLine &
                filePath &
                Environment.NewLine &
                Environment.NewLine &
                ex.Message,
                "Districts File Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error)

            Return False
        End Try

    End Function

    Private Class DistrictRecord

        Public Property Name As String = ""

        Public Property StartYear As Integer
        Public Property StartQuarter As Integer

        Public Property EndYear As Integer
        Public Property EndQuarter As Integer

        Public Property VolumeTo1851 As String = ""
        Public Property VolumeTo1946 As String = ""
        Public Property VolumeTo1965 As String = ""
        Public Property VolumeTo1974 As String = ""
        Public Property VolumeTo1993 As String = ""
        Public Property VolumeAfter1993 As String = ""

    End Class

End Module