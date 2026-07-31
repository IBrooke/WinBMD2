Imports System.Windows.Forms

Public Module CommandMetadata

    Private ReadOnly _definitions As Dictionary(Of AppCommand, CommandDefinition) =
        CreateDefinitions()

    Private ReadOnly _shortcuts As Dictionary(Of Keys, AppCommand) =
        CreateShortcutMap(_definitions)

    Public Function TryGetCommand(shortcut As Keys, ByRef command As AppCommand) As Boolean
        Return _shortcuts.TryGetValue(shortcut, command)
    End Function

    Public Function TryGetDefinition(command As AppCommand, ByRef definition As CommandDefinition) As Boolean
        Return _definitions.TryGetValue(command, definition)
    End Function

    Private Function CreateShortcutMap(
        definitions As Dictionary(Of AppCommand, CommandDefinition)) As Dictionary(Of Keys, AppCommand)

        Dim shortcuts As New Dictionary(Of Keys, AppCommand)

        For Each definition As CommandDefinition In definitions.Values
            If definition.Shortcut = Keys.None Then Continue For

            If shortcuts.ContainsKey(definition.Shortcut) Then
                Throw New InvalidOperationException(
                    $"The shortcut {definition.Shortcut} is assigned to more than one command.")
            End If

            shortcuts.Add(definition.Shortcut, definition.Command)
        Next

        Return shortcuts
    End Function

    Private Function CreateDefinitions() As Dictionary(Of AppCommand, CommandDefinition)
        Return New Dictionary(Of AppCommand, CommandDefinition) From {
            {
                AppCommand.OpenFile,
                New CommandDefinition(
                    AppCommand.OpenFile,
                    "Open",
                    Keys.F2,
                    "Open an existing transcription file.")
            },
            {
                AppCommand.SaveFile,
                New CommandDefinition(
                    AppCommand.SaveFile,
                    "Save",
                    Keys.F3,
                    "Save the current transcription file.")
            },
            {
                AppCommand.SaveFileAs,
                New CommandDefinition(
                    AppCommand.SaveFileAs,
                    "Save As",
                    Keys.Shift Or Keys.F3,
                    "Save the current transcription using a different filename.")
            }
        }
    End Function

End Module