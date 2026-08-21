Imports System.Collections.Generic
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class DirectiveEditorForm

    Private _rowIndex As Integer
    Private _suggestedPageNumber As Integer
    Private _loadingSelectedDirective As Boolean
    Private ReadOnly _toolTip As New System.Windows.Forms.ToolTip()

    Public ReadOnly Property Directives As New List(Of RowDirective)

    Public Sub New()

        InitializeComponent()

        _toolTip.InitialDelay = 50
        _toolTip.ReshowDelay = 50
        _toolTip.AutoPopDelay = 10000
        _toolTip.ShowAlways = True

        ApplyTheme()

        If typeCombo.Items.Count > 0 Then
            typeCombo.SelectedIndex = 0
        End If

        UpdateInputState()

    End Sub

    Public Sub New(
    rowNumber As Integer,
    existingDirectives As IEnumerable(Of RowDirective),
    suggestedPageNumber As Integer)

        Me.New()

        _rowIndex = rowNumber - 1
        _suggestedPageNumber = suggestedPageNumber

        Text = $"Directives for Row {rowNumber}"

        For Each directive As RowDirective In existingDirectives

            Directives.Add(
                New RowDirective With {
                    .RowIndex = directive.RowIndex,
                    .DirectiveType = directive.DirectiveType,
                    .Lines = directive.Lines,
                    .Text = directive.Text
})

        Next

        ReloadList()
        UpdateInputState()

    End Sub

    Private Sub ApplyTheme()

        ThemeManager.Apply(Me)

        ThemeManager.ApplyStandardButton(addButton)
        ThemeManager.ApplyStandardButton(updateButton)
        ThemeManager.ApplyStandardButton(deleteButton)
        ThemeManager.ApplyPrimaryButton(okButton)
        ThemeManager.ApplyStandardButton(closeButton)

        directiveList.BackColor = UiColors.PanelBackground
        directiveList.ForeColor = UiColors.UserText

        typeCombo.BackColor = SystemColors.Window
        typeCombo.ForeColor = UiColors.UserText

        rowsTextBox.BackColor = SystemColors.Window
        rowsTextBox.ForeColor = UiColors.UserText

        textTextBox.BackColor = SystemColors.Window
        textTextBox.ForeColor = UiColors.UserText

    End Sub
    Private Sub directiveList_DrawColumnHeader(
    sender As Object,
    e As DrawListViewColumnHeaderEventArgs) _
    Handles directiveList.DrawColumnHeader

        e.DrawDefault = True

    End Sub

    Private Sub directiveList_DrawItem(
    sender As Object,
    e As DrawListViewItemEventArgs) _
    Handles directiveList.DrawItem

        If directiveList.View <> View.Details Then
            e.DrawDefault = True
        End If

    End Sub

    Private Sub directiveList_DrawSubItem(
    sender As Object,
    e As DrawListViewSubItemEventArgs) _
    Handles directiveList.DrawSubItem

        Dim selected As Boolean =
        e.Item.Selected

        Dim backColor As Color =
        If(
            selected,
            UiColors.Selected,
            UiColors.PanelBackground)

        Dim textColor As Color =
        UiColors.UserText

        Using brush As New SolidBrush(backColor)
            e.Graphics.FillRectangle(
            brush,
            e.Bounds)
        End Using

        TextRenderer.DrawText(
        e.Graphics,
        e.SubItem.Text,
        directiveList.Font,
        e.Bounds,
        textColor,
        TextFormatFlags.Left Or
        TextFormatFlags.VerticalCenter Or
        TextFormatFlags.EndEllipsis)

    End Sub
    Private Sub ReloadList()

        Dim oldIndex As Integer = -1

        If directiveList.SelectedIndices.Count > 0 Then
            oldIndex = directiveList.SelectedIndices(0)
        End If

        directiveList.Items.Clear()

        For Each directive As RowDirective In Directives

            Dim parts =
                GetDisplayParts(directive)

            Dim item As New ListViewItem(parts.Type)

            item.SubItems.Add(parts.Rows)
            item.SubItems.Add(parts.Text)

            directiveList.Items.Add(item)

        Next

        If directiveList.Items.Count = 0 Then
            Return
        End If

        Dim newIndex As Integer =
            Math.Max(
                0,
                Math.Min(
                    oldIndex,
                    directiveList.Items.Count - 1))

        directiveList.Items(newIndex).Selected = True
        directiveList.Items(newIndex).Focused = True

    End Sub

    Private Shared Function GetDisplayParts(
        directive As RowDirective) _
        As (Type As String, Rows As String, Text As String)

        Dim type As String =
            If(directive.DirectiveType, "")

        Dim text As String =
            If(directive.Text, "")

        If type.StartsWith(
            "#COMMENT",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "COMMENT",
                If(directive.Lines.HasValue, directive.Lines.Value.ToString(), ""),
                text)

        End If

        If type.StartsWith(
            "#THEORY,REF",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "THEORY REF",
                "",
                text)

        End If

        If type.StartsWith(
            "#THEORY",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "THEORY",
                ExtractRows(type),
                text)

        End If

        If type.Equals(
            "#",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "Internal comment",
                "",
                text)

        End If

        If type.Equals(
            "+BREAK",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "BREAK",
                "",
                "")

        End If

        If type.Equals(
            "+PAGE",
            StringComparison.OrdinalIgnoreCase) Then

            Return (
                "PAGE",
                "",
                text)

        End If

        Return (
            type,
            "",
            text)

    End Function

    Private Shared Function ExtractRows(
        directiveType As String) As String

        Dim openBracket As Integer =
            directiveType.IndexOf("("c)

        Dim closeBracket As Integer =
            directiveType.IndexOf(")"c)

        If openBracket < 0 OrElse
           closeBracket <= openBracket Then

            Return ""

        End If

        Return directiveType.Substring(
            openBracket + 1,
            closeBracket - openBracket - 1)

    End Function

    Private Function SelectedDirectiveIndex() As Integer

        If directiveList.SelectedIndices.Count = 0 Then
            Return -1
        End If

        Return directiveList.SelectedIndices(0)

    End Function

    Private Sub directiveList_SelectedIndexChanged(
        sender As Object,
        e As EventArgs) _
        Handles directiveList.SelectedIndexChanged

        LoadSelectedDirective()

    End Sub

    Private Sub LoadSelectedDirective()

        Dim selectedIndex As Integer =
            SelectedDirectiveIndex()

        If selectedIndex < 0 OrElse
           selectedIndex >= Directives.Count Then

            Return

        End If

        _loadingSelectedDirective = True

        Try

            DecodeDirectiveForEditing(
                Directives(selectedIndex))

        Finally
            _loadingSelectedDirective = False
        End Try

        UpdateInputState()

    End Sub

    Private Sub DecodeDirectiveForEditing(
        directive As RowDirective)

        rowsTextBox.Text = ""
        textTextBox.Text = directive.Text

        Dim type As String =
            directive.DirectiveType.Trim()

        If type.StartsWith(
            "#COMMENT",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "COMMENT"

            If directive.Lines.HasValue Then
                rowsTextBox.Text = directive.Lines.Value.ToString()
            End If

        ElseIf type.StartsWith(
            "#THEORY,REF",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "THEORY REF"

        ElseIf type.StartsWith(
            "#THEORY",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "THEORY"
            rowsTextBox.Text = ExtractRows(type)

        ElseIf type.Equals(
            "#",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "Internal comment"

        ElseIf type.Equals(
            "+BREAK",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "BREAK"

        ElseIf type.Equals(
            "+PAGE",
            StringComparison.OrdinalIgnoreCase) Then

            typeCombo.SelectedItem = "PAGE"

        Else

            typeCombo.SelectedItem = "COMMENT"

        End If

    End Sub

    Private Sub typeCombo_SelectedIndexChanged(
        sender As Object,
        e As EventArgs) _
        Handles typeCombo.SelectedIndexChanged

        If Not _loadingSelectedDirective Then

            rowsTextBox.Text = ""

            Dim type As String =
                GetSelectedType()

            If type = "PAGE" Then
                textTextBox.Text =
                    _suggestedPageNumber.ToString()
            Else
                textTextBox.Text = ""
            End If

        End If

        UpdateInputState()

    End Sub

    Private Sub addButton_Click(
        sender As Object,
        e As EventArgs) _
        Handles addButton.Click

        AddDirective()

    End Sub

    Private Sub AddDirective()

        Dim directive As RowDirective = Nothing

        If Not TryBuildDirective(directive) Then
            Return
        End If

        Directives.Add(directive)

        ReloadList()

        If directiveList.Items.Count > 0 Then

            Dim index As Integer =
                Directives.Count - 1

            directiveList.Items(index).Selected = True
            directiveList.Items(index).Focused = True

        End If

    End Sub

    Private Sub updateButton_Click(
        sender As Object,
        e As EventArgs) _
        Handles updateButton.Click

        UpdateSelectedDirective()

    End Sub

    Private Sub UpdateSelectedDirective()

        Dim selectedIndex As Integer =
            SelectedDirectiveIndex()

        If selectedIndex < 0 OrElse
           selectedIndex >= Directives.Count Then

            Return

        End If

        Dim directive As RowDirective = Nothing

        If Not TryBuildDirective(directive) Then
            Return
        End If

        Directives(selectedIndex) =
            directive

        ReloadList()

    End Sub

    Private Sub deleteButton_Click(
        sender As Object,
        e As EventArgs) _
        Handles deleteButton.Click

        DeleteSelectedDirective()

    End Sub

    Private Sub DeleteSelectedDirective()

        Dim selectedIndex As Integer =
            SelectedDirectiveIndex()

        If selectedIndex < 0 OrElse
           selectedIndex >= Directives.Count Then

            Return

        End If

        Directives.RemoveAt(
            selectedIndex)

        ReloadList()

        If directiveList.Items.Count = 0 Then

            rowsTextBox.Text = ""
            textTextBox.Text = ""

        End If

    End Sub

    Private Function TryBuildDirective(
        ByRef directive As RowDirective) As Boolean

        directive = Nothing

        Dim selectedType As String =
            GetSelectedType()

        Dim rowsText As String =
            rowsTextBox.Text.Trim()

        Dim text As String =
            textTextBox.Text.Trim()

        If Not ValidateInput(
            selectedType,
            rowsText,
            text) Then

            Return False

        End If

        Dim directiveType As String

        Select Case selectedType

            Case "COMMENT"

                directiveType = "#COMMENT"

            Case "THEORY"

                If String.IsNullOrWhiteSpace(rowsText) Then
                    directiveType = "#THEORY"
                Else
                    directiveType =
                        $"#THEORY({rowsText})"
                End If

            Case "THEORY REF"

                directiveType =
                    "#THEORY,REF"

            Case "Internal comment"

                directiveType = "#"

            Case "BREAK"

                directiveType =
                    "+BREAK"

            Case "PAGE"

                directiveType =
                    "+PAGE"

            Case Else

                directiveType =
                    "#COMMENT"

        End Select

        Dim lines As Integer? = Nothing

        If selectedType = "COMMENT" AndAlso Not String.IsNullOrWhiteSpace(rowsText) Then

            lines = Integer.Parse(rowsText)

        End If

        directive =
            New RowDirective With {
                .RowIndex = _rowIndex,
                .DirectiveType = directiveType,
                .Lines = lines,
                .Text = If(selectedType = "BREAK", "", text)
    }

        Return True

    End Function

    Private Function ValidateInput(
        selectedType As String,
        rowsText As String,
        text As String) As Boolean

        If (selectedType = "BREAK" OrElse
            selectedType = "PAGE") AndAlso
           Not String.IsNullOrWhiteSpace(rowsText) Then

            MessageBox.Show(
                Me,
                "Rows can only be used with COMMENT or THEORY.",
                "Directive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Return False

        End If

        If (selectedType = "COMMENT" OrElse
            selectedType = "THEORY") AndAlso
           Not String.IsNullOrWhiteSpace(rowsText) Then

            Dim rows As Integer

            If Not Integer.TryParse(
                rowsText,
                rows) OrElse
               rows <= 0 Then

                MessageBox.Show(
                    Me,
                    "Rows must be a valid number.",
                    "Directive",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Return False

            End If

        End If

        If selectedType = "PAGE" Then

            Dim pageNumber As Integer

            If Not Integer.TryParse(
                text,
                pageNumber) OrElse
               pageNumber <= 0 Then

                MessageBox.Show(
                    Me,
                    "PAGE requires a valid page number.",
                    "Directive",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Return False

            End If

            Return True

        End If

        If selectedType = "BREAK" Then

            If Not String.IsNullOrWhiteSpace(text) Then

                MessageBox.Show(
                    Me,
                    "BREAK cannot have text.",
                    "Directive",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning)

                Return False

            End If

            Return True

        End If

        If String.IsNullOrWhiteSpace(text) Then

            MessageBox.Show(
                Me,
                "This directive requires text.",
                "Directive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning)

            Return False

        End If

        Return True

    End Function

    Private Function GetSelectedType() As String

        If typeCombo.SelectedItem Is Nothing Then
            Return "COMMENT"
        End If

        Return typeCombo.SelectedItem.ToString()

    End Function

    Private Sub UpdateInputState()

        Dim selectedType As String =
        GetSelectedType()

        Dim usesRows As Boolean =
        selectedType = "COMMENT" OrElse
        selectedType = "THEORY"

        Dim usesText As Boolean =
        selectedType <> "BREAK"

        rowsTextBox.Enabled = usesRows
        textTextBox.Enabled = usesText

        If Not usesRows Then
            rowsTextBox.Text = ""
        End If

        If Not usesText Then
            textTextBox.Text = ""
        End If

        If selectedType = "COMMENT" Then

            _toolTip.SetToolTip(
            rowsTextBox,
            "Optional. Enter the number of following rows this comment also applies to." &
            Environment.NewLine &
            "Leave blank if it applies only to the preceding row.")

            _toolTip.SetToolTip(
            textTextBox,
            "Enter only the comment text." &
            Environment.NewLine &
            "Do not enter #COMMENT or the row count.")

        ElseIf selectedType = "THEORY" Then

            _toolTip.SetToolTip(
            rowsTextBox,
            "Optional. Enter the number of following rows this theory also applies to." &
            Environment.NewLine &
            "Leave blank if it applies only to the preceding row.")

            _toolTip.SetToolTip(
            textTextBox,
            "Enter only the theory text.")

        Else

            _toolTip.SetToolTip(rowsTextBox, "")
            _toolTip.SetToolTip(textTextBox, "")

        End If

    End Sub

End Class