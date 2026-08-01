'------------------------------------------------------------------------------
' FormBoundsHelper
'
' Provides common routines for restoring and saving the size, position and
' window state of persisted forms.
'
' The helper reads the current bounds directly from the Form passed to it.
' Each form remains responsible for storing the returned values in its own
' ProjectValues properties.
'
' Minimized forms are restored using RestoreBounds and are never reopened in
' the minimized state.
'------------------------------------------------------------------------------
Public Module FormBoundsHelper

    Public Sub RestoreForm(
        targetForm As Form,
        savedLeft As Integer,
        savedTop As Integer,
        savedWidth As Integer,
        savedHeight As Integer,
        savedMaximized As Boolean)

        Dim width As Integer =
            Math.Max(targetForm.MinimumSize.Width, savedWidth)

        Dim height As Integer =
            Math.Max(targetForm.MinimumSize.Height, savedHeight)

        Dim savedBounds As New Rectangle(
            savedLeft,
            savedTop,
            width,
            height)

        If BoundsAreVisible(savedBounds) Then
            targetForm.StartPosition = FormStartPosition.Manual
            targetForm.Bounds = savedBounds
        Else
            targetForm.StartPosition = FormStartPosition.CenterScreen
            targetForm.Size = New Size(width, height)
        End If

        If savedMaximized Then
            targetForm.WindowState = FormWindowState.Maximized
        Else
            targetForm.WindowState = FormWindowState.Normal
        End If

    End Sub

    Public Function GetBoundsToSave(targetForm As Form) As Rectangle

        If targetForm.WindowState = FormWindowState.Normal Then
            Return targetForm.Bounds
        End If

        Return targetForm.RestoreBounds

    End Function

    Public Function ShouldRestoreMaximized(targetForm As Form) As Boolean

        Return targetForm.WindowState = FormWindowState.Maximized

    End Function

    Private Function BoundsAreVisible(savedBounds As Rectangle) As Boolean

        If savedBounds.Left = -1 AndAlso savedBounds.Top = -1 Then
            Return False
        End If

        For Each display As Screen In Screen.AllScreens

            Dim visibleArea As Rectangle =
                Rectangle.Intersect(
                    display.WorkingArea,
                    savedBounds)

            If visibleArea.Width >= 100 AndAlso
               visibleArea.Height >= 100 Then

                Return True
            End If

        Next

        Return False

    End Function

End Module
