Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Drawing
Public Module ProjectValuesStore
    Private ReadOnly _saveTimer As New System.Windows.Forms.Timer With {.Interval = 10000}
    Private _savePending As Boolean = False
    Public Event SavingSettings()       ' Allows the open batch to update its MRU position before settings are written.
    Private _settingsReady As Boolean = False   ' Saving is allowed only after settings have been loaded or defaults accepted.

    Public Function Initialise() As Boolean

        DebugLog.WriteAlways("[SETTINGS] ProjectValuesStore.Initialise called.")

        _settingsReady = False
        _savePending = False
        _saveTimer.Stop()

        If Not Load() Then Return False

        _settingsReady = True

        ' Register these only after the user has agreed to continue.
        RemoveHandler _saveTimer.Tick, AddressOf SaveTimer_Tick
        RemoveHandler Application.ApplicationExit, AddressOf SaveTimer_Tick
        AddHandler _saveTimer.Tick, AddressOf SaveTimer_Tick
        AddHandler Application.ApplicationExit, AddressOf SaveTimer_Tick

        ProjectValues.WriteToDebugLog()
        Return True

    End Function
    Public Function Load() As Boolean

        ProjectValues.LoadingPersistedValues = True

        Try


            Dim values As ProjectValuesData = Nothing
            Dim recoveredFromBackup As Boolean = False

            DebugLog.WriteAlways($"[SETTINGS] General settings path: {AppPaths.SettingsFilePath}")

            For Each filePath As String In {AppPaths.SettingsFilePath, AppPaths.SettingsFilePath & ".bak"}

                Dim isBackup As Boolean = filePath <> AppPaths.SettingsFilePath
                values = TryReadGeneralSettings(filePath, isBackup)

                If values IsNot Nothing Then
                    recoveredFromBackup = isBackup
                    Exit For
                End If

            Next

            If values Is Nothing Then

                If Not AskToContinueWithDefaultSettings() Then
                    DebugLog.WriteAlways("[SETTINGS] User declined standard settings. No settings will be saved.")
                    Return False
                End If

                DebugLog.WriteAlways("[SETTINGS] User accepted standard settings.")
                PreserveSettingsForRecovery()
                values = New ProjectValuesData()

            ElseIf recoveredFromBackup Then

                DebugLog.WriteAlways("[SETTINGS] General settings recovered from backup.")

                Dim choice As DialogResult = MessageBox.Show(
                    "WinBMD2 could not load your main settings file, but has recovered settings from its automatic backup." &
                    Environment.NewLine & Environment.NewLine &
                    "Some recent changes to your preferences or recent-file list may be missing." &
                    Environment.NewLine & Environment.NewLine &
                    "Click OK to continue using these recovered settings." &
                    Environment.NewLine &
                    "Click Cancel to close WinBMD2 without saving settings, so you can restore another backup.",
                    "WinBMD2 — Settings recovered",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2)

                If choice <> DialogResult.OK Then
                    DebugLog.WriteAlways("[SETTINGS] User declined recovered settings. Closing without saving.")
                    Return False
                End If

                PreserveSettingsForRecovery()

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
            ProjectValues.HelpFormBounds = If(values.HelpFormBounds, New FormBoundsData())
            ProjectValues.UserName = If(values.UserName, "")
            ProjectValues.UserEmail = If(values.UserEmail, "")
            ProjectValues.UserPW = If(values.UserPW, "")
            ProjectValues.FormatPicklistSelections = values.FormatPicklistSelections
            ProjectValues.PickListCompletion = values.PickListCompletion
            ProjectValues.ShowForenamePickList = values.ShowForenamePickList
            ProjectValues.ShowDistrictPickList = values.ShowDistrictPickList
            ProjectValues.FloatingPickList = values.FloatingPickList
            ProjectValues.PickListBounds = values.PickListBounds
            ProjectValues.Creator = If(values.Creator, "")
            ProjectValues.CreatorEmail = If(values.CreatorEmail, "")

            ProjectValues.EnableDiagnosticLogging = values.EnableDiagnosticLogging

            ProjectValues.BatchType = If(values.BatchType, "")
            ProjectValues.Year = values.Year
            ProjectValues.Quarter = values.Quarter
            ProjectValues.Month = values.Month
            ProjectValues.BatchName = If(values.BatchName, "")
            ProjectValues.Created = If(values.Created, "")
            ProjectValues.DateModified = values.DateModified
            ProjectValues.Page = values.Page
            ProjectValues.PageSource = values.PageSource
            ProjectValues.PageLetter = If(values.PageLetter, "")
            ProjectValues.PageSuffix = If(values.PageSuffix, "")

            ProjectValues.VNF = If(values.VNF, "")
            ProjectValues.SourceRef = If(values.SourceRef, "")
            ProjectValues.Syndicate = If(values.Syndicate, "")
            ProjectValues.Comments = If(values.Comments, "")
            ProjectValues.DistrictVersion = If(values.DistrictVersion, "??")
            ProjectValues.SkipSurname = values.SkipSurname
            ProjectValues.SaveFolder = If(values.SaveFolder, "")
            ProjectValues.OutputCharacterSet = If(String.IsNullOrWhiteSpace(values.OutputCharacterSet), "ISO-8859-1", values.OutputCharacterSet)
            ProjectValues.UploadServerUrl = If(values.UploadServerUrl, "www.freebmd.org.uk")
            ProjectValues.Match3VolChars = values.Match3VolChars
            ProjectValues.ScanViewSettings =
            If(
                values.ScanViewSettings,
                New Dictionary(Of String, ScanViewData))
            ProjectValues.RulerSettings =
            If(
                values.RulerSettings,
                New Dictionary(Of String, RulerData))
            ProjectValues.VerifyFieldLayouts =
                If(
                    values.VerifyFieldLayouts,
                    New Dictionary(Of String, Dictionary(Of String, VerifyFieldLayoutData)))
            ProjectValues.AutoShowScan = values.AutoShowScan
            ProjectValues.AutoShowRuler = values.AutoShowRuler
            ProjectValues.SequenceType =
            If(
                String.IsNullOrWhiteSpace(values.SequenceType),
                "SEQUENCED",
                values.SequenceType)
            ProjectValues.ColourScheme = values.ColourScheme
            ProjectValues.IgnoreAutoComplete = values.IgnoreAutoComplete
            ProjectValues.EntryMode = values.EntryMode
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
            ProjectValues.UiFontStyle = values.UiFontStyle
            ProjectValues.UiFontColourArgb = values.UiFontColourArgb
            ProjectValues.RulerColourArgb = values.RulerColourArgb
            ProjectValues.RulerTranslucent = values.RulerTranslucent
            ProjectValues.RulerHeight = Math.Max(4, values.RulerHeight)
            ProjectValues.VerifyFontName = If(values.VerifyFontName, "")
            ProjectValues.VerifyFontSize = values.VerifyFontSize
            ProjectValues.VerifyFontStyle = values.VerifyFontStyle
            ProjectValues.VerifyFontColor = values.VerifyFontColor
            ProjectValues.GridColumnWidths = If(values.GridColumnWidths, New Dictionary(Of String, List(Of Integer)))

            For Each filePath As String In {AppPaths.BatchSettingsFilePath, AppPaths.BatchSettingsFilePath & ".bak"}

                Try

                    If Not File.Exists(filePath) Then Continue For

                    Dim batchJson As String = File.ReadAllText(filePath)
                    Dim batchValues As BatchSettingsData = JsonSerializer.Deserialize(Of BatchSettingsData)(batchJson)

                    If batchValues Is Nothing Then Continue For

                    ProjectValues.ScanViewSettings = If(batchValues.ScanViewSettings, New Dictionary(Of String, ScanViewData))
                    ProjectValues.RulerSettings = If(batchValues.RulerSettings, New Dictionary(Of String, RulerData))
                    ProjectValues.VerifyFieldLayouts = If(batchValues.VerifyFieldLayouts, New Dictionary(Of String, Dictionary(Of String, VerifyFieldLayoutData)))
                    ProjectValues.GridColumnWidths = If(batchValues.GridColumnWidths, New Dictionary(Of String, List(Of Integer)))

                    If filePath <> AppPaths.BatchSettingsFilePath Then
                        DebugLog.WriteAlways("[SETTINGS] Batch settings recovered from backup.")
                    End If

                    Exit For

                Catch ex As Exception

                    DebugLog.WriteAlways($"[SETTINGS] Could not load '{Path.GetFileName(filePath)}': {ex.Message}")

                End Try

            Next

            ProjectValues.RecentFiles =
    If(values.RecentFiles, New List(Of RecentFileEntry))

            ProjectValues.RecentFiles.RemoveAll(
    Function(entry) entry Is Nothing OrElse
                    String.IsNullOrWhiteSpace(entry.FilePath))

            While ProjectValues.RecentFiles.Count > 10
                ProjectValues.RecentFiles.RemoveAt(
        ProjectValues.RecentFiles.Count - 1)
            End While
            SanityCheckPersistedValues()
            Return True

        Catch ex As Exception

            DebugLog.WriteAlways($"[SETTINGS] Error applying settings: {ex}")

            ' File-loading failures are handled above. This is an unexpected
            ' error applying values, so do not save a partially restored state.
            MessageBox.Show(
                "WinBMD2 encountered an unexpected error while applying your settings and must close." &
                Environment.NewLine & Environment.NewLine &
                "No settings will be saved. Details have been written to the debug log." &
                Environment.NewLine & Environment.NewLine & ex.Message,
                "WinBMD2 — Settings error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Return False

        Finally

            ProjectValues.LoadingPersistedValues = False

        End Try

    End Function

    Private Sub WriteSettingsFile(filePath As String, json As String)

        Dim temporaryPath As String = filePath & ".tmp"
        Dim backupPath As String = filePath & ".bak"

        For attempt As Integer = 1 To 3

            Try
                Directory.CreateDirectory(Path.GetDirectoryName(filePath))

                ' Write and flush the new contents before touching the existing file.
                Using stream As New FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None)

                    Using writer As New StreamWriter(stream)
                        writer.Write(json)
                        writer.Flush()
                        stream.Flush(True)
                    End Using

                End Using

                ' Verify the temporary file.
                Using document As JsonDocument = JsonDocument.Parse(File.ReadAllText(temporaryPath))
                    If document.RootElement.ValueKind <> JsonValueKind.Object Then
                        Throw New InvalidDataException("Settings must contain a JSON object.")
                    End If
                End Using

                If File.Exists(filePath) Then

                    Dim existingFileValid As Boolean = False

                    ' Check the existing file before allowing it to become our backup.
                    Try
                        Using document As JsonDocument = JsonDocument.Parse(File.ReadAllText(filePath))
                            existingFileValid = document.RootElement.ValueKind = JsonValueKind.Object
                        End Using
                    Catch ex As JsonException
                        DebugLog.WriteAlways($"[SETTINGS] Existing '{Path.GetFileName(filePath)}' contains invalid JSON.")
                    End Try

                    If existingFileValid Then

                        ' The previous valid file becomes the backup.
                        File.Replace(temporaryPath, filePath, backupPath)

                    Else

                        ' Preserve any existing backup.
                        File.Move(temporaryPath, filePath, True)

                    End If

                Else

                    ' Preserve any existing backup when the primary file is missing.
                    File.Move(temporaryPath, filePath)

                End If

                Return

            Catch ex As IOException

                DebugLog.WriteAlways($"[SETTINGS] Save attempt {attempt} failed for '{Path.GetFileName(filePath)}': {ex.Message}")

            Catch ex As UnauthorizedAccessException

                DebugLog.WriteAlways($"[SETTINGS] Save attempt {attempt} failed for '{Path.GetFileName(filePath)}': {ex.Message}")

            Catch ex As Exception

                DebugLog.WriteAlways($"[SETTINGS] Failed to save '{Path.GetFileName(filePath)}': {ex.Message}")
                Exit For

            Finally

                Try
                    If File.Exists(temporaryPath) Then File.Delete(temporaryPath)
                Catch
                    ' A temporary file that cannot be deleted is harmless.
                End Try

            End Try

            If attempt < 3 Then Threading.Thread.Sleep(150)

        Next

    End Sub
    ' To ensure that we don't save too frequently, we use a timer to delay the actual save operation. This allows multiple changes to be batched together.
    Public Sub Save()
        _savePending = True

        _saveTimer.Stop()
        _saveTimer.Start()

    End Sub
    Private Sub SaveImmediately()

        If Not _settingsReady OrElse ProjectValues.LoadingPersistedValues Then Return

        Try
            RaiseEvent SavingSettings()
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
                .HelpFormBounds = ProjectValues.HelpFormBounds,
                .ColourScheme = ProjectValues.ColourScheme,
                .UiFontName = ProjectValues.UiFontName,
                .UiFontSize = ProjectValues.UiFontSize,
                .UiFontStyle = ProjectValues.UiFontStyle,
                .UiFontColourArgb = ProjectValues.UiFontColourArgb,
                .RulerColourArgb = ProjectValues.RulerColourArgb,
                .RulerTranslucent = ProjectValues.RulerTranslucent,
                .RulerHeight = ProjectValues.RulerHeight,
                .VerifyFontName = ProjectValues.VerifyFontName,
                .VerifyFontSize = ProjectValues.VerifyFontSize,
                .VerifyFontStyle = ProjectValues.VerifyFontStyle,
                .VerifyFontColor = ProjectValues.VerifyFontColor,
                .UserName = ProjectValues.UserName,
                .UserEmail = ProjectValues.UserEmail,
                .UserPW = ProjectValues.UserPW,     ' We keep this encrypted
                .Creator = ProjectValues.Creator,
                .CreatorEmail = ProjectValues.CreatorEmail,
                .EnableDiagnosticLogging = ProjectValues.EnableDiagnosticLogging,
                .IgnoreAutoComplete = ProjectValues.IgnoreAutoComplete,
                .FormatPicklistSelections = ProjectValues.FormatPicklistSelections,
                .PickListCompletion = ProjectValues.PickListCompletion,
                .ShowForenamePickList = ProjectValues.ShowForenamePickList,
                .ShowDistrictPickList = ProjectValues.ShowDistrictPickList,
                .FloatingPickList = ProjectValues.FloatingPickList,
                .PickListBounds = ProjectValues.PickListBounds,
                .EntryMode = ProjectValues.EntryMode,
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
                .Comments = ProjectValues.Comments,
                .DistrictVersion = ProjectValues.DistrictVersion,
                .Match3VolChars = ProjectValues.Match3VolChars,
                .AutoShowScan = ProjectValues.AutoShowScan,
                .AutoShowRuler = ProjectValues.AutoShowRuler,
                .SequenceType = ProjectValues.SequenceType,
                .BatchName = ProjectValues.BatchName,
                .Created = ProjectValues.Created,
                .DateModified = ProjectValues.DateModified,
                .RecentFiles = ProjectValues.RecentFiles,
                .SaveFolder = ProjectValues.SaveFolder,
                .OutputCharacterSet = ProjectValues.OutputCharacterSet,
                .UploadServerUrl = ProjectValues.UploadServerUrl,
                .SkipSurname = ProjectValues.SkipSurname,
                .FirstRun = False
            }

            Dim options As New JsonSerializerOptions With {
                .WriteIndented = True
            }

            Dim json As String = JsonSerializer.Serialize(values, options)
            Dim jsonNode As JsonNode = JsonNode.Parse(json)

            Dim jsonObject As JsonObject = DirectCast(jsonNode, JsonObject)

            jsonObject.Remove("ScanViewSettings")
            jsonObject.Remove("RulerSettings")
            jsonObject.Remove("VerifyFieldLayouts")
            jsonObject.Remove("GridColumnWidths")

            WriteSettingsFile(AppPaths.SettingsFilePath, jsonObject.ToJsonString(options))

            Dim batchValues As New BatchSettingsData With {
                .ScanViewSettings = ProjectValues.ScanViewSettings,
                .RulerSettings = ProjectValues.RulerSettings,
                .VerifyFieldLayouts = ProjectValues.VerifyFieldLayouts,
                .GridColumnWidths = ProjectValues.GridColumnWidths
            }

            Dim batchJson As String = JsonSerializer.Serialize(batchValues, options)

            WriteSettingsFile(AppPaths.BatchSettingsFilePath, batchJson)

        Catch ex As Exception
            DebugLog.WriteAlways($"[SETTINGS] Error saving settings: {ex.Message}")
        End Try
    End Sub
    ' Writes the current settings immediately before switching batches or closing.
    Public Sub Flush()
        _saveTimer.Stop()
        _savePending = False
        SaveImmediately()
    End Sub
    Private Sub SaveTimer_Tick(sender As Object, e As EventArgs)

        _saveTimer.Stop()

        If Not _savePending Then Return

        _savePending = False

        SaveImmediately()

    End Sub
    Private Sub SanityCheckPersistedValues()

        For Each item In ProjectValues.ScanViewSettings

            Dim setting As ScanViewData = item.Value

            If setting Is Nothing Then Continue For

            If Single.IsNaN(setting.Zoom) OrElse Single.IsInfinity(setting.Zoom) OrElse setting.Zoom < 0.05F OrElse setting.Zoom > 8.0F Then
                DebugLog.WriteAlways($"[SETTINGS] Invalid scan Zoom={setting.Zoom} for '{item.Key}'. Reset to 1.")
                setting.Zoom = 1.0F
            End If

            If Single.IsNaN(setting.Rotation) OrElse Single.IsInfinity(setting.Rotation) OrElse Math.Abs(setting.Rotation) > 45.0F Then
                DebugLog.WriteAlways($"[SETTINGS] Invalid scan Rotation={setting.Rotation} for '{item.Key}'. Reset to 0.")
                setting.Rotation = 0.0F
            End If

        Next

    End Sub
    Private Function TryReadGeneralSettings(filePath As String, isBackup As Boolean) As ProjectValuesData

        Try

            DebugLog.WriteAlways($"[SETTINGS] Attempting to load: {filePath}")

            ' Read directly so an access error is not mistaken for a missing file.
            Dim json As String = File.ReadAllText(filePath)
            Dim settings As JsonObject = TryCast(JsonNode.Parse(json), JsonObject)

            If settings Is Nothing OrElse settings.Count = 0 Then
                Throw New InvalidDataException("The file does not contain a settings object.")
            End If

            ' Retain support for the older filename-only recent-file list.
            Dim recentFiles As JsonArray = TryCast(settings("RecentFiles"), JsonArray)

            If recentFiles IsNot Nothing Then

                For index As Integer = 0 To recentFiles.Count - 1

                    Dim oldEntry As JsonValue = TryCast(recentFiles(index), JsonValue)
                    Dim recentPath As String = Nothing

                    If oldEntry IsNot Nothing AndAlso oldEntry.TryGetValue(Of String)(recentPath) Then
                        recentFiles(index) = New JsonObject From {{"FilePath", JsonValue.Create(recentPath)}}
                    End If

                Next

            End If

            Dim values As ProjectValuesData = JsonSerializer.Deserialize(Of ProjectValuesData)(settings.ToJsonString())

            If values Is Nothing Then
                Throw New InvalidDataException("No settings could be read.")
            End If

            If values.FirstRun Then

                ' A first-run template is not a recovery copy of user settings.
                If isBackup Then
                    Throw New InvalidDataException("The backup contains a first-run template.")
                End If

                DebugLog.WriteAlways("[SETTINGS] Installer first-run file recognised.")
                Return New ProjectValuesData()

            End If

            DebugLog.WriteAlways($"[SETTINGS] Successfully loaded: {filePath}")
            Return values

        Catch ex As FileNotFoundException

            DebugLog.WriteAlways($"[SETTINGS] File missing: {filePath}")

        Catch ex As DirectoryNotFoundException

            DebugLog.WriteAlways($"[SETTINGS] Folder missing: {filePath}")

        Catch ex As Exception

            DebugLog.WriteAlways($"[SETTINGS] Could not load '{filePath}': {ex}")

        End Try

        Return Nothing

    End Function
    Private Function AskToContinueWithDefaultSettings() As Boolean

        Using dialog As New Form()

            dialog.Text = "WinBMD2 — Your settings could not be loaded"
            dialog.StartPosition = FormStartPosition.CenterScreen
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog
            dialog.MinimizeBox = False
            dialog.MaximizeBox = False
            dialog.AutoScaleMode = AutoScaleMode.Font
            dialog.AutoSize = True
            dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink
            dialog.Font = SystemFonts.MessageBoxFont

            Dim message As New Label With {
                .AutoSize = True,
                .MaximumSize = New Size(580, 0),
                .Margin = New Padding(0, 0, 0, 20),
                .Text = "WinBMD2 cannot find or read your settings or its automatic backup." &
                    Environment.NewLine & Environment.NewLine &
                    "If you have a backup of your settings, choose Close WinBMD2 so that you or someone helping you can restore it." &
                    Environment.NewLine & Environment.NewLine &
                    "Otherwise, choose Continue with standard settings. You may need to enter your contributor details again, choose your preferred appearance and locate your batch file." &
                    Environment.NewLine & Environment.NewLine &
                    "We have not checked whether your transcription files are still available."
            }

            Dim closeButton As New Button With {
                .Text = "Close WinBMD2",
                .AutoSize = True,
                .Padding = New Padding(8, 5, 8, 5),
                .DialogResult = DialogResult.Cancel,
                .TabIndex = 0
            }

            Dim continueButton As New Button With {
                .Text = "Continue with standard settings",
                .AutoSize = True,
                .Padding = New Padding(8, 5, 8, 5),
                .DialogResult = DialogResult.OK,
                .TabIndex = 1
            }

            Dim buttons As New FlowLayoutPanel With {
                .AutoSize = True,
                .FlowDirection = FlowDirection.LeftToRight,
                .WrapContents = False,
                .Margin = New Padding(0)
            }

            buttons.Controls.Add(closeButton)
            buttons.Controls.Add(continueButton)

            Dim layout As New TableLayoutPanel With {
                .AutoSize = True,
                .AutoSizeMode = AutoSizeMode.GrowAndShrink,
                .ColumnCount = 1,
                .RowCount = 2,
                .Padding = New Padding(20),
                .Dock = DockStyle.Fill
            }

            layout.Controls.Add(message, 0, 0)
            layout.Controls.Add(buttons, 0, 1)
            dialog.Controls.Add(layout)

            dialog.CancelButton = closeButton
            dialog.AcceptButton = closeButton
            dialog.ActiveControl = closeButton

            Return dialog.ShowDialog() = DialogResult.OK

        End Using

    End Function
    Private Sub PreserveSettingsForRecovery()

        Dim suffix As String = ".recovery-" & DateTime.Now.ToString("yyyyMMdd-HHmmss-fff") & "-" & Guid.NewGuid().ToString("N")

        For Each filePath As String In {AppPaths.SettingsFilePath, AppPaths.SettingsFilePath & ".bak"}

            Try

                Dim recoveryPath As String = filePath & suffix
                File.Copy(filePath, recoveryPath, False)
                DebugLog.WriteAlways($"[SETTINGS] Preserved recovery copy: {recoveryPath}")

            Catch ex As FileNotFoundException

                ' There is no file to preserve.

            Catch ex As DirectoryNotFoundException

                ' There is no folder containing a file to preserve.

            Catch ex As Exception

                DebugLog.WriteAlways($"[SETTINGS] Could not preserve '{filePath}': {ex}")

            End Try

        Next

    End Sub
    Private Class BatchSettingsData

        Public Property ScanViewSettings As New Dictionary(Of String, ScanViewData)
        Public Property RulerSettings As New Dictionary(Of String, RulerData)
        Public Property VerifyFieldLayouts As New Dictionary(Of String, Dictionary(Of String, VerifyFieldLayoutData))
        Public Property GridColumnWidths As New Dictionary(Of String, List(Of Integer))

    End Class
    Private Class ProjectValuesData

        Public Property FilePanelExpanded As Boolean = True
        Public Property TranscriptionFormBounds As New Dictionary(Of String, FormBoundsData)
        Public Property ScanViewLeft As Integer = -1
        Public Property ScanViewTop As Integer = -1
        Public Property ScanViewWidth As Integer = 900
        Public Property ScanViewHeight As Integer = 650
        Public Property ScanViewMaximized As Boolean
        Public Property ScanViewSettings As New Dictionary(Of String, ScanViewData)
        Public Property RulerSettings As New Dictionary(Of String, RulerData)
        Public Property VerifyFieldLayouts As New Dictionary(Of String, Dictionary(Of String, VerifyFieldLayoutData))
        Public Property HeaderFormLeft As Integer = -1
        Public Property HeaderFormTop As Integer = -1
        Public Property HeaderFormWidth As Integer = 964
        Public Property HeaderFormHeight As Integer = 681
        Public Property HeaderFormMaximized As Boolean
        Public Property GridColumnWidths As New Dictionary(Of String, List(Of Integer))
        Public Property UserName As String = ""
        Public Property UserEmail As String = ""
        Public Property UserPW As String = ""
        Public Property Creator As String = ""
        Public Property CreatorEmail As String = ""
        Public Property EnableDiagnosticLogging As Boolean = True
        Public Property IgnoreAutoComplete As IgnoreAutoCompleteKey = IgnoreAutoCompleteKey.Tab
        Public Property EntryMode As EntryMode = EntryMode.Horizontal
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
        Public Property DistrictVersion As String = "??"
        Public Property BatchName As String = ""
        Public Property Created As String = ""
        Public Property DateModified As Date = Date.Today
        Public Property SkipSurname As Boolean = False
        Public Property SaveFolder As String = ""
        Public Property UploadServerUrl As String = "www.freebmd.org.uk"
        Public Property OutputCharacterSet As String = "ISO-8859-1"
        Public Property AutoShowScan As Boolean = True
        Public Property AutoShowRuler As Boolean = True
        Public Property FormatPicklistSelections As Boolean = False
        Public Property PickListCompletion As Boolean = True    ' The picklist column auto-complete characters
        Public Property ShowForenamePickList As Boolean = True
        Public Property ShowDistrictPickList As Boolean = True
        Public Property FloatingPickList As Boolean = False
        Public Property PickListBounds As New FormBoundsData With {.Width = 300, .Height = 250}
        Public Property Match3VolChars As Boolean = False
        Public Property SequenceType As String = "SEQUENCED"
        Public Property ColourScheme As UiColourScheme = UiColourScheme.Teal
        Public Property UiFontName As String = SystemFonts.MessageBoxFont.FontFamily.Name
        Public Property UiFontSize As Single = SystemFonts.MessageBoxFont.Size
        Public Property UiFontStyle As FontStyle = SystemFonts.MessageBoxFont.Style
        Public Property UiFontColourArgb As Integer = SystemColors.ControlText.ToArgb()
        Public Property VerifyFontName As String = ""
        Public Property VerifyFontSize As Single = 0
        Public Property VerifyFontStyle As FontStyle = FontStyle.Regular
        Public Property VerifyFontColor As Integer = Color.Empty.ToArgb()
        Public Property RecentFiles As New List(Of RecentFileEntry)
        Public Property HelpFormBounds As New FormBoundsData()
        Public Property RulerColourArgb As Integer = Color.Red.ToArgb()
        Public Property RulerTranslucent As Boolean = False
        Public Property RulerHeight As Integer = 4
        Public Property FirstRun As Boolean = False             ' True only in the installer-supplied template json.
    End Class

End Module