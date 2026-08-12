Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Text.RegularExpressions
Public NotInheritable Class ScanSearchResult

    Public Property Success As Boolean
    Public Property LocalPath As String = ""
    Public Property RemoteUrl As String = ""
    Public Property Message As String = ""
    Public Property Unauthorized As Boolean
    Public Property AlternativeRemoteUrl As String = ""
    Public Property AlternativeFound As Boolean
    Public Property AlternativeSourceRef As String = ""
    Public Property AlternativeFileName As String = ""

End Class
Public NotInheritable Class ScanLocator
    Private ReadOnly _http As HttpClient
    Private ReadOnly _hasCredentials As Boolean

    Private _lastErrorMessage As String = ""
    Private _alternativeSourceRef As String = ""
    Private _alternativeFileName As String = ""
    Private _lastRequestUnauthorized As Boolean
    Private _alternativeRemoteUrl As String = ""
    Private Enum ScanCandidateKind
        Exact = 0
        PreviousDoublePage = 1
    End Enum

    Private NotInheritable Class ScanCandidate
        Public Property RemoteUrl As String = ""
        Public Property FileName As String = ""
        Public Property SourceReference As String = ""
        Public Property Kind As ScanCandidateKind
        Public Property ImageRank As Integer
        Public Property CrawlOrder As Integer
    End Class
    Public Sub New()

        Dim user As String = ProjectValues.UserName.Trim()
        Dim password As String = ProjectValues.UserPW.Trim()

        _hasCredentials =
        Not String.IsNullOrWhiteSpace(user) AndAlso
        Not String.IsNullOrWhiteSpace(password)

        Dim handler As New HttpClientHandler With {
        .AllowAutoRedirect = True
    }

        If _hasCredentials Then
            handler.Credentials = New NetworkCredential(user, password)
        End If

        _http = New HttpClient(handler)

    End Sub
    Public Function BuildStartUrl() As String

        Dim url As String = ProjectConstants.ScanBaseUrl.TrimEnd("/"c)

        url &= "/" & ProjectValues.Year.ToString()

        Select Case ProjectValues.BatchType

            Case "B"
                url &= "/Births"

            Case "M"
                url &= "/Marriages"

            Case "D"
                url &= "/Deaths"

            Case Else
                Throw New InvalidOperationException(
                $"Unknown event type '{ProjectValues.BatchType}'.")

        End Select

        If ProjectValues.Quarter > 0 Then

            Dim monthFolder As String

            Select Case ProjectValues.Quarter

                Case 1
                    monthFolder = "March"

                Case 2
                    monthFolder = "June"

                Case 3
                    monthFolder = "September"

                Case 4
                    monthFolder = "December"

                Case Else
                    Throw New InvalidOperationException(
                    $"Unknown quarter '{ProjectValues.Quarter}'.")

            End Select

            url &= "/" & monthFolder

        End If

        Return url & "/"

    End Function
    Private Async Function GetHtmlAsync(url As String) As Task(Of String)

        Try

            Using response As HttpResponseMessage = Await _http.GetAsync(url)

                If response.StatusCode = HttpStatusCode.Unauthorized Then
                    _lastRequestUnauthorized = True
                    _lastErrorMessage = "The scan website did not accept your UserName or Password."
                    Return Nothing
                End If

                If response.StatusCode = HttpStatusCode.NotFound Then
                    Return Nothing
                End If

                If response.StatusCode = HttpStatusCode.Forbidden Then
                    _lastErrorMessage = "Access to the scan website was denied."
                    Return Nothing
                End If

                If Not response.IsSuccessStatusCode Then
                    _lastErrorMessage = $"Scan website returned {CInt(response.StatusCode)} {response.ReasonPhrase}."
                    Return Nothing
                End If

                Return Await response.Content.ReadAsStringAsync()

            End Using

        Catch ex As HttpRequestException

            _lastErrorMessage = "Unable to contact the scan website: " & ex.Message
            Return Nothing

        Catch ex As TaskCanceledException

            _lastErrorMessage = "The scan website request timed out."
            Return Nothing

        Catch ex As Exception

            _lastErrorMessage = "Unexpected error while reading scan website: " & ex.Message
            Return Nothing

        End Try

    End Function
    Private Async Function DownloadFileAsync(url As String, localPath As String) As Task(Of Boolean)

        Dim tempPath As String = localPath & ".part"

        Try

            If File.Exists(tempPath) Then
                File.Delete(tempPath)
            End If

            Using response As HttpResponseMessage =
            Await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)

                If response.StatusCode = HttpStatusCode.Unauthorized Then
                    _lastRequestUnauthorized = True
                    _lastErrorMessage = "The scan website did not accept your UserName or Password."
                    Return False
                End If

                If Not response.IsSuccessStatusCode Then
                    _lastErrorMessage = $"Download failed: {CInt(response.StatusCode)} {response.ReasonPhrase}."
                    Return False
                End If

                Using input As Stream = Await response.Content.ReadAsStreamAsync()
                    Using output As FileStream = File.Create(tempPath)
                        Await input.CopyToAsync(output)
                    End Using
                End Using

            End Using

            Dim info As New FileInfo(tempPath)

            If info.Length = 0 Then
                _lastErrorMessage = "Downloaded scan file was empty."
                Return False
            End If

            If File.Exists(localPath) Then
                File.Delete(localPath)
            End If

            File.Move(tempPath, localPath)

            Return True

        Catch ex As HttpRequestException

            _lastErrorMessage = "Unable to download scan: " & ex.Message
            Return False

        Catch ex As TaskCanceledException

            _lastErrorMessage = "The scan download timed out."
            Return False

        Catch ex As Exception

            _lastErrorMessage = "Unexpected error while downloading scan: " & ex.Message
            Return False

        Finally

            If File.Exists(tempPath) Then

                Try
                    File.Delete(tempPath)
                Catch
                End Try

            End If

        End Try

    End Function
    Private NotInheritable Class ParsedScanName

        Public Property IsValid As Boolean
        Public Property NameOnly As String = ""
        Public Property Prefix As String = ""
        Public Property PageLetterPart As String = ""
        Public Property Page As Integer
        Public Property PageSuffix As String = ""

    End Class

    Public Function TryFindLocalScan(scanFolder As String) As String

        If Not Directory.Exists(scanFolder) Then
            DebugLog.Write($"[SCAN LOCAL] Folder does not exist: {scanFolder}")
            Return Nothing
        End If

        Dim extensions() As String = {
            ".jpg",
            ".jpeg",
            ".tif",
            ".tiff",
            ".png",
            ".gif"
        }

        For Each extension As String In extensions

            For Each fullPath As String In Directory.EnumerateFiles(scanFolder, "*" & extension, SearchOption.TopDirectoryOnly)

                Dim info As FileInfo

                Try
                    info = New FileInfo(fullPath)
                Catch ex As Exception
                    DebugLog.Write($"[SCAN LOCAL] Could not read file info for '{fullPath}': {ex.Message}")
                    Continue For
                End Try

                If info.Length <= 0 Then
                    DebugLog.Write($"[SCAN LOCAL] Ignoring empty file '{fullPath}'.")
                    Continue For
                End If

                Dim fileName As String = Path.GetFileName(fullPath)

                If RemoteFilenameMatchesPage(fileName, ProjectValues.Page, ProjectValues.PageSuffix) Then
                    DebugLog.Write($"[SCAN LOCAL] Match found: {fullPath}")
                    Return fullPath
                End If

            Next

        Next

        DebugLog.Write("[SCAN LOCAL] No local scan found.")

        Return Nothing

    End Function

    Private Shared Function GetExpectedPrefix() As String

        Dim quarterPart As String

        If ProjectValues.Year >= ProjectConstants.FirstYearNoQtrs Then
            quarterPart = ""
        Else
            quarterPart = ProjectValues.Quarter.ToString().Trim()
        End If

        Return $"{ProjectValues.Year:0000}{ProjectValues.BatchType}{quarterPart}"

    End Function

    Private Shared Function ParseScanName(fileName As String) As ParsedScanName

        Dim nameOnly As String = Path.GetFileNameWithoutExtension(fileName).Trim()

        If String.IsNullOrWhiteSpace(nameOnly) Then
            Return New ParsedScanName With {.IsValid = False}
        End If

        Dim parts() As String = nameOnly.Split("-"c)

        Dim prefix As String
        Dim pageLetterPart As String
        Dim pagePart As String

        If parts.Length >= 3 Then

            prefix = parts(0).Trim()
            pageLetterPart = parts(1).Trim()
            pagePart = parts(2).Trim()

        ElseIf parts.Length = 2 Then

            prefix = parts(0).Trim()
            pageLetterPart = ProjectValues.PageLetter.Trim()
            pagePart = parts(1).Trim()

        Else

            Return New ParsedScanName With {
                .IsValid = False,
                .NameOnly = nameOnly
            }

        End If

        If String.IsNullOrWhiteSpace(prefix) Then
            Return New ParsedScanName With {
                .IsValid = False,
                .NameOnly = nameOnly
            }
        End If

        If String.IsNullOrWhiteSpace(pageLetterPart) OrElse pageLetterPart.Length >= 4 Then
            Return New ParsedScanName With {
                .IsValid = False,
                .NameOnly = nameOnly
            }
        End If

        Dim page As Integer
        Dim pageSuffix As String = ""

        If Integer.TryParse(pagePart, page) Then

            ' Nothing else needed.

        Else

            If pagePart.Length < 2 Then
                Return New ParsedScanName With {
                    .IsValid = False,
                    .NameOnly = nameOnly
                }
            End If

            pageSuffix = pagePart(pagePart.Length - 1).ToString().ToUpperInvariant()

            Dim pageDigits As String = pagePart.Substring(0, pagePart.Length - 1)

            If Not Integer.TryParse(pageDigits, page) Then
                Return New ParsedScanName With {
                    .IsValid = False,
                    .NameOnly = nameOnly
                }
            End If

        End If

        Return New ParsedScanName With {
            .IsValid = True,
            .NameOnly = nameOnly,
            .Prefix = prefix,
            .PageLetterPart = pageLetterPart.ToUpperInvariant(),
            .Page = page,
            .PageSuffix = pageSuffix
        }

    End Function

    Private Shared Function PrefixMatchesCurrentBatch(prefix As String) As Boolean

        Return String.Equals(
            prefix,
            GetExpectedPrefix(),
            StringComparison.OrdinalIgnoreCase)

    End Function

    Private Shared Function PageLetterMatchesCurrentBatch(pageLetterPart As String) As Boolean

        Dim wanted As String = ProjectValues.PageLetter.Trim().ToUpperInvariant()

        If String.IsNullOrWhiteSpace(wanted) Then
            Return False
        End If

        Return String.Equals(
            pageLetterPart,
            wanted,
            StringComparison.OrdinalIgnoreCase)

    End Function

    Private Shared Function RemoteFilenameMatchesPage(
        fileName As String,
        page As Integer,
        suffix As String) As Boolean

        Dim parsed As ParsedScanName = ParseScanName(fileName)

        If Not parsed.IsValid Then
            Return False
        End If

        If Not PrefixMatchesCurrentBatch(parsed.Prefix) Then
            Return False
        End If

        If Not PageLetterMatchesCurrentBatch(parsed.PageLetterPart) Then
            Return False
        End If

        Dim wantedSuffix As String = If(suffix, "").Trim().ToUpperInvariant()

        Return parsed.Page = page AndAlso
               String.Equals(
                   parsed.PageSuffix,
                   wantedSuffix,
                   StringComparison.OrdinalIgnoreCase)

    End Function
    Private Shared Function ExtractLinks(html As String) As IEnumerable(Of String)

        Dim matches As MatchCollection =
            Regex.Matches(
                html,
                "href\s*=\s*[""'](?<url>[^""']+)[""']",
                RegexOptions.IgnoreCase)

        Dim results As New List(Of String)

        For Each match As Match In matches

            Dim href As String = match.Groups("url").Value.Trim()

            If Not String.IsNullOrWhiteSpace(href) Then
                results.Add(href)
            End If

        Next

        Return results

    End Function

    Private Shared Function IsImageLink(href As String) As Boolean

        Dim extension As String = Path.GetExtension(href).ToLowerInvariant()

        Return extension = ".jpg" OrElse
               extension = ".jpeg" OrElse
               extension = ".tif" OrElse
               extension = ".tiff" OrElse
               extension = ".png" OrElse
               extension = ".gif"

    End Function

    Private Shared Function LooksLikeFolderLink(href As String) As Boolean

        If String.IsNullOrWhiteSpace(href) Then
            Return False
        End If

        If href.StartsWith("../", StringComparison.Ordinal) Then
            Return False
        End If

        If href.StartsWith("?", StringComparison.Ordinal) Then
            Return False
        End If

        If href.StartsWith("#", StringComparison.Ordinal) Then
            Return False
        End If

        Return href.EndsWith("/", StringComparison.Ordinal)

    End Function

    Private Shared Function NormalizeUrl(url As String) As String

        Dim uri As New Uri(url)
        Dim normalized As String = uri.GetLeftPart(UriPartial.Path)

        If normalized.EndsWith("/", StringComparison.Ordinal) Then
            Return normalized
        End If

        Return normalized & "/"

    End Function

    Private Shared Function TryMakeChildUrl(pageUrl As String, href As String) As String

        Try
            Return New Uri(New Uri(pageUrl), href).ToString()
        Catch
            Return Nothing
        End Try

    End Function

    Private Shared Function IsChildUrl(parentUrl As String, possibleChildUrl As String) As Boolean

        Dim currentPrefix As String = NormalizeUrl(parentUrl).TrimEnd("/"c) & "/"
        Dim normalizedChild As String = NormalizeUrl(possibleChildUrl)

        If Not normalizedChild.StartsWith(currentPrefix, StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        If String.Equals(normalizedChild, NormalizeUrl(parentUrl), StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        Return True

    End Function

    Private Shared Function GetLastUrlSegment(url As String) As String

        Try

            Dim path As String = New Uri(url).AbsolutePath.TrimEnd("/"c)
            Dim parts() As String = path.Split("/"c)

            If parts.Length = 0 Then
                Return ""
            End If

            Return Uri.UnescapeDataString(parts(parts.Length - 1))

        Catch
            Return ""
        End Try

    End Function

    Private Shared Function GetImagePreference(fileNameOrUrl As String) As Integer

        If fileNameOrUrl.IndexOf("rescan", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return 0
        End If

        Dim extension As String = Path.GetExtension(fileNameOrUrl).ToLowerInvariant()

        Select Case extension
            Case ".jpg", ".jpeg"
                Return 1
            Case ".png"
                Return 2
            Case ".tif", ".tiff"
                Return 3
            Case ".gif"
                Return 4
            Case Else
                Return 999
        End Select

    End Function

    Private Shared Function IsIgnoredScanVariant(fileNameOrUrl As String) As Boolean

        Dim text As String = Path.GetFileNameWithoutExtension(fileNameOrUrl)

        Return text.IndexOf("blank", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               text.IndexOf("front", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               text.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0

    End Function
    Private Async Function GetSourceFoldersAsync(
    startUrl As String) As Task(Of List(Of (Url As String, SourceRef As String)))

        Dim results As New List(Of (Url As String, SourceRef As String))

        DebugLog.Write(
            $"[SCAN SOURCE] Enumerating source folders below '{startUrl}'")

        Dim html As String = Await GetHtmlAsync(startUrl)

        If String.IsNullOrWhiteSpace(html) Then
            Return results
        End If

        For Each href As String In ExtractLinks(html)

            If Not LooksLikeFolderLink(href) Then
                Continue For
            End If

            Dim childUrl As String =
                TryMakeChildUrl(startUrl, href)

            If childUrl Is Nothing Then
                Continue For
            End If

            Dim normalizedChild As String =
                NormalizeUrl(childUrl)

            If Not IsChildUrl(startUrl, normalizedChild) Then
                Continue For
            End If

            Dim sourceRef As String =
                GetLastUrlSegment(normalizedChild)

            DebugLog.Write(
                $"[SCAN SOURCE] Found SourceRef='{sourceRef}'")

            results.Add(
                (normalizedChild, sourceRef))

        Next

        DebugLog.Write(
            $"[SCAN SOURCE] Total SourceRefs={results.Count}")

        Return results

    End Function
    Private Shared Function NormalizeSourceRefForFolderMatch(
    sourceRef As String) As String

        If String.IsNullOrWhiteSpace(sourceRef) Then
            Return ""
        End If

        Dim value As String = sourceRef.Trim()

        Dim separatorIndex As Integer =
            value.IndexOfAny(
                New Char() {" "c, ControlChars.Tab, ","c})

        If separatorIndex > 0 Then
            value = value.Substring(0, separatorIndex)
        End If

        Return value.Trim()

    End Function

    Private Shared Function NormalizeSourceText(
        value As String) As String

        Return Regex.Replace(
            value.Trim().ToUpperInvariant(),
            "[^A-Z0-9]",
            "")

    End Function

    Private Shared Function SourceFolderMatches(
        folderName As String,
        enteredSourceRef As String) As Boolean

        Dim folder As String =
            NormalizeSourceText(folderName)

        Dim entered As String =
            NormalizeSourceText(enteredSourceRef)

        If String.IsNullOrWhiteSpace(folder) OrElse
           String.IsNullOrWhiteSpace(entered) Then

            Return False

        End If

        If entered.Length < 5 Then

            Return folder.StartsWith(
                entered,
                StringComparison.OrdinalIgnoreCase)

        End If

        Return String.Equals(
            folder,
            entered,
            StringComparison.OrdinalIgnoreCase)

    End Function
    Private Async Function FindMatchingSourceFolderAsync(
    startUrl As String,
    enteredSourceRef As String) As Task(Of String)

        Dim visited As New HashSet(Of String)(
            StringComparer.OrdinalIgnoreCase)

        Dim queue As New Queue(Of String)

        Dim normalizedStart As String =
            NormalizeUrl(startUrl)

        queue.Enqueue(normalizedStart)
        visited.Add(normalizedStart)

        Dim pagesChecked As Integer = 0

        While queue.Count > 0

            Dim pageUrl As String =
                queue.Dequeue()

            pagesChecked += 1

            DebugLog.Write(
                $"[SCAN SOURCE] [{pagesChecked}] Searching folders in {pageUrl}")

            Dim html As String =
                Await GetHtmlAsync(pageUrl)

            If String.IsNullOrWhiteSpace(html) Then
                Continue While
            End If

            For Each href As String In ExtractLinks(html)

                If Not LooksLikeFolderLink(href) Then
                    Continue For
                End If

                Dim childUrl As String =
                    TryMakeChildUrl(pageUrl, href)

                If childUrl Is Nothing Then
                    Continue For
                End If

                Dim normalizedChild As String =
                    NormalizeUrl(childUrl)

                If Not IsChildUrl(pageUrl, normalizedChild) Then
                    Continue For
                End If

                If Not visited.Add(normalizedChild) Then
                    Continue For
                End If

                Dim folderName As String =
                    GetLastUrlSegment(normalizedChild)

                DebugLog.Write(
                    $"[SCAN SOURCE] Considering folder '{folderName}' for SourceRef='{enteredSourceRef}'.")

                If SourceFolderMatches(
                    folderName,
                    enteredSourceRef) Then

                    DebugLog.Write(
                        $"[SCAN SOURCE] Matched folder '{folderName}' for SourceRef='{enteredSourceRef}'.")

                    Return normalizedChild

                End If

                queue.Enqueue(normalizedChild)

            Next

        End While

        Return Nothing

    End Function
    Private Shared Function ComparePageLetterToWanted(
    pageLetterPart As String) As Integer

        Dim wanted As String =
            ProjectValues.PageLetter.Trim().ToUpperInvariant()

        If String.IsNullOrWhiteSpace(wanted) Then
            Return 0
        End If

        If String.IsNullOrWhiteSpace(pageLetterPart) Then
            Return 0
        End If

        Dim wantedLetter As Char = wanted(0)
        Dim foundLetter As Char = pageLetterPart.Trim().ToUpperInvariant()(0)

        Return foundLetter.CompareTo(wantedLetter)

    End Function
    Private Function RemoteFilenameMatchesPreviousDoublePage(
    fileName As String) As Boolean

        Dim parsed As ParsedScanName =
            ParseScanName(fileName)

        If Not parsed.IsValid Then
            Return False
        End If

        If Not PrefixMatchesCurrentBatch(parsed.Prefix) Then
            Return False
        End If

        If Not PageLetterMatchesCurrentBatch(parsed.PageLetterPart) Then
            Return False
        End If

        If Not String.IsNullOrWhiteSpace(parsed.PageSuffix) Then
            Return False
        End If

        Return parsed.Page = ProjectValues.Page - 1

    End Function
    Private Function CollectImageCandidatesOnPage(
    pageUrl As String,
    links As List(Of String),
    sourceReferenceOverride As String,
    ByRef crawlOrder As Integer,
    ByRef reachedBeyondWantedPage As Boolean) As List(Of ScanCandidate)

        Dim candidates As New List(Of ScanCandidate)

        reachedBeyondWantedPage = False

        Dim hasSuffix As Boolean =
            Not String.IsNullOrWhiteSpace(ProjectValues.PageSuffix)

        For Each href As String In links

            Dim absoluteUrl As String =
                TryMakeChildUrl(pageUrl, href)

            If absoluteUrl Is Nothing Then
                Continue For
            End If

            If Not IsImageLink(absoluteUrl) Then
                Continue For
            End If

            Dim fileName As String =
                Path.GetFileName(
                    New Uri(absoluteUrl).LocalPath)

            If IsIgnoredScanVariant(fileName) Then

                DebugLog.Write(
                    $"[SCAN IMAGE] Ignoring variant '{fileName}'")

                Continue For

            End If

            Dim rank As Integer =
                GetImagePreference(absoluteUrl)

            Dim sourceReference As String =
                If(
                    String.IsNullOrWhiteSpace(sourceReferenceOverride),
                    GetLastUrlSegment(pageUrl),
                    sourceReferenceOverride)

            If RemoteFilenameMatchesPage(
                fileName,
                ProjectValues.Page,
                ProjectValues.PageSuffix) Then

                crawlOrder += 1

                Dim candidate As New ScanCandidate With {
                    .RemoteUrl = absoluteUrl,
                    .FileName = fileName,
                    .SourceReference = sourceReference,
                    .Kind = ScanCandidateKind.Exact,
                    .ImageRank = rank,
                    .CrawlOrder = crawlOrder
                }

                candidates.Add(candidate)

                DebugLog.Write(
                    $"[SCAN CANDIDATE] Exact candidate. File='{fileName}', Rank={rank}, SourceReference='{sourceReference}', Url='{absoluteUrl}'")

                Continue For

            End If

            If Not hasSuffix AndAlso
               ProjectValues.Page > 1 AndAlso
               RemoteFilenameMatchesPreviousDoublePage(fileName) Then

                crawlOrder += 1

                Dim candidate As New ScanCandidate With {
                    .RemoteUrl = absoluteUrl,
                    .FileName = fileName,
                    .SourceReference = sourceReference,
                    .Kind = ScanCandidateKind.PreviousDoublePage,
                    .ImageRank = rank,
                    .CrawlOrder = crawlOrder
                }

                candidates.Add(candidate)

                DebugLog.Write(
                    $"[SCAN CANDIDATE] Previous-page double-page candidate. File='{fileName}', Rank={rank}, SourceReference='{sourceReference}', Url='{absoluteUrl}'")

                Continue For

            End If

            Dim parsed As ParsedScanName =
                ParseScanName(fileName)

            If parsed.IsValid AndAlso
               PrefixMatchesCurrentBatch(parsed.Prefix) Then

                Dim letterCompare As Integer =
                    ComparePageLetterToWanted(parsed.PageLetterPart)

                If letterCompare > 0 Then

                    DebugLog.Write(
                        $"[SCAN IMAGE] Later page letter reached. File='{fileName}' has letter '{parsed.PageLetterPart}' but wanted '{ProjectValues.PageLetter}'.")

                    reachedBeyondWantedPage = True
                    Exit For

                End If

                If letterCompare = 0 AndAlso
                   String.IsNullOrWhiteSpace(ProjectValues.PageSuffix) AndAlso
                   parsed.Page > ProjectValues.Page Then

                    DebugLog.Write(
                        $"[SCAN IMAGE] Later page reached. File='{fileName}' has page {parsed.Page} but wanted {ProjectValues.Page} for letter '{ProjectValues.PageLetter}'.")

                    reachedBeyondWantedPage = True
                    Exit For

                End If

            Else

                DebugLog.Write(
                    $"[SCAN IMAGE] Rejected non-matching image '{fileName}'")

            End If

        Next

        Return candidates

    End Function
    Private Async Function CollectCandidatesBelowAsync(
    startUrl As String,
    sourceReferenceOverride As String,
    stopGloballyWhenBeyondWantedPage As Boolean) As Task(Of List(Of ScanCandidate))

        Dim candidates As New List(Of ScanCandidate)

        Dim visited As New HashSet(Of String)(
            StringComparer.OrdinalIgnoreCase)

        Dim queue As New Queue(Of String)

        Dim normalizedStart As String =
            NormalizeUrl(startUrl)

        queue.Enqueue(normalizedStart)
        visited.Add(normalizedStart)

        Dim pagesChecked As Integer = 0
        Dim crawlOrder As Integer = 0
        Dim stopSearch As Boolean = False

        While queue.Count > 0 AndAlso Not stopSearch

            Dim pageUrl As String =
                queue.Dequeue()

            pagesChecked += 1

            DebugLog.Write(
                $"[SCAN CRAWL] [{pagesChecked}] Searching {pageUrl}")

            Dim html As String =
                Await GetHtmlAsync(pageUrl)

            If String.IsNullOrWhiteSpace(html) Then
                Continue While
            End If

            Dim links As List(Of String) =
                ExtractLinks(html).ToList()

            Dim folderCount As Integer =
    Enumerable.Count(
        links,
        Function(link) LooksLikeFolderLink(link))

            Dim imageCount As Integer =
    Enumerable.Count(
        links,
        Function(link) IsImageLink(link))

            DebugLog.Write(
    $"[SCAN CRAWL] links={links.Count}, folders={folderCount}, images={imageCount}")

            Dim pageReachedBeyondWanted As Boolean

            Dim pageCandidates As List(Of ScanCandidate) =
                CollectImageCandidatesOnPage(
                    pageUrl,
                    links,
                    sourceReferenceOverride,
                    crawlOrder,
                    pageReachedBeyondWanted)

            candidates.AddRange(pageCandidates)

            If pageReachedBeyondWanted AndAlso
               stopGloballyWhenBeyondWantedPage Then

                DebugLog.Write(
                    "[SCAN CRAWL] Stopping remote scan search: later page letter/page reached.")

                stopSearch = True
                Exit While

            End If

            For Each href As String In links

                If Not LooksLikeFolderLink(href) Then
                    Continue For
                End If

                Dim childUrl As String =
                    TryMakeChildUrl(pageUrl, href)

                If childUrl Is Nothing Then
                    Continue For
                End If

                Dim normalizedChild As String =
                    NormalizeUrl(childUrl)

                If Not IsChildUrl(pageUrl, normalizedChild) Then
                    Continue For
                End If

                If visited.Add(normalizedChild) Then

                    DebugLog.Write(
                        $"[SCAN CRAWL] queue {normalizedChild}")

                    queue.Enqueue(normalizedChild)

                End If

            Next

        End While

        DebugLog.Write(
            $"[SCAN CANDIDATE] Candidate collection complete. Count={candidates.Count}")

        For Each candidate As ScanCandidate In candidates

            DebugLog.Write(
                $"[SCAN CANDIDATE] Collected File='{candidate.FileName}', Kind={candidate.Kind}, Rank={candidate.ImageRank}, Order={candidate.CrawlOrder}, SourceReference='{candidate.SourceReference}', Url='{candidate.RemoteUrl}'")

        Next

        Return candidates

    End Function
    Private Async Function FindRemoteScanCandidateAsync(
    startUrl As String,
    enteredSourceRef As String) As Task(Of ScanCandidate)

        Dim normalizedStart As String =
        NormalizeUrl(startUrl)

        DebugLog.Write(
        $"[SCAN] Remote search starting. StartUrl='{normalizedStart}', EnteredSourceRef='{enteredSourceRef}'")

        Dim sourceFolders As List(Of (Url As String, SourceRef As String)) =
        Await GetSourceFoldersAsync(normalizedStart)

        If Not String.IsNullOrWhiteSpace(enteredSourceRef) Then

            Dim sourceFolderUrl As String =
            Await FindMatchingSourceFolderAsync(
                normalizedStart,
                enteredSourceRef)

            If sourceFolderUrl Is Nothing Then

                DebugLog.Write(
                $"[SCAN SOURCE] No source folder matched SourceRef='{enteredSourceRef}'. Searching other source folders for matching page.")

                Dim alternativeCandidates As New List(Of ScanCandidate)

                For Each folder In sourceFolders

                    DebugLog.Write(
                    $"[SCAN SOURCE] Searching alternative SourceRef='{folder.SourceRef}'")

                    Dim folderCandidates As List(Of ScanCandidate) =
                    Await CollectCandidatesBelowAsync(
                        folder.Url,
                        folder.SourceRef,
                        True)

                    alternativeCandidates.AddRange(folderCandidates)

                Next

                Dim alternative As ScanCandidate =
                SelectBestCandidate(alternativeCandidates)

                If alternative IsNot Nothing Then

                    _lastErrorMessage =
                    $"Scan not found under SourceRef '{enteredSourceRef}'." &
                    Environment.NewLine &
                    $"A matching scan was found under SourceRef '{alternative.SourceReference}'."

                    _alternativeSourceRef =
                    alternative.SourceReference

                    _alternativeFileName =
                    alternative.FileName

                    _alternativeRemoteUrl =
                    alternative.RemoteUrl

                    DebugLog.Write(
                    $"[SCAN SOURCE] Alternative matching scan found. EnteredSourceRef='{enteredSourceRef}', FoundSourceRef='{alternative.SourceReference}', File='{alternative.FileName}', Url='{alternative.RemoteUrl}'")

                    Return Nothing

                End If

                _lastErrorMessage =
                "Scan not found. Please check the scan reference."

                DebugLog.Write(
                $"[SCAN SOURCE] No source folder matched SourceRef='{enteredSourceRef}', and no matching scan was found elsewhere.")

                Return Nothing

            End If

            Dim sourceReference As String =
            GetLastUrlSegment(sourceFolderUrl)

            DebugLog.Write(
            $"[SCAN SOURCE] Source folder matched. SourceRef='{enteredSourceRef}', SourceReference='{sourceReference}', Url='{sourceFolderUrl}'")

            Dim candidates As List(Of ScanCandidate) =
            Await CollectCandidatesBelowAsync(
                sourceFolderUrl,
                sourceReference,
                True)

            Dim selected As ScanCandidate =
            SelectBestCandidate(candidates)

            If selected Is Nothing Then

                _lastErrorMessage =
                "Scan not found. Please check the scan reference."

                DebugLog.Write(
                $"[SCAN SOURCE] Source folder was found but no matching scan was found inside it. SourceReference='{sourceReference}', Url='{sourceFolderUrl}'")

                Return Nothing

            End If

            Return selected

        End If

        DebugLog.Write(
        "[SCAN SOURCE] SourceRef is blank. Searching all source folders.")

        Dim allCandidates As New List(Of ScanCandidate)

        For Each folder In sourceFolders

            DebugLog.Write(
            $"[SCAN SOURCE] Searching SourceRef='{folder.SourceRef}'")

            Dim candidates As List(Of ScanCandidate) =
            Await CollectCandidatesBelowAsync(
                folder.Url,
                folder.SourceRef,
                True)

            allCandidates.AddRange(candidates)

            Dim bestSoFar As ScanCandidate =
            SelectBestCandidate(allCandidates)

            If bestSoFar IsNot Nothing AndAlso
           bestSoFar.Kind = ScanCandidateKind.Exact AndAlso
           bestSoFar.ImageRank = 1 Then

                DebugLog.Write(
                $"[SCAN SOURCE] Early stop: found exact top-ranked match '{bestSoFar.FileName}' in '{bestSoFar.SourceReference}'")

                Exit For

            End If

        Next

        Return SelectBestCandidate(allCandidates)

    End Function
    Private Shared Function SelectBestCandidate(
    candidates As List(Of ScanCandidate)) As ScanCandidate

        If candidates.Count = 0 Then

            DebugLog.Write(
                "[SCAN CANDIDATE] No candidates available to select.")

            Return Nothing

        End If

        Dim selected As ScanCandidate =
            candidates.
            OrderBy(Function(candidate) candidate.Kind).
            ThenBy(Function(candidate) candidate.ImageRank).
            ThenBy(Function(candidate) candidate.CrawlOrder).
            First()

        DebugLog.Write(
            $"[SCAN CANDIDATE] Selected best candidate from {candidates.Count}. File='{selected.FileName}', Kind={selected.Kind}, Rank={selected.ImageRank}, Order={selected.CrawlOrder}, SourceReference='{selected.SourceReference}', Url='{selected.RemoteUrl}'")

        Return selected

    End Function
    Public Async Function LocateScanAsync() As Task(Of ScanSearchResult)

        Dim scanFolder As String =
            AppPaths.DownloadedScansFolder

        DebugLog.Write(
            $"[SCAN] Locate requested. " &
            $"Year={ProjectValues.Year}, Quarter={ProjectValues.Quarter}, BatchType={ProjectValues.BatchType}, " &
            $"Page={ProjectValues.Page}, PageLetter='{ProjectValues.PageLetter}', PageSuffix='{ProjectValues.PageSuffix}', " &
            $"SourceRef='{ProjectValues.SourceRef}', ExpectedPrefix='{GetExpectedPrefix()}', " &
            $"StartUrl='{BuildStartUrl()}'")

        Dim local As String =
            TryFindLocalScan(scanFolder)

        If Not String.IsNullOrWhiteSpace(local) Then

            DebugLog.Write(
                $"[SCAN] Found local scan. Path='{local}'. Remote search skipped.")

            Return New ScanSearchResult With {
                .Success = True,
                .LocalPath = local,
                .Message = "Found local scan."
            }

        End If

        If Not _hasCredentials Then

            Return New ScanSearchResult With {
                .Success = False,
                .Message =
                    "You have not set your Username or Password." &
                    Environment.NewLine &
                    "You must do that before a scan can be downloaded."
            }

        End If

        _lastRequestUnauthorized = False
        _lastErrorMessage = ""
        _alternativeSourceRef = ""
        _alternativeFileName = ""
        _alternativeRemoteUrl = ""

        Dim startUrl As String =
            BuildStartUrl()

        Dim enteredSourceRef As String =
            NormalizeSourceRefForFolderMatch(
                ProjectValues.SourceRef)

        Dim candidate As ScanCandidate =
            Await FindRemoteScanCandidateAsync(
                startUrl,
                enteredSourceRef)

        If _lastRequestUnauthorized Then

            Return New ScanSearchResult With {
                .Success = False,
                .Unauthorized = True,
                .Message =
                    "The scan website did not accept your Username or Password." &
                    Environment.NewLine &
                    "Please check them on the Header Form."
            }

        End If

        If candidate Is Nothing Then

            Return New ScanSearchResult With {
                .Success = False,
                .Message =
                    If(
                        String.IsNullOrWhiteSpace(_lastErrorMessage),
                        "No matching scan found online.",
                        _lastErrorMessage),
                .AlternativeFound =
                    Not String.IsNullOrWhiteSpace(_alternativeSourceRef),
                .AlternativeSourceRef = _alternativeSourceRef,
                .AlternativeFileName = _alternativeFileName,
                .AlternativeRemoteUrl = _alternativeRemoteUrl
            }

        End If

        Directory.CreateDirectory(scanFolder)

        Dim fileName As String =
            Path.GetFileName(
                New Uri(candidate.RemoteUrl).LocalPath)

        Dim localPath As String =
            Path.Combine(
                scanFolder,
                fileName)

        DebugLog.Write(
            $"[SCAN FOUND] Selected candidate. " &
            $"File='{candidate.FileName}', Kind={candidate.Kind}, Rank={candidate.ImageRank}, " &
            $"SourceReference='{candidate.SourceReference}', Url='{candidate.RemoteUrl}'")

        DebugLog.Write(
            $"[SCAN] Downloading as local file: {localPath}")

        Dim downloaded As Boolean =
            Await DownloadFileAsync(
                candidate.RemoteUrl,
                localPath)

        If Not downloaded Then

            Return New ScanSearchResult With {
                .Success = False,
                .RemoteUrl = candidate.RemoteUrl,
                .Message =
                    If(
                        String.IsNullOrWhiteSpace(_lastErrorMessage),
                        "Found scan online but download failed.",
                        _lastErrorMessage)
            }

        End If

        Return New ScanSearchResult With {
            .Success = True,
            .LocalPath = localPath,
            .RemoteUrl = candidate.RemoteUrl,
            .Message = "Downloaded scan."
        }

    End Function
    Public Async Function DownloadAlternativeScanAsync(
    result As ScanSearchResult) As Task(Of ScanSearchResult)

        If result Is Nothing OrElse
           Not result.AlternativeFound OrElse
           String.IsNullOrWhiteSpace(result.AlternativeRemoteUrl) Then

            Return New ScanSearchResult With {
                .Success = False,
                .Message = "No alternative scan is available."
            }

        End If

        Dim scanFolder As String =
            AppPaths.DownloadedScansFolder

        Directory.CreateDirectory(scanFolder)

        Dim fileName As String =
            Path.GetFileName(
                New Uri(result.AlternativeRemoteUrl).LocalPath)

        Dim localPath As String =
            Path.Combine(
                scanFolder,
                fileName)

        DebugLog.Write(
            $"[SCAN SOURCE] Downloading alternative scan. " &
            $"SourceReference='{result.AlternativeSourceRef}', " &
            $"File='{fileName}', Url='{result.AlternativeRemoteUrl}'")

        Dim downloaded As Boolean =
            Await DownloadFileAsync(
                result.AlternativeRemoteUrl,
                localPath)

        If Not downloaded Then

            Return New ScanSearchResult With {
                .Success = False,
                .RemoteUrl = result.AlternativeRemoteUrl,
                .Message =
                    If(
                        String.IsNullOrWhiteSpace(_lastErrorMessage),
                        "The alternative scan could not be downloaded.",
                        _lastErrorMessage)
            }

        End If

        Return New ScanSearchResult With {
            .Success = True,
            .LocalPath = localPath,
            .RemoteUrl = result.AlternativeRemoteUrl,
            .Message = "Alternative scan downloaded."
        }

    End Function
End Class
