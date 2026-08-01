<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HeaderForm
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        rootLayout = New TableLayoutPanel()
        headerPanel = New Panel()
        headerLogoBox = New PictureBox()
        headerSubtitleLabel = New Label()
        headerTitleLabel = New Label()
        bodyLayout = New TableLayoutPanel()
        batchPanel = New Panel()
        batchLayout = New TableLayoutPanel()
        batchTitleLabel = New Label()
        recordTypeLabel = New Label()
        eventPanel = New FlowLayoutPanel()
        birthsRadioButton = New RadioButton()
        marriagesRadioButton = New RadioButton()
        deathsRadioButton = New RadioButton()
        registrationRow = New TableLayoutPanel()
        yearPanel = New Panel()
        yearTextBox = New TextBox()
        yearLabel = New Label()
        quarterPanel = New Panel()
        quarterComboBox = New ComboBox()
        quarterLabel = New Label()
        pageSourcePanel = New Panel()
        pageSourceComboBox = New ComboBox()
        pageSourceLabel = New Label()
        pageRow = New TableLayoutPanel()
        pagePanel = New Panel()
        pageTextBox = New TextBox()
        pageLabel = New Label()
        suffixPanel = New Panel()
        suffixTextBox = New TextBox()
        suffixLabel = New Label()
        pageLetterPanel = New Panel()
        pageLetterTextBox = New TextBox()
        pageLetterLabel = New Label()
        vnfPanel = New Panel()
        vnfComboBox = New ComboBox()
        vnfLabel = New Label()
        sourceRefPanel = New Panel()
        sourceRefTextBox = New TextBox()
        sourceRefLabel = New Label()
        commentsLabel = New Label()
        commentsTextBox = New TextBox()
        creditLayout = New TableLayoutPanel()
        creditNamePanel = New Panel()
        creditNameTextBox = New TextBox()
        creditNameLabel = New Label()
        creditEmailPanel = New Panel()
        creditEmailTextBox = New TextBox()
        creditEmailLabel = New Label()
        creditTypePanel = New Panel()
        creditTypeTextBox = New TextBox()
        creditTypeLabel = New Label()
        sideLayout = New TableLayoutPanel()
        contributorPanel = New Panel()
        contributorLayout = New TableLayoutPanel()
        contributorTitleLabel = New Label()
        creatorPanel = New Panel()
        creatorTextBox = New TextBox()
        creatorLabel = New Label()
        creatorEmailPanel = New Panel()
        creatorEmailTextBox = New TextBox()
        creatorEmailLabel = New Label()
        syndicatePanel = New Panel()
        syndicateTextBox = New TextBox()
        syndicateLabel = New Label()
        userNamePanel = New Panel()
        userNameTextBox = New TextBox()
        userNameLabel = New Label()
        passwordPanel = New Panel()
        passwordInnerLayout = New TableLayoutPanel()
        userPasswordTextBox = New TextBox()
        showPasswordButton = New Button()
        passwordLabel = New Label()
        rulesPanel = New Panel()
        rulesTextLabel = New Label()
        rulesTitleLabel = New Label()
        footerPanel = New Panel()
        validationSummaryLabel = New Label()
        btnStart = New Button()
        btnCancel = New Button()
        optionsButton = New Button()
        openBatchButton = New Button()
        rootLayout.SuspendLayout()
        headerPanel.SuspendLayout()
        CType(headerLogoBox, ComponentModel.ISupportInitialize).BeginInit()
        bodyLayout.SuspendLayout()
        batchPanel.SuspendLayout()
        batchLayout.SuspendLayout()
        eventPanel.SuspendLayout()
        registrationRow.SuspendLayout()
        yearPanel.SuspendLayout()
        quarterPanel.SuspendLayout()
        pageSourcePanel.SuspendLayout()
        pageRow.SuspendLayout()
        pagePanel.SuspendLayout()
        suffixPanel.SuspendLayout()
        pageLetterPanel.SuspendLayout()
        vnfPanel.SuspendLayout()
        sourceRefPanel.SuspendLayout()
        creditLayout.SuspendLayout()
        creditNamePanel.SuspendLayout()
        creditEmailPanel.SuspendLayout()
        creditTypePanel.SuspendLayout()
        sideLayout.SuspendLayout()
        contributorPanel.SuspendLayout()
        contributorLayout.SuspendLayout()
        creatorPanel.SuspendLayout()
        creatorEmailPanel.SuspendLayout()
        syndicatePanel.SuspendLayout()
        userNamePanel.SuspendLayout()
        passwordPanel.SuspendLayout()
        passwordInnerLayout.SuspendLayout()
        rulesPanel.SuspendLayout()
        footerPanel.SuspendLayout()
        SuspendLayout()
        ' 
        ' rootLayout
        ' 
        rootLayout.BackColor = Color.FromArgb(CByte(239), CByte(243), CByte(248))
        rootLayout.ColumnCount = 1
        rootLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        rootLayout.Controls.Add(headerPanel, 0, 0)
        rootLayout.Controls.Add(bodyLayout, 0, 1)
        rootLayout.Controls.Add(footerPanel, 0, 2)
        rootLayout.Dock = DockStyle.Fill
        rootLayout.Location = New Point(0, 0)
        rootLayout.Margin = New Padding(0)
        rootLayout.Name = "rootLayout"
        rootLayout.Padding = New Padding(24)
        rootLayout.RowCount = 3
        rootLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 94.0F))
        rootLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        rootLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 82.0F))
        rootLayout.Size = New Size(964, 681)
        rootLayout.TabIndex = 0
        ' 
        ' headerPanel
        ' 
        headerPanel.BackColor = Color.White
        headerPanel.BorderStyle = BorderStyle.FixedSingle
        headerPanel.Controls.Add(headerLogoBox)
        headerPanel.Controls.Add(headerSubtitleLabel)
        headerPanel.Controls.Add(headerTitleLabel)
        headerPanel.Dock = DockStyle.Fill
        headerPanel.Location = New Point(24, 24)
        headerPanel.Margin = New Padding(0, 0, 0, 14)
        headerPanel.Name = "headerPanel"
        headerPanel.Size = New Size(916, 80)
        headerPanel.TabIndex = 0
        ' 
        ' headerLogoBox
        ' 
        headerLogoBox.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        headerLogoBox.BackColor = Color.Transparent
        headerLogoBox.Location = New Point(836, 7)
        headerLogoBox.Name = "headerLogoBox"
        headerLogoBox.Size = New Size(66, 66)
        headerLogoBox.SizeMode = PictureBoxSizeMode.Zoom
        headerLogoBox.TabIndex = 2
        headerLogoBox.TabStop = False
        ' 
        ' headerSubtitleLabel
        ' 
        headerSubtitleLabel.AutoSize = True
        headerSubtitleLabel.ForeColor = Color.FromArgb(CByte(102), CByte(113), CByte(127))
        headerSubtitleLabel.Location = New Point(23, 49)
        headerSubtitleLabel.Name = "headerSubtitleLabel"
        headerSubtitleLabel.Size = New Size(372, 15)
        headerSubtitleLabel.TabIndex = 1
        headerSubtitleLabel.Text = "Enter the page and contributor details before opening the workspace."
        ' 
        ' headerTitleLabel
        ' 
        headerTitleLabel.AutoSize = True
        headerTitleLabel.Font = New Font("Segoe UI", 17.0F, FontStyle.Bold)
        headerTitleLabel.ForeColor = Color.FromArgb(CByte(29), CByte(38), CByte(51))
        headerTitleLabel.Location = New Point(20, 12)
        headerTitleLabel.Name = "headerTitleLabel"
        headerTitleLabel.Size = New Size(311, 31)
        headerTitleLabel.TabIndex = 0
        headerTitleLabel.Text = "Set up a transcription batch"
        ' 
        ' bodyLayout
        ' 
        bodyLayout.ColumnCount = 2
        bodyLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 59.0F))
        bodyLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 41.0F))
        bodyLayout.Controls.Add(batchPanel, 0, 0)
        bodyLayout.Controls.Add(sideLayout, 1, 0)
        bodyLayout.Dock = DockStyle.Fill
        bodyLayout.Location = New Point(24, 118)
        bodyLayout.Margin = New Padding(0)
        bodyLayout.Name = "bodyLayout"
        bodyLayout.RowCount = 1
        bodyLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        bodyLayout.Size = New Size(916, 457)
        bodyLayout.TabIndex = 1
        ' 
        ' batchPanel
        ' 
        batchPanel.BackColor = Color.White
        batchPanel.BorderStyle = BorderStyle.FixedSingle
        batchPanel.Controls.Add(batchLayout)
        batchPanel.Dock = DockStyle.Fill
        batchPanel.Location = New Point(0, 0)
        batchPanel.Margin = New Padding(0, 0, 10, 0)
        batchPanel.Name = "batchPanel"
        batchPanel.Padding = New Padding(18)
        batchPanel.Size = New Size(530, 457)
        batchPanel.TabIndex = 0
        ' 
        ' batchLayout
        ' 
        batchLayout.ColumnCount = 1
        batchLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        batchLayout.Controls.Add(batchTitleLabel, 0, 0)
        batchLayout.Controls.Add(recordTypeLabel, 0, 1)
        batchLayout.Controls.Add(eventPanel, 0, 2)
        batchLayout.Controls.Add(registrationRow, 0, 3)
        batchLayout.Controls.Add(pageRow, 0, 4)
        batchLayout.Controls.Add(sourceRefPanel, 0, 5)
        batchLayout.Controls.Add(commentsLabel, 0, 6)
        batchLayout.Controls.Add(commentsTextBox, 0, 7)
        batchLayout.Controls.Add(creditLayout, 0, 8)
        batchLayout.Dock = DockStyle.Fill
        batchLayout.Location = New Point(18, 18)
        batchLayout.Margin = New Padding(0)
        batchLayout.Name = "batchLayout"
        batchLayout.RowCount = 9
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 20.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 39.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 59.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 20.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        batchLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
        batchLayout.Size = New Size(492, 419)
        batchLayout.TabIndex = 0
        ' 
        ' batchTitleLabel
        ' 
        batchTitleLabel.Dock = DockStyle.Fill
        batchTitleLabel.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        batchTitleLabel.ForeColor = Color.FromArgb(CByte(31), CByte(39), CByte(51))
        batchTitleLabel.Location = New Point(0, 0)
        batchTitleLabel.Margin = New Padding(0)
        batchTitleLabel.Name = "batchTitleLabel"
        batchTitleLabel.Size = New Size(492, 34)
        batchTitleLabel.TabIndex = 0
        batchTitleLabel.Text = "Batch details"
        batchTitleLabel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' recordTypeLabel
        ' 
        recordTypeLabel.Dock = DockStyle.Fill
        recordTypeLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        recordTypeLabel.Location = New Point(3, 34)
        recordTypeLabel.Name = "recordTypeLabel"
        recordTypeLabel.Size = New Size(486, 20)
        recordTypeLabel.TabIndex = 1
        recordTypeLabel.Text = "Record type"
        recordTypeLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' eventPanel
        ' 
        eventPanel.Controls.Add(birthsRadioButton)
        eventPanel.Controls.Add(marriagesRadioButton)
        eventPanel.Controls.Add(deathsRadioButton)
        eventPanel.Dock = DockStyle.Fill
        eventPanel.Location = New Point(0, 54)
        eventPanel.Margin = New Padding(0)
        eventPanel.Name = "eventPanel"
        eventPanel.Size = New Size(492, 39)
        eventPanel.TabIndex = 2
        eventPanel.WrapContents = False
        ' 
        ' birthsRadioButton
        ' 
        birthsRadioButton.AutoSize = True
        birthsRadioButton.Checked = True
        birthsRadioButton.Location = New Point(3, 8)
        birthsRadioButton.Margin = New Padding(3, 8, 18, 0)
        birthsRadioButton.Name = "birthsRadioButton"
        birthsRadioButton.Size = New Size(55, 19)
        birthsRadioButton.TabIndex = 0
        birthsRadioButton.TabStop = True
        birthsRadioButton.Text = "Births"
        birthsRadioButton.UseVisualStyleBackColor = True
        ' 
        ' marriagesRadioButton
        ' 
        marriagesRadioButton.AutoSize = True
        marriagesRadioButton.Location = New Point(79, 8)
        marriagesRadioButton.Margin = New Padding(3, 8, 18, 0)
        marriagesRadioButton.Name = "marriagesRadioButton"
        marriagesRadioButton.Size = New Size(77, 19)
        marriagesRadioButton.TabIndex = 1
        marriagesRadioButton.Text = "Marriages"
        marriagesRadioButton.UseVisualStyleBackColor = True
        ' 
        ' deathsRadioButton
        ' 
        deathsRadioButton.AutoSize = True
        deathsRadioButton.Location = New Point(177, 8)
        deathsRadioButton.Margin = New Padding(3, 8, 18, 0)
        deathsRadioButton.Name = "deathsRadioButton"
        deathsRadioButton.Size = New Size(61, 19)
        deathsRadioButton.TabIndex = 2
        deathsRadioButton.Text = "Deaths"
        deathsRadioButton.UseVisualStyleBackColor = True
        ' 
        ' registrationRow
        ' 
        registrationRow.ColumnCount = 3
        registrationRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 38.0F))
        registrationRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 28.0F))
        registrationRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 34.0F))
        registrationRow.Controls.Add(yearPanel, 0, 0)
        registrationRow.Controls.Add(quarterPanel, 1, 0)
        registrationRow.Controls.Add(pageSourcePanel, 2, 0)
        registrationRow.Dock = DockStyle.Fill
        registrationRow.Location = New Point(0, 93)
        registrationRow.Margin = New Padding(0)
        registrationRow.Name = "registrationRow"
        registrationRow.RowCount = 1
        registrationRow.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        registrationRow.Size = New Size(492, 62)
        registrationRow.TabIndex = 3
        ' 
        ' yearPanel
        ' 
        yearPanel.Controls.Add(yearTextBox)
        yearPanel.Controls.Add(yearLabel)
        yearPanel.Dock = DockStyle.Fill
        yearPanel.Location = New Point(0, 0)
        yearPanel.Margin = New Padding(0, 0, 10, 5)
        yearPanel.Name = "yearPanel"
        yearPanel.Size = New Size(176, 57)
        yearPanel.TabIndex = 0
        ' 
        ' yearTextBox
        ' 
        yearTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        yearTextBox.BorderStyle = BorderStyle.FixedSingle
        yearTextBox.Location = New Point(0, 24)
        yearTextBox.Name = "yearTextBox"
        yearTextBox.Size = New Size(176, 23)
        yearTextBox.TabIndex = 1
        ' 
        ' yearLabel
        ' 
        yearLabel.Dock = DockStyle.Top
        yearLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        yearLabel.Location = New Point(0, 0)
        yearLabel.Name = "yearLabel"
        yearLabel.Size = New Size(176, 19)
        yearLabel.TabIndex = 0
        yearLabel.Text = "Year"
        yearLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' quarterPanel
        ' 
        quarterPanel.Controls.Add(quarterComboBox)
        quarterPanel.Controls.Add(quarterLabel)
        quarterPanel.Dock = DockStyle.Fill
        quarterPanel.Location = New Point(186, 0)
        quarterPanel.Margin = New Padding(0, 0, 10, 5)
        quarterPanel.Name = "quarterPanel"
        quarterPanel.Size = New Size(127, 57)
        quarterPanel.TabIndex = 1
        ' 
        ' quarterComboBox
        ' 
        quarterComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        quarterComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        quarterComboBox.FormattingEnabled = True
        quarterComboBox.Items.AddRange(New Object() {"March", "June", "September", "December"})
        quarterComboBox.Location = New Point(0, 24)
        quarterComboBox.Name = "quarterComboBox"
        quarterComboBox.Size = New Size(127, 23)
        quarterComboBox.TabIndex = 1
        ' 
        ' quarterLabel
        ' 
        quarterLabel.Dock = DockStyle.Top
        quarterLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        quarterLabel.Location = New Point(0, 0)
        quarterLabel.Name = "quarterLabel"
        quarterLabel.Size = New Size(127, 19)
        quarterLabel.TabIndex = 0
        quarterLabel.Text = "Quarter"
        quarterLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' pageSourcePanel
        ' 
        pageSourcePanel.Controls.Add(pageSourceComboBox)
        pageSourcePanel.Controls.Add(pageSourceLabel)
        pageSourcePanel.Dock = DockStyle.Fill
        pageSourcePanel.Location = New Point(323, 0)
        pageSourcePanel.Margin = New Padding(0, 0, 0, 5)
        pageSourcePanel.Name = "pageSourcePanel"
        pageSourcePanel.Size = New Size(169, 57)
        pageSourcePanel.TabIndex = 2
        ' 
        ' pageSourceComboBox
        ' 
        pageSourceComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pageSourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        pageSourceComboBox.FormattingEnabled = True
        pageSourceComboBox.Items.AddRange(New Object() {"Scan", "Fiche", "Film"})
        pageSourceComboBox.Location = New Point(0, 24)
        pageSourceComboBox.Name = "pageSourceComboBox"
        pageSourceComboBox.Size = New Size(169, 23)
        pageSourceComboBox.TabIndex = 1
        ' 
        ' pageSourceLabel
        ' 
        pageSourceLabel.Dock = DockStyle.Top
        pageSourceLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        pageSourceLabel.Location = New Point(0, 0)
        pageSourceLabel.Name = "pageSourceLabel"
        pageSourceLabel.Size = New Size(169, 19)
        pageSourceLabel.TabIndex = 0
        pageSourceLabel.Text = "Page source"
        pageSourceLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' pageRow
        ' 
        pageRow.ColumnCount = 4
        pageRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        pageRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20.0F))
        pageRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25.0F))
        pageRow.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 30.0F))
        pageRow.Controls.Add(pagePanel, 0, 0)
        pageRow.Controls.Add(suffixPanel, 1, 0)
        pageRow.Controls.Add(pageLetterPanel, 2, 0)
        pageRow.Controls.Add(vnfPanel, 3, 0)
        pageRow.Dock = DockStyle.Fill
        pageRow.Location = New Point(0, 155)
        pageRow.Margin = New Padding(0)
        pageRow.Name = "pageRow"
        pageRow.RowCount = 1
        pageRow.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        pageRow.Size = New Size(492, 62)
        pageRow.TabIndex = 4
        ' 
        ' pagePanel
        ' 
        pagePanel.Controls.Add(pageTextBox)
        pagePanel.Controls.Add(pageLabel)
        pagePanel.Dock = DockStyle.Fill
        pagePanel.Location = New Point(0, 0)
        pagePanel.Margin = New Padding(0, 0, 10, 5)
        pagePanel.Name = "pagePanel"
        pagePanel.Size = New Size(113, 57)
        pagePanel.TabIndex = 0
        ' 
        ' pageTextBox
        ' 
        pageTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pageTextBox.BorderStyle = BorderStyle.FixedSingle
        pageTextBox.Location = New Point(0, 24)
        pageTextBox.Name = "pageTextBox"
        pageTextBox.Size = New Size(113, 23)
        pageTextBox.TabIndex = 1
        pageTextBox.TextAlign = HorizontalAlignment.Center
        ' 
        ' pageLabel
        ' 
        pageLabel.Dock = DockStyle.Top
        pageLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        pageLabel.Location = New Point(0, 0)
        pageLabel.Name = "pageLabel"
        pageLabel.Size = New Size(113, 19)
        pageLabel.TabIndex = 0
        pageLabel.Text = "Page"
        pageLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' suffixPanel
        ' 
        suffixPanel.Controls.Add(suffixTextBox)
        suffixPanel.Controls.Add(suffixLabel)
        suffixPanel.Dock = DockStyle.Fill
        suffixPanel.Location = New Point(123, 0)
        suffixPanel.Margin = New Padding(0, 0, 10, 5)
        suffixPanel.Name = "suffixPanel"
        suffixPanel.Size = New Size(88, 57)
        suffixPanel.TabIndex = 1
        ' 
        ' suffixTextBox
        ' 
        suffixTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        suffixTextBox.BorderStyle = BorderStyle.FixedSingle
        suffixTextBox.Location = New Point(0, 24)
        suffixTextBox.MaxLength = 1
        suffixTextBox.Name = "suffixTextBox"
        suffixTextBox.Size = New Size(88, 23)
        suffixTextBox.TabIndex = 1
        suffixTextBox.TextAlign = HorizontalAlignment.Center
        ' 
        ' suffixLabel
        ' 
        suffixLabel.Dock = DockStyle.Top
        suffixLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        suffixLabel.Location = New Point(0, 0)
        suffixLabel.Name = "suffixLabel"
        suffixLabel.Size = New Size(88, 19)
        suffixLabel.TabIndex = 0
        suffixLabel.Text = "Suffix"
        suffixLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' pageLetterPanel
        ' 
        pageLetterPanel.Controls.Add(pageLetterTextBox)
        pageLetterPanel.Controls.Add(pageLetterLabel)
        pageLetterPanel.Dock = DockStyle.Fill
        pageLetterPanel.Location = New Point(221, 0)
        pageLetterPanel.Margin = New Padding(0, 0, 10, 5)
        pageLetterPanel.Name = "pageLetterPanel"
        pageLetterPanel.Size = New Size(113, 57)
        pageLetterPanel.TabIndex = 2
        ' 
        ' pageLetterTextBox
        ' 
        pageLetterTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        pageLetterTextBox.BorderStyle = BorderStyle.FixedSingle
        pageLetterTextBox.Location = New Point(0, 24)
        pageLetterTextBox.MaxLength = 1
        pageLetterTextBox.Name = "pageLetterTextBox"
        pageLetterTextBox.Size = New Size(113, 23)
        pageLetterTextBox.TabIndex = 1
        pageLetterTextBox.TextAlign = HorizontalAlignment.Center
        ' 
        ' pageLetterLabel
        ' 
        pageLetterLabel.Dock = DockStyle.Top
        pageLetterLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        pageLetterLabel.Location = New Point(0, 0)
        pageLetterLabel.Name = "pageLetterLabel"
        pageLetterLabel.Size = New Size(113, 19)
        pageLetterLabel.TabIndex = 0
        pageLetterLabel.Text = "Page letter"
        pageLetterLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' vnfPanel
        ' 
        vnfPanel.Controls.Add(vnfComboBox)
        vnfPanel.Controls.Add(vnfLabel)
        vnfPanel.Dock = DockStyle.Fill
        vnfPanel.Location = New Point(344, 0)
        vnfPanel.Margin = New Padding(0, 0, 0, 5)
        vnfPanel.Name = "vnfPanel"
        vnfPanel.Size = New Size(148, 57)
        vnfPanel.TabIndex = 3
        ' 
        ' vnfComboBox
        ' 
        vnfComboBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        vnfComboBox.DropDownStyle = ComboBoxStyle.DropDownList
        vnfComboBox.FormattingEnabled = True
        vnfComboBox.Items.AddRange(New Object() {"9Z", "99", "XX", "999"})
        vnfComboBox.Location = New Point(0, 24)
        vnfComboBox.Name = "vnfComboBox"
        vnfComboBox.Size = New Size(148, 23)
        vnfComboBox.TabIndex = 1
        ' 
        ' vnfLabel
        ' 
        vnfLabel.Dock = DockStyle.Top
        vnfLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        vnfLabel.Location = New Point(0, 0)
        vnfLabel.Name = "vnfLabel"
        vnfLabel.Size = New Size(148, 19)
        vnfLabel.TabIndex = 0
        vnfLabel.Text = "Volume format"
        vnfLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' sourceRefPanel
        ' 
        sourceRefPanel.Controls.Add(sourceRefTextBox)
        sourceRefPanel.Controls.Add(sourceRefLabel)
        sourceRefPanel.Dock = DockStyle.Fill
        sourceRefPanel.Location = New Point(0, 217)
        sourceRefPanel.Margin = New Padding(0, 0, 0, 5)
        sourceRefPanel.Name = "sourceRefPanel"
        sourceRefPanel.Size = New Size(492, 54)
        sourceRefPanel.TabIndex = 5
        ' 
        ' sourceRefTextBox
        ' 
        sourceRefTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        sourceRefTextBox.BorderStyle = BorderStyle.FixedSingle
        sourceRefTextBox.Location = New Point(0, 24)
        sourceRefTextBox.Name = "sourceRefTextBox"
        sourceRefTextBox.Size = New Size(492, 23)
        sourceRefTextBox.TabIndex = 1
        ' 
        ' sourceRefLabel
        ' 
        sourceRefLabel.Dock = DockStyle.Top
        sourceRefLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        sourceRefLabel.Location = New Point(0, 0)
        sourceRefLabel.Name = "sourceRefLabel"
        sourceRefLabel.Size = New Size(492, 19)
        sourceRefLabel.TabIndex = 0
        sourceRefLabel.Text = "Source reference (optional)"
        sourceRefLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' commentsLabel
        ' 
        commentsLabel.Dock = DockStyle.Fill
        commentsLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        commentsLabel.Location = New Point(3, 276)
        commentsLabel.Name = "commentsLabel"
        commentsLabel.Size = New Size(486, 20)
        commentsLabel.TabIndex = 6
        commentsLabel.Text = "Comments"
        commentsLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' commentsTextBox
        ' 
        commentsTextBox.BorderStyle = BorderStyle.FixedSingle
        commentsTextBox.Dock = DockStyle.Fill
        commentsTextBox.Location = New Point(3, 299)
        commentsTextBox.Multiline = True
        commentsTextBox.Name = "commentsTextBox"
        commentsTextBox.ScrollBars = ScrollBars.Vertical
        commentsTextBox.Size = New Size(486, 55)
        commentsTextBox.TabIndex = 7
        ' 
        ' creditLayout
        ' 
        creditLayout.ColumnCount = 3
        creditLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33333F))
        creditLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33333F))
        creditLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33333F))
        creditLayout.Controls.Add(creditNamePanel, 0, 0)
        creditLayout.Controls.Add(creditEmailPanel, 1, 0)
        creditLayout.Controls.Add(creditTypePanel, 2, 0)
        creditLayout.Dock = DockStyle.Fill
        creditLayout.Location = New Point(0, 357)
        creditLayout.Margin = New Padding(0)
        creditLayout.Name = "creditLayout"
        creditLayout.RowCount = 1
        creditLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        creditLayout.Size = New Size(492, 62)
        creditLayout.TabIndex = 8
        ' 
        ' creditNamePanel
        ' 
        creditNamePanel.Controls.Add(creditNameTextBox)
        creditNamePanel.Controls.Add(creditNameLabel)
        creditNamePanel.Dock = DockStyle.Fill
        creditNamePanel.Location = New Point(0, 0)
        creditNamePanel.Margin = New Padding(0, 0, 10, 0)
        creditNamePanel.Name = "creditNamePanel"
        creditNamePanel.Size = New Size(154, 62)
        creditNamePanel.TabIndex = 0
        ' 
        ' creditNameTextBox
        ' 
        creditNameTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        creditNameTextBox.BorderStyle = BorderStyle.FixedSingle
        creditNameTextBox.Location = New Point(0, 24)
        creditNameTextBox.Name = "creditNameTextBox"
        creditNameTextBox.Size = New Size(154, 23)
        creditNameTextBox.TabIndex = 1
        ' 
        ' creditNameLabel
        ' 
        creditNameLabel.Dock = DockStyle.Top
        creditNameLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        creditNameLabel.Location = New Point(0, 0)
        creditNameLabel.Name = "creditNameLabel"
        creditNameLabel.Size = New Size(154, 19)
        creditNameLabel.TabIndex = 0
        creditNameLabel.Text = "Credit name"
        creditNameLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' creditEmailPanel
        ' 
        creditEmailPanel.Controls.Add(creditEmailTextBox)
        creditEmailPanel.Controls.Add(creditEmailLabel)
        creditEmailPanel.Dock = DockStyle.Fill
        creditEmailPanel.Location = New Point(164, 0)
        creditEmailPanel.Margin = New Padding(0, 0, 10, 0)
        creditEmailPanel.Name = "creditEmailPanel"
        creditEmailPanel.Size = New Size(154, 62)
        creditEmailPanel.TabIndex = 1
        ' 
        ' creditEmailTextBox
        ' 
        creditEmailTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        creditEmailTextBox.BorderStyle = BorderStyle.FixedSingle
        creditEmailTextBox.Location = New Point(0, 24)
        creditEmailTextBox.Name = "creditEmailTextBox"
        creditEmailTextBox.Size = New Size(154, 23)
        creditEmailTextBox.TabIndex = 1
        ' 
        ' creditEmailLabel
        ' 
        creditEmailLabel.Dock = DockStyle.Top
        creditEmailLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        creditEmailLabel.Location = New Point(0, 0)
        creditEmailLabel.Name = "creditEmailLabel"
        creditEmailLabel.Size = New Size(154, 19)
        creditEmailLabel.TabIndex = 0
        creditEmailLabel.Text = "Credit email"
        creditEmailLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' creditTypePanel
        ' 
        creditTypePanel.Controls.Add(creditTypeTextBox)
        creditTypePanel.Controls.Add(creditTypeLabel)
        creditTypePanel.Dock = DockStyle.Fill
        creditTypePanel.Location = New Point(328, 0)
        creditTypePanel.Margin = New Padding(0)
        creditTypePanel.Name = "creditTypePanel"
        creditTypePanel.Size = New Size(164, 62)
        creditTypePanel.TabIndex = 2
        ' 
        ' creditTypeTextBox
        ' 
        creditTypeTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        creditTypeTextBox.BorderStyle = BorderStyle.FixedSingle
        creditTypeTextBox.Location = New Point(0, 24)
        creditTypeTextBox.Name = "creditTypeTextBox"
        creditTypeTextBox.Size = New Size(164, 23)
        creditTypeTextBox.TabIndex = 1
        ' 
        ' creditTypeLabel
        ' 
        creditTypeLabel.Dock = DockStyle.Top
        creditTypeLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        creditTypeLabel.Location = New Point(0, 0)
        creditTypeLabel.Name = "creditTypeLabel"
        creditTypeLabel.Size = New Size(164, 19)
        creditTypeLabel.TabIndex = 0
        creditTypeLabel.Text = "Credit type"
        creditTypeLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' sideLayout
        ' 
        sideLayout.ColumnCount = 1
        sideLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        sideLayout.Controls.Add(contributorPanel, 0, 0)
        sideLayout.Controls.Add(rulesPanel, 0, 1)
        sideLayout.Dock = DockStyle.Fill
        sideLayout.Location = New Point(550, 0)
        sideLayout.Margin = New Padding(10, 0, 0, 0)
        sideLayout.Name = "sideLayout"
        sideLayout.RowCount = 2
        sideLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 375.0F))
        sideLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        sideLayout.Size = New Size(366, 457)
        sideLayout.TabIndex = 1
        ' 
        ' contributorPanel
        ' 
        contributorPanel.BackColor = Color.White
        contributorPanel.BorderStyle = BorderStyle.FixedSingle
        contributorPanel.Controls.Add(contributorLayout)
        contributorPanel.Dock = DockStyle.Fill
        contributorPanel.Location = New Point(0, 0)
        contributorPanel.Margin = New Padding(0, 0, 0, 14)
        contributorPanel.Name = "contributorPanel"
        contributorPanel.Padding = New Padding(16)
        contributorPanel.Size = New Size(366, 326)
        contributorPanel.TabIndex = 0
        ' 
        ' contributorLayout
        ' 
        contributorLayout.ColumnCount = 1
        contributorLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        contributorLayout.Controls.Add(contributorTitleLabel, 0, 0)
        contributorLayout.Controls.Add(creatorPanel, 0, 1)
        contributorLayout.Controls.Add(creatorEmailPanel, 0, 2)
        contributorLayout.Controls.Add(syndicatePanel, 0, 3)
        contributorLayout.Controls.Add(userNamePanel, 0, 4)
        contributorLayout.Controls.Add(passwordPanel, 0, 5)
        contributorLayout.Dock = DockStyle.Fill
        contributorLayout.Location = New Point(16, 16)
        contributorLayout.Margin = New Padding(0)
        contributorLayout.Name = "contributorLayout"
        contributorLayout.RowCount = 6
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55.0F))
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55.0F))
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55.0F))
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55.0F))
        contributorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62.0F))
        contributorLayout.Size = New Size(332, 292)
        contributorLayout.TabIndex = 0
        ' 
        ' contributorTitleLabel
        ' 
        contributorTitleLabel.Dock = DockStyle.Fill
        contributorTitleLabel.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        contributorTitleLabel.ForeColor = Color.FromArgb(CByte(31), CByte(39), CByte(51))
        contributorTitleLabel.Location = New Point(0, 0)
        contributorTitleLabel.Margin = New Padding(0)
        contributorTitleLabel.Name = "contributorTitleLabel"
        contributorTitleLabel.Size = New Size(332, 34)
        contributorTitleLabel.TabIndex = 0
        contributorTitleLabel.Text = "Contributor details"
        contributorTitleLabel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' creatorPanel
        ' 
        creatorPanel.Controls.Add(creatorTextBox)
        creatorPanel.Controls.Add(creatorLabel)
        creatorPanel.Dock = DockStyle.Fill
        creatorPanel.Location = New Point(0, 34)
        creatorPanel.Margin = New Padding(0, 0, 0, 5)
        creatorPanel.Name = "creatorPanel"
        creatorPanel.Size = New Size(332, 50)
        creatorPanel.TabIndex = 1
        ' 
        ' creatorTextBox
        ' 
        creatorTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        creatorTextBox.BorderStyle = BorderStyle.FixedSingle
        creatorTextBox.Location = New Point(0, 24)
        creatorTextBox.Name = "creatorTextBox"
        creatorTextBox.Size = New Size(332, 23)
        creatorTextBox.TabIndex = 1
        ' 
        ' creatorLabel
        ' 
        creatorLabel.Dock = DockStyle.Top
        creatorLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        creatorLabel.Location = New Point(0, 0)
        creatorLabel.Name = "creatorLabel"
        creatorLabel.Size = New Size(332, 19)
        creatorLabel.TabIndex = 0
        creatorLabel.Text = "Original creator"
        creatorLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' creatorEmailPanel
        ' 
        creatorEmailPanel.Controls.Add(creatorEmailTextBox)
        creatorEmailPanel.Controls.Add(creatorEmailLabel)
        creatorEmailPanel.Dock = DockStyle.Fill
        creatorEmailPanel.Location = New Point(0, 89)
        creatorEmailPanel.Margin = New Padding(0, 0, 0, 5)
        creatorEmailPanel.Name = "creatorEmailPanel"
        creatorEmailPanel.Size = New Size(332, 50)
        creatorEmailPanel.TabIndex = 2
        ' 
        ' creatorEmailTextBox
        ' 
        creatorEmailTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        creatorEmailTextBox.BorderStyle = BorderStyle.FixedSingle
        creatorEmailTextBox.Location = New Point(0, 24)
        creatorEmailTextBox.Name = "creatorEmailTextBox"
        creatorEmailTextBox.Size = New Size(332, 23)
        creatorEmailTextBox.TabIndex = 1
        ' 
        ' creatorEmailLabel
        ' 
        creatorEmailLabel.Dock = DockStyle.Top
        creatorEmailLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        creatorEmailLabel.Location = New Point(0, 0)
        creatorEmailLabel.Name = "creatorEmailLabel"
        creatorEmailLabel.Size = New Size(332, 19)
        creatorEmailLabel.TabIndex = 0
        creatorEmailLabel.Text = "Creator email"
        creatorEmailLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' syndicatePanel
        ' 
        syndicatePanel.Controls.Add(syndicateTextBox)
        syndicatePanel.Controls.Add(syndicateLabel)
        syndicatePanel.Dock = DockStyle.Fill
        syndicatePanel.Location = New Point(0, 144)
        syndicatePanel.Margin = New Padding(0, 0, 0, 5)
        syndicatePanel.Name = "syndicatePanel"
        syndicatePanel.Size = New Size(332, 50)
        syndicatePanel.TabIndex = 3
        ' 
        ' syndicateTextBox
        ' 
        syndicateTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        syndicateTextBox.BorderStyle = BorderStyle.FixedSingle
        syndicateTextBox.Location = New Point(0, 24)
        syndicateTextBox.Name = "syndicateTextBox"
        syndicateTextBox.Size = New Size(332, 23)
        syndicateTextBox.TabIndex = 1
        ' 
        ' syndicateLabel
        ' 
        syndicateLabel.Dock = DockStyle.Top
        syndicateLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        syndicateLabel.Location = New Point(0, 0)
        syndicateLabel.Name = "syndicateLabel"
        syndicateLabel.Size = New Size(332, 19)
        syndicateLabel.TabIndex = 0
        syndicateLabel.Text = "Syndicate"
        syndicateLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' userNamePanel
        ' 
        userNamePanel.Controls.Add(userNameTextBox)
        userNamePanel.Controls.Add(userNameLabel)
        userNamePanel.Dock = DockStyle.Fill
        userNamePanel.Location = New Point(0, 199)
        userNamePanel.Margin = New Padding(0, 0, 0, 5)
        userNamePanel.Name = "userNamePanel"
        userNamePanel.Size = New Size(332, 50)
        userNamePanel.TabIndex = 4
        ' 
        ' userNameTextBox
        ' 
        userNameTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        userNameTextBox.BorderStyle = BorderStyle.FixedSingle
        userNameTextBox.Location = New Point(0, 24)
        userNameTextBox.Name = "userNameTextBox"
        userNameTextBox.Size = New Size(332, 23)
        userNameTextBox.TabIndex = 1
        ' 
        ' userNameLabel
        ' 
        userNameLabel.Dock = DockStyle.Top
        userNameLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        userNameLabel.Location = New Point(0, 0)
        userNameLabel.Name = "userNameLabel"
        userNameLabel.Size = New Size(332, 19)
        userNameLabel.TabIndex = 0
        userNameLabel.Text = "Current FreeBMD User ID"
        userNameLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' passwordPanel
        ' 
        passwordPanel.Controls.Add(passwordInnerLayout)
        passwordPanel.Controls.Add(passwordLabel)
        passwordPanel.Dock = DockStyle.Fill
        passwordPanel.Location = New Point(0, 254)
        passwordPanel.Margin = New Padding(0)
        passwordPanel.Name = "passwordPanel"
        passwordPanel.Size = New Size(332, 62)
        passwordPanel.TabIndex = 5
        ' 
        ' passwordInnerLayout
        ' 
        passwordInnerLayout.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        passwordInnerLayout.ColumnCount = 2
        passwordInnerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        passwordInnerLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 40.0F))
        passwordInnerLayout.Controls.Add(userPasswordTextBox, 0, 0)
        passwordInnerLayout.Controls.Add(showPasswordButton, 1, 0)
        passwordInnerLayout.Location = New Point(0, 24)
        passwordInnerLayout.Margin = New Padding(0)
        passwordInnerLayout.Name = "passwordInnerLayout"
        passwordInnerLayout.RowCount = 1
        passwordInnerLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))
        passwordInnerLayout.Size = New Size(332, 30)
        passwordInnerLayout.TabIndex = 1
        ' 
        ' userPasswordTextBox
        ' 
        userPasswordTextBox.BorderStyle = BorderStyle.FixedSingle
        userPasswordTextBox.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        userPasswordTextBox.Location = New Point(0, 1)
        userPasswordTextBox.Margin = New Padding(0)
        userPasswordTextBox.Size = New Size(292, 23)
        userPasswordTextBox.Name = "userPasswordTextBox"
        userPasswordTextBox.Size = New Size(292, 23)
        userPasswordTextBox.TabIndex = 0
        userPasswordTextBox.UseSystemPasswordChar = True
        ' 
        ' showPasswordButton
        ' 
        showPasswordButton.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        showPasswordButton.Height = 25
        showPasswordButton.FlatStyle = FlatStyle.Flat
        showPasswordButton.Location = New Point(292, 0)
        showPasswordButton.Margin = New Padding(0)
        showPasswordButton.Name = "showPasswordButton"
        showPasswordButton.Size = New Size(40, 23)
        showPasswordButton.TabIndex = 1
        showPasswordButton.TabStop = False
        showPasswordButton.Text = "👁"
        showPasswordButton.UseVisualStyleBackColor = True
        ' 
        ' passwordLabel
        ' 
        passwordLabel.Dock = DockStyle.Top
        passwordLabel.ForeColor = Color.FromArgb(CByte(95), CByte(104), CByte(117))
        passwordLabel.Location = New Point(0, 0)
        passwordLabel.Name = "passwordLabel"
        passwordLabel.Size = New Size(332, 19)
        passwordLabel.TabIndex = 0
        passwordLabel.Text = "Password"
        passwordLabel.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' rulesPanel
        ' 
        rulesPanel.BackColor = Color.FromArgb(CByte(255), CByte(248), CByte(230))
        rulesPanel.BorderStyle = BorderStyle.FixedSingle
        rulesPanel.Controls.Add(rulesTextLabel)
        rulesPanel.Controls.Add(rulesTitleLabel)
        rulesPanel.Dock = DockStyle.Fill
        rulesPanel.Location = New Point(0, 340)
        rulesPanel.Margin = New Padding(0)
        rulesPanel.Name = "rulesPanel"
        rulesPanel.Padding = New Padding(16)
        rulesPanel.Size = New Size(366, 117)
        rulesPanel.TabIndex = 1
        ' 
        ' rulesTextLabel
        ' 
        rulesTextLabel.Dock = DockStyle.Fill
        rulesTextLabel.ForeColor = Color.FromArgb(CByte(116), CByte(90), CByte(34))
        rulesTextLabel.Location = New Point(16, 40)
        rulesTextLabel.Name = "rulesTextLabel"
        rulesTextLabel.Size = New Size(332, 59)
        rulesTextLabel.TabIndex = 1
        rulesTextLabel.Text = "Quarter is required before 1984. Volume format is suggested from the record type, year and quarter."
        ' 
        ' rulesTitleLabel
        ' 
        rulesTitleLabel.Dock = DockStyle.Top
        rulesTitleLabel.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        rulesTitleLabel.ForeColor = Color.FromArgb(CByte(116), CByte(90), CByte(34))
        rulesTitleLabel.Location = New Point(16, 16)
        rulesTitleLabel.Name = "rulesTitleLabel"
        rulesTitleLabel.Size = New Size(332, 24)
        rulesTitleLabel.TabIndex = 0
        rulesTitleLabel.Text = "Helpful information"
        ' 
        ' footerPanel
        ' 
        footerPanel.BackColor = Color.White
        footerPanel.BorderStyle = BorderStyle.FixedSingle
        footerPanel.Controls.Add(validationSummaryLabel)
        footerPanel.Controls.Add(btnStart)
        footerPanel.Controls.Add(btnCancel)
        footerPanel.Controls.Add(optionsButton)
        footerPanel.Controls.Add(openBatchButton)
        footerPanel.Dock = DockStyle.Fill
        footerPanel.Location = New Point(24, 589)
        footerPanel.Margin = New Padding(0, 14, 0, 0)
        footerPanel.Name = "footerPanel"
        footerPanel.Size = New Size(916, 68)
        footerPanel.TabIndex = 2
        ' 
        ' validationSummaryLabel
        ' 
        validationSummaryLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        validationSummaryLabel.ForeColor = Color.FromArgb(CByte(130), CByte(75), CByte(20))
        validationSummaryLabel.Location = New Point(276, 7)
        validationSummaryLabel.Name = "validationSummaryLabel"
        validationSummaryLabel.Size = New Size(416, 52)
        validationSummaryLabel.TabIndex = 2
        validationSummaryLabel.Text = "To continue: complete the required fields."
        validationSummaryLabel.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnStart
        ' 
        btnStart.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnStart.BackColor = Color.FromArgb(CByte(46), CByte(105), CByte(180))
        btnStart.Enabled = False
        btnStart.FlatAppearance.BorderSize = 0
        btnStart.FlatStyle = FlatStyle.Flat
        btnStart.ForeColor = Color.White
        btnStart.Location = New Point(806, 16)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(94, 36)
        btnStart.TabIndex = 4
        btnStart.Text = "Start batch"
        btnStart.UseVisualStyleBackColor = False
        ' 
        ' btnCancel
        ' 
        btnCancel.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Location = New Point(708, 16)
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(88, 36)
        btnCancel.TabIndex = 3
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        ' 
        ' optionsButton
        ' 
        optionsButton.Location = New Point(168, 16)
        optionsButton.Name = "optionsButton"
        optionsButton.Size = New Size(92, 36)
        optionsButton.TabIndex = 1
        optionsButton.Text = "Options"
        optionsButton.UseVisualStyleBackColor = True
        ' 
        ' openBatchButton
        ' 
        openBatchButton.Location = New Point(16, 16)
        openBatchButton.Name = "openBatchButton"
        openBatchButton.Size = New Size(142, 36)
        openBatchButton.TabIndex = 0
        openBatchButton.Text = "Open existing batch"
        openBatchButton.UseVisualStyleBackColor = True
        ' 
        ' HeaderForm
        ' 
        AcceptButton = btnStart
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(239), CByte(243), CByte(248))
        ClientSize = New Size(964, 681)
        ControlBox = True
        Controls.Add(rootLayout)
        Font = New Font("Segoe UI", 9.0F)
        MaximizeBox = True
        MinimizeBox = True
        MinimumSize = New Size(880, 640)
        Name = "HeaderForm"
        StartPosition = FormStartPosition.Manual
        Text = "New Batch"
        rootLayout.ResumeLayout(False)
        headerPanel.ResumeLayout(False)
        headerPanel.PerformLayout()
        CType(headerLogoBox, ComponentModel.ISupportInitialize).EndInit()
        bodyLayout.ResumeLayout(False)
        batchPanel.ResumeLayout(False)
        batchLayout.ResumeLayout(False)
        batchLayout.PerformLayout()
        eventPanel.ResumeLayout(False)
        eventPanel.PerformLayout()
        registrationRow.ResumeLayout(False)
        yearPanel.ResumeLayout(False)
        yearPanel.PerformLayout()
        quarterPanel.ResumeLayout(False)
        pageSourcePanel.ResumeLayout(False)
        pageRow.ResumeLayout(False)
        pagePanel.ResumeLayout(False)
        pagePanel.PerformLayout()
        suffixPanel.ResumeLayout(False)
        suffixPanel.PerformLayout()
        pageLetterPanel.ResumeLayout(False)
        pageLetterPanel.PerformLayout()
        vnfPanel.ResumeLayout(False)
        sourceRefPanel.ResumeLayout(False)
        sourceRefPanel.PerformLayout()
        creditLayout.ResumeLayout(False)
        creditNamePanel.ResumeLayout(False)
        creditNamePanel.PerformLayout()
        creditEmailPanel.ResumeLayout(False)
        creditEmailPanel.PerformLayout()
        creditTypePanel.ResumeLayout(False)
        creditTypePanel.PerformLayout()
        sideLayout.ResumeLayout(False)
        contributorPanel.ResumeLayout(False)
        contributorLayout.ResumeLayout(False)
        creatorPanel.ResumeLayout(False)
        creatorPanel.PerformLayout()
        creatorEmailPanel.ResumeLayout(False)
        creatorEmailPanel.PerformLayout()
        syndicatePanel.ResumeLayout(False)
        syndicatePanel.PerformLayout()
        userNamePanel.ResumeLayout(False)
        userNamePanel.PerformLayout()
        passwordPanel.ResumeLayout(False)
        passwordInnerLayout.ResumeLayout(False)
        passwordInnerLayout.PerformLayout()
        rulesPanel.ResumeLayout(False)
        footerPanel.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents rootLayout As TableLayoutPanel
    Friend WithEvents headerPanel As Panel
    Friend WithEvents headerLogoBox As PictureBox
    Friend WithEvents headerSubtitleLabel As Label
    Friend WithEvents headerTitleLabel As Label
    Friend WithEvents bodyLayout As TableLayoutPanel
    Friend WithEvents batchPanel As Panel
    Friend WithEvents batchLayout As TableLayoutPanel
    Friend WithEvents batchTitleLabel As Label
    Friend WithEvents recordTypeLabel As Label
    Friend WithEvents eventPanel As FlowLayoutPanel
    Friend WithEvents birthsRadioButton As RadioButton
    Friend WithEvents marriagesRadioButton As RadioButton
    Friend WithEvents deathsRadioButton As RadioButton
    Friend WithEvents registrationRow As TableLayoutPanel
    Friend WithEvents yearPanel As Panel
    Friend WithEvents yearLabel As Label
    Friend WithEvents yearTextBox As TextBox
    Friend WithEvents quarterPanel As Panel
    Friend WithEvents quarterLabel As Label
    Friend WithEvents quarterComboBox As ComboBox
    Friend WithEvents pageSourcePanel As Panel
    Friend WithEvents pageSourceLabel As Label
    Friend WithEvents pageSourceComboBox As ComboBox
    Friend WithEvents pageRow As TableLayoutPanel
    Friend WithEvents pagePanel As Panel
    Friend WithEvents pageLabel As Label
    Friend WithEvents pageTextBox As TextBox
    Friend WithEvents suffixPanel As Panel
    Friend WithEvents suffixLabel As Label
    Friend WithEvents suffixTextBox As TextBox
    Friend WithEvents pageLetterPanel As Panel
    Friend WithEvents pageLetterLabel As Label
    Friend WithEvents pageLetterTextBox As TextBox
    Friend WithEvents vnfPanel As Panel
    Friend WithEvents vnfLabel As Label
    Friend WithEvents vnfComboBox As ComboBox
    Friend WithEvents sourceRefPanel As Panel
    Friend WithEvents sourceRefLabel As Label
    Friend WithEvents sourceRefTextBox As TextBox
    Friend WithEvents commentsLabel As Label
    Friend WithEvents commentsTextBox As TextBox
    Friend WithEvents creditLayout As TableLayoutPanel
    Friend WithEvents creditNamePanel As Panel
    Friend WithEvents creditNameLabel As Label
    Friend WithEvents creditNameTextBox As TextBox
    Friend WithEvents creditEmailPanel As Panel
    Friend WithEvents creditEmailLabel As Label
    Friend WithEvents creditEmailTextBox As TextBox
    Friend WithEvents creditTypePanel As Panel
    Friend WithEvents creditTypeLabel As Label
    Friend WithEvents creditTypeTextBox As TextBox
    Friend WithEvents sideLayout As TableLayoutPanel
    Friend WithEvents contributorPanel As Panel
    Friend WithEvents contributorLayout As TableLayoutPanel
    Friend WithEvents contributorTitleLabel As Label
    Friend WithEvents creatorPanel As Panel
    Friend WithEvents creatorLabel As Label
    Friend WithEvents creatorTextBox As TextBox
    Friend WithEvents creatorEmailPanel As Panel
    Friend WithEvents creatorEmailLabel As Label
    Friend WithEvents creatorEmailTextBox As TextBox
    Friend WithEvents syndicatePanel As Panel
    Friend WithEvents syndicateLabel As Label
    Friend WithEvents syndicateTextBox As TextBox
    Friend WithEvents userNamePanel As Panel
    Friend WithEvents userNameLabel As Label
    Friend WithEvents userNameTextBox As TextBox
    Friend WithEvents passwordPanel As Panel
    Friend WithEvents passwordLabel As Label
    Friend WithEvents passwordInnerLayout As TableLayoutPanel
    Friend WithEvents userPasswordTextBox As TextBox
    Friend WithEvents showPasswordButton As Button
    Friend WithEvents rulesPanel As Panel
    Friend WithEvents rulesTitleLabel As Label
    Friend WithEvents rulesTextLabel As Label
    Friend WithEvents footerPanel As Panel
    Friend WithEvents openBatchButton As Button
    Friend WithEvents optionsButton As Button
    Friend WithEvents validationSummaryLabel As Label
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnStart As Button
End Class