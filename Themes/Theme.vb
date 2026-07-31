Imports System.Drawing

Public Module Theme

#Region "Fonts"

    Public Const UiFontName As String = "Segoe UI"

    Public Const SmallFontSize As Single = 8.0F
    Public Const NormalFontSize As Single = 9.0F
    Public Const HeaderFontSize As Single = 9.0F
    Public Const LargeFontSize As Single = 11.0F

    Private ReadOnly _smallFont As New Font(UiFontName, SmallFontSize, FontStyle.Regular)
    Private ReadOnly _normalFont As New Font(UiFontName, NormalFontSize, FontStyle.Regular)
    Private ReadOnly _headerFont As New Font(UiFontName, HeaderFontSize, FontStyle.Bold)
    Private ReadOnly _largeFont As New Font(UiFontName, LargeFontSize, FontStyle.Regular)

    Public ReadOnly Property SmallFont As Font
        Get
            Return _smallFont
        End Get
    End Property

    Public ReadOnly Property NormalFont As Font
        Get
            Return _normalFont
        End Get
    End Property

    Public ReadOnly Property HeaderFont As Font
        Get
            Return _headerFont
        End Get
    End Property

    Public ReadOnly Property LargeFont As Font
        Get
            Return _largeFont
        End Get
    End Property

#End Region

#Region "Panel Colours"

    Public ReadOnly Property PanelBorderColor As Color
        Get
            Return Color.Silver
        End Get
    End Property

    Public ReadOnly Property PanelHeaderBackColor As Color
        Get
            Return Color.Gainsboro
        End Get
    End Property

    Public ReadOnly Property PanelHeaderHoverBackColor As Color
        Get
            Return Color.LightGray
        End Get
    End Property

    Public ReadOnly Property PanelHeaderTextColor As Color
        Get
            Return Color.Black
        End Get
    End Property

#End Region

End Module