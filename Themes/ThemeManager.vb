Imports System.Windows.Forms

'------------------------------------------------------------------------------
' ThemeManager
'
' Applies the current WinBMD2 theme to forms and controls.
'
' The intention is that forms contain little or no colour-setting code.
' Common appearance is applied automatically here, while individual forms
' only need to style controls with special behaviour (such as the Start button).
'------------------------------------------------------------------------------
Public Module ThemeManager

    Public Sub Apply(form As Form)

        form.BackColor = UiColors.AppBackground
        form.ForeColor = UiColors.TextPrimary

        ApplyControls(form.Controls)

    End Sub

    Private Sub ApplyControls(controls As Control.ControlCollection)

        For Each control As Control In controls

            control.ForeColor = UiColors.TextPrimary

            If TypeOf control Is Button Then
                ApplyStandardButton(DirectCast(control, Button))
            End If

            If control.HasChildren Then
                ApplyControls(control.Controls)
            End If

        Next

    End Sub
    Public Sub ApplyStandardButton(button As Button)

        button.BackColor = UiColors.ThemeSoft
        button.ForeColor = UiColors.TextPrimary

        button.FlatStyle = FlatStyle.Flat
        button.UseVisualStyleBackColor = False

        button.FlatAppearance.BorderSize = 1
        button.FlatAppearance.BorderColor = UiColors.AccentBorder
        button.FlatAppearance.MouseOverBackColor = UiColors.ThemeShaded
        button.FlatAppearance.MouseDownBackColor = UiColors.ThemeShaded

    End Sub
    Private Sub ApplyControl(control As Control)

        Select Case True

            Case TypeOf control Is Label

                control.ForeColor = UiColors.TextPrimary
                control.BackColor = Color.Transparent

            Case TypeOf control Is Panel,
                 TypeOf control Is TableLayoutPanel,
                 TypeOf control Is FlowLayoutPanel

                control.BackColor = UiColors.PanelBackground

            Case TypeOf control Is TextBox

                control.ForeColor = UiColors.UserText

            Case TypeOf control Is ComboBox

                control.ForeColor = UiColors.UserText

            Case TypeOf control Is Button

                ApplySecondaryButton(
                    DirectCast(control, Button))

        End Select

    End Sub
    Public Sub ApplyNavigationButton(button As Button)

        button.BackColor = UiColors.ThemeSoft
        button.ForeColor = UiColors.TextPrimary

        button.FlatStyle = FlatStyle.Flat
        button.UseVisualStyleBackColor = False

        button.FlatAppearance.BorderSize = 1
        button.FlatAppearance.BorderColor = UiColors.Border
        button.FlatAppearance.MouseOverBackColor = UiColors.ThemeShaded
        button.FlatAppearance.MouseDownBackColor = UiColors.ThemeShaded

    End Sub
    Public Sub ApplyPrimaryButton(
        button As Button)

        button.BackColor = UiColors.Primary
        button.ForeColor = UiColors.TextOnDark

        button.FlatStyle = FlatStyle.Flat
        button.FlatAppearance.BorderSize = 0

    End Sub

    Public Sub ApplySecondaryButton(
        button As Button)

        button.BackColor = UiColors.PanelBackground
        button.ForeColor = UiColors.TextPrimary

        button.FlatStyle = FlatStyle.Flat
        button.FlatAppearance.BorderColor = UiColors.GridLine
        button.FlatAppearance.BorderSize = 1

    End Sub

    Public Sub ApplyApplicationBackground(control As Control)

        control.BackColor = UiColors.AppBackground
        control.ForeColor = UiColors.TextPrimary

    End Sub

    Public Sub ApplyCardPanel(panel As Panel)

        panel.BackColor = UiColors.PanelBackground
        panel.ForeColor = UiColors.TextPrimary

    End Sub

    Public Sub ApplyInformationPanel(panel As Panel)

        panel.BackColor = UiColors.ThemeSoft
        panel.ForeColor = UiColors.TextPrimary

    End Sub
    Public Sub ApplyDataGrid(grid As DataGridView)

        grid.EnableHeadersVisualStyles = False

        grid.BackgroundColor = UiColors.PanelBackground
        grid.GridColor = UiColors.GridLine
        grid.BorderStyle = BorderStyle.None

        grid.ColumnHeadersDefaultCellStyle.BackColor =
            UiColors.ThemeSolid

        grid.ColumnHeadersDefaultCellStyle.ForeColor =
            UiColors.TextOnDark

        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
            UiColors.ThemeSolid

        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
            UiColors.TextOnDark

        grid.ColumnHeadersDefaultCellStyle.Font =
            New Font(grid.Font, FontStyle.Bold)

        grid.ColumnHeadersHeight = 30
        grid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.DisableResizing

        grid.DefaultCellStyle.BackColor =
            UiColors.PanelBackground

        grid.DefaultCellStyle.ForeColor =
            UiColors.UserText

        grid.DefaultCellStyle.SelectionBackColor =
            UiColors.Selected

        grid.DefaultCellStyle.SelectionForeColor =
            UiColors.TextPrimary

        grid.DefaultCellStyle.Padding =
            New Padding(3, 0, 3, 0)

        grid.AlternatingRowsDefaultCellStyle.BackColor =
            UiColors.AlternateRowBackground

        grid.RowTemplate.Height = 26

    End Sub
End Module