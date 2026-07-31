Public Module ProjectValues

#Region "Command Panels"

    Public Property FilePanelExpanded As Boolean = True

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

End Module