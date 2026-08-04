Imports WinBMD2.My.Resources

Public Class OptionsForm

    Public Sub New()

        InitializeComponent()

        Icon = WinBMDResources.WinBMD2Icon
        Text = "Options"

        ThemeManager.Apply(Me)
        ApplyOptionsAppearance()
        ShowGeneralPage()

    End Sub
    Private Sub ApplyOptionsAppearance()

        headerPanel.BackColor = UiColors.PanelBackground
        footerPanel.BackColor = UiColors.PanelBackground

        mainSplitContainer.BackColor = UiColors.Border
        mainSplitContainer.Panel1.BackColor = UiColors.PanelBackground
        mainSplitContainer.Panel2.BackColor = UiColors.PanelBackground

        lblTitle.Font = UiFonts.Title
        lblTitle.ForeColor = UiColors.TextPrimary

        lblSubtitle.Font = UiFonts.Normal
        lblSubtitle.ForeColor = UiColors.TextSecondary

        lblPageTitle.Font = UiFonts.SectionHeading
        lblPageTitle.ForeColor = UiColors.TextPrimary

        lblPageDescription.Font = UiFonts.Normal
        lblPageDescription.ForeColor = UiColors.TextSecondary

        ThemeManager.ApplyNavigationButton(btnGeneral)
        ThemeManager.ApplyNavigationButton(btnEntry)
        ThemeManager.ApplyNavigationButton(btnCapitalisation)
        ThemeManager.ApplyNavigationButton(btnAdvanced)

        ThemeManager.ApplyPrimaryButton(btnOk)
        ThemeManager.ApplySecondaryButton(btnCancel)

    End Sub
    Private Sub SelectNavigationButton(selectedButton As Button)

        For Each button As Button In {
        btnGeneral,
        btnEntry,
        btnCapitalisation,
        btnAdvanced
    }

            ThemeManager.ApplyNavigationButton(button)

        Next

        selectedButton.BackColor = UiColors.ThemeShaded
        selectedButton.FlatAppearance.BorderColor = UiColors.AccentBorder

    End Sub

    Private Sub ShowGeneralPage()

        SelectNavigationButton(btnGeneral)

        lblPageTitle.Text = "General"
        lblPageDescription.Text =
            "Appearance, scan display, upload and diagnostic settings."

    End Sub

    Private Sub ShowEntryPage()

        SelectNavigationButton(btnEntry)

        lblPageTitle.Text = "Entry"
        lblPageDescription.Text =
            "Grid navigation, picklist, autocomplete and entry settings."

    End Sub

    Private Sub ShowCapitalisationPage()

        SelectNavigationButton(btnCapitalisation)

        lblPageTitle.Text = "Capitalisation"
        lblPageDescription.Text =
            "Capitalisation settings for the fields in the current batch."

    End Sub

    Private Sub ShowAdvancedPage()

        SelectNavigationButton(btnAdvanced)

        lblPageTitle.Text = "Advanced"
        lblPageDescription.Text =
            "Advanced settings will be added later."

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

    Private Sub btnOK_Click(
        sender As Object,
        e As EventArgs) Handles btnOk.Click

        ProjectValuesStore.Save()

        DialogResult = DialogResult.OK
        Close()

    End Sub

End Class