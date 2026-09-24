#Region "Appearance"

Public Enum UiColourScheme
    Blue
    Teal
    Magenta
    Violet
    Gold
    Green
    Yellow
    Orange
End Enum

#End Region

#Region "Grid"

Public Enum CellTextAlign
    Left
    Center
    Right
End Enum

#End Region

#Region "Directives"
Public Enum DirectiveType
    Comment
    Theory
    TheoryRef
    InternalComment
    Break
    Page
End Enum
' Identifies the different warning conditions which may be recorded
' against an individual directive.
Public Enum DirectiveWarningType
    InvalidPageNumber
    IncorrectPageSequence
End Enum
#End Region

#Region "Entry"
Public Enum EntryMode
    Horizontal
    Vertical
End Enum
Public Enum IgnoreAutoCompleteKey
    None
    Tab
    [Return]
    All
End Enum
Public Enum ValidationState
    Ok
    Warning
    [Error]
End Enum
Public Enum ValidationMode
    Entry
    Upload
End Enum
#End Region

#Region "Buttons"
Public Enum ScanCommandIcon
    None
    ZoomOut
    ZoomIn
    RotateLeft
    RotateRight
End Enum
#End Region
#Region "Scan"
Public Enum RulerSetupStage
    None
    AwaitingRow1
    AwaitingRow10
    Complete
End Enum
#End Region
#Region "Capitalisation"
Public Enum CapitalisationMode
    AsTyped = 1
    Upper = 2
    Lower = 3
    Name = 4
End Enum
#End Region