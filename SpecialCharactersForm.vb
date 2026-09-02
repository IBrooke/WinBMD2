Imports System.ComponentModel

Public Class SpecialCharactersForm
    <Browsable(False)>
    <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedCharacter As String = ""
    Private ReadOnly _magnifierForm As New CharacterMagnifierForm()
    Private ReadOnly _magnifierLabel As New Label()
    Private Sub SpecialCharactersForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        _magnifierForm.FormBorderStyle = FormBorderStyle.None
        _magnifierForm.ShowInTaskbar = False
        _magnifierForm.StartPosition = FormStartPosition.Manual
        _magnifierForm.AutoSize = False
        _magnifierForm.ClientSize = New Size(34, 52)

        _magnifierLabel.AutoSize = False
        _magnifierLabel.Dock = DockStyle.None
        _magnifierLabel.Location = Point.Empty
        _magnifierLabel.Size = New Size(34, 52)
        _magnifierLabel.TextAlign = ContentAlignment.MiddleCenter
        _magnifierLabel.Font = New Font(Font.FontFamily, 24.0F, FontStyle.Regular)
        _magnifierLabel.BorderStyle = BorderStyle.FixedSingle
        _magnifierLabel.Padding = New Padding(2, 0, 0, 0)
        _magnifierForm.Controls.Add(_magnifierLabel)

        BuildCharacterButtons()

    End Sub

    Private Sub BuildCharacterButtons()

        Dim unusedCodes As New HashSet(Of Integer) From {
            208, 215, 222, 240, 247, 254
        }

        Const columns As Integer = 10
        Dim index As Integer = 0

        For code As Integer = 192 To 255

            If unusedCodes.Contains(code) Then
                Continue For
            End If

            Dim button As New Button With {
                .Text = ChrW(code),
                .Size = btnCharacterTemplate.Size,
                .Font = btnCharacterTemplate.Font,
                .FlatStyle = btnCharacterTemplate.FlatStyle,
                .Tag = ChrW(code)
            }

            Dim row As Integer = index \ columns
            Dim column As Integer = index Mod columns

            button.Dock = DockStyle.Fill
            button.Margin = New Padding(1)

            characterTable.Controls.Add(button, column, row)

            AddHandler button.Click, AddressOf CharacterButton_Click

            AddHandler button.MouseEnter, AddressOf CharacterButton_MouseEnter
            AddHandler button.MouseLeave, AddressOf CharacterButton_MouseLeave

            index += 1

        Next

        btnCharacterTemplate.Visible = False

    End Sub
    Private Sub CharacterButton_Click(sender As Object, e As EventArgs)

        Dim button As Button = DirectCast(sender, Button)

        SelectedCharacter = button.Text
        DialogResult = DialogResult.OK
        Close()

    End Sub
    Private Sub SpecialCharactersForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        If e.KeyCode = Keys.Escape Then
            DialogResult = DialogResult.Cancel
            Close()
        End If

    End Sub

    Private Sub CharacterButton_MouseLeave(sender As Object, e As EventArgs)

        _magnifierForm.Hide()

    End Sub
    Private Sub SpecialCharactersForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed

        _magnifierForm.Close()

    End Sub

    Private Sub CharacterButton_MouseEnter(sender As Object, e As EventArgs)

        Dim button As Button = DirectCast(sender, Button)

        _magnifierLabel.Text = button.Text

        Dim screenPoint As Point = button.PointToScreen(Point.Empty)

        _magnifierForm.Location = New Point(screenPoint.X, screenPoint.Y - _magnifierForm.Height)
        _magnifierForm.Show(Me)
        _magnifierForm.BringToFront()

    End Sub
End Class
Friend Class CharacterMagnifierForm
    Inherits Form

    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

End Class