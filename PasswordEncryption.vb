Imports System.Security.Cryptography
Imports System.Text

Friend Module PasswordEncryption

    Private Const VersionPrefix As String = "v1:"

    Friend Function Encrypt(password As String, userId As String) As String

        If String.IsNullOrEmpty(password) Then Return ""

        Dim key() As Byte = CreateKey(userId)

        Dim nonce(11) As Byte
        RandomNumberGenerator.Fill(nonce)

        Dim plainBytes() As Byte = Encoding.UTF8.GetBytes(password)
        Dim cipherBytes(plainBytes.Length - 1) As Byte
        Dim tag(15) As Byte

        Using aes As New AesGcm(key, 16)
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag)
        End Using

        Return VersionPrefix &
            Convert.ToBase64String(nonce) & ":" &
            Convert.ToBase64String(tag) & ":" &
            Convert.ToBase64String(cipherBytes)

    End Function

    Friend Function Decrypt(value As String, userId As String) As String

        If String.IsNullOrEmpty(value) Then Return ""

        ' Existing settings may still contain a plain-text password.
        If Not value.StartsWith(VersionPrefix, StringComparison.Ordinal) Then Return value

        Try
            Dim parts() As String = value.Split(":"c)

            If parts.Length <> 4 Then Return ""

            Dim nonce() As Byte = Convert.FromBase64String(parts(1))
            Dim tag() As Byte = Convert.FromBase64String(parts(2))
            Dim cipherBytes() As Byte = Convert.FromBase64String(parts(3))
            Dim plainBytes(cipherBytes.Length - 1) As Byte
            Dim key() As Byte = CreateKey(userId)

            Using aes As New AesGcm(key, 16)
                aes.Decrypt(nonce, cipherBytes, tag, plainBytes)
            End Using

            Return Encoding.UTF8.GetString(plainBytes)

        Catch
            Return ""
        End Try

    End Function

    Private Function CreateKey(userId As String) As Byte()

        If userId Is Nothing Then userId = ""

        Dim keyText As String = FreeMonths & userId

        Return SHA256.HashData(Encoding.UTF8.GetBytes(keyText))

    End Function

End Module