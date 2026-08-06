Imports System.IO
Imports System.Text

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

        Dim filePath As String =
            Path.Combine(AppPaths.FilesFolder, DistrictsFileName)

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

                Dim fields() As String =
                    rawLine.Split("|"c)

                If fields.Length <> 11 Then

                    invalidLines += 1

                    DebugLog.Write(
                        $"[DISTRICTS] Line {lineNumber}: " &
                        $"expected 11 fields, found {fields.Length}.")

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

                    DebugLog.Write(
                        $"[DISTRICTS] Line {lineNumber}: " &
                        "invalid year or quarter value.")

                    Continue For

                End If

                Dim districtName As String =
                    fields(0).Trim()

                If districtName.Length = 0 Then

                    invalidLines += 1

                    DebugLog.Write(
                        $"[DISTRICTS] Line {lineNumber}: " &
                        "district name is blank.")

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

            _districts.Sort(
                Function(left, right)

                    Return String.Compare(
                        left.Name,
                        right.Name,
                        StringComparison.OrdinalIgnoreCase)

                End Function)

            DebugLog.Write(
                $"[DISTRICTS] Loaded {_districts.Count} " &
                $"base district records. Invalid lines: {invalidLines}.")

            Return True

        Catch ex As Exception

            _districts.Clear()

            DebugLog.LogException(
                "Loading base districts",
                ex)

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

    Public Function GetMatches(
        prefix As String,
        Optional maximumResults As Integer = 9) As List(Of DistrictMatch)

        Dim results As New List(Of DistrictMatch)

        If String.IsNullOrWhiteSpace(prefix) Then
            Return results
        End If

        prefix = prefix.Trim()

        Dim selectedYear As Integer =
            ProjectValues.Year

        Dim selectedQuarter As Integer =
            ProjectValues.Quarter

        If selectedQuarter < 1 OrElse selectedQuarter > 4 Then
            selectedQuarter = 1
        End If

        For Each record As DistrictRecord In _districts

            If Not record.Name.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase) Then

                Continue For

            End If

            If Not IsAlive(
                record,
                selectedYear,
                selectedQuarter) Then

                Continue For

            End If

            Dim volume As String =
                GetVolume(record, selectedYear)

            volume =
                NormalizeVolumeForVnf(volume)

            Dim duplicateExists As Boolean =
                results.Any(
                    Function(existing)

                        Return String.Equals(
                            existing.Name,
                            record.Name,
                            StringComparison.OrdinalIgnoreCase) AndAlso
                               String.Equals(
                                   existing.Volume,
                                   volume,
                                   StringComparison.OrdinalIgnoreCase)

                    End Function)

            If duplicateExists Then
                Continue For
            End If

            results.Add(
                New DistrictMatch With {
                    .Name = record.Name,
                    .Volume = volume
                })

            If results.Count >= maximumResults Then
                Exit For
            End If

        Next

        Return results

    End Function

    Private Function IsAlive(
        record As DistrictRecord,
        selectedYear As Integer,
        selectedQuarter As Integer) As Boolean

        If selectedYear < record.StartYear Then
            Return False
        End If

        If selectedYear = record.StartYear AndAlso
           selectedQuarter < record.StartQuarter Then

            Return False

        End If

        If selectedYear > record.EndYear Then
            Return False
        End If

        If selectedYear = record.EndYear AndAlso
           selectedQuarter > record.EndQuarter Then

            Return False

        End If

        Return True

    End Function

    Private Function GetVolume(
        record As DistrictRecord,
        selectedYear As Integer) As String

        If selectedYear <= 1851 Then
            Return record.VolumeTo1851
        End If

        If selectedYear <= 1946 Then
            Return record.VolumeTo1946
        End If

        If selectedYear <= 1965 Then
            Return record.VolumeTo1965
        End If

        If selectedYear <= 1974 Then
            Return record.VolumeTo1974
        End If

        If selectedYear <= 1992 OrElse
           (selectedYear = 1993 AndAlso
            String.Equals(
                ProjectValues.BatchType,
                "M",
                StringComparison.OrdinalIgnoreCase)) Then

            Return record.VolumeTo1993

        End If

        Return record.VolumeAfter1993

    End Function

    Private Function NormalizeVolumeForVnf(
        value As String) As String

        value =
            If(value, "").Trim()

        If value.Length = 0 Then
            Return ""
        End If

        If Not String.Equals(
            ProjectValues.VNF,
            "XX",
            StringComparison.OrdinalIgnoreCase) Then

            Return value

        End If

        Dim number As Integer

        If Not Integer.TryParse(value, number) Then
            Return value
        End If

        Return ToRoman(number)

    End Function

    Private Function ToRoman(
        number As Integer) As String

        If number <= 0 OrElse number > 3999 Then
            Return number.ToString()
        End If

        Dim values() As Integer = {
            1000, 900, 500, 400,
            100, 90, 50, 40,
            10, 9, 5, 4, 1
        }

        Dim symbols() As String = {
            "M", "CM", "D", "CD",
            "C", "XC", "L", "XL",
            "X", "IX", "V", "IV", "I"
        }

        Dim result As New StringBuilder()

        For index As Integer = 0 To values.Length - 1

            While number >= values(index)

                result.Append(symbols(index))
                number -= values(index)

            End While

        Next

        Return result.ToString()

    End Function

    Public NotInheritable Class DistrictMatch

        Public Property Name As String = ""
        Public Property Volume As String = ""

    End Class

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