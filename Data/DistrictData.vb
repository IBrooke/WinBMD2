Imports System.IO
Imports System.Text

Public Module DistrictData

    Private Const DistrictsFileName As String = "WinBMD_Districts.txt"

    Private ReadOnly _districts As New List(Of DistrictRecord)  ' the collection of all districts loaded from the districts file
    Private ReadOnly _aliveDistricts As New List(Of DistrictEntry)  ' the collection of districts that are alive for the current year and quarter, includes those from _districts and from the Supplementary file

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
    Public Function LoadAliveDistricts() As Boolean

        _aliveDistricts.Clear()

        Dim selectedYear As Integer = ProjectValues.Year
        Dim selectedQuarter As Integer = ProjectValues.Quarter
        Dim baseCount As Integer
        Dim supplementaryCount As Integer

        Try

            ' First add all base districts which are alive for this batch.
            For Each record As DistrictRecord In _districts

                If Not IsAlive(record, selectedYear, selectedQuarter) Then
                    Continue For
                End If

                Dim volume As String = NormalizeVolumeForVnf(GetVolume(record, selectedYear))

                If AddAliveDistrict(record.Name, volume, False) Then
                    baseCount += 1
                End If

            Next

            ' Now add the supplementary districts for this batch.
            Dim supplementaryFileName As String = GetSupplementaryFilename(selectedYear, selectedQuarter)
            Dim supplementaryFilePath As String = Path.Combine(AppPaths.FilesFolder, supplementaryFileName)

            DebugLog.Write($"[DISTRICTS] Supplementary district file for {selectedYear} Q{selectedQuarter}: {supplementaryFileName}")

            If Not File.Exists(supplementaryFilePath) Then
                DebugLog.Write($"[DISTRICTS] Supplementary file not found, creating empty file: {supplementaryFilePath}")
                File.WriteAllText(supplementaryFilePath, "", Encoding.UTF8)
            End If

            Dim lineNumber As Integer
            Dim invalidLines As Integer

            For Each rawLine As String In File.ReadLines(supplementaryFilePath)

                lineNumber += 1

                If String.IsNullOrWhiteSpace(rawLine) Then
                    Continue For
                End If

                Dim fields() As String = rawLine.Split(","c)

                If fields.Length <> 2 Then
                    invalidLines += 1
                    DebugLog.Write($"[DISTRICTS] Supplementary file line {lineNumber}: expected 2 fields, found {fields.Length}.")
                    Continue For
                End If

                Dim districtName As String = fields(0).Trim()
                Dim volume As String = NormalizeVolumeForVnf(fields(1))

                If districtName.Length = 0 OrElse volume.Length = 0 Then
                    invalidLines += 1
                    DebugLog.Write($"[DISTRICTS] Supplementary file line {lineNumber}: district or volume is blank.")
                    Continue For
                End If

                If AddAliveDistrict(districtName, volume, True) Then
                    supplementaryCount += 1
                End If

            Next

            _aliveDistricts.Sort(Function(left, right) String.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase))

            DebugLog.Write($"[DISTRICTS] Alive base districts added: {baseCount}.")
            DebugLog.Write($"[DISTRICTS] Supplementary districts added from {supplementaryFileName}: {supplementaryCount}. Invalid lines: {invalidLines}.")
            DebugLog.Write($"[DISTRICTS] Total alive districts: {_aliveDistricts.Count}.")

            Return True

        Catch ex As Exception

            _aliveDistricts.Clear()

            DebugLog.LogException("Loading alive districts", ex)

            MessageBox.Show("The district information for this batch could not be loaded." & Environment.NewLine & Environment.NewLine & ex.Message, "Districts File Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Return False

        End Try

    End Function
    Private Function AddAliveDistrict(name As String, volume As String, supplementary As Boolean) As Boolean

        name = If(name, "").Trim()
        volume = If(volume, "").Trim()

        If name.Length = 0 OrElse volume.Length = 0 Then
            Return False
        End If

        Dim duplicateExists As Boolean = _aliveDistricts.Any(Function(existing) String.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(existing.Volume, volume, StringComparison.OrdinalIgnoreCase))

        If duplicateExists Then
            Return False
        End If

        _aliveDistricts.Add(New DistrictEntry With {
        .Name = name,
        .Volume = volume,
        .Supplementary = supplementary
    })

        Return True

    End Function
    Private Function GetSupplementaryFilename(year As Integer, quarter As Integer) As String

        If year <= 1945 Then
            Return "WinBMD_Supp_Districts.txt"
        End If

        If year = 1946 Then
            Return If(quarter < 3, "WinBMD_Supp_Districts.txt", "WinBMD_Supp_Districts_1946.txt")
        End If

        If year <= 1964 Then
            Return "WinBMD_Supp_Districts_1946.txt"
        End If

        If year = 1965 Then
            Return If(quarter < 2, "WinBMD_Supp_Districts_1946.txt", "WinBMD_Supp_Districts_1965.txt")
        End If

        If year <= 1973 Then
            Return "WinBMD_Supp_Districts_1965.txt"
        End If

        If year <= 1992 Then
            Return If(year = 1974 AndAlso quarter < 2, "WinBMD_Supp_Districts_1965.txt", "WinBMD_Supp_Districts_1974.txt")
        End If

        If year = 1993 AndAlso String.Equals(ProjectValues.BatchType, "M", StringComparison.OrdinalIgnoreCase) Then
            Return "WinBMD_Supp_Districts_1974.txt"
        End If

        Return "WinBMD_Supp_Districts_1993.txt"

    End Function
    Public Function GetMatches(prefix As String, Optional maximumResults As Integer = 9) As List(Of DistrictMatch)

        Dim results As New List(Of DistrictMatch)

        If String.IsNullOrWhiteSpace(prefix) Then
            Return results
        End If

        prefix = prefix.Trim()

        For Each record As DistrictEntry In _aliveDistricts

            If Not record.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            Dim duplicateExists As Boolean = results.Any(Function(existing) String.Equals(existing.Name, record.Name, StringComparison.OrdinalIgnoreCase) AndAlso String.Equals(existing.Volume, record.Volume, StringComparison.OrdinalIgnoreCase))

            If duplicateExists Then
                Continue For
            End If

            results.Add(New DistrictMatch With {
            .Name = record.Name,
            .Volume = record.Volume
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
    Public Function ContainsDistrictAndCode(district As String, code As String) As Boolean

        district = If(district, "").Trim()
        code = NormalizeDistrictCodeForMatch(code)

        If district.Length = 0 OrElse code.Length = 0 Then
            Return False
        End If

        For Each entry As DistrictEntry In _aliveDistricts

            If Not String.Equals(entry.Name, district, StringComparison.OrdinalIgnoreCase) Then
                Continue For
            End If

            If String.Equals(NormalizeDistrictCodeForMatch(entry.Volume), code, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If

        Next

        Return False

    End Function

    Private Function NormalizeDistrictCodeForMatch(code As String) As String

        Dim value As String = If(code, "").Trim()

        If ProjectValues.Match3VolChars AndAlso value.Length > 3 Then
            value = value.Substring(0, 3)
        End If

        Return value.ToUpperInvariant()

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
    Public Function RomanToInt(roman As String) As Integer

        If String.IsNullOrWhiteSpace(roman) Then
            Return 0
        End If

        roman = roman.Trim().ToUpperInvariant()

        Dim values As New Dictionary(Of Char, Integer) From {
        {"I"c, 1},
        {"V"c, 5},
        {"X"c, 10},
        {"L"c, 50},
        {"C"c, 100},
        {"D"c, 500},
        {"M"c, 1000}
    }

        Dim total As Integer
        Dim previous As Integer

        For index As Integer = roman.Length - 1 To 0 Step -1

            Dim current As Integer

            If Not values.TryGetValue(roman(index), current) Then
                Return 0
            End If

            If current < previous Then
                total -= current
            Else
                total += current
                previous = current
            End If

        Next

        If total <= 0 OrElse total > 3999 Then
            Return 0
        End If

        If Not String.Equals(ToRoman(total), roman, StringComparison.Ordinal) Then
            Return 0
        End If

        Return total

    End Function
    Public Function AddSupplementaryDistrict(district As String, code As String) As Boolean

        district = If(district, "").Trim()
        code = If(code, "").Trim()

        If district.Length = 0 OrElse code.Length = 0 Then
            Return False
        End If

        If district.IndexOfAny({"*"c, "?"c, "_"c}) >= 0 OrElse code.IndexOfAny({"*"c, "?"c, "_"c}) >= 0 Then
            Return False
        End If

        If ContainsDistrictAndCode(district, code) Then
            Return False
        End If

        Dim newEntry As New DistrictEntry With {
        .Name = district,
        .Volume = code,
        .Supplementary = True
    }

        _aliveDistricts.Add(newEntry)
        _aliveDistricts.Sort(Function(a, b) String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase))

        If SaveSupplementaryDistrictFile() Then
            Return True
        End If

        _aliveDistricts.Remove(newEntry)
        Return False

    End Function

    Private Function SaveSupplementaryDistrictFile() As Boolean

        Try
            Dim fileName As String = GetSupplementaryFilename(ProjectValues.Year, ProjectValues.Quarter)
            Dim filePath As String = Path.Combine(AppPaths.FilesFolder, fileName)

            Dim lines As List(Of String) = _aliveDistricts.
            Where(Function(entry) entry.Supplementary).
            OrderBy(Function(entry) entry.Name, StringComparer.OrdinalIgnoreCase).
            ThenBy(Function(entry) entry.Volume, StringComparer.OrdinalIgnoreCase).
            Select(Function(entry) $"{entry.Name},{NormalizeDistrictCodeForSave(entry.Volume)}").
            ToList()

            File.WriteAllLines(filePath, lines, New UTF8Encoding(False))

            DebugLog.WriteAlways($"[DISTRICTS] Supplementary district file saved: '{fileName}', Entries={lines.Count}")
            Return True

        Catch ex As Exception
            DebugLog.WriteAlways($"[DISTRICTS] Failed to save supplementary district file: {ex}")

            MessageBox.Show(
            "The supplementary district could not be saved.",
            "WinBMD2",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

            Return False

        End Try

    End Function

    Private Function NormalizeDistrictCodeForSave(code As String) As String

        Dim value As String = If(code, "").Trim()

        If Not String.Equals(ProjectValues.VNF, "XX", StringComparison.OrdinalIgnoreCase) Then
            Return value
        End If

        Dim numericValue As Integer = RomanToInt(value)

        If numericValue > 0 Then
            Return numericValue.ToString()
        End If

        Return value

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
    Private Class DistrictEntry

        Public Property Name As String = ""
        Public Property Volume As String = ""
        Public Property Supplementary As Boolean

    End Class
End Module