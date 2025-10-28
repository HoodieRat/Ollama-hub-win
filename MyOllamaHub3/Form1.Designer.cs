namespace MyOllamaHub3
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            panel1 = new Panel();
            panel7 = new Panel();
            aiOutputTxt = new RichTextBox();
            panel5 = new Panel();
            diagnosticsTextBox = new TextBox();
            diagnosticsContextMenu = new ContextMenuStrip(components);
            copyTraceToolStripMenuItem = new ToolStripMenuItem();
            openUrlToolStripMenuItem = new ToolStripMenuItem();
            activeModelLabel = new Label();
            warmupStatusLabel = new Label();
            autoModelCheckBox = new CheckBox();
            modelsComboBox = new ComboBox();
            generationPresetComboBox = new ComboBox();
            userPromptTxt = new RichTextBox();
            sendPromptBtn = new Button();
            cancelPromptBtn = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newSessionToolStripMenuItem = new ToolStripMenuItem();
            exportSessionToolStripMenuItem = new ToolStripMenuItem();
            renameSessionToolStripMenuItem = new ToolStripMenuItem();
            deleteSessionToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            advancedToolStripMenuItem = new ToolStripMenuItem();
            historyLimitToolStripMenuItem = new ToolStripMenuItem();
            continuationAttemptsToolStripMenuItem = new ToolStripMenuItem();
            clearOutputToolStripMenuItem = new ToolStripMenuItem();
            tuneModelsToolStripMenuItem = new ToolStripMenuItem();
            panel2 = new Panel();
            label1 = new Label();
            splitContainer1 = new SplitContainer();
            tabControl1 = new TabControl();
            promptTabPage = new TabPage();
            panel3 = new Panel();
            richTextBox1 = new RichTextBox();
            assistantInstructionsLabel = new Label();
            label3 = new Label();
            personalityComboBox = new ComboBox();
            label2 = new Label();
            modelsTabPage = new TabPage();
            modelSummaryListView = new ListView();
            summaryCategoryColumnHeader = new ColumnHeader();
            summaryBestModelsColumnHeader = new ColumnHeader();
            summaryNotesColumnHeader = new ColumnHeader();
            modelSummaryLabel = new Label();
            modelLegendTextBox = new TextBox();
            modelLegendLabel = new Label();
            modelTableListView = new ListView();
            modelNameColumnHeader = new ColumnHeader();
            modelTypeColumnHeader = new ColumnHeader();
            modelIdealUseColumnHeader = new ColumnHeader();
            modelOutputLengthColumnHeader = new ColumnHeader();
            modelSpeedColumnHeader = new ColumnHeader();
            modelAnalyticalColumnHeader = new ColumnHeader();
            modelCreativityColumnHeader = new ColumnHeader();
            modelAccuracyColumnHeader = new ColumnHeader();
            modelNotesColumnHeader = new ColumnHeader();
            sessionsTabPage = new TabPage();
            sessionsListView = new ListView();
            sessionTitleColumnHeader = new ColumnHeader();
            sessionUpdatedColumnHeader = new ColumnHeader();
            panel6 = new Panel();
            modelHeaderLayoutPanel = new TableLayoutPanel();
            panel1.SuspendLayout();
            panel7.SuspendLayout();
            panel5.SuspendLayout();
            diagnosticsContextMenu.SuspendLayout();
            menuStrip1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            promptTabPage.SuspendLayout();
            panel3.SuspendLayout();
            modelsTabPage.SuspendLayout();
            sessionsTabPage.SuspendLayout();
            panel6.SuspendLayout();
            modelHeaderLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.Window;
            panel1.Controls.Add(panel7);
            panel1.Controls.Add(panel5);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(12);
            panel1.Size = new Size(807, 961);
            panel1.TabIndex = 0;
            // 
            // panel7
            // 
            panel7.BackColor = SystemColors.Window;
            panel7.Controls.Add(aiOutputTxt);
            panel7.Dock = DockStyle.Fill;
            panel7.Location = new Point(12, 12);
            panel7.Name = "panel7";
            panel7.Padding = new Padding(8);
            panel7.Size = new Size(783, 829);
            panel7.TabIndex = 7;
            // 
            // aiOutputTxt
            // 
            aiOutputTxt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            aiOutputTxt.Location = new Point(8, 46);
            aiOutputTxt.Name = "aiOutputTxt";
            aiOutputTxt.ScrollBars = RichTextBoxScrollBars.Vertical;
            aiOutputTxt.Size = new Size(767, 775);
            aiOutputTxt.TabIndex = 0;
            aiOutputTxt.Text = "";
            aiOutputTxt.Visible = false;
            aiOutputTxt.LinkClicked += AiOutputTxt_LinkClicked;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.ControlLightLight;
            panel5.Controls.Add(diagnosticsTextBox);
            panel5.Controls.Add(activeModelLabel);
            panel5.Dock = DockStyle.Bottom;
            panel5.Location = new Point(12, 841);
            panel5.Name = "panel5";
            panel5.Padding = new Padding(12);
            panel5.Size = new Size(783, 108);
            panel5.TabIndex = 1;
            // 
            // diagnosticsTextBox
            // 
            diagnosticsTextBox.BackColor = SystemColors.Window;
            diagnosticsTextBox.BorderStyle = BorderStyle.None;
            diagnosticsTextBox.ContextMenuStrip = diagnosticsContextMenu;
            diagnosticsTextBox.Dock = DockStyle.Fill;
            diagnosticsTextBox.Location = new Point(12, 27);
            diagnosticsTextBox.Margin = new Padding(0);
            diagnosticsTextBox.Multiline = true;
            diagnosticsTextBox.Name = "diagnosticsTextBox";
            diagnosticsTextBox.ReadOnly = true;
            diagnosticsTextBox.ScrollBars = ScrollBars.Vertical;
            diagnosticsTextBox.Size = new Size(759, 69);
            diagnosticsTextBox.TabIndex = 4;
            diagnosticsTextBox.TabStop = false;
            // 
            // diagnosticsContextMenu
            // 
            diagnosticsContextMenu.Items.AddRange(new ToolStripItem[] { copyTraceToolStripMenuItem, openUrlToolStripMenuItem });
            diagnosticsContextMenu.Name = "diagnosticsContextMenu";
            diagnosticsContextMenu.Size = new Size(134, 48);
            // 
            // copyTraceToolStripMenuItem
            // 
            copyTraceToolStripMenuItem.Name = "copyTraceToolStripMenuItem";
            copyTraceToolStripMenuItem.Size = new Size(133, 22);
            copyTraceToolStripMenuItem.Text = "Copy Trace";
            // 
            // openUrlToolStripMenuItem
            // 
            openUrlToolStripMenuItem.Name = "openUrlToolStripMenuItem";
            openUrlToolStripMenuItem.Size = new Size(133, 22);
            openUrlToolStripMenuItem.Text = "Open URL";
            // 
            // activeModelLabel
            // 
            activeModelLabel.AutoSize = true;
            activeModelLabel.BackColor = Color.Transparent;
            activeModelLabel.Dock = DockStyle.Top;
            activeModelLabel.Location = new Point(12, 12);
            activeModelLabel.Margin = new Padding(0, 0, 0, 6);
            activeModelLabel.Name = "activeModelLabel";
            activeModelLabel.Size = new Size(80, 15);
            activeModelLabel.TabIndex = 5;
            activeModelLabel.Text = "Active Model:";
            // 
            // warmupStatusLabel
            // 
            warmupStatusLabel.AutoEllipsis = true;
            warmupStatusLabel.Dock = DockStyle.Fill;
            warmupStatusLabel.ForeColor = SystemColors.GrayText;
            warmupStatusLabel.Location = new Point(455, 0);
            warmupStatusLabel.Margin = new Padding(0);
            warmupStatusLabel.Name = "warmupStatusLabel";
            warmupStatusLabel.Size = new Size(240, 36);
            warmupStatusLabel.TabIndex = 6;
            warmupStatusLabel.TextAlign = ContentAlignment.MiddleRight;
            warmupStatusLabel.Visible = false;
            // 
            // autoModelCheckBox
            // 
            autoModelCheckBox.Anchor = AnchorStyles.Left;
            autoModelCheckBox.AutoSize = true;
            autoModelCheckBox.Location = new Point(703, 8);
            autoModelCheckBox.Margin = new Padding(8, 2, 0, 2);
            autoModelCheckBox.Name = "autoModelCheckBox";
            autoModelCheckBox.Size = new Size(79, 19);
            autoModelCheckBox.TabIndex = 7;
            autoModelCheckBox.Text = "Auto-pick";
            autoModelCheckBox.UseVisualStyleBackColor = true;
            autoModelCheckBox.Visible = false;
            autoModelCheckBox.CheckedChanged += autoModelCheckBox_CheckedChanged;
            // 
            // modelsComboBox
            // 
            modelsComboBox.Dock = DockStyle.Fill;
            modelsComboBox.DropDownWidth = 360;
            modelsComboBox.FormattingEnabled = true;
            modelsComboBox.Location = new Point(182, 0);
            modelsComboBox.Margin = new Padding(0, 0, 12, 0);
            modelsComboBox.Name = "modelsComboBox";
            modelsComboBox.Size = new Size(261, 23);
            modelsComboBox.TabIndex = 3;
            // 
            // generationPresetComboBox
            // 
            generationPresetComboBox.Dock = DockStyle.Fill;
            generationPresetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            generationPresetComboBox.FormattingEnabled = true;
            generationPresetComboBox.Location = new Point(0, 0);
            generationPresetComboBox.Margin = new Padding(0, 0, 12, 0);
            generationPresetComboBox.Name = "generationPresetComboBox";
            generationPresetComboBox.Size = new Size(170, 23);
            generationPresetComboBox.TabIndex = 2;
            // 
            // userPromptTxt
            // 
            userPromptTxt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            userPromptTxt.Location = new Point(6, 30);
            userPromptTxt.Name = "userPromptTxt";
            userPromptTxt.Size = new Size(310, 130);
            userPromptTxt.TabIndex = 1;
            userPromptTxt.Text = "";
            // 
            // sendPromptBtn
            // 
            sendPromptBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            sendPromptBtn.Location = new Point(244, 178);
            sendPromptBtn.Name = "sendPromptBtn";
            sendPromptBtn.Size = new Size(75, 32);
            sendPromptBtn.TabIndex = 2;
            sendPromptBtn.Text = "Send";
            sendPromptBtn.UseVisualStyleBackColor = true;
            sendPromptBtn.Click += sendPromptBtn_Click;
            // 
            // cancelPromptBtn
            // 
            cancelPromptBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelPromptBtn.Enabled = false;
            cancelPromptBtn.Location = new Point(163, 178);
            cancelPromptBtn.Name = "cancelPromptBtn";
            cancelPromptBtn.Size = new Size(75, 32);
            cancelPromptBtn.TabIndex = 3;
            cancelPromptBtn.Text = "Cancel";
            cancelPromptBtn.UseVisualStyleBackColor = true;
            cancelPromptBtn.Click += CancelPromptBtn_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, advancedToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1162, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newSessionToolStripMenuItem, exportSessionToolStripMenuItem, renameSessionToolStripMenuItem, deleteSessionToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // newSessionToolStripMenuItem
            // 
            newSessionToolStripMenuItem.Name = "newSessionToolStripMenuItem";
            newSessionToolStripMenuItem.Size = new Size(159, 22);
            newSessionToolStripMenuItem.Text = "New Session";
            newSessionToolStripMenuItem.Click += newSessionToolStripMenuItem_Click;
            // 
            // exportSessionToolStripMenuItem
            // 
            exportSessionToolStripMenuItem.Name = "exportSessionToolStripMenuItem";
            exportSessionToolStripMenuItem.Size = new Size(159, 22);
            exportSessionToolStripMenuItem.Text = "Export Session";
            exportSessionToolStripMenuItem.Click += exportSessionToolStripMenuItem_Click;
            // 
            // renameSessionToolStripMenuItem
            // 
            renameSessionToolStripMenuItem.Name = "renameSessionToolStripMenuItem";
            renameSessionToolStripMenuItem.Size = new Size(159, 22);
            renameSessionToolStripMenuItem.Text = "Rename Session";
            renameSessionToolStripMenuItem.Click += renameSessionToolStripMenuItem_Click;
            // 
            // deleteSessionToolStripMenuItem
            // 
            deleteSessionToolStripMenuItem.Name = "deleteSessionToolStripMenuItem";
            deleteSessionToolStripMenuItem.Size = new Size(159, 22);
            deleteSessionToolStripMenuItem.Text = "Delete Session";
            deleteSessionToolStripMenuItem.Click += deleteSessionToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(159, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // advancedToolStripMenuItem
            // 
            advancedToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { historyLimitToolStripMenuItem, continuationAttemptsToolStripMenuItem, clearOutputToolStripMenuItem, tuneModelsToolStripMenuItem });
            advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            advancedToolStripMenuItem.Size = new Size(72, 20);
            advancedToolStripMenuItem.Text = "Advanced";
            // 
            // historyLimitToolStripMenuItem
            // 
            historyLimitToolStripMenuItem.Name = "historyLimitToolStripMenuItem";
            historyLimitToolStripMenuItem.Size = new Size(205, 22);
            historyLimitToolStripMenuItem.Text = "History Limit...";
            historyLimitToolStripMenuItem.Click += historyLimitToolStripMenuItem_Click;
            // 
            // continuationAttemptsToolStripMenuItem
            // 
            continuationAttemptsToolStripMenuItem.Name = "continuationAttemptsToolStripMenuItem";
            continuationAttemptsToolStripMenuItem.Size = new Size(205, 22);
            continuationAttemptsToolStripMenuItem.Text = "Continuation Attempts...";
            continuationAttemptsToolStripMenuItem.Click += continuationAttemptsToolStripMenuItem_Click;
            // 
            // clearOutputToolStripMenuItem
            // 
            clearOutputToolStripMenuItem.Name = "clearOutputToolStripMenuItem";
            clearOutputToolStripMenuItem.Size = new Size(205, 22);
            clearOutputToolStripMenuItem.Text = "Clear Output";
            clearOutputToolStripMenuItem.Click += clearOutputToolStripMenuItem_Click;
            // 
            // tuneModelsToolStripMenuItem
            // 
            tuneModelsToolStripMenuItem.Name = "tuneModelsToolStripMenuItem";
            tuneModelsToolStripMenuItem.Size = new Size(205, 22);
            tuneModelsToolStripMenuItem.Text = "Tune Models";
            tuneModelsToolStripMenuItem.Click += tuneModelsToolStripMenuItem_Click;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.Controls.Add(cancelPromptBtn);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(userPromptTxt);
            panel2.Controls.Add(sendPromptBtn);
            panel2.Location = new Point(6, 19);
            panel2.Name = "panel2";
            panel2.Size = new Size(319, 213);
            panel2.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 12);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 6;
            label1.Text = "User Prompt:";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panel6);
            splitContainer1.Panel2.Controls.Add(panel1);
            splitContainer1.Size = new Size(1162, 961);
            splitContainer1.SplitterDistance = 351;
            splitContainer1.TabIndex = 6;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(promptTabPage);
            tabControl1.Controls.Add(modelsTabPage);
            tabControl1.Controls.Add(sessionsTabPage);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(351, 961);
            tabControl1.TabIndex = 11;
            // 
            // promptTabPage
            // 
            promptTabPage.Controls.Add(panel2);
            promptTabPage.Controls.Add(panel3);
            promptTabPage.Location = new Point(4, 24);
            promptTabPage.Name = "promptTabPage";
            promptTabPage.Padding = new Padding(3);
            promptTabPage.Size = new Size(343, 933);
            promptTabPage.TabIndex = 0;
            promptTabPage.Text = "Prompt";
            promptTabPage.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel3.Controls.Add(richTextBox1);
            panel3.Controls.Add(assistantInstructionsLabel);
            panel3.Controls.Add(label3);
            panel3.Controls.Add(personalityComboBox);
            panel3.Controls.Add(label2);
            panel3.Location = new Point(6, 250);
            panel3.Name = "panel3";
            panel3.Size = new Size(312, 632);
            panel3.TabIndex = 7;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(6, 98);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(303, 116);
            richTextBox1.TabIndex = 10;
            richTextBox1.Text = "";
            // 
            // assistantInstructionsLabel
            // 
            assistantInstructionsLabel.AutoSize = true;
            assistantInstructionsLabel.Location = new Point(6, 80);
            assistantInstructionsLabel.Name = "assistantInstructionsLabel";
            assistantInstructionsLabel.Size = new Size(159, 15);
            assistantInstructionsLabel.TabIndex = 11;
            assistantInstructionsLabel.Text = "Persona guidance (optional):";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(141, 47);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 8;
            label3.Text = "Personality";
            // 
            // personalityComboBox
            // 
            personalityComboBox.FormattingEnabled = true;
            personalityComboBox.Location = new Point(14, 44);
            personalityComboBox.Name = "personalityComboBox";
            personalityComboBox.Size = new Size(121, 23);
            personalityComboBox.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 13);
            label2.Name = "label2";
            label2.Size = new Size(102, 15);
            label2.TabIndex = 6;
            label2.Text = "Assistant Settings:";
            // 
            // modelsTabPage
            // 
            modelsTabPage.Controls.Add(modelSummaryListView);
            modelsTabPage.Controls.Add(modelSummaryLabel);
            modelsTabPage.Controls.Add(modelLegendTextBox);
            modelsTabPage.Controls.Add(modelLegendLabel);
            modelsTabPage.Controls.Add(modelTableListView);
            modelsTabPage.Location = new Point(4, 24);
            modelsTabPage.Name = "modelsTabPage";
            modelsTabPage.Padding = new Padding(3);
            modelsTabPage.Size = new Size(343, 933);
            modelsTabPage.TabIndex = 3;
            modelsTabPage.Text = "Models";
            modelsTabPage.UseVisualStyleBackColor = true;
            // 
            // modelSummaryListView
            // 
            modelSummaryListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            modelSummaryListView.Columns.AddRange(new ColumnHeader[] { summaryCategoryColumnHeader, summaryBestModelsColumnHeader, summaryNotesColumnHeader });
            modelSummaryListView.FullRowSelect = true;
            modelSummaryListView.GridLines = true;
            modelSummaryListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            modelSummaryListView.Location = new Point(9, 441);
            modelSummaryListView.MultiSelect = false;
            modelSummaryListView.Name = "modelSummaryListView";
            modelSummaryListView.Size = new Size(449, 1022);
            modelSummaryListView.TabIndex = 4;
            modelSummaryListView.UseCompatibleStateImageBehavior = false;
            modelSummaryListView.View = View.Details;
            // 
            // summaryCategoryColumnHeader
            // 
            summaryCategoryColumnHeader.Text = "Category";
            summaryCategoryColumnHeader.Width = 120;
            // 
            // summaryBestModelsColumnHeader
            // 
            summaryBestModelsColumnHeader.Text = "Best Model(s)";
            summaryBestModelsColumnHeader.Width = 150;
            // 
            // summaryNotesColumnHeader
            // 
            summaryNotesColumnHeader.Text = "Notes";
            summaryNotesColumnHeader.Width = 220;
            // 
            // modelSummaryLabel
            // 
            modelSummaryLabel.AutoSize = true;
            modelSummaryLabel.Location = new Point(9, 423);
            modelSummaryLabel.Name = "modelSummaryLabel";
            modelSummaryLabel.Size = new Size(114, 15);
            modelSummaryLabel.TabIndex = 3;
            modelSummaryLabel.Text = "Quick Use Summary";
            // 
            // modelLegendTextBox
            // 
            modelLegendTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modelLegendTextBox.Location = new Point(9, 330);
            modelLegendTextBox.Multiline = true;
            modelLegendTextBox.Name = "modelLegendTextBox";
            modelLegendTextBox.ReadOnly = true;
            modelLegendTextBox.ScrollBars = ScrollBars.Vertical;
            modelLegendTextBox.Size = new Size(449, 90);
            modelLegendTextBox.TabIndex = 2;
            modelLegendTextBox.TabStop = false;
            // 
            // modelLegendLabel
            // 
            modelLegendLabel.AutoSize = true;
            modelLegendLabel.Location = new Point(9, 312);
            modelLegendLabel.Name = "modelLegendLabel";
            modelLegendLabel.Size = new Size(46, 15);
            modelLegendLabel.TabIndex = 1;
            modelLegendLabel.Text = "Legend";
            // 
            // modelTableListView
            // 
            modelTableListView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            modelTableListView.Columns.AddRange(new ColumnHeader[] { modelNameColumnHeader, modelTypeColumnHeader, modelIdealUseColumnHeader, modelOutputLengthColumnHeader, modelSpeedColumnHeader, modelAnalyticalColumnHeader, modelCreativityColumnHeader, modelAccuracyColumnHeader, modelNotesColumnHeader });
            modelTableListView.FullRowSelect = true;
            modelTableListView.GridLines = true;
            modelTableListView.Location = new Point(9, 9);
            modelTableListView.MultiSelect = false;
            modelTableListView.Name = "modelTableListView";
            modelTableListView.Size = new Size(449, 300);
            modelTableListView.TabIndex = 0;
            modelTableListView.UseCompatibleStateImageBehavior = false;
            modelTableListView.View = View.Details;
            // 
            // modelNameColumnHeader
            // 
            modelNameColumnHeader.Text = "Model Name";
            modelNameColumnHeader.Width = 140;
            // 
            // modelTypeColumnHeader
            // 
            modelTypeColumnHeader.Text = "Type / Purpose";
            modelTypeColumnHeader.Width = 140;
            // 
            // modelIdealUseColumnHeader
            // 
            modelIdealUseColumnHeader.Text = "Ideal Use";
            modelIdealUseColumnHeader.Width = 160;
            // 
            // modelOutputLengthColumnHeader
            // 
            modelOutputLengthColumnHeader.Text = "Output";
            modelOutputLengthColumnHeader.Width = 80;
            // 
            // modelSpeedColumnHeader
            // 
            modelSpeedColumnHeader.Text = "Speed";
            modelSpeedColumnHeader.Width = 70;
            // 
            // modelAnalyticalColumnHeader
            // 
            modelAnalyticalColumnHeader.Text = "Analytical";
            modelAnalyticalColumnHeader.Width = 80;
            // 
            // modelCreativityColumnHeader
            // 
            modelCreativityColumnHeader.Text = "Creativity";
            modelCreativityColumnHeader.Width = 80;
            // 
            // modelAccuracyColumnHeader
            // 
            modelAccuracyColumnHeader.Text = "Accuracy";
            modelAccuracyColumnHeader.Width = 80;
            // 
            // modelNotesColumnHeader
            // 
            modelNotesColumnHeader.Text = "Notes";
            modelNotesColumnHeader.Width = 220;
            // 
            // sessionsTabPage
            // 
            sessionsTabPage.Controls.Add(sessionsListView);
            sessionsTabPage.Location = new Point(4, 24);
            sessionsTabPage.Name = "sessionsTabPage";
            sessionsTabPage.Padding = new Padding(3);
            sessionsTabPage.Size = new Size(343, 933);
            sessionsTabPage.TabIndex = 1;
            sessionsTabPage.Text = "Sessions";
            sessionsTabPage.UseVisualStyleBackColor = true;
            // 
            // sessionsListView
            // 
            sessionsListView.Columns.AddRange(new ColumnHeader[] { sessionTitleColumnHeader, sessionUpdatedColumnHeader });
            sessionsListView.Dock = DockStyle.Fill;
            sessionsListView.FullRowSelect = true;
            sessionsListView.Location = new Point(3, 3);
            sessionsListView.Name = "sessionsListView";
            sessionsListView.Size = new Size(337, 927);
            sessionsListView.TabIndex = 0;
            sessionsListView.UseCompatibleStateImageBehavior = false;
            sessionsListView.View = View.Details;
            sessionsListView.SelectedIndexChanged += sessionsListView_SelectedIndexChanged;
            // 
            // sessionTitleColumnHeader
            // 
            sessionTitleColumnHeader.Text = "Session";
            sessionTitleColumnHeader.Width = 200;
            // 
            // sessionUpdatedColumnHeader
            // 
            sessionUpdatedColumnHeader.Text = "Last Updated";
            sessionUpdatedColumnHeader.Width = 110;
            // 
            // panel6
            // 
            panel6.BackColor = SystemColors.ControlLightLight;
            panel6.Controls.Add(modelHeaderLayoutPanel);
            panel6.Dock = DockStyle.Top;
            panel6.Location = new Point(0, 0);
            panel6.Name = "panel6";
            panel6.Padding = new Padding(12, 8, 12, 8);
            panel6.Size = new Size(807, 52);
            panel6.TabIndex = 1;
            // 
            // modelHeaderLayoutPanel
            // 
            modelHeaderLayoutPanel.ColumnCount = 4;
            modelHeaderLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            modelHeaderLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            modelHeaderLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            modelHeaderLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            modelHeaderLayoutPanel.Controls.Add(generationPresetComboBox, 0, 0);
            modelHeaderLayoutPanel.Controls.Add(modelsComboBox, 1, 0);
            modelHeaderLayoutPanel.Controls.Add(warmupStatusLabel, 2, 0);
            modelHeaderLayoutPanel.Controls.Add(autoModelCheckBox, 3, 0);
            modelHeaderLayoutPanel.Dock = DockStyle.Fill;
            modelHeaderLayoutPanel.Location = new Point(12, 8);
            modelHeaderLayoutPanel.Margin = new Padding(0);
            modelHeaderLayoutPanel.Name = "modelHeaderLayoutPanel";
            modelHeaderLayoutPanel.RowCount = 1;
            modelHeaderLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            modelHeaderLayoutPanel.Size = new Size(783, 36);
            modelHeaderLayoutPanel.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1162, 985);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            Shown += Form1_Shown;
            panel1.ResumeLayout(false);
            panel7.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            diagnosticsContextMenu.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            promptTabPage.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            modelsTabPage.ResumeLayout(false);
            modelsTabPage.PerformLayout();
            sessionsTabPage.ResumeLayout(false);
            panel6.ResumeLayout(false);
            modelHeaderLayoutPanel.ResumeLayout(false);
            modelHeaderLayoutPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private RichTextBox aiOutputTxt;
        private RichTextBox userPromptTxt;
        private Button sendPromptBtn;
        private Button cancelPromptBtn;
        private TextBox diagnosticsTextBox;
        private ContextMenuStrip diagnosticsContextMenu;
        private ToolStripMenuItem copyTraceToolStripMenuItem;
        private ToolStripMenuItem openUrlToolStripMenuItem;
    private ComboBox modelsComboBox;
    private ComboBox generationPresetComboBox;
        private Label warmupStatusLabel;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem advancedToolStripMenuItem;
        private Label activeModelLabel;
        private Panel panel2;
        private Label label1;
        private ToolStripMenuItem newSessionToolStripMenuItem;
        private ToolStripMenuItem exportSessionToolStripMenuItem;
        private ToolStripMenuItem renameSessionToolStripMenuItem;
        private ToolStripMenuItem deleteSessionToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem historyLimitToolStripMenuItem;
        private ToolStripMenuItem continuationAttemptsToolStripMenuItem;
        private ToolStripMenuItem clearOutputToolStripMenuItem;
        private ToolStripMenuItem tuneModelsToolStripMenuItem;
        private SplitContainer splitContainer1;
        private Panel panel3;
        private Label label3;
        private ComboBox personalityComboBox;
        private Label label2;
        private Label assistantInstructionsLabel;
        private TabControl tabControl1;
        private TabPage promptTabPage;
        private TabPage sessionsTabPage;
        private ListView sessionsListView;
        private ColumnHeader sessionTitleColumnHeader;
        private ColumnHeader sessionUpdatedColumnHeader;
        private CheckBox autoModelCheckBox;
        private TabPage modelsTabPage;
        private ListView modelTableListView;
        private ColumnHeader modelNameColumnHeader;
        private ColumnHeader modelTypeColumnHeader;
        private ColumnHeader modelIdealUseColumnHeader;
        private ColumnHeader modelOutputLengthColumnHeader;
        private ColumnHeader modelSpeedColumnHeader;
        private ColumnHeader modelAnalyticalColumnHeader;
        private ColumnHeader modelCreativityColumnHeader;
        private ColumnHeader modelAccuracyColumnHeader;
        private ColumnHeader modelNotesColumnHeader;
        private Label modelLegendLabel;
        private TextBox modelLegendTextBox;
        private Label modelSummaryLabel;
        private ListView modelSummaryListView;
        private ColumnHeader summaryCategoryColumnHeader;
        private ColumnHeader summaryBestModelsColumnHeader;
        private ColumnHeader summaryNotesColumnHeader;
        private RichTextBox richTextBox1;
        private Panel panel5;
        private Panel panel6;
        private TableLayoutPanel modelHeaderLayoutPanel;
        private Panel panel7;
    }
}
