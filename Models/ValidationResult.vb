Public NotInheritable Class ValidationResult

    Public ReadOnly Property State As ValidationState

    Public ReadOnly Property Message As String

    Private Sub New(state As ValidationState, message As String)

        Me.State = state
        Me.Message = message

    End Sub

    Public Shared Function Ok() As ValidationResult

        Return New ValidationResult(ValidationState.Ok, "")

    End Function

    Public Shared Function Warning(message As String) As ValidationResult

        Return New ValidationResult(ValidationState.Warning, message)

    End Function

    Public Shared Function [Error](message As String) As ValidationResult

        Return New ValidationResult(ValidationState.Error, message)

    End Function

    Public ReadOnly Property IsOk As Boolean
        Get
            Return State = ValidationState.Ok
        End Get
    End Property

    Public ReadOnly Property IsWarning As Boolean
        Get
            Return State = ValidationState.Warning
        End Get
    End Property

    Public ReadOnly Property IsError As Boolean
        Get
            Return State = ValidationState.Error
        End Get
    End Property

End Class
Public NotInheritable Class DirectiveValidationResult

    Public ReadOnly Property Directive As RowDirective
    Public ReadOnly Property WarningType As DirectiveWarningType
    Public ReadOnly Property Result As ValidationResult

    Public Sub New(directive As RowDirective, warningType As DirectiveWarningType, result As ValidationResult)

        Me.Directive = directive
        Me.WarningType = warningType
        Me.Result = result

    End Sub

End Class