Imports System.IO

Public Module AppPaths

    Public ReadOnly Property BaseFolder As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FreeBMD", "WinBMD2_Files")
    Public ReadOnly Property DebugLogFilePath As String =
    Path.Combine(BaseFolder, "WinBMD2.log")
    Public ReadOnly Property DownloadedScansFolder As String =
        Path.Combine(BaseFolder, "DownloadedScans")

    Public ReadOnly Property FilesFolder As String =
        Path.Combine(BaseFolder, "Files")

    Public ReadOnly Property OutputFolder As String =
        Path.Combine(BaseFolder, "Output")

    Public ReadOnly Property SettingsFilePath As String =
        Path.Combine(BaseFolder, "settings.json")

    Public Sub EnsureFoldersExist()
        Directory.CreateDirectory(BaseFolder)
        Directory.CreateDirectory(DownloadedScansFolder)
        Directory.CreateDirectory(FilesFolder)
        Directory.CreateDirectory(OutputFolder)
    End Sub

End Module