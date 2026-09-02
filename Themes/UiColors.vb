Imports System.Drawing

'------------------------------------------------------------------------------
' UiColors
'
' Provides the colours used throughout WinBMD2.
'
' The selected colour scheme is stored in ProjectValues. Each scheme supplies
' a soft tint, a stronger shaded colour and a solid accent colour. The module
' then exposes named colours for forms, panels, grids, buttons, selections,
' warnings, errors and verified data.
'
' Forms and controls should use these named properties rather than containing
' their own hard-coded colours. This allows the complete application appearance
' to change consistently when the user selects another colour scheme.
'------------------------------------------------------------------------------

Public Module UiColors

    Private ReadOnly Property Palette As UiPalette
        Get

            Select Case ProjectValues.ColourScheme

                Case UiColourScheme.Teal
                    Return New UiPalette(Color.FromArgb(234, 248, 246), Color.FromArgb(202, 232, 228), Color.FromArgb(28, 124, 116))

                Case UiColourScheme.Magenta
                    Return New UiPalette(Color.FromArgb(249, 238, 245), Color.FromArgb(232, 205, 221), Color.FromArgb(172, 72, 124))

                Case UiColourScheme.Violet
                    Return New UiPalette(Color.FromArgb(246, 241, 251), Color.FromArgb(220, 207, 239), Color.FromArgb(104, 78, 164))

                Case UiColourScheme.Gold
                    Return New UiPalette(Color.FromArgb(254, 248, 230), Color.FromArgb(239, 222, 170), Color.FromArgb(166, 126, 36))

                Case UiColourScheme.Green
                    Return New UiPalette(Color.FromArgb(239, 248, 239), Color.FromArgb(207, 231, 208), Color.FromArgb(62, 132, 76))

                Case Else
                    Return New UiPalette(Color.FromArgb(238, 246, 252), Color.FromArgb(203, 226, 241), Color.FromArgb(38, 116, 158))

            End Select

        End Get
    End Property

#Region "Theme Colours"

    Public ReadOnly Property ThemeSoft As Color
        Get
            Return Palette.Soft
        End Get
    End Property

    Public ReadOnly Property ThemeShaded As Color
        Get
            Return Palette.Shaded
        End Get
    End Property

    Public ReadOnly Property ThemeSolid As Color
        Get
            Return Palette.Solid
        End Get
    End Property

#End Region

#Region "Grid"

    Public ReadOnly Property GridLine As Color
        Get
            Return Color.FromArgb(170, 175, 185)
        End Get
    End Property

    Public ReadOnly Property GridDivider As Color
        Get
            Return Color.FromArgb(150, 155, 165)
        End Get
    End Property

    Public ReadOnly Property AlternateRowBackground As Color
        Get
            Return ThemeSoft
        End Get
    End Property

    Public ReadOnly Property EditBackground As Color
        Get
            Return Color.White
        End Get
    End Property

    Public ReadOnly ValidationError As Color = Color.Red
    Public ReadOnly ValidationWarning As Color = Color.Goldenrod

#End Region

#Region "Backgrounds"

    Public ReadOnly Property AppBackground As Color
        Get
            Return ThemeShaded
        End Get
    End Property

    Public ReadOnly Property PanelBackground As Color
        Get
            Return Color.White
        End Get
    End Property

    Public ReadOnly Property SubtleBackground As Color
        Get
            Return Color.FromArgb(246, 248, 251)
        End Get
    End Property

#End Region

#Region "Text"

    Public ReadOnly Property TextPrimary As Color
        Get
            Return Color.FromArgb(35, 42, 52)
        End Get
    End Property

    Public ReadOnly Property TextSecondary As Color
        Get
            Return Color.FromArgb(105, 112, 123)
        End Get
    End Property

    Public ReadOnly Property UserText As Color
        Get
            Return TextPrimary
        End Get
    End Property

    Public ReadOnly Property TextOnDark As Color
        Get
            Return Color.White
        End Get
    End Property

#End Region

#Region "Option Switches"

    Public ReadOnly Property ToggleOn As Color
        Get
            Return Color.FromArgb(55, 170, 90)
        End Get
    End Property

    Public ReadOnly Property ToggleOff As Color
        Get
            Return Color.FromArgb(170, 170, 170)
        End Get
    End Property

    Public ReadOnly Property ToggleKnob As Color
        Get
            Return Color.White
        End Get
    End Property

#End Region

#Region "Borders And Accents"

    Public ReadOnly Property AccentBorder As Color
        Get
            Return ThemeSolid
        End Get
    End Property

    Public ReadOnly Property Border As Color
        Get
            Return ThemeSoft
        End Get
    End Property

    Public ReadOnly Property Primary As Color
        Get
            Return ThemeSolid
        End Get
    End Property

    Public ReadOnly Property Selected As Color
        Get
            Return ThemeShaded
        End Get
    End Property

#End Region

#Region "Success"

    Public ReadOnly Property SuccessFill As Color
        Get
            Return Color.FromArgb(210, 235, 218)
        End Get
    End Property

    Public ReadOnly Property SuccessBorder As Color
        Get
            Return Color.FromArgb(170, 205, 180)
        End Get
    End Property

    Public ReadOnly Property SuccessText As Color
        Get
            Return Color.FromArgb(35, 70, 45)
        End Get
    End Property

#End Region

#Region "Warning"

    Public ReadOnly Property WarningFill As Color
        Get
            Return Color.FromArgb(255, 220, 80)
        End Get
    End Property

    Public ReadOnly Property WarningBorder As Color
        Get
            Return Color.FromArgb(180, 130, 0)
        End Get
    End Property

    Public ReadOnly Property WarningText As Color
        Get
            Return Color.FromArgb(120, 86, 16)
        End Get
    End Property

#End Region

#Region "Error"

    Public ReadOnly Property ErrorFill As Color
        Get
            Return Color.FromArgb(255, 210, 210)
        End Get
    End Property

    Public ReadOnly Property ErrorBorder As Color
        Get
            Return Color.FromArgb(190, 70, 70)
        End Get
    End Property

#End Region

End Module