Imports System.Collections.Generic
Public Module ProjectValues

#Region "Command Panels"

    Public Property FilePanelExpanded As Boolean = True

#End Region

#Region "Transcription Form"

    Public Property TranscriptionFormBounds As New Dictionary(Of String, FormBoundsData)

#End Region

#Region "Scan View Form"

    Public Property ScanViewLeft As Integer = -1
    Public Property ScanViewTop As Integer = -1
    Public Property ScanViewWidth As Integer = 900
    Public Property ScanViewHeight As Integer = 650
    Public Property ScanViewMaximized As Boolean

#End Region

#Region "Header Form"

    Public Property HeaderFormLeft As Integer = -1
    Public Property HeaderFormTop As Integer = -1
    Public Property HeaderFormWidth As Integer = 964
    Public Property HeaderFormHeight As Integer = 681
    Public Property HeaderFormMaximized As Boolean

#End Region

#Region "Grid"

    Public Property GridColumnWidths As New Dictionary(Of String, Dictionary(Of String, Integer))

#End Region

#Region "Current User"

    ' Current FreeBMD account used for uploading.
    Public Property UserName As String = ""
    Public Property UserEmail As String = ""
    Public Property UserPW As String = ""

#End Region

#Region "File Creator"

    ' Original creator of the currently open transcription.
    Public Property Creator As String = ""
    Public Property CreatorEmail As String = ""

#End Region

#Region "Diagnostics"

    Public Property EnableDiagnosticLogging As Boolean = True

#End Region

#Region "Header"

    Public Property BatchType As String = ""

    Public Property Year As Integer
    Public Property Quarter As Integer
    Public Property Month As Integer

    Public Property Page As Integer
    Public Property PageSource As Integer = -1
    Public Property PageLetter As String = ""
    Public Property PageSuffix As String = ""

    Public Property VNF As String = ""
    Public Property SourceRef As String = ""
    Public Property Syndicate As String = ""
    Public Property Comments As String = ""

#End Region

#Region "Appearance"

    Public Property ColourScheme As UiColourScheme = UiColourScheme.Teal

#End Region
End Module