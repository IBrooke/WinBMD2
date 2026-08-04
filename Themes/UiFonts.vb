Imports System.Drawing

Public Module UiFonts

    Public ReadOnly Property Normal As Font
        Get
            Return SystemFonts.MessageBoxFont
        End Get
    End Property

    Public ReadOnly Property Title As Font
        Get
            Return New Font(
                SystemFonts.MessageBoxFont.FontFamily,
                16.0F,
                FontStyle.Bold)
        End Get
    End Property

    Public ReadOnly Property SectionHeading As Font
        Get
            Return New Font(
                SystemFonts.MessageBoxFont.FontFamily,
                12.0F,
                FontStyle.Bold)
        End Get
    End Property

End Module