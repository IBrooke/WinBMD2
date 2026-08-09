Imports System.Text.RegularExpressions

Public NotInheritable Class Validator
    Private Const UnusualChars As String = "+#<>;=^%$~`"
    Private Sub New()
    End Sub

    ' Validates a value entered into any transcription field.
    '
    ' UCF validation always takes precedence. If a value contains any
    ' UCF characters, only the UCF syntax is checked and no normal
    ' field-specific validation is performed.
    Public Shared Function Validate(
    field As GridField,
    value As String) As ValidationResult

        value = If(value, "")

        ' UCF validation always takes precedence.
        ' If UCF characters are present, no further validation is performed.
        If ContainsUcf(value) Then
            Return ValidateUcf(value)
        End If

        ' When validating for upload, blank fields are errors.
        ' AaD is the exception when IgnoreAaD applies to a Death batch.
        If ProjectValues.ValidationMode = ValidationMode.Upload AndAlso
       String.IsNullOrWhiteSpace(value) Then

            Dim ignoreBlankAaD As Boolean =
            field = GridField.AaD AndAlso
            String.Equals(
                ProjectValues.BatchType,
                "D",
                StringComparison.OrdinalIgnoreCase) AndAlso
            ProjectValues.IgnoreAaD

            If Not ignoreBlankAaD Then
                Return ValidationResult.Error(
                "This field is required for upload.")
            End If

        End If

        ' Apply validation common to all ordinary transcription fields.
        Dim result As ValidationResult =
        ValidateGeneralCellCharacters(field, value)

        If Not result.IsOk Then
            Return result
        End If

        result =
        ValidateTripleLetters(field, value)

        If Not result.IsOk Then
            Return result
        End If

        ' Apply the validator assigned to this field in FieldMetaData.
        Dim fieldInformation As FieldMeta =
        FieldMetaData.Meta(field)

        If fieldInformation.Validator IsNot Nothing Then

            result =
            fieldInformation.Validator(value)

            If Not result.IsOk Then
                Return result
            End If

        End If

        ' Final general warning check.
        Return ValidateUnusualCharacters(value)

    End Function
    ' Checks characters and bracket pairing that are common to
    ' transcription fields before the field-specific validator runs.
    Private Shared Function ValidateGeneralCellCharacters(
    field As GridField,
    value As String) As ValidationResult

        If String.IsNullOrEmpty(value) Then
            Return ValidationResult.Ok()
        End If

        For Each ch As Char In value

            If IsAllowedGeneralCellCharacter(field, ch) Then
                Continue For
            End If

            Return ValidationResult.Warning(
            $"Contains unexpected character '{ch}'.")

        Next

        If Not HasBalancedBrackets(value, "("c, ")"c) Then
            Return ValidationResult.Warning(
            "Unmatched brackets.")
        End If

        If Not HasBalancedBrackets(value, "["c, "]"c) Then
            Return ValidationResult.Warning(
            "Unmatched square brackets.")
        End If

        If Not HasBalancedBrackets(value, "{"c, "}"c) Then
            Return ValidationResult.Warning(
            "Unmatched wildcard brackets.")
        End If

        Return ValidationResult.Ok()

    End Function
    ' Final common check for characters which are legal enough to
    ' retain but sufficiently unusual that the user should review them.
    Private Shared Function ValidateUnusualCharacters(
    value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim position As Integer =
        value.IndexOfAny(UnusualChars.ToCharArray())

        If position >= 0 Then

            Return ValidationResult.Warning(
            $"Unusual character '{value(position)}' found.")

        End If

        Return ValidationResult.Ok()

    End Function

    ' Defines the characters normally permitted in transcription cells.
    ' District and AaD have a few additional field-specific characters.
    Private Shared Function IsAllowedGeneralCellCharacter(
    field As GridField,
    ch As Char) As Boolean

        If Char.IsLetterOrDigit(ch) Then
            Return True
        End If

        ' FreeBMD extended character range.
        If AscW(ch) >= 192 AndAlso AscW(ch) <= 255 Then
            Return True
        End If

        If ch = "-"c OrElse
       ch = "."c OrElse
       ch = ","c OrElse
       ch = " "c Then

            Return True
        End If

        If "[]*?_{}".Contains(ch) Then
            Return True
        End If

        If field = GridField.District Then

            Return ch = "'"c OrElse
               ch = "&"c OrElse
               ch = ":"c OrElse
               ch = "("c OrElse
               ch = ")"c OrElse
               ch = "/"c OrElse
               ch = "\"c

        End If

        If field = GridField.AaD Then
            Return ch = "("c OrElse ch = ")"c
        End If

        Return False

    End Function
    Private Shared Function HasBalancedBrackets(
    value As String,
    openCharacter As Char,
    closeCharacter As Char) As Boolean

        Dim depth As Integer = 0

        For Each ch As Char In value

            If ch = openCharacter Then

                depth += 1

            ElseIf ch = closeCharacter Then

                depth -= 1

                If depth < 0 Then
                    Return False
                End If

            End If

        Next

        Return depth = 0

    End Function
    ' Warns about three identical consecutive letters in name fields.
    ' Digits break the sequence and are deliberately ignored.
    Private Shared Function ValidateTripleLetters(
    field As GridField,
    value As String) As ValidationResult

        If field <> GridField.Surname AndAlso
       field <> GridField.Forename AndAlso
       field <> GridField.Mother AndAlso
       field <> GridField.Spouse Then

            Return ValidationResult.Ok()
        End If

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim previous As Char = ChrW(0)
        Dim count As Integer = 0

        For Each ch As Char In value

            If Char.IsDigit(ch) Then

                previous = ChrW(0)
                count = 0
                Continue For

            End If

            Dim compare As Char =
            Char.ToUpperInvariant(ch)

            If compare = previous Then

                count += 1

                If count >= 3 Then

                    Return ValidationResult.Warning(
                    $"Triple letter '{ch}{ch}{ch}' is unusual.")

                End If

            Else

                previous = compare
                count = 1

            End If

        Next

        Return ValidationResult.Ok()

    End Function

    ' Returns True if the field contains any character which forms part
    ' of FreeBMD's Uncertain Character Format.
    Private Shared Function ContainsUcf(value As String) As Boolean

        If String.IsNullOrEmpty(value) Then
            Return False
        End If

        Return value.IndexOfAny({"_"c, "*"c, "["c, "]"c, "{"c, "}"c, "?"c}) >= 0

    End Function

    ' Validates FreeBMD Uncertain Character Format (UCF).
    ' Returns an empty string when the UCF is valid.
    Private Shared Function ValidateUcf(
    value As String) As ValidationResult

        ' ? represents an unambiguously empty field and therefore
        ' must be the only character in the field.
        If value.Contains("?"c) Then

            If value = "?" Then
                Return ValidationResult.Ok()
            End If

            Return ValidationResult.Error(
            "A question mark must be the only character in the field.")

        End If

        Dim index As Integer = 0

        While index < value.Length

            Dim character As Char = value(index)

            Select Case character

                Case "_"c

                    ' Underscores may be repeated, but cannot touch an asterisk.
                    If index > 0 AndAlso value(index - 1) = "*"c Then

                        Return ValidationResult.Error(
                        "An underscore must not immediately follow an asterisk.")

                    End If

                    If index < value.Length - 1 AndAlso
                   value(index + 1) = "*"c Then

                        Return ValidationResult.Error(
                        "An underscore must not immediately precede an asterisk.")

                    End If

                    index += 1

                Case "*"c

                    ' An asterisk cannot be immediately beside another
                    ' asterisk or an underscore.
                    If index > 0 Then

                        Dim previousCharacter As Char =
                        value(index - 1)

                        If previousCharacter = "*"c OrElse
                       previousCharacter = "_"c Then

                            Return ValidationResult.Error(
                            "An asterisk must not immediately follow an underscore or another asterisk.")

                        End If

                    End If

                    If index < value.Length - 1 Then

                        Dim nextCharacter As Char =
                        value(index + 1)

                        If nextCharacter = "*"c OrElse
                       nextCharacter = "_"c Then

                            Return ValidationResult.Error(
                            "An asterisk must not immediately precede an underscore or another asterisk.")

                        End If

                    End If

                    index += 1

                Case "["c

                    Dim closingBracket As Integer =
                    value.IndexOf("]"c, index + 1)

                    If closingBracket < 0 Then

                        Return ValidationResult.Error(
                        "An uncertain-character list is missing its closing bracket.")

                    End If

                    Dim contents As String =
                    value.Substring(
                        index + 1,
                        closingBracket - index - 1)

                    If contents.Length < 2 Then

                        Return ValidationResult.Error(
                        "An uncertain-character list must contain at least two characters.")

                    End If

                    If contents.Contains("["c) Then

                        Return ValidationResult.Error(
                        "Nested uncertain-character lists are not valid.")

                    End If

                    index = closingBracket + 1

                Case "]"c

                    Return ValidationResult.Error(
                    "A closing bracket has no matching opening bracket.")

                Case "{"c

                    ' A repeat count applies to the preceding character,
                    ' so it cannot appear at the beginning of a field.
                    If index = 0 Then

                        Return ValidationResult.Error(
                        "A repeat count must follow a character.")

                    End If

                    Dim closingBrace As Integer =
                    value.IndexOf("}"c, index + 1)

                    If closingBrace < 0 Then

                        Return ValidationResult.Error(
                        "A repeat count is missing its closing brace.")

                    End If

                    Dim contents As String =
                    value.Substring(
                        index + 1,
                        closingBrace - index - 1)

                    Dim result As ValidationResult =
                    ValidateRepeatCount(contents)

                    If result.IsError Then
                        Return result
                    End If

                    index = closingBrace + 1

                Case "}"c

                    Return ValidationResult.Error(
                    "A closing brace has no matching opening brace.")

                Case Else

                    index += 1

            End Select

        End While

        Return ValidationResult.Ok()

    End Function

    ' Validates the contents of a UCF repeat count such as:
    '
    '   {0,1}
    '   {1,4}
    '   {1,}
    '
    ' The maximum may be omitted, but the minimum and comma are required.
    Private Shared Function ValidateRepeatCount(
    contents As String) As ValidationResult

        Dim commaIndex As Integer =
        contents.IndexOf(","c)

        If commaIndex < 0 Then

            Return ValidationResult.Error(
            "A repeat count must contain a comma.")

        End If

        If contents.IndexOf(","c, commaIndex + 1) >= 0 Then

            Return ValidationResult.Error(
            "A repeat count may contain only one comma.")

        End If

        Dim minimumText As String =
        contents.Substring(0, commaIndex).Trim()

        Dim maximumText As String =
        contents.Substring(commaIndex + 1).Trim()

        If minimumText.Length = 0 Then

            Return ValidationResult.Error(
            "A repeat count must contain a minimum value.")

        End If

        Dim minimum As Integer

        If Not Integer.TryParse(minimumText, minimum) OrElse
       minimum < 0 Then

            Return ValidationResult.Error(
            "The minimum repeat count must be a non-negative number.")

        End If

        ' A blank maximum means that there is no upper limit,
        ' for example {1,}.
        If maximumText.Length = 0 Then
            Return ValidationResult.Ok()
        End If

        Dim maximum As Integer

        If Not Integer.TryParse(maximumText, maximum) OrElse
       maximum < 0 Then

            Return ValidationResult.Error(
            "The maximum repeat count must be a non-negative number.")

        End If

        If maximum < minimum Then

            Return ValidationResult.Error(
            "The maximum repeat count cannot be less than the minimum.")

        End If

        Return ValidationResult.Ok()

    End Function
    Public Shared Function ValidateSurname(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Warning("Surname is blank")
        End If

        Dim text As String = value.Trim()

        If text.Length > 0 AndAlso
           text(0) <> "+"c AndAlso
           text(0) <> "#"c Then

            Dim expected As String =
                If(ProjectValues.PageLetter, "").Trim()

            If Not String.IsNullOrWhiteSpace(expected) Then

                If Char.ToUpperInvariant(text(0)) <>
                   Char.ToUpperInvariant(expected(0)) Then

                    Return ValidationResult.Warning(
                        $"Surname does not match page letter '{expected.ToUpperInvariant()}'.")
                End If

            End If

        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateForename(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Warning("Forename is blank")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateDistrict(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        If value.Contains("(*)", StringComparison.Ordinal) Then
            Return ValidationResult.Error("(*) not allowed in District.")
        End If

        For Each ch As Char In value

            If Char.IsLetterOrDigit(ch) Then
                Continue For
            End If

            Select Case ch
                Case ","c, "."c, " "c, "'"c, "&"c, ":"c,
                     "-"c, "("c, ")"c, "/"c, "\"c

                    Continue For
            End Select

            Return ValidationResult.Warning(
                $"Invalid character in District name - {ch}")

        Next

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateVolume(value As String) As ValidationResult

        Dim actual As String = CodeFormat(value)
        Dim expected As String = ProjectValues.VNF

        If String.IsNullOrEmpty(actual) OrElse actual = "*" Then
            Return ValidationResult.Ok()
        End If

        Dim volume As String = value.Trim()

        If String.Equals(
            expected,
            "XX",
            StringComparison.OrdinalIgnoreCase) Then

            If Not IsValidRomanNumeral(volume) Then
                Return ValidationResult.Error(
                    "Volume must be a valid Roman numeral")
            End If

            Return ValidationResult.Ok()

        End If

        If String.IsNullOrWhiteSpace(expected) Then
            Return ValidationResult.Ok()
        End If

        If Not String.Equals(
            actual,
            expected,
            StringComparison.OrdinalIgnoreCase) Then

            Return ValidationResult.Error(
                $"Volume format does not match VNF {expected}")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateDistNum(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = value.Trim()

        For Each ch As Char In text

            If Not Char.IsLetterOrDigit(ch) Then
                Return ValidationResult.Warning(
                    "Unusual char in districtnum")
            End If

        Next

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidatePage(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        If value.Any(Function(ch) Not Char.IsDigit(ch)) Then

            Return ValidationResult.Warning(
                "Non-numeric page numbers are unusual.")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateAaD(value As String) As ValidationResult

        If ProjectValues.IgnoreAaD Then

            If Not String.IsNullOrWhiteSpace(value) Then

                Return ValidationResult.Error(
                    "Age at Death must be blank when Ignore AaD is enabled.")
            End If

        End If

        ' UCF values have already been validated by the common
        ' wildcard validator, so do not apply normal AaD rules.
        If ContainsWildChars(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = Compact(value)

        If text = "-" Then
            Return ValidationResult.Ok()
        End If

        If ProjectValues.Year < 1866 Then

            If Not String.IsNullOrWhiteSpace(text) Then
                Return ValidationResult.Error(
                    "Age at Death must be blank before 1866")
            End If

        Else

            If String.IsNullOrWhiteSpace(text) Then
                Return ValidationResult.Error(
                    "Age at Death must be completed from 1866 onward")
            End If

        End If

        Dim match As Match = AgeRegex.Match(text)

        If Not match.Success Then
            Return ValidationResult.Warning(
                "Age should be numeric 0-150")
        End If

        Dim age1 As Integer =
            Integer.Parse(match.Groups(1).Value)

        Dim age2 As Integer =
            If(
                match.Groups(2).Success,
                Integer.Parse(match.Groups(2).Value),
                age1)

        If age1 < 0 OrElse age1 > 150 OrElse
           age2 < 0 OrElse age2 > 150 Then

            Return ValidationResult.Warning(
                "Age should be numeric 0-150")
        End If

        If age1 > age2 Then
            Return ValidationResult.Error(
                "First age must not exceed second age")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateMother(value As String) As ValidationResult

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateSpouse(value As String) As ValidationResult

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateDoR(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = value.Trim()

        For Each ch As Char In text

            If Not Char.IsDigit(ch) AndAlso ch <> "."c Then
                Return ValidationResult.Warning(
                    "Unusual char in DoR")
            End If

        Next

        If text.Length <> 4 Then
            Return ValidationResult.Warning(
                "DoR should be 4 chars")
        End If

        Dim yearPart As String =
            text.Substring(2, 2)

        Dim expectedYear As Integer =
            ProjectValues.Year Mod 100

        Dim year As Integer

        If Not Integer.TryParse(yearPart, year) OrElse
           year <> expectedYear Then

            Return ValidationResult.Warning(
                $"DoR Year should be {expectedYear:00}")
        End If

        Dim monthPart As String =
            text.Substring(0, 2)

        Dim month As Integer

        If Not Integer.TryParse(monthPart, month) OrElse
           month < 1 OrElse month > 12 Then

            Return ValidationResult.Warning(
                "DoR Month not 01-12")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateReg(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String =
            Compact(value).Trim()

        If text.Length = 0 Then
            Return ValidationResult.Ok()
        End If

        If Not text.All(
            Function(ch) "0123456789.".Contains(ch)) Then

            Return ValidationResult.Warning(
                "Unusual char in Reg")
        End If

        If Not text.Contains("."c) Then
            Return ValidationResult.Warning(
                "Missing period in reg")
        End If

        If text.Length <> 5 Then
            Return ValidationResult.Warning(
                "Reg has unusual format")
        End If

        Dim yearText As String =
            text.Substring(text.Length - 2)

        Dim regYear As Integer

        If Not Integer.TryParse(yearText, regYear) Then
            Return ValidationResult.Warning(
                "Reg Year not numeric")
        End If

        Dim expectedYear As Integer =
            ProjectValues.Year Mod 100

        If regYear <> expectedYear Then
            Return ValidationResult.Warning(
                $"Reg Year should be {expectedYear}")
        End If

        Dim monthText As String =
            text.Substring(0, 2)

        Dim regMonth As Integer

        If Not Integer.TryParse(monthText, regMonth) Then
            Return ValidationResult.Warning(
                "Reg Month not numeric")
        End If

        If regMonth < 1 OrElse regMonth > 12 Then
            Return ValidationResult.Warning(
                "Reg Month not 01-12")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateRegNum(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = value.Trim()

        If text.Length < 2 OrElse text.Length > 5 Then
            Return ValidationResult.Warning(
                "Regnum is usually 2 to 5 chars")
        End If

        For Each ch As Char In text

            Dim upper As Char =
                Char.ToUpperInvariant(ch)

            If Not "/0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(upper) Then

                Return ValidationResult.Warning(
                    "Unusual char in Reg")
            End If

        Next

        If text(0) = "0"c Then
            Return ValidationResult.Warning(
                $"Unusual First char - {text(0)}")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateEntry(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = value.Trim()

        For Each ch As Char In text

            If Not Char.IsDigit(ch) Then
                Return ValidationResult.Warning(
                    "Entry is usually numeric")
            End If

        Next

        Dim entryNumber As Integer

        If Not Integer.TryParse(text, entryNumber) Then
            Return ValidationResult.Warning(
                "Entry is usually numeric")
        End If

        If entryNumber < 1 Then
            Return ValidationResult.Warning(
                "Entry should not be 0")
        End If

        If text.Length <> 3 Then
            Return ValidationResult.Warning(
                "Entry should be 3 characters")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateMonth(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String =
            value.Trim().ToUpperInvariant()

        If text.Length < 3 Then
            Return ValidationResult.Ok()
        End If

        Dim months() As String = {
            "JAN", "FEB", "MAR", "APR",
            "MAY", "JUN", "JUL", "AUG",
            "SEP", "OCT", "NOV", "DEC"
        }

        If months.Contains(text) Then
            Return ValidationResult.Ok()
        End If

        Return ValidationResult.Warning(
            "Unusual month")

    End Function


    Public Shared Function ValidateSource(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = value.Trim()

        If text.Length > 0 AndAlso
           Not Char.IsUpper(text(0)) Then

            Return ValidationResult.Warning(
                "Source normally starts with an uppercase letter")
        End If

        Return ValidationResult.Ok()

    End Function


    Public Shared Function ValidateDoB(value As String) As ValidationResult

        If String.IsNullOrWhiteSpace(value) Then
            Return ValidationResult.Ok()
        End If

        Dim text As String = Compact(value)
        Dim upper As String = text.Trim().ToUpperInvariant()

        If upper.StartsWith("ABOUT") OrElse
           upper.StartsWith("ABOU") OrElse
           upper.StartsWith("ABT") OrElse
           upper.StartsWith("UNKNOWN") Then

            Return ValidationResult.Ok()
        End If

        Dim parts() As String =
            SplitDate(text)

        If parts.Length <> 3 OrElse
           String.IsNullOrWhiteSpace(parts(0)) OrElse
           String.IsNullOrWhiteSpace(parts(1)) OrElse
           String.IsNullOrWhiteSpace(parts(2)) Then

            Return ValidationResult.Error(
                "Invalid DoB format")
        End If

        Dim dayText As String =
            parts(0).Trim()

        Dim monthText As String =
            parts(1).Trim().ToUpperInvariant()

        Dim yearText As String =
            parts(2).Trim()

        If dayText = "-" OrElse monthText = "-" Then
            Return ValidationResult.Ok()
        End If

        Dim day As Integer

        If Not Integer.TryParse(dayText, day) Then
            Return ValidationResult.Error(
                "DoB day should be a number")
        End If

        Dim year As Integer

        If Not Integer.TryParse(yearText, year) Then
            Return ValidationResult.Error(
                "DoB year should be a number")
        End If

        If day < 1 OrElse day > 31 Then
            Return ValidationResult.Error(
                "DoB days should be 1-31")
        End If

        Dim validMonths() As String = {
            "JA", "FE", "MR", "AP", "MY", "JN",
            "JL", "AU", "SE", "OC", "NO", "DE"
        }

        If Not validMonths.Contains(monthText) Then
            Return ValidationResult.Error(
                "Invalid DoB")
        End If

        If year > ProjectValues.Year Then
            Return ValidationResult.Error(
                $"DoB cannot be > {ProjectValues.Year}")
        End If

        Return ValidationResult.Ok()

    End Function
    Private Shared ReadOnly AgeRegex As New Regex(
    "^(\d{1,3})(?:-(\d{1,3}))?(m)?$",
    RegexOptions.IgnoreCase Or RegexOptions.Compiled)

    Private Shared Function Compact(input As String) As String

        If String.IsNullOrWhiteSpace(input) Then
            Return String.Empty
        End If

        Dim result As String = input.Trim()

        While result.Contains("  ")
            result = result.Replace("  ", " ")
        End While

        Return result

    End Function
    Private Shared Function ContainsWildChars(value As String) As Boolean

        If String.IsNullOrEmpty(value) Then
            Return False
        End If

        Return value.IndexOfAny(
            "[]*?_{}".ToCharArray()) >= 0

    End Function
    Private Shared Function CodeFormat(code As String) As String

        If String.IsNullOrWhiteSpace(code) Then
            Return ""
        End If

        If ContainsWildChars(code) Then
            Return "*"
        End If

        If ProjectValues.Year = 1993 AndAlso
           String.Equals(
               ProjectValues.BatchType,
               "M",
               StringComparison.OrdinalIgnoreCase) Then

            Return "99"
        End If

        If ProjectValues.Year > 1992 Then
            Return "999"
        End If

        If code.All(Function(ch) Char.IsDigit(ch)) Then
            Return "99"
        End If

        For Each ch As Char In code.ToUpperInvariant()

            If ch <> "X"c AndAlso
               ch <> "I"c AndAlso
               ch <> "V"c Then

                Return "9z"
            End If

        Next

        Return "XX"

    End Function

    Private Shared Function IsValidRomanNumeral(value As String) As Boolean

        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If

        Return Regex.IsMatch(
            value.Trim().ToUpperInvariant(),
            "^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$")

    End Function

    Private Shared Function SplitDate(input As String) As String()

        Dim text As String =
            Compact(input).ToUpperInvariant()

        Dim parts() As String =
            text.Split(
                " "c,
                StringSplitOptions.RemoveEmptyEntries)

        If parts.Length = 3 Then
            Return parts
        End If

        text = text.Replace(" ", "")

        Dim result(2) As String

        If text.Length < 4 Then

            result(0) = input

            Return result
        End If

        Dim monthPosition As Integer
        Dim yearPosition As Integer

        Dim second As Char = text(1)

        If Char.IsLetter(second) Then

            monthPosition = 1
            yearPosition = 3

        ElseIf Char.IsDigit(second) Then

            monthPosition = 2
            yearPosition = 4

        Else

            result(0) = input

            Return result

        End If

        result(0) =
            text.Substring(0, monthPosition)

        result(1) =
            text.Substring(
                monthPosition,
                yearPosition - monthPosition)

        result(2) =
            text.Substring(yearPosition)

        Return result

    End Function
End Class
