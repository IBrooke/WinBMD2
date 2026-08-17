Imports WinBMD2.My.Resources

Public Class OptionsForm

    Private ReadOnly _generalPanel As New Panel()
    Private ReadOnly _generalTabs As New TabControl()

    Private ReadOnly _appearanceTab As New TabPage()
    Private ReadOnly _scanningTab As New TabPage()
    Private ReadOnly _uploadTab As New TabPage()
    Private ReadOnly _diagnosticsTab As New TabPage()

    Private ReadOnly _colourSchemeComboBox As New ComboBox()
    Private ReadOnly _diagnosticLoggingCheckBox As New CheckBox()
    Private ReadOnly _generalTabImages As New ImageList()
    Private ReadOnly _originalColourScheme As UiColourScheme
    Private _loadingOptionValues As Boolean
    Private ReadOnly _uiFontComboBox As New ComboBox()
    Private ReadOnly _uiFontSizeNumeric As New NumericUpDown()
    Private ReadOnly _verifyFontSizeNumeric As New NumericUpDown()
    Private ReadOnly _entryPanel As New Panel()
    Private ReadOnly _ignoreAutoCompleteComboBox As New ComboBox()
    Private ReadOnly _autoShowScanToggle As New ToggleSwitch()
    Private ReadOnly _autoShowRulerToggle As New ToggleSwitch()
    Public Sub New()

        InitializeComponent()

        Icon = WinBMDResources.WinBMD2Icon
        Text = "Options"

        _originalColourScheme =
        ProjectValues.ColourScheme

        BuildGeneralPage()
        BuildEntryPage()
        LoadOptionValues()

        ThemeManager.Apply(Me)
        ApplyOptionsAppearance()

        ShowGeneralPage()

    End Sub

#Region "Page Construction"

    Private Sub BuildGeneralPage()

        ConfigureGeneralPanel()
        ConfigureGeneralTabs()
        ConfigureGeneralTabImages()

        BuildAppearanceTab()
        BuildScanningTab()
        BuildUploadTab()
        BuildDiagnosticsTab()

        _generalTabs.TabPages.Add(_appearanceTab)
        _generalTabs.TabPages.Add(_scanningTab)
        _generalTabs.TabPages.Add(_uploadTab)
        _generalTabs.TabPages.Add(_diagnosticsTab)

        _generalPanel.Controls.Add(_generalTabs)
        mainSplitContainer.Panel2.Controls.Add(_generalPanel)

    End Sub
    Private Sub BuildEntryPage()

        _entryPanel.Name = "entryOptionsPanel"
        _entryPanel.Location = New Point(24, 56)

        _entryPanel.Size =
        New Size(
            Math.Max(
                300,
                mainSplitContainer.Panel2.ClientSize.Width - 48),
            Math.Max(
                200,
                mainSplitContainer.Panel2.ClientSize.Height - 136))

        _entryPanel.Anchor =
        AnchorStyles.Top Or
        AnchorStyles.Bottom Or
        AnchorStyles.Left Or
        AnchorStyles.Right

        Dim titleLabel As New Label With {
        .Name = "ignoreAutoCompleteTitleLabel",
        .Text = "Picklist selection",
        .AutoSize = True,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(20, 0)
    }

        Dim descriptionLabel As New Label With {
        .Name = "ignoreAutoCompleteDescriptionLabel",
        .Text =
            "Choose which navigation keys ignore the highlighted picklist suggestion.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 40)
    }

        Dim optionLabel As New Label With {
        .Name = "ignoreAutoCompleteLabel",
        .Text = "Ignore the picklist when pressing",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 106)
    }

        _ignoreAutoCompleteComboBox.Name =
        "ignoreAutoCompleteComboBox"

        _ignoreAutoCompleteComboBox.DropDownStyle =
        ComboBoxStyle.DropDownList

        _ignoreAutoCompleteComboBox.Location =
        New Point(22, 118)

        _ignoreAutoCompleteComboBox.Size =
        New Size(190, 23)

        _ignoreAutoCompleteComboBox.Items.AddRange(
        New Object() {
            IgnoreAutoCompleteKey.None,
            IgnoreAutoCompleteKey.Tab,
            IgnoreAutoCompleteKey.Return,
            IgnoreAutoCompleteKey.All
        })

        Dim helpLabel As New Label With {
        .Name = "ignoreAutoCompleteHelpLabel",
        .Text =
            "If a key ignores the suggestion and the cell is empty, " &
            "the value from the cell above will be copied instead.",
        .AutoSize = False,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 146),
        .Size = New Size(460, 48),
        .Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Left Or
            AnchorStyles.Right
    }

        _entryPanel.Controls.Add(titleLabel)
        _entryPanel.Controls.Add(descriptionLabel)
        _entryPanel.Controls.Add(optionLabel)
        _entryPanel.Controls.Add(_ignoreAutoCompleteComboBox)
        _entryPanel.Controls.Add(helpLabel)

        mainSplitContainer.Panel2.Controls.Add(_entryPanel)

    End Sub
    Private Sub ConfigureGeneralPanel()

        _generalPanel.Name = "generalOptionsPanel"
        _generalPanel.Location = New Point(24, 74)

        _generalPanel.Size =
            New Size(
                Math.Max(
                    300,
                    mainSplitContainer.Panel2.ClientSize.Width - 48),
                Math.Max(
                    200,
                    mainSplitContainer.Panel2.ClientSize.Height - 136))

        _generalPanel.Anchor =
            AnchorStyles.Top Or
            AnchorStyles.Bottom Or
            AnchorStyles.Left Or
            AnchorStyles.Right

    End Sub

    Private Sub ConfigureGeneralTabs()

        _generalTabs.Name = "generalOptionsTabs"
        _generalTabs.Dock = DockStyle.Fill

        _generalTabs.SizeMode =
        TabSizeMode.Fixed

        _generalTabs.ItemSize =
        New Size(112, 34)

        _generalTabs.Padding =
        New Point(12, 5)

    End Sub
    Private Sub ConfigureGeneralTabImages()

        _generalTabImages.ImageSize =
        New Size(16, 16)

        _generalTabImages.ColorDepth =
        ColorDepth.Depth32Bit

        ' Icons will be added here once they are available in resources.

        _generalTabs.ImageList =
        _generalTabImages

    End Sub
    Private Sub BuildAppearanceTab()

        _appearanceTab.Name = "appearanceTab"
        _appearanceTab.Text = "Appearance"
        _appearanceTab.Padding = New Padding(20)

        Dim titleLabel As New Label With {
        .Name = "appearanceTitleLabel",
        .Text = "Application appearance",
        .AutoSize = True,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(20, 20)
    }

        Dim descriptionLabel As New Label With {
        .Name = "appearanceDescriptionLabel",
        .Text = "Choose the colours and fonts used throughout WinBMD2.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 52)
    }

        Dim colourSchemeLabel As New Label With {
        .Name = "colourSchemeLabel",
        .Text = "Colour scheme",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 88)
    }

        _colourSchemeComboBox.Name = "colourSchemeComboBox"
        _colourSchemeComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _colourSchemeComboBox.Location = New Point(22, 110)
        _colourSchemeComboBox.Size = New Size(190, 23)

        _colourSchemeComboBox.Items.AddRange(
        New Object() {
            UiColourScheme.Blue,
            UiColourScheme.Teal,
            UiColourScheme.Magenta,
            UiColourScheme.Violet,
            UiColourScheme.Gold,
            UiColourScheme.Green
        })

        AddHandler _colourSchemeComboBox.SelectedIndexChanged,
        AddressOf ColourSchemeComboBox_SelectedIndexChanged

        Dim uiFontLabel As New Label With {
        .Name = "uiFontLabel",
        .Text = "User interface font",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 152)
    }

        _uiFontComboBox.Name = "uiFontComboBox"
        _uiFontComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _uiFontComboBox.Location = New Point(22, 174)
        _uiFontComboBox.Size = New Size(230, 23)

        _uiFontComboBox.Items.AddRange(
        New Object() {
            "Segoe UI",
            "Calibri",
            "Arial",
            "Tahoma",
            "Verdana",
            "Microsoft Sans Serif"
        })

        Dim uiFontSizeLabel As New Label With {
        .Name = "uiFontSizeLabel",
        .Text = "User interface font size",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(282, 152)
    }

        _uiFontSizeNumeric.Name = "uiFontSizeNumeric"
        _uiFontSizeNumeric.Location = New Point(282, 174)
        _uiFontSizeNumeric.Size = New Size(72, 23)
        _uiFontSizeNumeric.Minimum = 8D
        _uiFontSizeNumeric.Maximum = 20D
        _uiFontSizeNumeric.DecimalPlaces = 1
        _uiFontSizeNumeric.Increment = 0.5D

        Dim verifyFontSizeLabel As New Label With {
        .Name = "verifyFontSizeLabel",
        .Text = "Verification font size",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 222)
    }

        _verifyFontSizeNumeric.Name = "verifyFontSizeNumeric"
        _verifyFontSizeNumeric.Location = New Point(22, 244)
        _verifyFontSizeNumeric.Size = New Size(72, 23)
        _verifyFontSizeNumeric.Minimum = 8D
        _verifyFontSizeNumeric.Maximum = 24D
        _verifyFontSizeNumeric.DecimalPlaces = 1
        _verifyFontSizeNumeric.Increment = 0.5D

        Dim verifyHelpLabel As New Label With {
        .Name = "verifyFontHelpLabel",
        .Text = "Controls the text size used in the verification bar.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(108, 248)
    }

        _appearanceTab.Controls.Add(titleLabel)
        _appearanceTab.Controls.Add(descriptionLabel)
        _appearanceTab.Controls.Add(colourSchemeLabel)
        _appearanceTab.Controls.Add(_colourSchemeComboBox)
        _appearanceTab.Controls.Add(uiFontLabel)
        _appearanceTab.Controls.Add(_uiFontComboBox)
        _appearanceTab.Controls.Add(uiFontSizeLabel)
        _appearanceTab.Controls.Add(_uiFontSizeNumeric)
        _appearanceTab.Controls.Add(verifyFontSizeLabel)
        _appearanceTab.Controls.Add(_verifyFontSizeNumeric)
        _appearanceTab.Controls.Add(verifyHelpLabel)

    End Sub
    Private Sub BuildScanningTab()

        _scanningTab.Name = "scanningTab"
        _scanningTab.Text = "Scan"
        _scanningTab.Padding = New Padding(20)

        Dim titleLabel As New Label With {
        .Name = "scanningTitleLabel",
        .Text = "Scan display",
        .AutoSize = True,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(20, 20)
    }

        Dim descriptionLabel As New Label With {
        .Name = "scanningDescriptionLabel",
        .Text = "Choose how WinBMD2 displays scans while transcribing.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 54)
    }

        Dim autoShowScanLabel As New Label With {
        .Name = "autoShowScanLabel",
        .Text = "Automatically show scan when a batch is opened",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 104)
    }

        _autoShowScanToggle.Name = "autoShowScanToggle"
        _autoShowScanToggle.Location = New Point(370, 99)

        Dim autoShowRulerLabel As New Label With {
        .Name = "autoShowRulerLabel",
        .Text = "Automatically show ruler",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 146)
    }

        _autoShowRulerToggle.Name = "autoShowRulerToggle"
        _autoShowRulerToggle.Location = New Point(370, 141)

        _scanningTab.Controls.Add(titleLabel)
        _scanningTab.Controls.Add(descriptionLabel)
        _scanningTab.Controls.Add(autoShowScanLabel)
        _scanningTab.Controls.Add(_autoShowScanToggle)
        _scanningTab.Controls.Add(autoShowRulerLabel)
        _scanningTab.Controls.Add(_autoShowRulerToggle)

    End Sub
    Private Sub BuildUploadTab()

        _uploadTab.Name = "uploadTab"
        _uploadTab.Text = "Upload"
        _uploadTab.Padding = New Padding(20)

        Dim titleLabel As New Label With {
            .Name = "uploadTitleLabel",
            .Text = "Upload and file settings",
            .AutoSize = True,
            .Font = UiFonts.SectionHeading,
            .ForeColor = UiColors.TextPrimary,
            .Location = New Point(20, 20)
        }

        Dim descriptionLabel As New Label With {
            .Name = "uploadDescriptionLabel",
            .Text =
                "Upload server and saved-file character-set settings " &
                "will be added here.",
            .AutoSize = False,
            .ForeColor = UiColors.TextSecondary,
            .Location = New Point(22, 54),
            .Size = New Size(420, 50),
            .Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Left Or
                AnchorStyles.Right
        }

        _uploadTab.Controls.Add(titleLabel)
        _uploadTab.Controls.Add(descriptionLabel)

    End Sub

    Private Sub BuildDiagnosticsTab()

        _diagnosticsTab.Name = "diagnosticsTab"
        _diagnosticsTab.Text = "Diagnostics"
        _diagnosticsTab.Padding = New Padding(20)

        Dim titleLabel As New Label With {
            .Name = "diagnosticsTitleLabel",
            .Text = "Troubleshooting",
            .AutoSize = True,
            .Font = UiFonts.SectionHeading,
            .ForeColor = UiColors.TextPrimary,
            .Location = New Point(20, 20)
        }

        Dim descriptionLabel As New Label With {
            .Name = "diagnosticsDescriptionLabel",
            .Text =
                "Diagnostic logging can help investigate problems " &
                "with WinBMD2.",
            .AutoSize = True,
            .ForeColor = UiColors.TextSecondary,
            .Location = New Point(22, 52)
        }

        _diagnosticLoggingCheckBox.Name =
            "diagnosticLoggingCheckBox"

        _diagnosticLoggingCheckBox.Text =
            "Create troubleshooting log"

        _diagnosticLoggingCheckBox.AutoSize =
            True

        _diagnosticLoggingCheckBox.Location =
            New Point(22, 94)

        Dim helpLabel As New Label With {
            .Name = "diagnosticHelpLabel",
            .Text =
                "Normally leave this turned off. Enable it only when " &
                "investigating a problem or when asked to provide a log.",
            .AutoSize = False,
            .ForeColor = UiColors.TextSecondary,
            .Location = New Point(44, 124),
            .Size = New Size(400, 52),
            .Anchor =
                AnchorStyles.Top Or
                AnchorStyles.Left Or
                AnchorStyles.Right
        }

        _diagnosticsTab.Controls.Add(titleLabel)
        _diagnosticsTab.Controls.Add(descriptionLabel)
        _diagnosticsTab.Controls.Add(
            _diagnosticLoggingCheckBox)

        _diagnosticsTab.Controls.Add(helpLabel)

    End Sub

#End Region

#Region "Loading And Saving"

    Private Sub LoadOptionValues()

        _loadingOptionValues = True

        Try
            _colourSchemeComboBox.SelectedItem =
            ProjectValues.ColourScheme
            _ignoreAutoCompleteComboBox.SelectedItem = ProjectValues.IgnoreAutoComplete
            If _uiFontComboBox.Items.Contains(ProjectValues.UiFontName) Then
                _uiFontComboBox.SelectedItem = ProjectValues.UiFontName
            Else
                _uiFontComboBox.Items.Add(ProjectValues.UiFontName)
                _uiFontComboBox.SelectedItem = ProjectValues.UiFontName
            End If
            _autoShowScanToggle.Checked = ProjectValues.AutoShowScan
            _autoShowRulerToggle.Checked = ProjectValues.AutoShowRuler
            _uiFontSizeNumeric.Value =
    Math.Min(
        _uiFontSizeNumeric.Maximum,
        Math.Max(
            _uiFontSizeNumeric.Minimum,
            CDec(ProjectValues.UiFontSize)))

            _verifyFontSizeNumeric.Value =
    Math.Min(
        _verifyFontSizeNumeric.Maximum,
        Math.Max(
            _verifyFontSizeNumeric.Minimum,
            CDec(ProjectValues.VerifyFontSize)))

            _diagnosticLoggingCheckBox.Checked =
            ProjectValues.EnableDiagnosticLogging

        Finally
            _loadingOptionValues = False
        End Try

    End Sub
    Private Sub ColourSchemeComboBox_SelectedIndexChanged(
    sender As Object,
    e As EventArgs)

        If _loadingOptionValues Then
            Return
        End If

        If Not TypeOf _colourSchemeComboBox.SelectedItem Is UiColourScheme Then
            Return
        End If

        ProjectValues.ColourScheme =
        DirectCast(
            _colourSchemeComboBox.SelectedItem,
            UiColourScheme)

        ThemeManager.Apply(Me)
        ApplyOptionsAppearance()
        SelectNavigationButton(btnGeneral)

    End Sub
    Private Sub btnCancel_Click(
    sender As Object,
    e As EventArgs) Handles btnCancel.Click

        ProjectValues.ColourScheme =
        _originalColourScheme

        DialogResult = DialogResult.Cancel
        Close()

    End Sub
    Private Sub SaveOptionValues()

        If TypeOf _colourSchemeComboBox.SelectedItem Is
           UiColourScheme Then

            ProjectValues.ColourScheme =
                DirectCast(
                    _colourSchemeComboBox.SelectedItem,
                    UiColourScheme)

        End If
        If TypeOf _ignoreAutoCompleteComboBox.SelectedItem Is IgnoreAutoCompleteKey Then

            ProjectValues.IgnoreAutoComplete = DirectCast(
                _ignoreAutoCompleteComboBox.SelectedItem,
                IgnoreAutoCompleteKey)

        End If

        ProjectValues.AutoShowScan = _autoShowScanToggle.Checked
        ProjectValues.AutoShowRuler = _autoShowRulerToggle.Checked

        If _uiFontComboBox.SelectedItem IsNot Nothing Then
            ProjectValues.UiFontName =
        _uiFontComboBox.SelectedItem.ToString()
        End If

        ProjectValues.UiFontSize =
    CSng(_uiFontSizeNumeric.Value)

        ProjectValues.VerifyFontSize =
    CSng(_verifyFontSizeNumeric.Value)
        Dim diagnosticLoggingWasEnabled As Boolean =
            ProjectValues.EnableDiagnosticLogging

        ProjectValues.EnableDiagnosticLogging =
            _diagnosticLoggingCheckBox.Checked

        If Not diagnosticLoggingWasEnabled AndAlso
           ProjectValues.EnableDiagnosticLogging Then

            DebugLog.Clear()

        End If

        ProjectValuesStore.Save()

    End Sub

#End Region

#Region "Appearance"

    Private Sub ApplyOptionsAppearance()

        headerPanel.BackColor =
            UiColors.PanelBackground

        footerPanel.BackColor =
            UiColors.PanelBackground

        mainSplitContainer.BackColor =
            UiColors.Border

        mainSplitContainer.Panel1.BackColor =
            UiColors.PanelBackground

        mainSplitContainer.Panel2.BackColor =
            UiColors.PanelBackground

        lblTitle.Font =
            UiFonts.Title

        lblTitle.ForeColor =
            UiColors.TextPrimary

        lblSubtitle.Font =
            UiFonts.Normal

        lblSubtitle.ForeColor =
            UiColors.TextSecondary

        lblPageTitle.Font =
            UiFonts.SectionHeading

        lblPageTitle.ForeColor =
            UiColors.TextPrimary

        lblPageDescription.Font =
            UiFonts.Normal

        lblPageDescription.ForeColor =
            UiColors.TextSecondary

        _generalPanel.BackColor =
            UiColors.PanelBackground

        _entryPanel.BackColor =
            UiColors.PanelBackground

        _appearanceTab.BackColor =
            UiColors.PanelBackground

        _scanningTab.BackColor =
            UiColors.PanelBackground

        _uploadTab.BackColor =
            UiColors.PanelBackground

        _diagnosticsTab.BackColor =
            UiColors.PanelBackground

        ThemeManager.ApplyNavigationButton(
            btnGeneral)

        ThemeManager.ApplyNavigationButton(
            btnEntry)

        ThemeManager.ApplyNavigationButton(
            btnCapitalisation)

        ThemeManager.ApplyNavigationButton(
            btnAdvanced)

        ThemeManager.ApplyPrimaryButton(
            btnOk)

        ThemeManager.ApplySecondaryButton(
            btnCancel)

    End Sub

    Private Sub SelectNavigationButton(
        selectedButton As Button)

        For Each button As Button In {
            btnGeneral,
            btnEntry,
            btnCapitalisation,
            btnAdvanced
        }

            ThemeManager.ApplyNavigationButton(
                button)

        Next

        selectedButton.BackColor =
            UiColors.ThemeShaded

        selectedButton.FlatAppearance.BorderColor =
            UiColors.AccentBorder

    End Sub

#End Region

#Region "Pages"

    Private Sub HideOptionPages()

        _generalPanel.Visible = False
        _entryPanel.Visible = False

    End Sub

    Private Sub ShowGeneralPage()

        HideOptionPages()
        SelectNavigationButton(btnGeneral)

        lblPageTitle.Text =
            "General"

        lblPageDescription.Text =
            "Appearance, scan display, upload and diagnostic settings."

        _generalPanel.Visible = True
        _generalPanel.BringToFront()

    End Sub

    Private Sub ShowEntryPage()

        HideOptionPages()
        SelectNavigationButton(btnEntry)

        lblPageTitle.Text =
        "Entry"

        lblPageDescription.Text =
        "Grid navigation, picklist, autocomplete and entry settings."

        _entryPanel.Visible = True
        _entryPanel.BringToFront()

    End Sub

    Private Sub ShowCapitalisationPage()

        HideOptionPages()
        SelectNavigationButton(btnCapitalisation)

        lblPageTitle.Text =
            "Capitalisation"

        lblPageDescription.Text =
            "Capitalisation settings for the fields in the current batch."

    End Sub

    Private Sub ShowAdvancedPage()

        HideOptionPages()
        SelectNavigationButton(btnAdvanced)

        lblPageTitle.Text =
            "Advanced"

        lblPageDescription.Text =
            "Advanced settings will be added later."

    End Sub

#End Region

#Region "Events"

    Private Sub btnGeneral_Click(
        sender As Object,
        e As EventArgs) Handles btnGeneral.Click

        ShowGeneralPage()

    End Sub

    Private Sub btnEntry_Click(
        sender As Object,
        e As EventArgs) Handles btnEntry.Click

        ShowEntryPage()

    End Sub

    Private Sub btnCapitalisation_Click(
        sender As Object,
        e As EventArgs) Handles btnCapitalisation.Click

        ShowCapitalisationPage()

    End Sub

    Private Sub btnAdvanced_Click(
        sender As Object,
        e As EventArgs) Handles btnAdvanced.Click

        ShowAdvancedPage()

    End Sub

    Private Sub btnOk_Click(
        sender As Object,
        e As EventArgs) Handles btnOk.Click

        SaveOptionValues()

        DialogResult =
            DialogResult.OK

        Close()

    End Sub

#End Region

End Class