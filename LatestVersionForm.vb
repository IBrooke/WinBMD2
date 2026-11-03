Imports System.Diagnostics
Imports System.Net.Http
Imports System.Windows.Forms

Public Class LatestVersionForm
    Inherits Form

    Private Const VersionUrl As String = "https://www.dropbox.com/scl/fi/q88d5armpcdm62h80gu18/WinBMD2_Version_File.txt?rlkey=vr9uvtxntzg4el40o0z5n005l&dl=1"

    Private ReadOnly installedLabel As New Label()
    Private ReadOnly latestLabel As New Label()
    Private ReadOnly statusLabel As New Label()
    Private ReadOnly changesBox As New TextBox()
    Private ReadOnly downloadLink As New LinkLabel()
    Private ReadOnly closeButton As New Button()
    Private ReadOnly checkCancellation As New Threading.CancellationTokenSource()

    Private downloadUrl As String = ""

    Public Sub New()

        Text = "WinBMD2 — Latest version"
        StartPosition = FormStartPosition.CenterParent
        ClientSize = New Drawing.Size(620, 450)
        MinimumSize = New Drawing.Size(480, 350)
        MinimizeBox = False
        MaximizeBox = False
        ShowInTaskbar = False
        AutoScaleMode = AutoScaleMode.Dpi

        installedLabel.AutoSize = True
        installedLabel.Text = "Installed version:"

        latestLabel.AutoSize = True
        latestLabel.Text = "Latest version: Checking…"

        statusLabel.AutoSize = True
        statusLabel.Text = "Contacting the update server…"

        changesBox.Multiline = True
        changesBox.ReadOnly = True
        changesBox.ScrollBars = ScrollBars.Vertical
        changesBox.Dock = DockStyle.Fill
        changesBox.BackColor = Drawing.SystemColors.Window

        downloadLink.AutoSize = True
        downloadLink.Text = "Download latest version"
        downloadLink.Enabled = False
        downloadLink.Margin = New Padding(0, 8, 0, 8)

        closeButton.Text = "Close"
        closeButton.AutoSize = True
        closeButton.Anchor = AnchorStyles.Right
        closeButton.DialogResult = DialogResult.Cancel

        Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .Padding = New Padding(16), .ColumnCount = 1, .RowCount = 7}

        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        layout.Controls.Add(installedLabel, 0, 0)
        layout.Controls.Add(latestLabel, 0, 1)
        layout.Controls.Add(statusLabel, 0, 2)
        layout.Controls.Add(New Label With {.Text = "Changes in the latest version:", .AutoSize = True, .Margin = New Padding(0, 12, 0, 6)}, 0, 3)
        layout.Controls.Add(changesBox, 0, 4)
        layout.Controls.Add(downloadLink, 0, 5)
        layout.Controls.Add(closeButton, 0, 6)

        Controls.Add(layout)
        CancelButton = closeButton

        AddHandler Shown, AddressOf LatestVersionForm_Shown
        AddHandler downloadLink.LinkClicked, AddressOf DownloadLink_LinkClicked
        AddHandler FormClosing, Sub(sender, e) checkCancellation.Cancel()

    End Sub

    Private Async Sub LatestVersionForm_Shown(sender As Object, e As EventArgs)

        Try

            Dim versionInfo As FileVersionInfo = FileVersionInfo.GetVersionInfo(Application.ExecutablePath)
            Dim installedVersion As New Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart)

            installedLabel.Text = "Installed version: " & installedVersion.ToString(3)

            Dim versionFileText As String

            Using client As New HttpClient With {.Timeout = TimeSpan.FromSeconds(20)}
                versionFileText = Await client.GetStringAsync(VersionUrl, checkCancellation.Token)
            End Using

            If IsDisposed OrElse Disposing OrElse checkCancellation.IsCancellationRequested Then Return

            If String.IsNullOrWhiteSpace(versionFileText) Then
                Throw New InvalidOperationException("The WinBMD2 version file is empty.")
            End If

            Dim lines() As String = versionFileText.Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
            Dim parsedVersion As Version = Nothing

            If Not Version.TryParse(lines(0).Trim(), parsedVersion) Then
                Throw New InvalidOperationException("The latest version number is not valid.")
            End If

            Dim latestVersion As New Version(parsedVersion.Major, parsedVersion.Minor, Math.Max(0, parsedVersion.Build), Math.Max(0, parsedVersion.Revision))

            latestLabel.Text = "Latest version: " & latestVersion.ToString(3)

            If latestVersion > installedVersion Then
                statusLabel.Text = "A newer version is available."
            ElseIf latestVersion = installedVersion Then
                statusLabel.Text = "You are up to date."
            Else
                statusLabel.Text = "Your installed version is newer than the published version."
            End If

            changesBox.Text = If(lines.Length > 2, String.Join(Environment.NewLine, lines, 2, lines.Length - 2).Trim(), "")

            If changesBox.Text = "" Then
                changesBox.Text = "No change details were supplied."
            End If

            If lines.Length > 1 Then
                downloadUrl = lines(1).Trim()
            End If

            Dim downloadUri As Uri = Nothing

            downloadLink.Enabled = Uri.TryCreate(downloadUrl, UriKind.Absolute, downloadUri) AndAlso (downloadUri.Scheme = Uri.UriSchemeHttps OrElse downloadUri.Scheme = Uri.UriSchemeHttp)

        Catch ex As Exception

            If IsDisposed OrElse Disposing OrElse checkCancellation.IsCancellationRequested Then Return

            latestLabel.Text = "Latest version: Unavailable"
            statusLabel.Text = "The version check could not be completed."
            changesBox.Text = ex.Message
            downloadLink.Enabled = False

        End Try

    End Sub

    Private Sub DownloadLink_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)

        Try
            Process.Start(New ProcessStartInfo(downloadUrl) With {.UseShellExecute = True})
        Catch ex As Exception
            MessageBox.Show(Me, "The download link could not be opened." & Environment.NewLine & Environment.NewLine & ex.Message, "WinBMD2", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try

    End Sub

End Class