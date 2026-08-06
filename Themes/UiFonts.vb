Imports System.Drawing

Public Module UiFonts

    Public ReadOnly Property Normal As Font
        Get
            Return CreateFont(
                ProjectValues.UiFontSize,
                FontStyle.Regular)
        End Get
    End Property

    Public ReadOnly Property Title As Font
        Get
            Return CreateFont(
                ProjectValues.UiFontSize + 7.0F,
                FontStyle.Bold)
        End Get
    End Property

    Public ReadOnly Property SectionHeading As Font
        Get
            Return CreateFont(
                ProjectValues.UiFontSize + 3.0F,
                FontStyle.Bold)
        End Get
    End Property

    Private Function CreateFont(
        fontSize As Single,
        style As FontStyle) As Font

        Try
            Return New Font(
                ProjectValues.UiFontName,
                fontSize,
                style)
        Catch
            Return New Font(
                SystemFonts.MessageBoxFont.FontFamily,
                fontSize,
                style)
        End Try

    End Function

End Module