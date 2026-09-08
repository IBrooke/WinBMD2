Imports System.Collections.Generic
Imports System.Drawing
Imports System.Reflection
Public Module ProjectValues
    Public Property LoadingPersistedValues As Boolean = True
    Public Event StatusMessageRequested(message As String, duration As Integer)

#Region "Districts"
    Public Property DistrictVersion As String = ""
#End Region

#Region "Command Panels"

    Public Property FilePanelExpanded As Boolean = True

#End Region

#Region "Paths and Character Set"
    Public Property SaveFolder As String
    Public Property OutputCharacterSet As String = "ISO-8859-1"
    Public Property InputCharacterSet As String = "ISO-8859-1"
    Public Property UploadServerUrl As String = "www.freebmd.org.uk"
#End Region

#Region "Transcription Form"
    Public Property TranscriptionFormBounds As New Dictionary(Of String, FormBoundsData)

#End Region

#Region "Picklists"
    Public Property FormatPicklistSelections As Boolean = False
    Public Property PickListCompletion As Boolean = True
    Public Property ShowForenamePickList As Boolean = True
    Public Property ShowDistrictPickList As Boolean = True
    Public Property Match3VolChars As Boolean = False

#End Region

#Region "Scan View Form"

    Public Property ScanViewLeft As Integer = -1
    Public Property ScanViewTop As Integer = -1
    Public Property ScanViewWidth As Integer = 900
    Public Property ScanViewHeight As Integer = 650
    Public Property ScanViewMaximized As Boolean
    Public Property AutoShowScan As Boolean = True
    Public Property AutoShowRuler As Boolean = True
    Public Property ScanViewSettings As New Dictionary(Of String, ScanViewData)
    Public Property RulerSettings As New Dictionary(Of String, RulerData)
    Public Property VerifyFieldLayouts As New Dictionary(Of String, Dictionary(Of String, VerifyFieldLayoutData))
    Public Property EntryMode As EntryMode = EntryMode.Horizontal
    Public Property SkipSurname As Boolean = False
#End Region

#Region "Header Form"

    Public Property HeaderFormLeft As Integer = -1
    Public Property HeaderFormTop As Integer = -1
    Public Property HeaderFormWidth As Integer = 964
    Public Property HeaderFormHeight As Integer = 681
    Public Property HeaderFormMaximized As Boolean

#End Region

#Region "Grid"

    Public Property GridColumnWidths As New Dictionary(Of String, List(Of Integer))

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

#Region "Header Form"

    Public Property BatchType As String = ""
    Public Property BatchName As String = ""
    Private _year As Integer

    Public Property Year As Integer
        Get
            Return _year
        End Get
        Set(value As Integer)
            _year = value

            If LoadingPersistedValues Then
                Return
            End If

            Dim shouldMatch3 As Boolean = value >= 1993

            If Match3VolChars = shouldMatch3 Then
                Return
            End If

            Match3VolChars = shouldMatch3

            If shouldMatch3 Then
                RaiseEvent StatusMessageRequested("First 3-character volume matching has been enabled for post-1992 batches.", 10000)
            Else
                RaiseEvent StatusMessageRequested("First 3-character volume matching has been disabled for pre-1993 batches.", 10000)
            End If

            DebugLog.Write($"[OPTIONS] Match3VolChars automatically set to {Match3VolChars} for year {value}")
        End Set
    End Property
    Public Property Quarter As Integer
    Public Property Month As Integer
    Public Property Created As String = ""
    Public Property DateModified As Date = Date.Today
    Public Property Page As Integer
    Public Property PageSource As Integer = -1
    Public Property PageLetter As String = ""
    Public Property PageSuffix As String = ""
    Public Property SequenceType As String = "SEQUENCED"
    Public Property VNF As String = ""
    Public Property SourceRef As String = ""
    Public Property Syndicate As String = ""
    Public Property Comments As String = ""

#End Region
#Region "Appearance"

    Public Property ColourScheme As UiColourScheme = UiColourScheme.Teal
    Public Property UiFontName As String = SystemFonts.MessageBoxFont.FontFamily.Name
    Public Property UiFontSize As Single = SystemFonts.MessageBoxFont.Size
    Public Property UiFontColourArgb As Integer = SystemColors.ControlText.ToArgb()
    Public Property VerifyFontSize As Single = 12.0F
    Public Property ValidationMode As ValidationMode = ValidationMode.Entry
    Public Property RecentFiles As New List(Of String)
#End Region
#Region "Entry"

    Public Property IgnoreAutoComplete As IgnoreAutoCompleteKey = IgnoreAutoCompleteKey.Tab
    Public Property IgnoreAaD As Boolean = False

#End Region

    Public Sub WriteToDebugLog()

        DebugLog.WriteAlways("======= PROJECT VALUES =======")

        Dim properties() As PropertyInfo = GetType(ProjectValues).GetProperties(BindingFlags.Public Or BindingFlags.Static).OrderBy(Function(item) item.Name).ToArray()

        For Each propertyInfo As PropertyInfo In properties

            Dim propertyName As String = propertyInfo.Name

            If propertyName.Equals(NameOf(UserPW), StringComparison.OrdinalIgnoreCase) Then
                DebugLog.WriteAlways($"{propertyName,-24}: [not logged]")
                Continue For
            End If

            Dim value As Object = propertyInfo.GetValue(Nothing)

            If value Is Nothing Then
                DebugLog.WriteAlways($"{propertyName,-24}: <Nothing>")
                Continue For
            End If

            Dim dictionary As System.Collections.IDictionary = TryCast(value, System.Collections.IDictionary)

            If dictionary IsNot Nothing Then

                DebugLog.WriteAlways($"{propertyName,-24}:")

                Dim keys() As Object = dictionary.Keys.Cast(Of Object)().OrderBy(Function(item) item.ToString()).ToArray()

                For Each key As Object In keys

                    Dim itemValue As Object = dictionary(key)

                    If itemValue Is Nothing Then
                        DebugLog.WriteAlways($"    {key} : <Nothing>")
                        Continue For
                    End If

                    Dim nestedDictionary As System.Collections.IDictionary = TryCast(itemValue, System.Collections.IDictionary)

                    If nestedDictionary IsNot Nothing Then
                        Dim nestedValues = nestedDictionary.Keys.Cast(Of Object)().OrderBy(Function(item) item.ToString()).Select(Function(item) $"{item}={nestedDictionary(item)}")
                        DebugLog.WriteAlways($"    {key} : {String.Join(", ", nestedValues)}")
                        Continue For
                    End If

                    Dim nestedCollection As System.Collections.ICollection = TryCast(itemValue, System.Collections.ICollection)

                    If nestedCollection IsNot Nothing AndAlso Not TypeOf itemValue Is String Then
                        Dim collectionValues = nestedCollection.Cast(Of Object)().Select(Function(item) item.ToString())
                        DebugLog.WriteAlways($"    {key} : {String.Join(", ", collectionValues)}")
                        Continue For
                    End If

                    Dim itemType As Type = itemValue.GetType()
                    Dim itemProperties() As PropertyInfo = itemType.GetProperties(BindingFlags.Public Or BindingFlags.Instance).Where(Function(item) item.CanRead AndAlso item.GetIndexParameters().Length = 0).OrderBy(Function(item) item.Name).ToArray()

                    If itemProperties.Length > 0 AndAlso itemType IsNot GetType(String) AndAlso Not itemType.IsPrimitive AndAlso Not itemType.IsEnum Then
                        Dim itemValues = itemProperties.Select(Function(item) $"{item.Name}={item.GetValue(itemValue)}")
                        DebugLog.WriteAlways($"    {key} : {String.Join(", ", itemValues)}")
                    Else
                        DebugLog.WriteAlways($"    {key} = {itemValue}")
                    End If

                Next

                Continue For

            End If

            Dim collection As System.Collections.ICollection = TryCast(value, System.Collections.ICollection)

            If collection IsNot Nothing AndAlso Not TypeOf value Is String Then

                DebugLog.WriteAlways($"{propertyName,-24}:")

                For Each item As Object In collection
                    DebugLog.WriteAlways($"    {item}")
                Next

                Continue For

            End If

            DebugLog.WriteAlways($"{propertyName,-24}: {value}")

        Next

        DebugLog.WriteAlways("==============================")

    End Sub
End Module