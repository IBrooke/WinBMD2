Imports System.IO
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
    Private ReadOnly _picklistsPanel As New Panel()
    Private ReadOnly _capitalisationPanel As New Panel()
    Private ReadOnly _capitalisationTable As New TableLayoutPanel()
    Private ReadOnly _ignoreAutoCompleteComboBox As New ComboBox()
    Private ReadOnly _autoShowScanToggle As New ToggleSwitch()
    Private ReadOnly _autoShowRulerToggle As New ToggleSwitch()
    Private ReadOnly _verticalTabToggle As New ToggleSwitch()
    Private ReadOnly _skipSurnameToggle As New ToggleSwitch()
    Private ReadOnly _formatPicklistSelectionsToggle As New ToggleSwitch()
    Private ReadOnly _originalCapitalisation As New Dictionary(Of GridField, CapitalisationMode)
    Private ReadOnly _outputCharacterSetComboBox As New ComboBox()
    Private ReadOnly _pickListCompletionToggle As New ToggleSwitch()
    Private ReadOnly _showForenamePickListToggle As New ToggleSwitch()
    Private ReadOnly _showDistrictPickListToggle As New ToggleSwitch()
    Private ReadOnly _match3VolCharsToggle As New ToggleSwitch()
    Public ReadOnly Property CapitalisationChanges As New List(Of CapitalisationChange)
    Public Sub New(Optional allowCapitalisation As Boolean = False)

        InitializeComponent()

        btnCapitalisation.Enabled = allowCapitalisation
        Icon = WinBMDResources.WinBMD2Icon
        Text = "Options"

        _originalColourScheme =
        ProjectValues.ColourScheme

        BuildGeneralPage()
        BuildEntryPage()
        BuildPicklistsPage()
        BuildCapitalisationPage()
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
    Private Sub BuildCapitalisationPage()

        _capitalisationPanel.Name = "capitalisationOptionsPanel"
        _capitalisationPanel.Location = New Point(24, 56)
        _capitalisationPanel.Size = New Size(Math.Max(300, mainSplitContainer.Panel2.ClientSize.Width - 48), Math.Max(200, mainSplitContainer.Panel2.ClientSize.Height - 136))
        _capitalisationPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        mainSplitContainer.Panel2.Controls.Add(_capitalisationPanel)
        BuildCapitalisationTable()
    End Sub
    Private Sub BuildCapitalisationTable()

        _capitalisationTable.Name = "capitalisationTable"
        _capitalisationTable.Location = New Point(20, 20)
        _capitalisationTable.AutoSize = True
        _capitalisationTable.AutoSizeMode = AutoSizeMode.GrowAndShrink
        _capitalisationTable.ColumnCount = 5
        _capitalisationTable.RowCount = 1

        _capitalisationTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 150))
        _capitalisationTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        _capitalisationTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        _capitalisationTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))
        _capitalisationTable.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))

        _capitalisationTable.Controls.Add(CreateCapitalisationHeading("Upper"), 1, 0)
        _capitalisationTable.Controls.Add(CreateCapitalisationHeading("Lower"), 2, 0)
        _capitalisationTable.Controls.Add(CreateCapitalisationHeading("Name"), 3, 0)
        _capitalisationTable.Controls.Add(CreateCapitalisationHeading("As typed"), 4, 0)

        _capitalisationPanel.Controls.Add(_capitalisationTable)
        BuildCapitalisationRows()

    End Sub
    Private Sub BuildCapitalisationRows()

        Dim fields() As GridField = GridLayout.GetVisibleFields().Where(Function(field) FieldMetaData.Meta(field).IsDataColumn).ToArray()
        Dim savedValues As String = CapitalisationData.GetSettings(ProjectValues.BatchType, ProjectValues.Year)

        _originalCapitalisation.Clear()

        _capitalisationTable.RowCount = fields.Length + 1
        _capitalisationTable.RowStyles.Clear()

        _capitalisationTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))

        For index As Integer = 0 To fields.Length - 1
            _capitalisationTable.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        Next

        For fieldIndex As Integer = 0 To fields.Length - 1

            Dim field As GridField = fields(fieldIndex)
            Dim rowIndex As Integer = fieldIndex + 1
            Dim selectedMode As CapitalisationMode = CapitalisationMode.AsTyped

            If fieldIndex < savedValues.Length Then

                Dim enumValue As Integer

                If Integer.TryParse(savedValues(fieldIndex).ToString(), enumValue) AndAlso [Enum].IsDefined(GetType(CapitalisationMode), enumValue) Then
                    selectedMode = CType(enumValue, CapitalisationMode)
                End If

            End If

            _originalCapitalisation(field) = selectedMode

            Dim fieldLabel As New Label With {
            .Text = FieldMetaData.Meta(field).Header,
            .AutoSize = False,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .ForeColor = UiColors.TextPrimary
        }

            _capitalisationTable.Controls.Add(fieldLabel, 0, rowIndex)

            Dim modePanel As New Panel With {
            .Dock = DockStyle.Fill,
            .Margin = Padding.Empty
        }

            _capitalisationTable.Controls.Add(modePanel, 1, rowIndex)
            _capitalisationTable.SetColumnSpan(modePanel, 4)

            Dim modes() As CapitalisationMode = {
            CapitalisationMode.Upper,
            CapitalisationMode.Lower,
            CapitalisationMode.Name,
            CapitalisationMode.AsTyped
        }

            For modeIndex As Integer = 0 To modes.Length - 1

                Dim radioButton As New RadioButton With {
                .AutoSize = True,
                .Tag = New CapitalisationTag(field, modes(modeIndex)),
                .Location = New Point((modeIndex * 90) + 36, 7),
                .Checked = modes(modeIndex) = selectedMode
            }

                modePanel.Controls.Add(radioButton)

            Next

        Next

    End Sub
    Private Function CreateCapitalisationHeading(text As String) As Label

        Return New Label With {
        .Text = text,
        .AutoSize = False,
        .Dock = DockStyle.Fill,
        .TextAlign = ContentAlignment.MiddleCenter,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Margin = New Padding(3, 3, 3, 8)
    }

    End Function
    Private Sub BuildEntryPage()

        _entryPanel.Name = "entryOptionsPanel"
        _entryPanel.Location = New Point(24, 56)
        _entryPanel.Size = New Size(Math.Max(300, mainSplitContainer.Panel2.ClientSize.Width - 48), Math.Max(200, mainSplitContainer.Panel2.ClientSize.Height - 136))
        _entryPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

        Dim verticalTabLabel As New Label With {
        .Name = "verticalTabLabel",
        .Text = "Use vertical tab entry mode",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 40)
    }

        _verticalTabToggle.Name = "verticalTabToggle"
        _verticalTabToggle.Location = New Point(250, 35)

        Dim skipSurnameLabel As New Label With {
        .Name = "skipSurnameLabel",
        .Text = "Automatically copy and skip surname",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 81)
    }

        _skipSurnameToggle.Name = "skipSurnameToggle"
        _skipSurnameToggle.Location = New Point(250, 76)

        _entryPanel.Controls.Add(verticalTabLabel)
        _entryPanel.Controls.Add(_verticalTabToggle)
        _entryPanel.Controls.Add(skipSurnameLabel)
        _entryPanel.Controls.Add(_skipSurnameToggle)

        mainSplitContainer.Panel2.Controls.Add(_entryPanel)

    End Sub

    Private Sub BuildPicklistsPage()

        _picklistsPanel.Name = "picklistsOptionsPanel"
        _picklistsPanel.Location = New Point(24, 56)
        _picklistsPanel.Size = New Size(Math.Max(300, mainSplitContainer.Panel2.ClientSize.Width - 48), Math.Max(200, mainSplitContainer.Panel2.ClientSize.Height - 136))
        _picklistsPanel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right

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
        .Text = "Choose how the Forename and District picklists behave.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 40)
    }

        Dim optionLabel As New Label With {
        .Name = "ignoreAutoCompleteLabel",
        .Text = "Ignore the picklist when pressing",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 76)
    }

        _ignoreAutoCompleteComboBox.Name = "ignoreAutoCompleteComboBox"
        _ignoreAutoCompleteComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _ignoreAutoCompleteComboBox.Location = New Point(250, 72)
        _ignoreAutoCompleteComboBox.Size = New Size(190, 23)

        _ignoreAutoCompleteComboBox.Items.AddRange(New Object() {
        IgnoreAutoCompleteKey.None,
        IgnoreAutoCompleteKey.Tab,
        IgnoreAutoCompleteKey.Return,
        IgnoreAutoCompleteKey.All
    })

        Dim helpLabel As New Label With {
        .Name = "ignoreAutoCompleteHelpLabel",
        .Text = "If a key ignores the suggestion and the cell is empty, the value from the cell above will be copied instead.",
        .AutoSize = False,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 108),
        .Size = New Size(460, 36),
        .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
    }

        Dim showForenamePickListLabel As New Label With {
        .Name = "showForenamePickListLabel",
        .Text = "Show Forename picklist",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 158)
    }

        _showForenamePickListToggle.Name = "showForenamePickListToggle"
        _showForenamePickListToggle.Location = New Point(250, 153)

        Dim showDistrictPickListLabel As New Label With {
        .Name = "showDistrictPickListLabel",
        .Text = "Show District picklist",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 198)
    }

        _showDistrictPickListToggle.Name = "showDistrictPickListToggle"
        _showDistrictPickListToggle.Location = New Point(250, 193)

        Dim formatPicklistLabel As New Label With {
        .Name = "formatPicklistLabel",
        .Text = "Format picklist selections",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 238)
    }

        _formatPicklistSelectionsToggle.Name = "formatPicklistSelectionsToggle"
        _formatPicklistSelectionsToggle.Location = New Point(250, 233)

        Dim pickListCompletionLabel As New Label With {
        .Name = "pickListCompletionLabel",
        .Text = "Show completion characters",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 278)
    }

        _pickListCompletionToggle.Name = "pickListCompletionToggle"
        _pickListCompletionToggle.Location = New Point(250, 273)

        Dim match3VolCharsLabel As New Label With {
            .Name = "match3VolCharsLabel",
            .Text = "Match first 3 characters of Volume Code",
            .AutoSize = True,
            .ForeColor = UiColors.TextPrimary,
            .Location = New Point(22, 318)
        }

        _match3VolCharsToggle.Name = "match3VolCharsToggle"
        _match3VolCharsToggle.Location = New Point(250, 313)

        AddHandler _match3VolCharsToggle.CheckedChanged, AddressOf Match3VolCharsToggle_CheckedChanged

        _picklistsPanel.Controls.Add(titleLabel)
        _picklistsPanel.Controls.Add(descriptionLabel)
        _picklistsPanel.Controls.Add(optionLabel)
        _picklistsPanel.Controls.Add(_ignoreAutoCompleteComboBox)
        _picklistsPanel.Controls.Add(helpLabel)
        _picklistsPanel.Controls.Add(showForenamePickListLabel)
        _picklistsPanel.Controls.Add(_showForenamePickListToggle)
        _picklistsPanel.Controls.Add(showDistrictPickListLabel)
        _picklistsPanel.Controls.Add(_showDistrictPickListToggle)
        _picklistsPanel.Controls.Add(formatPicklistLabel)
        _picklistsPanel.Controls.Add(_formatPicklistSelectionsToggle)
        _picklistsPanel.Controls.Add(pickListCompletionLabel)
        _picklistsPanel.Controls.Add(_pickListCompletionToggle)
        _picklistsPanel.Controls.Add(match3VolCharsLabel)
        _picklistsPanel.Controls.Add(_match3VolCharsToggle)

        mainSplitContainer.Panel2.Controls.Add(_picklistsPanel)

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
                UiColourScheme.Yellow,
                UiColourScheme.Gold,
                UiColourScheme.Orange,
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
        _uploadTab.Text = "Files & Upload"
        _uploadTab.Padding = New Padding(20)

        Dim titleLabel As New Label With {
        .Name = "uploadTitleLabel",
        .Text = "File and upload settings",
        .AutoSize = True,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(20, 20)
    }

        Dim descriptionLabel As New Label With {
        .Name = "uploadDescriptionLabel",
        .Text = "Choose where transcription files are saved and configure file and upload settings.",
        .AutoSize = True,
        .ForeColor = UiColors.TextSecondary,
        .Location = New Point(22, 54)
    }

        Dim savedFilesLabel As New Label With {
        .Name = "savedFilesLabel",
        .Text = "Saved files",
        .AutoSize = True,
        .Font = UiFonts.SectionHeading,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 94)
    }

        Dim saveFolderLabel As New Label With {
        .Name = "saveFolderLabel",
        .Text = "Save folder",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 130)
    }

        Dim saveFolderTextBox As New TextBox With {
    .Name = "saveFolderTextBox",
    .Text = AppPaths.SaveFolder,
    .Location = New Point(22, 154),
    .Size = New Size(400, 23)
}

        Dim browseSaveFolderButton As New Button With {
    .Name = "browseSaveFolderButton",
    .Text = "Browse...",
    .Location = New Point(432, 152),
    .Size = New Size(75, 27)
}

        Dim updateSaveFolderButton As New Button With {
    .Name = "updateSaveFolderButton",
    .Text = "Update",
    .Location = New Point(517, 152),
    .Size = New Size(75, 27)
}

        AddHandler browseSaveFolderButton.Click,
        Sub()

            Using dialog As New FolderBrowserDialog()

                dialog.Description = "Select the folder used to save transcription files."
                dialog.SelectedPath = saveFolderTextBox.Text

                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    saveFolderTextBox.Text = dialog.SelectedPath
                End If

            End Using

        End Sub

        AddHandler updateSaveFolderButton.Click,
        Sub()

            Dim folder As String = saveFolderTextBox.Text.Trim()

            If String.IsNullOrWhiteSpace(folder) Then
                ' Temporarily clear the saved value so AppPaths.SaveFolder returns the default folder.
                ProjectValues.SaveFolder = ""
                folder = AppPaths.SaveFolder
            End If

            If String.IsNullOrWhiteSpace(folder) OrElse Not Directory.Exists(folder) Then

                MessageBox.Show(
                    Me,
                    "The selected save folder does not exist.",
                    "Save Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Return

            End If

            ProjectValues.SaveFolder = folder
            saveFolderTextBox.Text = folder

            MessageBox.Show(
                Me,
                "The save folder has been updated.",
                "Save Folder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information)

        End Sub

        Dim uploadServerLabel As New Label With {
    .Name = "uploadServerLabel",
    .Text = "Upload server",
    .AutoSize = True,
    .ForeColor = UiColors.TextPrimary,
    .Location = New Point(22, 200)
}

        Dim uploadServerUrlTextBox As New TextBox With {
            .Name = "uploadServerUrlTextBox",
            .Text = ProjectValues.UploadServerUrl,
            .Location = New Point(22, 224),
            .Size = New Size(400, 23)
        }

        Dim updateUploadServerButton As New Button With {
            .Name = "updateUploadServerButton",
            .Text = "Update",
            .Location = New Point(432, 222),
            .Size = New Size(75, 27)
        }

        AddHandler updateUploadServerButton.Click,
Sub()

    Dim uploadServerUrl As String = uploadServerUrlTextBox.Text.Trim()

    If String.IsNullOrWhiteSpace(uploadServerUrl) Then
        ' Restore the default FreeBMD upload server if the box has been cleared.
        uploadServerUrl = "www.freebmd.org.uk"
    End If

    If Uri.CheckHostName(uploadServerUrl) = UriHostNameType.Unknown Then

        MessageBox.Show(
            Me,
            "The upload server address is not valid.",
            "Upload Server",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning)

        Return

    End If

    ProjectValues.UploadServerUrl = uploadServerUrl
    uploadServerUrlTextBox.Text = uploadServerUrl
    ProjectValuesStore.Save()

    MessageBox.Show(
        Me,
        "The upload server has been updated.",
        "Upload Server",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information)
End Sub

        Dim outputCharacterSetLabel As New Label With {
        .Name = "outputCharacterSetLabel",
        .Text = "Output character set",
        .AutoSize = True,
        .ForeColor = UiColors.TextPrimary,
        .Location = New Point(22, 274)
    }

        _outputCharacterSetComboBox.Name = "outputCharacterSetComboBox"
        _outputCharacterSetComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        _outputCharacterSetComboBox.Location = New Point(160, 270)
        _outputCharacterSetComboBox.Size = New Size(160, 23)

        _outputCharacterSetComboBox.Items.AddRange(New Object() {
            "FreeBMD",
            "Input Format",
            "UTF-8"
        })

        _uploadTab.Controls.Add(titleLabel)
        _uploadTab.Controls.Add(descriptionLabel)
        _uploadTab.Controls.Add(savedFilesLabel)
        _uploadTab.Controls.Add(saveFolderLabel)
        _uploadTab.Controls.Add(saveFolderTextBox)
        _uploadTab.Controls.Add(browseSaveFolderButton)
        _uploadTab.Controls.Add(updateSaveFolderButton)
        _uploadTab.Controls.Add(uploadServerLabel)
        _uploadTab.Controls.Add(uploadServerUrlTextBox)
        _uploadTab.Controls.Add(updateUploadServerButton)
        _uploadTab.Controls.Add(outputCharacterSetLabel)
        _uploadTab.Controls.Add(_outputCharacterSetComboBox)

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
            _colourSchemeComboBox.SelectedItem = ProjectValues.ColourScheme
            _ignoreAutoCompleteComboBox.SelectedItem = ProjectValues.IgnoreAutoComplete
            If _uiFontComboBox.Items.Contains(ProjectValues.UiFontName) Then
                _uiFontComboBox.SelectedItem = ProjectValues.UiFontName
            Else
                _uiFontComboBox.Items.Add(ProjectValues.UiFontName)
                _uiFontComboBox.SelectedItem = ProjectValues.UiFontName
            End If
            _verticalTabToggle.Checked = ProjectValues.EntryMode = EntryMode.Vertical
            _autoShowScanToggle.Checked = ProjectValues.AutoShowScan
            _autoShowRulerToggle.Checked = ProjectValues.AutoShowRuler
            _skipSurnameToggle.Checked = ProjectValues.SkipSurname
            _formatPicklistSelectionsToggle.Checked = ProjectValues.FormatPicklistSelections
            _pickListCompletionToggle.Checked = ProjectValues.PickListCompletion
            _match3VolCharsToggle.Checked = ProjectValues.Match3VolChars
            _showForenamePickListToggle.Checked = ProjectValues.ShowForenamePickList
            _showDistrictPickListToggle.Checked = ProjectValues.ShowDistrictPickList
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

            _diagnosticLoggingCheckBox.Checked = ProjectValues.EnableDiagnosticLogging

            Select Case ProjectValues.OutputCharacterSet
                Case "Input Format"
                    _outputCharacterSetComboBox.SelectedItem = "Input Format"

                Case "UTF-8"
                    _outputCharacterSetComboBox.SelectedItem = "UTF-8"

                Case Else
                    _outputCharacterSetComboBox.SelectedItem = "FreeBMD"
            End Select

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

        If _verticalTabToggle.Checked Then
            ProjectValues.EntryMode = EntryMode.Vertical
        Else
            ProjectValues.EntryMode = EntryMode.Horizontal
        End If

        ProjectValues.AutoShowScan = _autoShowScanToggle.Checked
        ProjectValues.AutoShowRuler = _autoShowRulerToggle.Checked
        ProjectValues.SkipSurname = _skipSurnameToggle.Checked
        ProjectValues.FormatPicklistSelections = _formatPicklistSelectionsToggle.Checked
        ProjectValues.PickListCompletion = _pickListCompletionToggle.Checked
        ProjectValues.Match3VolChars = _match3VolCharsToggle.Checked
        ProjectValues.ShowForenamePickList = _showForenamePickListToggle.Checked
        ProjectValues.ShowDistrictPickList = _showDistrictPickListToggle.Checked

        If _uiFontComboBox.SelectedItem IsNot Nothing Then
            ProjectValues.UiFontName = _uiFontComboBox.SelectedItem.ToString()
        End If

        ProjectValues.UiFontSize = CSng(_uiFontSizeNumeric.Value)

        ProjectValues.VerifyFontSize = CSng(_verifyFontSizeNumeric.Value)
        Dim diagnosticLoggingWasEnabled As Boolean = ProjectValues.EnableDiagnosticLogging

        ProjectValues.EnableDiagnosticLogging = _diagnosticLoggingCheckBox.Checked

        If Not diagnosticLoggingWasEnabled AndAlso ProjectValues.EnableDiagnosticLogging Then
            DebugLog.Clear()
        End If

        Select Case _outputCharacterSetComboBox.SelectedItem?.ToString()
            Case "Input Format"
                ProjectValues.OutputCharacterSet = "Input Format"

            Case "UTF-8"
                ProjectValues.OutputCharacterSet = "UTF-8"

            Case Else
                ProjectValues.OutputCharacterSet = "ISO-8859-1"
        End Select

        ProjectValuesStore.Save()

    End Sub

#End Region

#Region "Appearance"

    Private Sub ApplyOptionsAppearance()

        _capitalisationPanel.BackColor = UiColors.PanelBackground

        headerPanel.BackColor = UiColors.PanelBackground

        footerPanel.BackColor = UiColors.PanelBackground

        mainSplitContainer.BackColor = UiColors.Border

        mainSplitContainer.Panel1.BackColor = UiColors.PanelBackground

        mainSplitContainer.Panel2.BackColor = UiColors.PanelBackground

        _picklistsPanel.BackColor = UiColors.PanelBackground

        lblTitle.Font = UiFonts.Title

        lblTitle.ForeColor = UiColors.TextPrimary

        lblSubtitle.Font = UiFonts.Normal

        lblSubtitle.ForeColor = UiColors.TextSecondary

        lblPageTitle.Font = UiFonts.SectionHeading

        lblPageTitle.ForeColor = UiColors.TextPrimary

        lblPageDescription.Font = UiFonts.Normal

        lblPageDescription.ForeColor = UiColors.TextSecondary

        _generalPanel.BackColor = UiColors.PanelBackground

        _entryPanel.BackColor = UiColors.PanelBackground

        _appearanceTab.BackColor = UiColors.PanelBackground

        _scanningTab.BackColor = UiColors.PanelBackground

        _uploadTab.BackColor = UiColors.PanelBackground

        _diagnosticsTab.BackColor = UiColors.PanelBackground

        ThemeManager.ApplyNavigationButton(btnGeneral)

        ThemeManager.ApplyNavigationButton(btnEntry)

        ThemeManager.ApplyNavigationButton(btnCapitalisation)

        ThemeManager.ApplyNavigationButton(btnAdvanced)

        ThemeManager.ApplyPrimaryButton(btnOk)

        ThemeManager.ApplySecondaryButton(btnCancel)

    End Sub

    Private Sub SelectNavigationButton(
        selectedButton As Button)

        For Each button As Button In {
            btnGeneral,
            btnEntry,
            btnPicklists,
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
        _capitalisationPanel.Visible = False
        _picklistsPanel.Visible = False

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
    Private Sub ShowPicklistsPage()

        HideOptionPages()
        SelectNavigationButton(btnPicklists)

        lblPageTitle.Text = "Picklists"
        lblPageDescription.Text = "Choose how the Forename and District picklists behave."

        _picklistsPanel.Visible = True
        _picklistsPanel.BringToFront()

    End Sub
    Private Sub ShowCapitalisationPage()

        HideOptionPages()
        SelectNavigationButton(btnCapitalisation)

        lblPageTitle.Text = "Capitalisation"
        lblPageDescription.Text = "Capitalisation settings for the fields in the current batch."

        _capitalisationPanel.Visible = True
        _capitalisationPanel.BringToFront()

    End Sub
    Private Sub SaveCapitalisationValues()

        CapitalisationChanges.Clear()

        Dim fields() As GridField = GridLayout.GetVisibleFields().Where(Function(field) FieldMetaData.Meta(field).IsDataColumn).ToArray()
        Dim values As New Text.StringBuilder()

        For Each field As GridField In fields

            Dim selectedMode As CapitalisationMode = CapitalisationMode.AsTyped

            For Each control As Control In _capitalisationTable.Controls

                Dim panel As Panel = TryCast(control, Panel)

                If panel Is Nothing Then
                    Continue For
                End If

                For Each child As Control In panel.Controls

                    Dim radioButton As RadioButton = TryCast(child, RadioButton)

                    If radioButton Is Nothing OrElse Not radioButton.Checked Then
                        Continue For
                    End If

                    Dim tag As CapitalisationTag = TryCast(radioButton.Tag, CapitalisationTag)

                    If tag IsNot Nothing AndAlso tag.Field = field Then
                        selectedMode = tag.Mode
                        Exit For
                    End If

                Next

            Next

            values.Append(CInt(selectedMode).ToString())

            Dim oldMode As CapitalisationMode

            If _originalCapitalisation.TryGetValue(field, oldMode) AndAlso oldMode <> selectedMode Then
                CapitalisationChanges.Add(New CapitalisationChange With {.Field = field, .OldMode = oldMode, .NewMode = selectedMode})
            End If

        Next

        CapitalisationData.SetSettings(ProjectValues.BatchType, ProjectValues.Year, values.ToString())
        CapitalisationData.Save()

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
    Private Sub Match3VolCharsToggle_CheckedChanged(sender As Object, e As EventArgs)

        If _loadingOptionValues Then
            Return
        End If

        If ProjectValues.Year < 1993 AndAlso _match3VolCharsToggle.Checked Then
            MessageBox.Show(Me, "This option should only be used for batches after 1992." & vbCrLf & vbCrLf & "For batches before 1993 the full volume code should be matched." & vbCrLf & vbCrLf & "WinBMD2 normally sets this option automatically from the batch year.", "Volume matching warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        ElseIf ProjectValues.Year >= 1993 AndAlso Not _match3VolCharsToggle.Checked Then
            MessageBox.Show(Me, "This option should normally be enabled for batches from 1993 onwards." & vbCrLf & vbCrLf & "For these batches only the first 3 characters of the volume code should be matched." & vbCrLf & vbCrLf & "WinBMD2 normally sets this option automatically from the batch year.", "Volume matching warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

    End Sub
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
    Private Sub btnPicklists_Click(sender As Object, e As EventArgs) Handles btnPicklists.Click

        ShowPicklistsPage()

    End Sub
    Private Sub btnCapitalisation_Click(
        sender As Object,
        e As EventArgs) Handles btnCapitalisation.Click

        ShowCapitalisationPage

    End Sub

    Private Sub btnAdvanced_Click(
        sender As Object,
        e As EventArgs) Handles btnAdvanced.Click

        ShowAdvancedPage()

    End Sub

    Private Sub btnOk_Click(sender As Object, e As EventArgs) Handles btnOk.Click

        SaveCapitalisationValues()
        SaveOptionValues()

        DialogResult = DialogResult.OK
        Close()

    End Sub

#End Region
    Private Class CapitalisationTag

        Public Property Field As GridField
        Public Property Mode As CapitalisationMode

        Public Sub New(field As GridField, mode As CapitalisationMode)
            Me.Field = field
            Me.Mode = mode
        End Sub

    End Class
    Public Class CapitalisationChange

        Public Property Field As GridField
        Public Property OldMode As CapitalisationMode
        Public Property NewMode As CapitalisationMode

    End Class
End Class