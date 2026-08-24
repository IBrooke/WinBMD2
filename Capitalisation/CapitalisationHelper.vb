Imports System.Text.RegularExpressions

Friend NotInheritable Class CapitalisationHelper

    Private Sub New()
    End Sub

    Public Shared Function Apply(value As String, mode As CapitalisationMode) As String

        Select Case mode

            Case CapitalisationMode.Upper
                Return value.ToUpper()

            Case CapitalisationMode.Lower
                Return value.ToLower()

            Case CapitalisationMode.Name
                Return ToNameCase(value)

            Case CapitalisationMode.AsTyped
                Return value

            Case Else
                Return value

        End Select

    End Function
    Public Shared Function ApplyTypedCharacter(character As Char, mode As CapitalisationMode, shiftPressed As Boolean, currentText As String, caretPosition As Integer) As Char

        ' Only letters are affected by capitalisation.
        If Not Char.IsLetter(character) Then
            Return character
        End If

        ' AsTyped leaves the character exactly as the user entered it.
        If mode = CapitalisationMode.AsTyped Then
            Return character
        End If

        Dim makeUpper As Boolean

        ' Determine the case that would normally be used for this character.
        Select Case mode

            Case CapitalisationMode.Upper
                makeUpper = True

            Case CapitalisationMode.Lower
                makeUpper = False

            Case CapitalisationMode.Name
                makeUpper = ShouldUppercaseNameCharacter(currentText, caretPosition)

            Case Else
                Return character

        End Select

        ' Holding Shift reverses the normal capitalisation for this one character.
        If shiftPressed Then
            makeUpper = Not makeUpper
        End If

        ' Apply the required case regardless of the case originally typed.
        If makeUpper Then
            Return Char.ToUpper(character)
        End If

        Return Char.ToLower(character)

    End Function
    Private Shared Function ShouldUppercaseNameCharacter(currentText As String, caretPosition As Integer) As Boolean

        ' The first letter of the value is uppercase.
        If caretPosition <= 0 OrElse String.IsNullOrEmpty(currentText) Then
            Return True
        End If

        ' A letter following a space, apostrophe or hyphen starts a new name part.
        Dim previousCharacter As Char = currentText(caretPosition - 1)

        If Char.IsWhiteSpace(previousCharacter) OrElse previousCharacter = "'"c OrElse previousCharacter = "-"c Then
            Return True
        End If

        ' Capitalise the third letter of a name beginning with Mc.
        If caretPosition >= 2 Then

            Dim firstCharacter As Char = currentText(caretPosition - 2)
            Dim secondCharacter As Char = currentText(caretPosition - 1)

            If Char.ToUpper(firstCharacter) = "M"c AndAlso Char.ToUpper(secondCharacter) = "C"c Then
                Return True
            End If

        End If

        Return False

    End Function
    Public Shared Function NormaliseReservedForename(value As String) As String

        Dim text As String = value.Trim()

        If text.Equals("(MALE)", StringComparison.OrdinalIgnoreCase) Then
            Return "(MALE)"
        End If

        If text.Equals("(FEMALE)", StringComparison.OrdinalIgnoreCase) Then
            Return "(FEMALE)"
        End If

        Return value

    End Function

    Private Shared Function ToNameCase(value As String) As String

        If String.IsNullOrWhiteSpace(value) Then
            Return value
        End If

        value = value.Trim()

        If Validator.IsDirective(value) Then
            Return value
        End If

        If value.IndexOfAny({"["c, "]"c, "{"c, "}"c, "*"c, "?"c, "_"c}) >= 0 Then
            Return value
        End If

        Dim parts() As String = Regex.Split(value, "(\s+)")

        For index As Integer = 0 To parts.Length - 1

            If Not String.IsNullOrWhiteSpace(parts(index)) Then
                parts(index) = ToNameCaseWord(parts(index))
            End If

        Next

        Return String.Concat(parts)

    End Function

    Private Shared Function ToNameCaseWord(word As String) As String

        word = word.ToLower()

        Dim chars() As Char = word.ToCharArray()
        Dim makeUpper As Boolean = True

        For index As Integer = 0 To chars.Length - 1

            If Char.IsLetter(chars(index)) Then

                If makeUpper Then
                    chars(index) = Char.ToUpper(chars(index))
                End If

                makeUpper = False

            Else

                makeUpper = chars(index) = "'"c OrElse chars(index) = "-"c

            End If

        Next

        word = New String(chars)

        If word.Length > 2 AndAlso word.StartsWith("Mc", StringComparison.Ordinal) AndAlso Char.IsLetter(word(2)) Then
            word = "Mc" & Char.ToUpper(word(2)) & word.Substring(3)
        End If

        Return word

    End Function

End Class
