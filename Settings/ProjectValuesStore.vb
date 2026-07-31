Imports System.IO
Imports System.Text.Json

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

        Catch ex As Exception
            Save()
        End Try
    End Sub

    Public Sub Save()
        Try
            AppPaths.EnsureFoldersExist()

            Dim values As New ProjectValuesData With {
                .FilePanelExpanded = ProjectValues.FilePanelExpanded,
                .UserName = ProjectValues.UserName,
                .UserEmail = ProjectValues.UserEmail,
                .UserPW = ProjectValues.UserPW,
                .Creator = ProjectValues.Creator,
                .CreatorEmail = ProjectValues.CreatorEmail,
                .EnableDiagnosticLogging = ProjectValues.EnableDiagnosticLogging,
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

        Public Property UserName As String = ""
        Public Property UserEmail As String = ""
        Public Property UserPW As String = ""

        Public Property Creator As String = ""
        Public Property CreatorEmail As String = ""

        Public Property EnableDiagnosticLogging As Boolean = True

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

    End Class

End Module