Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Security.Authentication
Imports System.Text
Imports System.Threading

Friend NotInheritable Class FreeBmdUploader
    Private Enum RequestMode
        NewUpload
        ReplaceExisting
        DistrictFile
    End Enum

    Private NotInheritable Class ServerResponse
        Public Property StatusCode As HttpStatusCode
        Public Property ReasonPhrase As String = ""
        Public Property Version As Version = New Version(1, 1)
        Public Property Body As String = ""
        Public Property RedirectLocation As Uri
    End Class
    Private Enum UploadReplyResult
        Unknown
        Ok
        Warnings
        Failed
    End Enum

    Private NotInheritable Class ParsedReply

        Public Property Result As UploadReplyResult
        Public Property Reason As String = ""
        Public Property Errors As String = ""
        Public Property Warnings As String = ""
        Public Property UploadLink As String = ""
        Public Property Information As String = ""
        Public Property UpdateBlock As String = ""
        Public Property RawText As String = ""
        Public Property SystemError As Boolean

    End Class

    Private Const EndpointPath As String = "/cgi/bmd-files.pl"
    Private Const DistrictIntro As String = "winbmd/Districts.txt"
    Private Const MaxRedirects As Integer = 5

    Private ReadOnly _setStatus As Action(Of String)
    Private ReadOnly _log As Action(Of String)
    Private ReadOnly _askYesNo As Func(Of String, String, Boolean)
    Private _infoMessage As String = ""
    Private _gotDistrict As Boolean
    Private _districtFile As String = ""
    Private _districtVersion As String = ""
    Public Sub New(setStatus As Action(Of String), log As Action(Of String), askYesNo As Func(Of String, String, Boolean))
        _setStatus = setStatus
        _log = log
        _askYesNo = askYesNo
    End Sub
    Private Function CreateHttpClient() As HttpClient

        Dim handler As New SocketsHttpHandler()

        handler.AllowAutoRedirect = False
        handler.ConnectCallback = Function(context, cancellationToken) New ValueTask(Of Stream)(ConnectSecureStreamAsync(context, cancellationToken))

        Dim client As New HttpClient(handler)
        client.Timeout = TimeSpan.FromSeconds(60)

        Return client

    End Function
    Private Async Function ConnectSecureStreamAsync(context As SocketsHttpConnectionContext, cancellationToken As CancellationToken) As Task(Of Stream)

        Dim socket As New Socket(SocketType.Stream, ProtocolType.Tcp)
        socket.NoDelay = True

        Try

            Await socket.ConnectAsync(context.DnsEndPoint, cancellationToken)

            Dim networkStream As New NetworkStream(socket, ownsSocket:=True)
            Dim sslStream As New SslStream(networkStream, leaveInnerStreamOpen:=False)

            Dim sslOptions As New SslClientAuthenticationOptions With {
            .TargetHost = context.DnsEndPoint.Host
        }

            Await sslStream.AuthenticateAsClientAsync(sslOptions, cancellationToken)

            _log("----- SECURE CONNECTION -----")
            _log($"Server: {context.DnsEndPoint.Host}:{context.DnsEndPoint.Port}")
            _log($"TLS protocol negotiated: {sslStream.SslProtocol}")
            _log($"Cipher suite: {sslStream.NegotiatedCipherSuite}")

            Return sslStream

        Catch

            socket.Dispose()
            Throw

        End Try

    End Function
    Private Async Function SendOneRequestAsync(uri As Uri, content As HttpContent, cancellationToken As CancellationToken) As Task(Of ServerResponse)

        Using client As HttpClient = CreateHttpClient()

            _log("----- OUTGOING REQUEST -----")
            _log($"POST {uri}")
            _log($"Host: {uri.Host}:{uri.Port}")

            Using response As HttpResponseMessage = Await client.PostAsync(uri, content, cancellationToken)

                Dim body As String = Await response.Content.ReadAsStringAsync(cancellationToken)

                _log("----- INCOMING RESPONSE -----")
                _log($"HTTP status: {CInt(response.StatusCode)} {response.ReasonPhrase}")
                _log($"HTTP version: {response.Version}")

                If response.Headers.Location IsNot Nothing Then _log($"Location: {response.Headers.Location}")

                _log("----- DATA RECEIVED -----")
                Dim lineNumber As Integer = 0
                Using reader As New StringReader(body)

                    Do
                        Dim line As String = reader.ReadLine()
                        If line Is Nothing Then Exit Do

                        lineNumber += 1
                        _log($"{lineNumber:0000}: {line}")
                    Loop

                End Using
                _log($"----- END DATA RECEIVED: {lineNumber} lines -----")
                Return New ServerResponse With {
                .StatusCode = response.StatusCode,
                .ReasonPhrase = If(response.ReasonPhrase, ""),
                .Version = response.Version,
                .Body = body,
                .RedirectLocation = response.Headers.Location
            }

            End Using

        End Using

    End Function
    Private Async Function SendUploadRequestAsync(request As UploadRequest, mode As RequestMode, cancellationToken As CancellationToken) As Task(Of ParsedReply)

        Dim redirectCount As Integer = 0

        Do

            Dim uriBuilder As New UriBuilder(System.Uri.UriSchemeHttps, request.Site, request.Port, EndpointPath)

            _log($"Upload file: {request.BatchFilePath}")
            _log($"Upload name: {request.UploadName}")
            _log($"Upload server: {request.Site}:{request.Port}")
            _log($"District version: {request.CurrentDistrictVersion}")

            Using content As MultipartFormDataContent = BuildUploadContent(request, mode)

                Dim response As ServerResponse = Await SendOneRequestAsync(uriBuilder.Uri, content, cancellationToken)

                If CInt(response.StatusCode) >= 300 AndAlso CInt(response.StatusCode) <= 399 Then

                    If response.RedirectLocation Is Nothing Then
                        Return New ParsedReply With {.RawText = response.Body}
                    End If

                    Dim redirectUri As Uri = response.RedirectLocation

                    If Not redirectUri.IsAbsoluteUri Then
                        redirectUri = New Uri(uriBuilder.Uri, redirectUri)
                    End If

                    If Not redirectUri.Scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidOperationException("FreeBMD returned a redirect which was not HTTPS.")
                    End If

                    If redirectUri.Host.IndexOf("freebmd", StringComparison.OrdinalIgnoreCase) < 0 Then
                        Throw New InvalidOperationException("FreeBMD returned a redirect to an unexpected server.")
                    End If

                    redirectCount += 1

                    If redirectCount > MaxRedirects Then
                        Throw New InvalidOperationException("Too many redirects were returned by the FreeBMD server.")
                    End If

                    request.Site = redirectUri.Host
                    request.Port = If(redirectUri.IsDefaultPort, 443, redirectUri.Port)

                    _log($"Following HTTP redirect {redirectCount}: {redirectUri}")
                    _log($"Upload site changed for current request only to: {request.Site}:{request.Port}")

                    Continue Do

                End If

                Dim parsed As ParsedReply = ParseResponse(response.Body)

                _log("----- PARSED RESPONSE -----")
                _log($"Result: {parsed.Result}")
                _log($"Reason: {parsed.Reason}")

                If Not String.IsNullOrWhiteSpace(parsed.UploadLink) Then _log($"UploadLink: {parsed.UploadLink}")
                If Not String.IsNullOrWhiteSpace(parsed.Information) Then _log($"Information: {parsed.Information}")
                If Not String.IsNullOrWhiteSpace(parsed.UpdateBlock) Then _log("District/update information was returned by the server.")

                Return parsed

            End Using

        Loop

    End Function
    Friend Async Function UploadFileAsync(request As UploadRequest, cancellationToken As CancellationToken) As Task(Of UploadResult)

        ResetState()
        ValidateRequest(request)

        If Not File.Exists(request.BatchFilePath) Then Throw New FileNotFoundException("Upload file not found.", request.BatchFilePath)

        Return Await RunAsync(request, cancellationToken)

    End Function

    Private Async Function RunAsync(request As UploadRequest, cancellationToken As CancellationToken) As Task(Of UploadResult)

        Dim mode As RequestMode = RequestMode.NewUpload

        Do

            cancellationToken.ThrowIfCancellationRequested()

            If mode = RequestMode.ReplaceExisting Then
                SetStatus("Updating existing file...")
            Else
                SetStatus("Uploading file...")
            End If

            Dim parsed As ParsedReply = Await SendUploadRequestAsync(request, mode, cancellationToken)

            If Not String.IsNullOrWhiteSpace(parsed.Information) Then _infoMessage = parsed.Information

            If parsed.SystemError Then Return Fail("FreeBMD System Error")

            Dim uploadSiteChanged As Boolean = TryUpdateUploadRequestSite(parsed.UploadLink, request)

            ProcessDistrictUpdate(parsed, request)

            Select Case parsed.Result

                Case UploadReplyResult.Ok

                    Return Success($"{request.UploadName} uploaded successfully", request)

                Case UploadReplyResult.Warnings

                    Return Success(parsed.Warnings, request)

                Case UploadReplyResult.Failed

                    Dim reason As String = parsed.Reason.ToUpperInvariant()

                    If reason = "FILEEXISTS" Then

                        Dim replace As Boolean = request.AutoReplaceExistingFile

                        If Not replace AndAlso request.AskBeforeReplace Then
                            replace = _askYesNo("There is already a file on FreeBMD with that name." & Environment.NewLine & "Do you want to replace it with this one?", "File Already Exists")
                            _log($"User response to replace existing file: {If(replace, "Yes", "No")}")
                        End If

                        If Not replace Then Return Fail("File not uploaded as it already exists on FreeBMD")

                        mode = RequestMode.ReplaceExisting
                        Continue Do

                    End If

                    If reason = "FILEDOESNOTEXIST" Then
                        mode = RequestMode.NewUpload
                        SetStatus("Resending as new file")
                        Continue Do
                    End If

                    If reason = "WRONGDOMAIN" Then

                        If uploadSiteChanged Then
                            SetStatus("Trying different machine")
                            Continue Do
                        End If

                        Return Fail("The FreeBMD machine address has changed but I did not receive a valid new https upload address.")

                    End If

                    Return Fail(BuildFailureMessage(reason, parsed.Errors))

                Case Else

                    Return Fail("There was no recognised response from FreeBMD.")

            End Select

        Loop

    End Function

    Private Sub ValidateRequest(request As UploadRequest)

        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If String.IsNullOrWhiteSpace(request.Username) Then Throw New InvalidOperationException("No FreeBMD Username specified.")
        If String.IsNullOrWhiteSpace(request.Password) Then Throw New InvalidOperationException("No FreeBMD Password specified.")
        If String.IsNullOrWhiteSpace(request.Site) Then Throw New InvalidOperationException("No FreeBMD site specified.")
        If request.Port <= 0 Then request.Port = 443
        If String.IsNullOrWhiteSpace(request.BatchFilePath) Then Throw New InvalidOperationException("No upload file specified.")
        If String.IsNullOrWhiteSpace(request.UploadName) Then Throw New InvalidOperationException("No upload name specified.")

    End Sub

    Private Sub ResetState()

        _gotDistrict = False
        _districtFile = ""
        _districtVersion = ""
        _infoMessage = ""

    End Sub

    Private Sub SetStatus(text As String)

        If _setStatus IsNot Nothing Then _setStatus(text)

    End Sub

    Private Function Success(message As String, request As UploadRequest) As UploadResult

        If _gotDistrict AndAlso Not String.IsNullOrWhiteSpace(_districtFile) Then

            SaveNewDistrictFile(request)

            If String.IsNullOrWhiteSpace(message) Then
                message = "I downloaded a new District File"
            Else
                message &= Environment.NewLine & "I downloaded a new District File"
            End If

        End If

        SetStatus("Upload complete")

        Return New UploadResult With {
        .Success = True,
        .Message = message,
        .AsName = request.UploadName,
        .InfoMessage = _infoMessage,
        .DownloadedNewDistrictFile = _gotDistrict,
        .NewDistrictVersion = _districtVersion,
        .Site = request.Site,
        .Port = request.Port
    }

    End Function

    Private Function Fail(message As String) As UploadResult

        SetStatus("Upload failed")

        Return New UploadResult With {
        .Success = False,
        .Message = message,
        .InfoMessage = _infoMessage,
        .DownloadedNewDistrictFile = False
    }

    End Function
    Private Function TryUpdateUploadRequestSite(rawLink As String, request As UploadRequest) As Boolean

        Dim host As String = ""
        Dim port As Integer = 443

        If Not TryExtractUploadTarget(rawLink, host, port) Then Return False

        Dim currentPort As Integer = If(request.Port <= 0, 443, request.Port)

        If String.Equals(request.Site, host, StringComparison.OrdinalIgnoreCase) AndAlso currentPort = port Then Return False

        request.Site = host
        request.Port = port

        _log($"Upload site changed for current request only to: {request.Site}:{request.Port}")

        Return True

    End Function

    Private Function TryExtractUploadTarget(rawLink As String, ByRef host As String, ByRef port As Integer) As Boolean

        host = ""
        port = 443

        If String.IsNullOrWhiteSpace(rawLink) Then Return False

        rawLink = rawLink.Trim()

        Dim uri As Uri = Nothing

        If Not System.Uri.TryCreate(rawLink, UriKind.Absolute, uri) Then
            _log($"Ignoring malformed uploadlink: {rawLink}")
            Return False
        End If

        If Not uri.Scheme.Equals(System.Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) Then
            _log($"Ignoring non-https uploadlink: {rawLink}")
            Return False
        End If

        If String.IsNullOrWhiteSpace(uri.Host) Then
            _log($"Ignoring uploadlink with no host: {rawLink}")
            Return False
        End If

        If uri.Host.IndexOf("freebmd", StringComparison.OrdinalIgnoreCase) < 0 Then
            _log($"Ignoring uploadlink to non-FreeBMD host: {rawLink}")
            Return False
        End If

        host = uri.Host
        port = If(uri.IsDefaultPort, 443, uri.Port)

        Return True

    End Function

    Private Function BuildFailureMessage(reason As String, errors As String) As String

        Select Case reason
            Case "MALFORMEDREQUEST"
                Return "FreeBMD has reported a Malformed Request due to: " & errors
            Case "SYSTEMERROR"
                Return "FreeBMD has reported a System Error: " & errors
            Case "HTTPERROR"
                Return errors
            Case "USERNOTKNOWN"
                Return "The UserID you specified is not known by FreeBMD."
            Case "PASSWORDFAILED"
                Return "The password you specified is incorrect for that UserID."
            Case "ACCOUNTSUSPENDED"
                Return "The UserName you specified is suspended. Contact your Coordinator."
            Case "NEEDSCHALLENGE"
                Return "The UserID you specified has not completed registration."
            Case "SYNTAXERROR"
                Return "Your file contains Syntax Error(s): " & errors
            Case Else
                Return "FreeBMD has sent an unknown reply."
        End Select

    End Function
    Private Function BuildUploadContent(request As UploadRequest, mode As RequestMode) As MultipartFormDataContent

        Dim fileBytes As Byte() = File.ReadAllBytes(request.BatchFilePath)
        Dim lines As String() = File.ReadAllLines(request.BatchFilePath, Encoding.GetEncoding("ISO-8859-1"))
        Dim form As New MultipartFormDataContent()
        Dim lineNumber As Integer = 0

        _log("----- DATA SENT -----")

        AddLoggedFormValue(form, "UploadAgent", "InterfaceVersion1.5", lineNumber)
        AddLoggedFormValue(form, "user", request.Username, lineNumber)
        AddLoggedFormValue(form, "password", request.Password, lineNumber)

        If mode = RequestMode.ReplaceExisting Then
            AddLoggedFormValue(form, "file_update", request.UploadName, lineNumber)
        Else
            AddLoggedFormValue(form, "file", request.UploadName, lineNumber)
        End If

        AddLoggedFormValue(form, "data_version", $"{DistrictIntro}:{request.CurrentDistrictVersion}", lineNumber)

        Dim fileContent As New ByteArrayContent(fileBytes)
        fileContent.Headers.ContentType = New MediaTypeHeaderValue("text/plain")
        form.Add(fileContent, "content2", request.UploadName)

        lineNumber += 1
        _log($"{lineNumber:0000}: content2={request.UploadName}")

        For Each line As String In lines
            lineNumber += 1
            _log($"{lineNumber:0000}: {line}")
        Next

        _log($"----- END DATA SENT: {lineNumber} lines, {fileBytes.Length} file bytes -----")

        Return form

    End Function
    Private Sub AddLoggedFormValue(form As MultipartFormDataContent, name As String, value As String, ByRef lineNumber As Integer)

        Dim actualValue As String = If(value, "")
        form.Add(New StringContent(actualValue), name)

        lineNumber += 1
        _log($"{lineNumber:0000}: {name}={actualValue}")

    End Sub
    Private Function ParseResponse(responseText As String) As ParsedReply

        If String.IsNullOrWhiteSpace(responseText) Then Return New ParsedReply()

        If responseText.IndexOf("FreeBMD System Error", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return New ParsedReply With {.SystemError = True}
        End If

        Dim intro As Integer = responseText.IndexOf("<FILEUPLOAD", StringComparison.OrdinalIgnoreCase)
        If intro < 0 Then Return New ParsedReply()

        Dim body As String = responseText.Substring(intro)
        Dim lines As String() = SplitLf(body)
        Dim firstLine As String = If(lines.Length > 0, lines(0).Trim(), "")
        Dim secondLine As String = If(lines.Length > 1, lines(1).Trim(), "")

        Dim result As UploadReplyResult = UploadReplyResult.Unknown

        If firstLine.Equals("<FILEUPLOAD RESULT=OK", StringComparison.OrdinalIgnoreCase) Then
            result = UploadReplyResult.Ok
        ElseIf firstLine.Equals("<FILEUPLOAD RESULT=WARNINGS", StringComparison.OrdinalIgnoreCase) Then
            result = UploadReplyResult.Warnings
        ElseIf firstLine.Equals("<FILEUPLOAD RESULT=FAILED", StringComparison.OrdinalIgnoreCase) Then
            result = UploadReplyResult.Failed
        End If

        Dim reason As String = ""

        If secondLine.StartsWith("REASON=", StringComparison.OrdinalIgnoreCase) Then
            reason = secondLine.Substring(7).TrimEnd(">"c)
        End If

        Return New ParsedReply With {
        .Result = result,
        .Reason = reason,
        .Errors = ExtractBlock(body, "errors"),
        .Warnings = ExtractBlock(body, "warnings"),
        .UploadLink = ExtractBlock(body, "uploadlink"),
        .Information = ExtractBlock(body, "information"),
        .UpdateBlock = ExtractUpdateBlock(body),
        .RawText = body
    }

    End Function

    Private Function ExtractBlock(text As String, tagName As String) As String

        Dim start As Integer = text.IndexOf("<" & tagName, StringComparison.OrdinalIgnoreCase)
        If start < 0 Then Return ""

        Dim openEnd As Integer = text.IndexOf(">"c, start)
        If openEnd < 0 Then Return ""

        Dim finish As Integer = text.IndexOf("</" & tagName, openEnd + 1, StringComparison.OrdinalIgnoreCase)
        If finish < 0 Then Return ""

        Dim content As String = text.Substring(openEnd + 1, finish - openEnd - 1)
        Dim lines As String() = SplitLf(content)

        For i As Integer = 0 To lines.Length - 1
            lines(i) = lines(i).TrimEnd(ControlChars.Cr)
        Next

        Return String.Join(Environment.NewLine, lines)

    End Function

    Private Function ExtractUpdateBlock(text As String) As String

        Dim start As Integer = text.IndexOf("<update ", StringComparison.OrdinalIgnoreCase)
        If start < 0 Then Return ""

        Dim finish As Integer = text.IndexOf("</update", start, StringComparison.OrdinalIgnoreCase)
        If finish < 0 Then Return ""

        Dim closeEnd As Integer = text.IndexOf(">"c, finish)
        If closeEnd < 0 Then Return ""

        Return text.Substring(start, closeEnd - start + 1)

    End Function
    Private Function SplitLf(text As String) As String()

        Return text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(ControlChars.Lf)

    End Function
    Private Sub ProcessDistrictUpdate(parsed As ParsedReply, request As UploadRequest)

        If String.IsNullOrWhiteSpace(parsed.UpdateBlock) Then Return
        If parsed.UpdateBlock.IndexOf(DistrictIntro, StringComparison.OrdinalIgnoreCase) < 0 Then Return

        Dim keyStart As Integer = parsed.UpdateBlock.IndexOf("key=", StringComparison.OrdinalIgnoreCase)

        If keyStart >= 0 Then

            keyStart += 4

            Dim keyEnd As Integer = parsed.UpdateBlock.IndexOfAny({" "c, ">"c, ControlChars.Tab}, keyStart)

            If keyEnd < 0 Then
                _districtVersion = parsed.UpdateBlock.Substring(keyStart).Trim()
            Else
                _districtVersion = parsed.UpdateBlock.Substring(keyStart, keyEnd - keyStart).Trim()
            End If

        End If

        Dim newDistrictFile As String = ExtractBlock(parsed.UpdateBlock, "update")
        If String.IsNullOrWhiteSpace(newDistrictFile) Then Return

        _districtFile = newDistrictFile
        _gotDistrict = True

        If Not String.IsNullOrWhiteSpace(_districtVersion) Then request.CurrentDistrictVersion = _districtVersion

        _log($"New District file returned by server.")
        _log($"New District version: {_districtVersion}")

    End Sub
    Private Sub SaveNewDistrictFile(request As UploadRequest)

        If String.IsNullOrWhiteSpace(request.DistrictFolderPath) Then Throw New InvalidOperationException("District folder path not supplied.")

        Dim folder As String = request.DistrictFolderPath
        Dim currentFile As String = Path.Combine(folder, "WinBMD_Districts.txt")
        Dim tempFile As String = Path.Combine(folder, "NewDistrictFile.txt")
        Dim backupFile As String = Path.Combine(folder, "CopyofDistrictFile" & DateTime.Now.ToString("ddMMyyHHmmss") & ".txt")

        If Not File.Exists(currentFile) Then Throw New FileNotFoundException("The current District file could not be found.", currentFile)

        _log($"Installing new District file.")
        _log($"Current District file: {currentFile}")
        _log($"Backup District file: {backupFile}")
        _log($"New District version: {_districtVersion}")

        Dim lines As String() = SplitLf(_districtFile)

        For i As Integer = 0 To lines.Length - 1
            lines(i) = lines(i).Replace(vbCr, " ").TrimEnd()
        Next

        File.WriteAllLines(tempFile, lines, New UTF8Encoding(False))

        If File.Exists(backupFile) Then File.Delete(backupFile)

        File.Move(currentFile, backupFile)
        File.Move(tempFile, currentFile)

        Try

            If Not DistrictData.LoadBaseDistricts() Then Throw New InvalidOperationException("The new District file could not be read.")
            If Not DistrictData.LoadAliveDistricts() Then Throw New InvalidOperationException("The active District list could not be rebuilt from the new District file.")

        Catch ex As Exception

            _log($"New District file could not be loaded. Restoring previous District file.")

            If File.Exists(tempFile) Then File.Delete(tempFile)
            If File.Exists(currentFile) Then File.Move(currentFile, tempFile)

            File.Move(backupFile, currentFile)

            ' Reload the original file so the in-memory district collections also
            ' return to the state they were in before the attempted update.
            DistrictData.LoadBaseDistricts()
            DistrictData.LoadAliveDistricts()

            Throw New InvalidOperationException("Unable to read the new District file, so I reinstated the old one.", ex)

        End Try

        ' Do not record the new version until the new file has been installed and
        ' both the base and current-batch district collections have reloaded successfully.
        If Not String.IsNullOrWhiteSpace(_districtVersion) Then
            ProjectValues.DistrictVersion = _districtVersion
            ProjectValuesStore.Save()
        End If

        _log($"New District file installed successfully.")
        _log($"District version now: {ProjectValues.DistrictVersion}")

    End Sub
    Friend NotInheritable Class UploadRequest

        Public Property BatchFilePath As String = ""
        Public Property UploadName As String = ""
        Public Property Username As String = ""
        Public Property Password As String = ""
        Public Property Site As String = ProjectValues.UploadServerUrl
        Public Property Port As Integer = 443
        Public Property CurrentDistrictVersion As String = ""
        Public Property DistrictFolderPath As String = ""
        Public Property Year As Integer
        Public Property Quarter As Integer
        Public Property AutoReplaceExistingFile As Boolean
        Public Property AskBeforeReplace As Boolean

    End Class

    Friend NotInheritable Class UploadResult

        Public Property Success As Boolean
        Public Property Message As String = ""
        Public Property AsName As String = ""
        Public Property InfoMessage As String = ""
        Public Property DownloadedNewDistrictFile As Boolean
        Public Property NewDistrictVersion As String = ""
        Public Property Site As String = ""
        Public Property Port As Integer

    End Class

End Class
