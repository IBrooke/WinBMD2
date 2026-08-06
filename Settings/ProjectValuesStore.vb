Imports System.IO
Imports System.Text.Json
Imports System.Drawing
Public Module ProjectValuesStore

    Public Sub Initialise()
        AppPaths.EnsureFoldersExist()
        Load()
    End Sub

    Public Sub Load()
        Try
            If Not File.Exists(AppPaths.SettingsFilePath) Then
                Save()
                Return
            End If

            Dim json As String = File.ReadAllText(AppPaths.SettingsFilePath)

            If String.IsNullOrWhiteSpace(json) Then
                Save()
                Return
            End If

            Dim values As ProjectValuesData = JsonSerializer.Deserialize(Of ProjectValuesData)(json)

            If values Is Nothing Then
                Save()
                Return
            End If

            ProjectValues.FilePanelExpanded = values.FilePanelExpanded
            ProjectValues.TranscriptionFormBounds =
            If(
                values.TranscriptionFormBounds,
                New Dictionary(Of String, FormBoundsData))
            ProjectValues.ScanViewLeft = values.ScanViewLeft
            ProjectValues.ScanViewTop = values.ScanViewTop
            ProjectValues.ScanViewWidth = values.ScanViewWidth
            ProjectValues.ScanViewHeight = values.ScanViewHeight
            ProjectValues.ScanViewMaximized = values.ScanViewMaximized
            ProjectValues.HeaderFormLeft = values.HeaderFormLeft
            ProjectValues.HeaderFormTop = values.HeaderFormTop
            ProjectValues.HeaderFormWidth = values.HeaderFormWidth
            ProjectValues.HeaderFormHeight = values.HeaderFormHeight
            ProjectValues.HeaderFormMaximized = values.HeaderFormMaximized
            ProjectValues.UserName = If(values.UserName, "")
            ProjectValues.UserEmail = If(values.UserEmail, "")
            ProjectValues.UserPW = If(values.UserPW, "")

            ProjectValues.Creator = If(values.Creator, "")
            ProjectValues.CreatorEmail = If(values.CreatorEmail, "")

            ProjectValues.EnableDiagnosticLogging = values.EnableDiagnosticLogging

            ProjectValues.BatchType = If(values.BatchType, "")
            ProjectValues.Year = values.Year
            ProjectValues.Quarter = values.Quarter
            ProjectValues.Month = values.Month

            ProjectValues.Page = values.Page
            ProjectValues.PageSource = values.PageSource
            ProjectValues.PageLetter = If(values.PageLetter, "")
            ProjectValues.PageSuffix = If(values.PageSuffix, "")

            ProjectValues.VNF = If(values.VNF, "")
            ProjectValues.SourceRef = If(values.SourceRef, "")
            ProjectValues.Syndicate = If(values.Syndicate, "")
            ProjectValues.Comments = If(values.Comments, "")
            ProjectValues.ColourScheme = values.ColourScheme
            ProjectValues.IgnoreAutoComplete = values.IgnoreAutoComplete
            ProjectValues.UiFontName =
            If(
                String.IsNullOrWhiteSpace(values.UiFontName),
                SystemFonts.MessageBoxFont.FontFamily.Name,
                values.UiFontName)

            ProjectValues.UiFontSize =
            If(
                values.UiFontSize >= 8.0F,
                values.UiFontSize,
                SystemFonts.MessageBoxFont.Size)

            ProjectValues.UiFontColourArgb =
            values.UiFontColourArgb

            ProjectValues.VerifyFontSize =
            If(
                values.VerifyFontSize >= 8.0F,
                values.VerifyFontSize,
                12.0F)
            ProjectValues.GridColumnWidths =
            If(
                values.GridColumnWidths,
                New Dictionary(Of String, Dictionary(Of String, Integer)))
        Catch ex As Exception
            Save()
        End Try
    End Sub

    Public Sub Save()
        Try
            AppPaths.EnsureFoldersExist()

            Dim values As New ProjectValuesData With {
                .FilePanelExpanded = ProjectValues.FilePanelExpanded, .TranscriptionFormBounds = ProjectValues.TranscriptionFormBounds,
                .ScanViewLeft = ProjectValues.ScanViewLeft,
                .ScanViewTop = ProjectValues.ScanViewTop,
                .ScanViewWidth = ProjectValues.ScanViewWidth,
                .ScanViewHeight = ProjectValues.ScanViewHeight,
                .ScanViewMaximized = ProjectValues.ScanViewMaximized,
                .HeaderFormLeft = ProjectValues.HeaderFormLeft,
                .HeaderFormTop = ProjectValues.HeaderFormTop,
                .HeaderFormWidth = ProjectValues.HeaderFormWidth,
                .HeaderFormHeight = ProjectValues.HeaderFormHeight,
                .HeaderFormMaximized = ProjectValues.HeaderFormMaximized,
                .GridColumnWidths = ProjectValues.GridColumnWidths,
                .ColourScheme = ProjectValues.ColourScheme,
                .UiFontName = ProjectValues.UiFontName,
                .UiFontSize = ProjectValues.UiFontSize,
                .UiFontColourArgb = ProjectValues.UiFontColourArgb,
                .VerifyFontSize = ProjectValues.VerifyFontSize,
                .UserName = ProjectValues.UserName,
                .UserEmail = ProjectValues.UserEmail,
                .UserPW = ProjectValues.UserPW,
                .Creator = ProjectValues.Creator,
                .CreatorEmail = ProjectValues.CreatorEmail,
                .EnableDiagnosticLogging = ProjectValues.EnableDiagnosticLogging,
                .IgnoreAutoComplete = ProjectValues.IgnoreAutoComplete,
                .BatchType = ProjectValues.BatchType,
                .Year = ProjectValues.Year,
                .Quarter = ProjectValues.Quarter,
                .Month = ProjectValues.Month,
                .Page = ProjectValues.Page,
                .PageSource = ProjectValues.PageSource,
                .PageLetter = ProjectValues.PageLetter,
                .PageSuffix = ProjectValues.PageSuffix,
                .VNF = ProjectValues.VNF,
                .SourceRef = ProjectValues.SourceRef,
                .Syndicate = ProjectValues.Syndicate,
                .Comments = ProjectValues.Comments
            }

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True
            }

            Dim json As String = JsonSerializer.Serialize(values, options)
            File.WriteAllText(AppPaths.SettingsFilePath, json)

        Catch ex As Exception
            ' A settings failure must not crash the application.
        End Try
    End Sub

    Private Class ProjectValuesData

        Public Property FilePanelExpanded As Boolean = True
        Public Property TranscriptionFormBounds As New Dictionary(Of String, FormBoundsData)
        Public Property ScanViewLeft As Integer = -1
        Public Property ScanViewTop As Integer = -1
        Public Property ScanViewWidth As Integer = 900
        Public Property ScanViewHeight As Integer = 650
        Public Property ScanViewMaximized As Boolean
        Public Property HeaderFormLeft As Integer = -1
        Public Property HeaderFormTop As Integer = -1
        Public Property HeaderFormWidth As Integer = 964
        Public Property HeaderFormHeight As Integer = 681
        Public Property HeaderFormMaximized As Boolean
        Public Property GridColumnWidths As New Dictionary(Of String, Dictionary(Of String, Integer))
        Public Property UserName As String = ""
        Public Property UserEmail As String = ""
        Public Property UserPW As String = ""
        Public Property Creator As String = ""
        Public Property CreatorEmail As String = ""
        Public Property EnableDiagnosticLogging As Boolean = True
        Public Property IgnoreAutoComplete As IgnoreAutoCompleteKey = IgnoreAutoCompleteKey.Tab
        Public Property BatchType As String = ""
        Public Property Year As Integer
        Public Property Quarter As Integer
        Public Property Month As Integer
        Public Property Page As Integer
        Public Property PageSource As Integer = -1
        Public Property PageLetter As String = ""
        Public Property PageSuffix As String = ""
        Public Property VNF As String = ""
        Public Property SourceRef As String = ""
        Public Property Syndicate As String = ""
        Public Property Comments As String = ""
        Public Property ColourScheme As UiColourScheme = UiColourScheme.Teal
        Public Property UiFontName As String = SystemFonts.MessageBoxFont.FontFamily.Name
        Public Property UiFontSize As Single = SystemFonts.MessageBoxFont.Size
        Public Property UiFontColourArgb As Integer = SystemColors.ControlText.ToArgb()
        Public Property VerifyFontSize As Single = 12.0F
    End Class

End Module