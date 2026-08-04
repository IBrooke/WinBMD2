'------------------------------------------------------------------------------
' FormBoundsHelper
'
' Provides common routines for restoring and saving the size, position and
' window state of persisted forms.
'
' Most forms store one ordinary set of bounds as separate ProjectValues.
' TranscriptionForm can also use FormBoundsData so that different grid layouts
' can retain different form bounds.
'
' Minimized forms use RestoreBounds and are never reopened minimized.
'------------------------------------------------------------------------------
Public Module FormBoundsHelper

    ' Used by forms such as HeaderForm and ScanView, which store one set of
    ' bounds as separate ProjectValues properties.
    Public Sub RestoreForm(
        targetForm As Form,
        savedLeft As Integer,
        savedTop As Integer,
        savedWidth As Integer,
        savedHeight As Integer,
        savedMaximized As Boolean)

        Dim savedBounds As New FormBoundsData With {
            .Left = savedLeft,
            .Top = savedTop,
            .Width = savedWidth,
            .Height = savedHeight,
            .Maximized = savedMaximized
        }

        RestoreForm(targetForm, savedBounds)

    End Sub

    ' Used by TranscriptionForm, which stores a FormBoundsData object for each
    ' event, year and quarter combination.
    Public Sub RestoreForm(
        targetForm As Form,
        savedBounds As FormBoundsData)

        Dim width As Integer =
            Math.Max(
                targetForm.MinimumSize.Width,
                savedBounds.Width)

        Dim height As Integer =
            Math.Max(
                targetForm.MinimumSize.Height,
                savedBounds.Height)

        Dim formBounds As New Rectangle(
            savedBounds.Left,
            savedBounds.Top,
            width,
            height)

        If BoundsAreVisible(formBounds) Then

            targetForm.StartPosition = FormStartPosition.Manual
            targetForm.Bounds = formBounds

        Else

            targetForm.StartPosition = FormStartPosition.CenterScreen
            targetForm.Size = New Size(width, height)

        End If

        If savedBounds.Maximized Then
            targetForm.WindowState = FormWindowState.Maximized
        Else
            targetForm.WindowState = FormWindowState.Normal
        End If

    End Sub

    ' Retained for HeaderForm and ScanView, whose existing save routines expect
    ' a Rectangle.
    Public Function GetBoundsToSave(
        targetForm As Form) As Rectangle

        If targetForm.WindowState = FormWindowState.Normal Then
            Return targetForm.Bounds
        End If

        Return targetForm.RestoreBounds

    End Function

    ' Used by TranscriptionForm to capture all five values together.
    Public Function GetBoundsDataToSave(
        targetForm As Form) As FormBoundsData

        Dim boundsToSave As Rectangle =
            GetBoundsToSave(targetForm)

        Return New FormBoundsData With {
            .Left = boundsToSave.Left,
            .Top = boundsToSave.Top,
            .Width = boundsToSave.Width,
            .Height = boundsToSave.Height,
            .Maximized =
                targetForm.WindowState = FormWindowState.Maximized
        }

    End Function

    Public Function ShouldRestoreMaximized(
        targetForm As Form) As Boolean

        Return targetForm.WindowState = FormWindowState.Maximized

    End Function

    Private Function BoundsAreVisible(
        savedBounds As Rectangle) As Boolean

        If savedBounds.Left = -1 AndAlso
           savedBounds.Top = -1 Then

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