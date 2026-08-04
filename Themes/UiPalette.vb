Imports System.Drawing

Public Structure UiPalette

    Public ReadOnly Property Soft As Color
    Public ReadOnly Property Shaded As Color
    Public ReadOnly Property Solid As Color

    Public Sub New(
        soft As Color,
        shaded As Color,
        solid As Color)

        Me.Soft = soft
        Me.Shaded = shaded
        Me.Solid = solid

    End Sub

End Structure
