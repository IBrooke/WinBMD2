Imports System.IO
Imports System.Net.Mail
Imports System.Linq
Imports WinBMD2.My.Resources
Imports System.ComponentModel
Imports System.Threading
Imports System.Runtime.CompilerServices

Public Class HeaderForm

    Private Const FirstYearWithoutQuarters As Integer = 1984
    Private Const FirstAllowedYear As Integer = 1837
    Private Const LastAllowedYear As Integer = 2000

    Private ReadOnly _toolTip As New ToolTip()
    Private _loadingValues As Boolean
    Private ReadOnly _openBatchMenu As New ContextMenuStrip()
    Private ReadOnly _editMode As Boolean
    Private _showVnfWarningWhenShown As Boolean
    Private _vnfWarningShown As Boolean

    Public Sub New(Optional editMode As Boolean = False)

        _editMode = editMode

        InitializeComponent()
        RestoreFormBounds()

        ApplyColourScheme()

        Icon = WinBMDResources.WinBMD2Icon
        Text = "WinBMD2 Header"
        If _editMode Then
            Text = "WinBMD2 - Edit Header"
        End If

        ConfigureControls()
        ConfigureToolTips()
        LoadFromProjectValues()
        HookValidationEvents()

        UpdateQuarterVisibility()
        _showVnfWarningWhenShown = SuggestVolumeFormat()
        ValidateForm()
    End Sub
    Private Sub HeaderForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        If _showVnfWarningWhenShown Then
            ShowVnfWarning()
            _showVnfWarningWhenShown = False
        End If

    End Sub
    Public Sub ApplyColourScheme()

        ThemeManager.Apply(Me)

        ThemeManager.ApplyApplicationBackground(rootLayout)
        ThemeManager.ApplyApplicationBackground(bodyLayout)
        ThemeManager.ApplyApplicationBackground(sideLayout)

        ThemeManager.ApplyCardPanel(headerPanel)
        ThemeManager.ApplyCardPanel(batchPanel)
        ThemeManager.ApplyCardPanel(contributorPanel)
        ThemeManager.ApplyCardPanel(footerPanel)

        ThemeManager.ApplyPrimaryButton(btnStart)
        ThemeManager.ApplyInformationPanel(rulesPanel)

        Invalidate(True)

    End Sub
    Private Sub RestoreFormBounds()

        FormBoundsHelper.RestoreForm(
        Me,
        ProjectValues.HeaderFormLeft,
        ProjectValues.HeaderFormTop,
        ProjectValues.HeaderFormWidth,
        ProjectValues.HeaderFormHeight,
        ProjectValues.HeaderFormMaximized)

    End Sub

    Private Sub SaveFormBounds()

        Dim boundsToSave As Rectangle =
        FormBoundsHelper.GetBoundsToSave(Me)

        ProjectValues.HeaderFormLeft = boundsToSave.Left
        ProjectValues.HeaderFormTop = boundsToSave.Top
        ProjectValues.HeaderFormWidth = boundsToSave.Width
        ProjectValues.HeaderFormHeight = boundsToSave.Height

        ProjectValues.HeaderFormMaximized =
        FormBoundsHelper.ShouldRestoreMaximized(Me)

        ProjectValuesStore.Save()

        DebugLog.Write(
        "[FORM] HeaderForm bounds saved: " &
        "Left=" & boundsToSave.Left.ToString() &
        ", Top=" & boundsToSave.Top.ToString() &
        ", Width=" & boundsToSave.Width.ToString() &
        ", Height=" & boundsToSave.Height.ToString() &
        ", Maximized=" &
        ProjectValues.HeaderFormMaximized.ToString())

    End Sub

    Private Sub HeaderForm_FormClosing(
    sender As Object,
    e As FormClosingEventArgs) Handles Me.FormClosing

        SaveFormBounds()

    End Sub
    Private Sub ConfigureControls()

        If pageSourceComboBox.Items.Count > 0 AndAlso pageSourceComboBox.SelectedIndex < 0 Then
            pageSourceComboBox.SelectedIndex = 0
        End If

        userPasswordTextBox.UseSystemPasswordChar = True

        btnStart.Enabled = False

        AddHandler showPasswordButton.Click, AddressOf ShowPasswordButton_Click
        AddHandler btnStart.Click, AddressOf btnStart_Click
        AddHandler btnCancel.Click, AddressOf btnCancel_Click
        AddHandler openBatchButton.Click, AddressOf OpenBatchButton_Click
        AddHandler optionsButton.Click, AddressOf optionsButton_Click

    End Sub

    Private Sub ConfigureToolTips()

        _toolTip.SetToolTip(
            birthsRadioButton,
            "Select this for a Births batch.")

        _toolTip.SetToolTip(
            marriagesRadioButton,
            "Select this for a Marriages batch.")

        _toolTip.SetToolTip(
            deathsRadioButton,
            "Select this for a Deaths batch.")

        _toolTip.SetToolTip(
            yearTextBox,
            "Enter the registration year for this batch." &
            Environment.NewLine &
            "Years before 1984 require a quarter.")

        _toolTip.SetToolTip(
            quarterComboBox,
            "Select the registration quarter." &
            Environment.NewLine &
            "Quarter is only used for years before 1984.")

        _toolTip.SetToolTip(
            pageSourceComboBox,
            "Select where the page image came from." &
            Environment.NewLine &
            "This will normally be Scan.")

        _toolTip.SetToolTip(
            pageTextBox,
            "Enter the FreeBMD page number for this batch.")

        _toolTip.SetToolTip(
            suffixTextBox,
            "Enter the page suffix when one exists." &
            Environment.NewLine &
            "For example, enter A when the page is shown as 123A.")

        _toolTip.SetToolTip(
            pageLetterTextBox,
            "Enter the first letter of the first surname on the page." &
            Environment.NewLine &
            "This is used when building the batch filename.")

        _toolTip.SetToolTip(
            vnfComboBox,
            "The volume format is suggested automatically from the record type, year and quarter.")

        _toolTip.SetToolTip(
            sourceRefTextBox,
            "Optional source reference used to locate the corresponding scan.")

        _toolTip.SetToolTip(
            commentsTextBox,
            "Optional notes saved in the batch header.")

        _toolTip.SetToolTip(
            creatorTextBox,
            "The FreeBMD User ID of the person who originally created this file.")

        _toolTip.SetToolTip(
            creatorEmailTextBox,
            "The email address of the person who originally created this file.")

        _toolTip.SetToolTip(
            syndicateTextBox,
            "The FreeBMD syndicate associated with the original creator.")

        _toolTip.SetToolTip(
            userNameTextBox,
            "The current FreeBMD User ID used when uploading this file.")

        _toolTip.SetToolTip(
            userPasswordTextBox,
            "The password belonging to the current FreeBMD User ID.")

        _toolTip.SetToolTip(
            showPasswordButton,
            "Show or hide the current user's password.")

        _toolTip.SetToolTip(
            creditNameTextBox,
            "Credit details are optional." &
            Environment.NewLine &
            "When one credit field is entered, all three credit fields are required.")

        _toolTip.SetToolTip(
            creditEmailTextBox,
            "Credit details are optional." &
            Environment.NewLine &
            "When one credit field is entered, all three credit fields are required.")

        _toolTip.SetToolTip(
            creditTypeTextBox,
            "Credit details are optional." &
            Environment.NewLine &
            "When one credit field is entered, all three credit fields are required.")

    End Sub
    Private Sub OpenBatchButton_Click(
    sender As Object,
    e As EventArgs)

        BuildOpenBatchMenu()

        _openBatchMenu.Show(
        openBatchButton,
        New Point(0, openBatchButton.Height))

    End Sub
    Private Sub optionsButton_Click(
    sender As Object,
    e As EventArgs)

        Using form As New OptionsForm()

            If form.ShowDialog(Me) = DialogResult.OK Then
                ApplyColourScheme()
            End If

        End Using

    End Sub
    Private Sub BuildOpenBatchMenu()

        _openBatchMenu.Items.Clear()

        For Each filePath As String In ProjectValues.RecentFiles

            If String.IsNullOrWhiteSpace(filePath) Then
                Continue For
            End If

            Dim item As New ToolStripMenuItem(
            Path.GetFileName(filePath))

            item.Tag = filePath

            AddHandler item.Click,
            AddressOf RecentBatch_Click

            _openBatchMenu.Items.Add(item)

        Next

        If _openBatchMenu.Items.Count > 0 Then
            _openBatchMenu.Items.Add(
            New ToolStripSeparator())
        End If

        Dim browseItem As New ToolStripMenuItem(
        "Browse...")

        AddHandler browseItem.Click,
        AddressOf BrowseBatch_Click

        _openBatchMenu.Items.Add(browseItem)

    End Sub
    Private Sub RecentBatch_Click(
    sender As Object,
    e As EventArgs)

        Dim item As ToolStripMenuItem =
        DirectCast(sender, ToolStripMenuItem)

        Dim filePath As String =
        DirectCast(item.Tag, String)

        If Not File.Exists(filePath) Then

            ProjectValues.RecentFiles.Remove(filePath)
            ProjectValuesStore.Save()

            MessageBox.Show(
            Me,
            "That file no longer exists and has been removed from the recent files list.",
            "Open Batch",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information)

            Return

        End If

        OpenExistingBatch(filePath)

    End Sub
    Private Sub BrowseBatch_Click(
    sender As Object,
    e As EventArgs)

        Using dialog As New OpenFileDialog()

            dialog.Filter =
            "BMD Files (*.BMD)|*.BMD|All Files (*.*)|*.*"

            dialog.Title =
            "Open Existing Batch"

            dialog.InitialDirectory =
            AppPaths.SaveFolder

            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            OpenExistingBatch(dialog.FileName)

        End Using

    End Sub
    Private Sub OpenExistingBatch(filePath As String)

        If Not File.Exists(filePath) Then
            Return
        End If

        ProjectValues.BatchName = Path.GetFileName(filePath)

        ProjectValuesStore.Save()

        DialogResult = DialogResult.OK

        Tag = filePath

        Close()

    End Sub
    Private Sub LoadFromProjectValues()

        _loadingValues = True

        Try
            yearTextBox.Text =
                If(ProjectValues.Year > 0,
                   ProjectValues.Year.ToString(),
                   "")

            pageTextBox.Text =
                If(ProjectValues.Page > 0,
                   ProjectValues.Page.ToString(),
                   "")

            If ProjectValues.PageSource >= 0 AndAlso
               ProjectValues.PageSource < pageSourceComboBox.Items.Count Then

                pageSourceComboBox.SelectedIndex = ProjectValues.PageSource

            ElseIf pageSourceComboBox.Items.Count > 0 Then
                pageSourceComboBox.SelectedIndex = 0
            End If

            suffixTextBox.Text = ProjectValues.PageSuffix
            pageLetterTextBox.Text = ProjectValues.PageLetter
            sourceRefTextBox.Text = ProjectValues.SourceRef
            commentsTextBox.Text = ProjectValues.Comments

            creatorTextBox.Text = ProjectValues.Creator
            creatorEmailTextBox.Text = ProjectValues.CreatorEmail
            syndicateTextBox.Text = ProjectValues.Syndicate

            userNameTextBox.Text = ProjectValues.UserName
            userPasswordTextBox.Text = ProjectValues.UserPW

            birthsRadioButton.Checked = ProjectValues.BatchType = "B"
            marriagesRadioButton.Checked = ProjectValues.BatchType = "M"
            deathsRadioButton.Checked = ProjectValues.BatchType = "D"

            If Not birthsRadioButton.Checked AndAlso
               Not marriagesRadioButton.Checked AndAlso
               Not deathsRadioButton.Checked Then

                birthsRadioButton.Checked = True
            End If

            If ProjectValues.Quarter >= 1 AndAlso
               ProjectValues.Quarter <= 4 Then

                quarterComboBox.SelectedIndex =
                    ProjectValues.Quarter - 1
            Else
                quarterComboBox.SelectedIndex = -1
            End If

            If Not String.IsNullOrWhiteSpace(ProjectValues.VNF) Then
                vnfComboBox.SelectedItem = ProjectValues.VNF
            Else
                vnfComboBox.SelectedIndex = -1
            End If

        Finally
            _loadingValues = False
        End Try

    End Sub

    Private Sub HookValidationEvents()

        AddHandler yearTextBox.TextChanged, AddressOf YearTextBox_TextChanged

        AddHandler pageTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler suffixTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler pageLetterTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler sourceRefTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler commentsTextBox.TextChanged, AddressOf HeaderValueChanged

        AddHandler creatorTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler creatorEmailTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler syndicateTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler userNameTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler userPasswordTextBox.TextChanged, AddressOf HeaderValueChanged

        AddHandler creditNameTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler creditEmailTextBox.TextChanged, AddressOf HeaderValueChanged
        AddHandler creditTypeTextBox.TextChanged, AddressOf HeaderValueChanged

        AddHandler quarterComboBox.SelectedIndexChanged, AddressOf QuarterChanged
        AddHandler pageSourceComboBox.SelectedIndexChanged, AddressOf HeaderValueChanged
        AddHandler vnfComboBox.SelectedIndexChanged, AddressOf HeaderValueChanged

        AddHandler birthsRadioButton.CheckedChanged, AddressOf RecordTypeChanged
        AddHandler marriagesRadioButton.CheckedChanged, AddressOf RecordTypeChanged
        AddHandler deathsRadioButton.CheckedChanged, AddressOf RecordTypeChanged

    End Sub

    Private Sub YearTextBox_TextChanged(
        sender As Object,
        e As EventArgs)

        If _loadingValues Then
            Return
        End If

        sourceRefTextBox.Clear()

        UpdateQuarterVisibility()
        If SuggestVolumeFormat() Then
            ShowVnfWarning()
        End If
        ValidateForm()

    End Sub

    Private Sub QuarterChanged(
        sender As Object,
        e As EventArgs)

        If _loadingValues Then
            Return
        End If

        sourceRefTextBox.Clear()

        If SuggestVolumeFormat() Then
            ShowVnfWarning()
        End If
        ValidateForm()

    End Sub

    Private Sub RecordTypeChanged(
        sender As Object,
        e As EventArgs)

        If _loadingValues Then
            Return
        End If

        sourceRefTextBox.Clear()

        If SuggestVolumeFormat() Then
            ShowVnfWarning()
        End If
        ValidateForm()

    End Sub

    Private Sub HeaderValueChanged(
        sender As Object,
        e As EventArgs)

        If _loadingValues Then
            Return
        End If

        ValidateForm()

    End Sub

    Private Sub UpdateQuarterVisibility()

        Dim year As Integer

        If Not Integer.TryParse(yearTextBox.Text.Trim(), year) Then
            quarterPanel.Visible = True
            Return
        End If

        Dim quarterRequired As Boolean =
            year < FirstYearWithoutQuarters

        quarterPanel.Visible = quarterRequired

        If Not quarterRequired Then
            quarterComboBox.SelectedIndex = -1
        End If

    End Sub

    Private Function SuggestVolumeFormat() As Boolean

        Dim quarter As Integer = If(quarterComboBox.SelectedIndex >= 0, quarterComboBox.SelectedIndex + 1, 0)
        Dim year As Integer

        ' Do not suggest a volume format until the year and, where required, the quarter are complete.
        ' The routine will be called again automatically when either value changes.

        If yearTextBox.Text.Trim().Length <> 4 OrElse Not Integer.TryParse(yearTextBox.Text.Trim(), year) Then
            Return False
        End If

        If year < FirstYearWithoutQuarters AndAlso quarterComboBox.SelectedIndex < 0 Then
            Return False
        End If

        If yearTextBox.Text.Trim().Length <> 4 OrElse Not Integer.TryParse(yearTextBox.Text.Trim(), year) Then
            Return False
        End If

        If year < FirstYearWithoutQuarters AndAlso quarterComboBox.SelectedIndex < 0 Then
            Return False
        End If

        Dim batchType As String = GetSelectedBatchType()
        Dim suggestedFormat As String = ""
        Dim warningNeeded As Boolean = False
        Dim earlyPeriod As Boolean = False

        Select Case batchType
            Case "B"
                If year < 1839 Then
                    suggestedFormat = "99"
                    warningNeeded = True
                    earlyPeriod = True
                ElseIf year < 1860 Then
                    suggestedFormat = "XX"
                    warningNeeded = True
                    earlyPeriod = True
                End If

            Case "M"
                If year < 1843 Then
                    suggestedFormat = "99"
                    warningNeeded = True
                    earlyPeriod = True
                ElseIf year < 1866 Then
                    suggestedFormat = "XX"
                    warningNeeded = True
                    earlyPeriod = True
                End If

            Case "D"
                If year < 1837 Then
                    suggestedFormat = "99"
                    warningNeeded = True
                    earlyPeriod = True
                ElseIf year < 1842 Then
                    suggestedFormat = "XX"
                    warningNeeded = True
                    earlyPeriod = True
                End If
        End Select

        If Not earlyPeriod Then
            If year < 1946 OrElse (year = 1946 AndAlso quarter <= 2) Then
                suggestedFormat = "9Z"
            ElseIf year < 1965 OrElse (year = 1965 AndAlso quarter <= 1) Then
                suggestedFormat = "9Z"
            ElseIf year < 1974 OrElse (year = 1974 AndAlso quarter <= 1) Then
                suggestedFormat = "9Z"
            ElseIf batchType = "M" Then
                suggestedFormat = If(year >= 1994, "999", "99")
            Else
                suggestedFormat = If(year >= 1993, "999", "99")
            End If
        End If

        vnfComboBox.SelectedItem = suggestedFormat

        Return warningNeeded

    End Function
    Private Sub ShowVnfWarning()
        ' Ensure the warning message is only shown once

        If _vnfWarningShown Then
            Return
        End If

        _vnfWarningShown = True

        MessageBox.Show(
        Me,
        "Early FreeBMD scans may exist in more than one volume-number format." & Environment.NewLine & Environment.NewLine &
        "Handwritten scans normally use Roman volume numbers (XX), while typed scans normally use Arabic volume numbers (99)." & Environment.NewLine & Environment.NewLine &
        "WinBMD2 has selected the most likely Volume Number Format for this year, quarter and record type, but this cannot always be determined in advance." & Environment.NewLine & Environment.NewLine &
        "Please check the scan carefully and change the Volume Number Format if necessary." & Environment.NewLine &
        "You can do this by clicking Edit Header on the transcription form.",
        "Check Volume Number Format",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning)

    End Sub
    Private Sub ValidateForm()

        Dim problems As New List(Of String)

        ValidateIdentityFields(problems)
        ValidateCreditFields(problems)
        ValidateBatchFields(problems)

        Dim ready As Boolean =
            problems.Count = 0

        btnStart.Enabled = ready

        If ready Then
            validationSummaryLabel.ForeColor =
                Color.FromArgb(40, 120, 60)

            validationSummaryLabel.Text =
                "Ready to start transcribing."

            btnStart.Text = If(_editMode, "Save changes", "Start batch")
        Else
            validationSummaryLabel.ForeColor =
                Color.FromArgb(130, 75, 20)

            validationSummaryLabel.Text =
                "Still required: " &
                String.Join(", ", problems)

            btnStart.Text = If(_editMode, "Save changes", "Start batch")
        End If

        ApplyRequiredFieldColours(problems)

    End Sub
    Private Function ProposedHeaderChangesGridLayout() As Boolean

        Dim oldFields() As GridField =
        GridLayout.GetVisibleFields(
            ProjectValues.BatchType,
            ProjectValues.Year,
            ProjectValues.Quarter)

        Dim proposedBatchType As String =
        GetSelectedBatchType()

        Dim proposedYear As Integer

        If Not Integer.TryParse(
        yearTextBox.Text.Trim(),
        proposedYear) Then

            Return True
        End If

        Dim proposedQuarter As Integer =
        If(
            quarterComboBox.SelectedIndex >= 0,
            quarterComboBox.SelectedIndex + 1,
            0)

        Dim newFields() As GridField =
        GridLayout.GetVisibleFields(
            proposedBatchType,
            proposedYear,
            proposedQuarter)
        'Return Not oldFields.SequenceEqual(newFields)
        Return oldFields.Length <> newFields.Length

    End Function
    Private Sub ValidateIdentityFields(
        problems As List(Of String))

        Dim creator As String =
            creatorTextBox.Text.Trim()

        Dim creatorEmail As String =
            creatorEmailTextBox.Text.Trim()

        Dim userName As String =
            userNameTextBox.Text.Trim()

        Dim userPassword As String =
            userPasswordTextBox.Text

        If creator.Length = 0 Then
            problems.Add("Original creator")
        End If

        If creatorEmail.Length = 0 Then
            problems.Add("Creator email")
        ElseIf Not IsValidEmailAddress(creatorEmail) Then
            problems.Add("Valid creator email")
        End If

        If syndicateTextBox.Text.Trim().Length = 0 Then
            problems.Add("Syndicate")
        End If

        If userName.Length = 0 Then
            problems.Add("Current FreeBMD User ID")
        End If

        If userPassword.Length = 0 Then
            problems.Add("Current user password")
        End If

    End Sub

    Private Sub ValidateCreditFields(
        problems As List(Of String))

        Dim creditName As String =
            creditNameTextBox.Text.Trim()

        Dim creditEmail As String =
            creditEmailTextBox.Text.Trim()

        Dim creditType As String =
            creditTypeTextBox.Text.Trim()

        Dim anyCreditEntered As Boolean =
            creditName.Length > 0 OrElse
            creditEmail.Length > 0 OrElse
            creditType.Length > 0

        If Not anyCreditEntered Then
            Return
        End If

        If creditName.Length = 0 Then
            problems.Add("Credit name")
        End If

        If creditEmail.Length = 0 Then
            problems.Add("Credit email")
        ElseIf Not IsValidEmailAddress(creditEmail) Then
            problems.Add("Valid credit email")
        End If

        If creditType.Length = 0 Then
            problems.Add("Credit type")
        End If

    End Sub

    Private Sub ValidateBatchFields(
        problems As List(Of String))

        If Not birthsRadioButton.Checked AndAlso
           Not marriagesRadioButton.Checked AndAlso
           Not deathsRadioButton.Checked Then

            problems.Add("Record type")
        End If

        Dim year As Integer

        If yearTextBox.Text.Trim().Length <> 4 OrElse
           Not Integer.TryParse(yearTextBox.Text.Trim(), year) Then

            problems.Add("Valid 4-digit year")
        Else
            If year < FirstAllowedYear OrElse
               year > LastAllowedYear Then

                problems.Add(
                    $"Year between {FirstAllowedYear} and {LastAllowedYear}")
            End If

            If year < FirstYearWithoutQuarters AndAlso
               quarterComboBox.SelectedIndex < 0 Then

                problems.Add("Quarter")
            End If
        End If

        If pageSourceComboBox.SelectedIndex < 0 Then
            problems.Add("Page source")
        End If

        Dim page As Integer

        If Not Integer.TryParse(
            pageTextBox.Text.Trim(),
            page) OrElse page <= 0 Then

            problems.Add("Valid page number")
        End If

        If vnfComboBox.SelectedIndex < 0 OrElse
           String.IsNullOrWhiteSpace(vnfComboBox.Text) Then

            problems.Add("Volume format")
        End If

        ValidatePageLetter(problems)
        ValidateSuffix(problems)

    End Sub

    Private Sub ValidatePageLetter(
        problems As List(Of String))

        Dim value As String =
            pageLetterTextBox.Text.Trim().ToUpperInvariant()

        If pageLetterTextBox.Text <> value Then
            pageLetterTextBox.Text = value
            pageLetterTextBox.SelectionStart =
                pageLetterTextBox.TextLength
        End If

        If value.Length = 0 Then
            problems.Add("Page letter")

        ElseIf value.Length <> 1 OrElse
               value(0) < "A"c OrElse
               value(0) > "Z"c Then

            problems.Add("Page letter must be A–Z")
        End If

    End Sub

    Private Sub ValidateSuffix(
        problems As List(Of String))

        Dim value As String =
            suffixTextBox.Text.Trim().ToUpperInvariant()

        If suffixTextBox.Text <> value Then
            suffixTextBox.Text = value
            suffixTextBox.SelectionStart =
                suffixTextBox.TextLength
        End If

        If value.Length = 0 Then
            Return
        End If

        If value.Length <> 1 OrElse
           value(0) < "A"c OrElse
           value(0) > "Z"c Then

            problems.Add("Suffix must be A–Z")
        End If

    End Sub

    Private Sub ApplyRequiredFieldColours(
        problems As List(Of String))

        Dim requiredColour As Color =
            Color.FromArgb(255, 249, 220)

        Dim normalColour As Color =
            SystemColors.Window

        creatorTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(creatorTextBox.Text),
               requiredColour,
               normalColour)

        creatorEmailTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(creatorEmailTextBox.Text),
               requiredColour,
               normalColour)

        syndicateTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(syndicateTextBox.Text),
               requiredColour,
               normalColour)

        userNameTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(userNameTextBox.Text),
               requiredColour,
               normalColour)

        userPasswordTextBox.BackColor =
            If(userPasswordTextBox.Text.Length = 0,
               requiredColour,
               normalColour)

        yearTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(yearTextBox.Text),
               requiredColour,
               normalColour)

        pageTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(pageTextBox.Text),
               requiredColour,
               normalColour)

        pageLetterTextBox.BackColor =
            If(String.IsNullOrWhiteSpace(pageLetterTextBox.Text),
               requiredColour,
               normalColour)

    End Sub

    Private Shared Function IsValidEmailAddress(
        value As String) As Boolean

        If String.IsNullOrWhiteSpace(value) Then
            Return False
        End If

        value = value.Trim()

        Try
            Dim address As New MailAddress(value)

            If Not String.Equals(
                address.Address,
                value,
                StringComparison.OrdinalIgnoreCase) Then

                Return False
            End If

            Dim host As String =
                address.Host

            If Not host.Contains("."c) Then
                Return False
            End If

            Dim parts() As String =
                host.Split("."c)

            If parts.Any(
                Function(part)
                    Return String.IsNullOrWhiteSpace(part)
                End Function) Then

                Return False
            End If

            If parts(parts.Length - 1).Length < 2 Then
                Return False
            End If

            Return True

        Catch
            Return False
        End Try

    End Function

    Private Function GetSelectedBatchType() As String

        If birthsRadioButton.Checked Then
            Return "B"
        End If

        If marriagesRadioButton.Checked Then
            Return "M"
        End If

        If deathsRadioButton.Checked Then
            Return "D"
        End If

        Return ""

    End Function

    Private Sub SaveToProjectValues()

        ProjectValues.BatchType =
            GetSelectedBatchType()

        Dim year As Integer
        Integer.TryParse(
            yearTextBox.Text.Trim(),
            year)

        ProjectValues.Year =
            year

        ProjectValues.Quarter =
            If(quarterComboBox.SelectedIndex >= 0,
               quarterComboBox.SelectedIndex + 1,
               0)

        ProjectValues.PageSource =
            pageSourceComboBox.SelectedIndex

        Dim page As Integer
        Integer.TryParse(
            pageTextBox.Text.Trim(),
            page)

        ProjectValues.Page =
            page

        ProjectValues.PageSuffix =
            suffixTextBox.Text.Trim().ToUpperInvariant()

        ProjectValues.PageLetter =
            pageLetterTextBox.Text.Trim().ToUpperInvariant()

        ProjectValues.VNF =
            vnfComboBox.Text.Trim()

        ProjectValues.SourceRef =
            sourceRefTextBox.Text.Trim()

        ProjectValues.Comments =
            commentsTextBox.Text.Trim()

        ProjectValues.Creator =
            creatorTextBox.Text.Trim()

        ProjectValues.CreatorEmail =
            creatorEmailTextBox.Text.Trim()

        ProjectValues.Syndicate =
            syndicateTextBox.Text.Trim()

        ProjectValues.UserName =
            userNameTextBox.Text.Trim()

        ProjectValues.UserPW =
            userPasswordTextBox.Text

        ProjectValues.BatchName =
            BuildBatchName()

        If String.IsNullOrWhiteSpace(ProjectValues.Created) Then
            ProjectValues.Created =
            Date.Today.ToString(
                "d-MMM-yyyy",
                Globalization.CultureInfo.InvariantCulture)
        End If

        ProjectValues.DateModified = Date.Today

        ProjectValuesStore.Save()

        DebugLog.Write(
    $"[HEADER] Accepted. BatchName='{ProjectValues.BatchName}', " &
    $"Created='{ProjectValues.Created}', " &
    $"DateModified={ProjectValues.DateModified:d}, " &
    $"Year={ProjectValues.Year}, " &
    $"Quarter={ProjectValues.Quarter}, " &
    $"BatchType={ProjectValues.BatchType}, " &
    $"Page={ProjectValues.Page}, " &
    $"PageLetter='{ProjectValues.PageLetter}', " &
    $"PageSuffix='{ProjectValues.PageSuffix}'")

    End Sub
    Private Function BuildBatchName() As String

        Dim year As Integer
        Integer.TryParse(yearTextBox.Text.Trim(), year)

        Dim quarter As Integer =
        If(
            quarterComboBox.SelectedIndex >= 0,
            quarterComboBox.SelectedIndex + 1,
            0)

        Dim batchType As String =
        GetSelectedBatchType()

        Dim page As Integer
        Integer.TryParse(pageTextBox.Text.Trim(), page)

        Dim quarterPart As String =
        If(
            year >= FirstYearWithoutQuarters,
            "",
            If(
                quarter > 0,
                quarter.ToString(),
                ""))

        Dim pageText As String =
        page.ToString("0000")

        Dim pageLetter As String =
        pageLetterTextBox.Text.Trim().ToUpperInvariant()

        If pageLetter.Length > 1 Then
            pageLetter = pageLetter.Substring(0, 1)
        End If

        Dim suffix As String =
        suffixTextBox.Text.Trim().ToUpperInvariant()

        If suffix.Length > 1 Then
            suffix = suffix.Substring(0, 1)
        End If

        Return $"{year:0000}{batchType}{quarterPart}{pageLetter}{pageText}{suffix}.BMD"

    End Function
    Private Sub ShowPasswordButton_Click(
        sender As Object,
        e As EventArgs)

        userPasswordTextBox.UseSystemPasswordChar =
            Not userPasswordTextBox.UseSystemPasswordChar

    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs)

        ValidateForm()

        If Not btnStart.Enabled Then
            Return
        End If

        If _editMode Then

            If Not ValidateHeaderEdit() Then
                Return
            End If

        End If

        SaveToProjectValues()

        DialogResult = DialogResult.OK

        Close()

    End Sub
    Private Function ValidateHeaderEdit() As Boolean

        If ProposedHeaderChangesGridLayout() Then

            MessageBox.Show(
            Me,
            "This change cannot be made because it would change the number of columns in the existing transcription grid.",
            "Edit Header",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

            Return False

        End If

        Dim newBatchName As String = BuildBatchName()

        Dim batchNameChanged As Boolean =
        Not String.Equals(
            newBatchName,
            ProjectValues.BatchName,
            StringComparison.OrdinalIgnoreCase)

        If batchNameChanged Then

            Dim newFilePath As String =
        Path.Combine(
            AppPaths.SaveFolder,
            newBatchName)

            If File.Exists(newFilePath) Then

                MessageBox.Show(
            Me,
            "A file with the new batch name already exists." &
            Environment.NewLine &
            Environment.NewLine &
            newBatchName &
            Environment.NewLine &
            Environment.NewLine &
            "The header cannot be changed to these values.",
            "Edit Header",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error)

                Return False

            End If

        End If

        Return True

    End Function
    Private Sub btnCancel_Click(sender As Object, e As EventArgs)

        DialogResult = DialogResult.Cancel

        Close()

    End Sub

End Class