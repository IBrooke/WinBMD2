Imports System.Windows.Forms

Public NotInheritable Class CommandDefinition

    Public Sub New(command As AppCommand, displayName As String, shortcut As Keys, description As String)
        Me.Command = command
        Me.DisplayName = displayName
        Me.Shortcut = shortcut
        Me.Description = description
    End Sub

    Public ReadOnly Property Command As AppCommand

    Public ReadOnly Property DisplayName As String

    Public ReadOnly Property Shortcut As Keys

    Public ReadOnly Property Description As String

End Class