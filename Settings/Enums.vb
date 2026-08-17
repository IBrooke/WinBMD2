#Region "Appearance"

Public Enum UiColourScheme
    Blue
    Teal
    Magenta
    Violet
    Gold
    Green
End Enum

#End Region

#Region "Grid"

Public Enum CellTextAlign
    Left
    Center
    Right
End Enum

#End Region

#Region "Entry"

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