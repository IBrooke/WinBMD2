
Imports System.IO
Imports System.Reflection
Imports WinBMD2.My.Resources
Public Class HelpForm
    Public Sub New(Optional editMode As Boolean = False)

        InitializeComponent()

        Dim names() As String =
        Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()

        For Each name As String In names
            DebugLog.Write("[HELP] Resource: " & name)
        Next

        RestoreFormBounds()

        ApplyColourScheme()

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2 Help"

    End Sub
    Private Sub RestoreFormBounds()

        FormBoundsHelper.RestoreForm(
        Me,
        ProjectValues.HelpFormBounds.Left,
        ProjectValues.HelpFormBounds.Top,
        ProjectValues.HelpFormBounds.Width,
        ProjectValues.HelpFormBounds.Height,
        ProjectValues.HelpFormBounds.Maximized)

    End Sub
    Public Sub ApplyColourScheme()

        ThemeManager.Apply(Me)

        ThemeManager.ApplyNavigationPanel(splHelp.Panel1)
        ThemeManager.ApplyNavigationPanel(lstTopics)

        Invalidate(True)

    End Sub
    Private Sub SaveFormBounds()

        Dim boundsToSave As Rectangle =
        FormBoundsHelper.GetBoundsToSave(Me)

        ProjectValues.HelpFormBounds.Left = boundsToSave.Left
        ProjectValues.HelpFormBounds.Top = boundsToSave.Top
        ProjectValues.HelpFormBounds.Width = boundsToSave.Width
        ProjectValues.HelpFormBounds.Height = boundsToSave.Height
        ProjectValues.HelpFormBounds.Maximized = FormBoundsHelper.ShouldRestoreMaximized(Me)

        ProjectValuesStore.Save()

        DebugLog.Write(
        "[FORM] HelpForm bounds saved: " &
        "Left=" & boundsToSave.Left.ToString() &
        ", Top=" & boundsToSave.Top.ToString() &
        ", Width=" & boundsToSave.Width.ToString() &
        ", Height=" & boundsToSave.Height.ToString() &
        ", Maximized=" & ProjectValues.HelpFormBounds.Maximized.ToString())

    End Sub

    Private Sub HelpForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub
    Private Sub HelpForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        lstTopics.Items.Add("Getting Started")
        lstTopics.Items.Add("The Header Form")
        lstTopics.Items.Add("Transcribing")
        lstTopics.Items.Add("The Grid")
        lstTopics.Items.Add("Verification")
        lstTopics.Items.Add("Scans")
        lstTopics.Items.Add("Ruler")
        lstTopics.Items.Add("Files")
        lstTopics.Items.Add("Uploading")
        lstTopics.Items.Add("Options")
        lstTopics.Items.Add("Keyboard Shortcuts")
        lstTopics.Items.Add("Troubleshooting")
        lstTopics.Items.Add("About WinBMD2")

        If lstTopics.Items.Count > 0 Then lstTopics.SelectedIndex = 0

    End Sub
    Private Sub lstTopics_DrawItem(sender As Object, e As DrawItemEventArgs) Handles lstTopics.DrawItem

        If e.Index < 0 Then
            Return
        End If

        Dim selected As Boolean =
        (e.State And DrawItemState.Selected) = DrawItemState.Selected

        Dim backColour As Color =
        If(selected, UiColors.Selected, UiColors.ThemeSoft)

        Dim textColour As Color =
        UiColors.TextPrimary

        Using backgroundBrush As New SolidBrush(backColour)
            e.Graphics.FillRectangle(backgroundBrush, e.Bounds)
        End Using

        TextRenderer.DrawText(
        e.Graphics,
        lstTopics.Items(e.Index).ToString(),
        lstTopics.Font,
        e.Bounds,
        textColour,
        TextFormatFlags.Left Or TextFormatFlags.VerticalCenter)

        e.DrawFocusRectangle()

    End Sub
    Private Sub lstTopics_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstTopics.SelectedIndexChanged

        Select Case lstTopics.SelectedItem?.ToString()

            Case "Getting Started"
                LoadHelpTopic("WinBMD2.Help_GettingStarted.rtf")

            Case "The Header Form"
                LoadHelpTopic("WinBMD2.Help_HeaderForm.rtf")

            Case "Transcribing"
                LoadHelpTopic("WinBMD2.Help_Transcribing.rtf")

            Case "The Grid"
                LoadHelpTopic("WinBMD2.Help_Grid.rtf")

            Case "Verification"
                LoadHelpTopic("WinBMD2.Help_Verification.rtf")

            Case "Scans"
                LoadHelpTopic("WinBMD2.Help_Scans.rtf")

            Case "Ruler"
                LoadHelpTopic("WinBMD2.Help_Ruler.rtf")

            Case "Files"
                LoadHelpTopic("WinBMD2.Help_Files.rtf")

            Case "Uploading"
                LoadHelpTopic("WinBMD2.Help_Uploading.rtf")

            Case "Options"
                LoadHelpTopic("WinBMD2.Help_Options.rtf")

            Case "Keyboard Shortcuts"
                LoadHelpTopic("WinBMD2.Help_KeyboardShortcuts.rtf")

            Case "Troubleshooting"
                LoadHelpTopic("WinBMD2.Help_Troubleshooting.rtf")

            Case "About WinBMD2"
                LoadHelpTopic("WinBMD2.Help_About.rtf")

        End Select

    End Sub
    Private Sub LoadHelpTopic(resourceName As String)

        Dim assembly As Assembly = Assembly.GetExecutingAssembly()

        Using stream As Stream = assembly.GetManifestResourceStream(resourceName)

            If stream Is Nothing Then
                rtbHelp.Text = "Help topic could not be found."
                Return
            End If

            Using reader As New StreamReader(stream)

                Dim rtf As String = reader.ReadToEnd()

                Dim version As Version = Assembly.GetExecutingAssembly().GetName().Version

                rtf = rtf.Replace("%%APPVERSION%%", $"{version.Major}.{version.Minor}.{version.Build}")

                rtbHelp.Rtf = rtf

            End Using
        End Using

    End Sub
    Private Sub rtbHelp_LinkClicked(sender As Object, e As LinkClickedEventArgs) Handles rtbHelp.LinkClicked

        If e.LinkText.StartsWith("winbmdhelp://", StringComparison.OrdinalIgnoreCase) Then

            Select Case e.LinkText

                Case "winbmdhelp://directives"
                    LoadSubHelp("directives")

                Case "winbmdhelp://findingscans"
                    LoadSubHelp("findingscans")

            End Select

            Return

        End If

        If e.LinkText.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
           e.LinkText.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then

            Process.Start(New ProcessStartInfo(e.LinkText) With {
                .UseShellExecute = True
            })

        End If

    End Sub
    Private Sub LoadSubHelp(topic As String)

        Select Case topic

            Case "directives"
                LoadHelpTopic("WinBMD2.Help_Directives.rtf")

            Case "findingscans"
                LoadHelpTopic("WinBMD2.Help_FindingScans.rtf")

        End Select

    End Sub
End Class