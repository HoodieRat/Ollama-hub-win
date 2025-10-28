//START
using System.ComponentModel;
using System.Windows.Forms;

namespace MyOllamaHub3
{
    partial class ModelTuner
    {
        private IContainer components = null;

        // Non-visual
        private ToolTip toolTip;
        private ErrorProvider errorProvider;
        private BindingSource bindingSourceOptions;

        // Tabs
        private TabControl tabMain;
        private TabPage tabSimple;
        private TabPage tabAdvanced;
    private TableLayoutPanel tableSimple;
    private TableLayoutPanel tableAdvanced;
    private TableLayoutPanel tableAdvancedTop;
    private TableLayoutPanel tableAdvancedBottom;
    private TableLayoutPanel tableAdvancedMid;
    private TableLayoutPanel presetRow;
    private TableLayoutPanel depthRow;
    private TableLayoutPanel speedRow;

        // Simple tab controls
        private Label lblPreset;
        private ComboBox cmbPreset;
        private Label lblDepth;
        private TrackBar tbDepth;
        private NumericUpDown nudDepth;
        private Label lblSpeed;
        private TrackBar tbSpeed;
        private NumericUpDown nudSpeed;
    private GroupBox grpQuickTest;
    private TableLayoutPanel tableQuickTest;
    private TableLayoutPanel tableQuickActions;
    private TextBox txtSamplePrompt;
    private Button btnTestSample;
    private ProgressBar progressTest;
    private RichTextBox rtbSampleOutput;

        // Advanced tab groups
    private GroupBox grpSampling;
    private TableLayoutPanel tableSampling;
    private Label lblTemperature;
    private TrackBar tbTemperature;
    private NumericUpDown nudTemperature;
    private Label lblTopP;
    private TrackBar tbTopP;
    private NumericUpDown nudTopP;
    private Label lblTopK;
    private TrackBar tbTopK;
    private NumericUpDown nudTopK;

    private GroupBox grpAntiRepeat;
    private TableLayoutPanel tableAntiRepeat;
    private Label lblRepeatPenalty;
    private TrackBar tbRepeatPenalty;
    private NumericUpDown nudRepeatPenalty;

    private GroupBox grpLength;
    private TableLayoutPanel tableLength;
    private Label lblNumPredict;
    private TrackBar tbNumPredict;
    private NumericUpDown nudNumPredict;
    private Label lblNumCtx;
    private TrackBar tbNumCtx;
    private NumericUpDown nudNumCtx;

    private GroupBox grpMirostat;
    private TableLayoutPanel tableMirostat;
    private Label lblMirostat;
    private ComboBox cmbMirostat;
    private Label lblMirostatTau;
    private TrackBar tbMirostatTau;
    private NumericUpDown nudMirostatTau;
    private Label lblMirostatEta;
    private TrackBar tbMirostatEta;
    private NumericUpDown nudMirostatEta;

    private GroupBox grpDeterminism;
    private TableLayoutPanel tableDeterminism;
    private Label lblSeed;
    private NumericUpDown nudSeed;
    private CheckBox chkRandomSeedEachRun;

    private GroupBox grpSystem;
    private TableLayoutPanel tableSystem;
    private TableLayoutPanel tableStopRow;
    private TableLayoutPanel tableStopActions;
        private Label lblSystemPrompt;
        private TextBox txtSystemPrompt;
        private Label lblStopSeq;
        private ListBox lstStopSequences;
        private TextBox txtNewStopSequence;
        private Button btnAddStopSequence;
        private Button btnRemoveStopSequence;

        // Bottom command bar
    private TableLayoutPanel commandBar;
        private Button btnDefaults;
        private Button btnImport;
        private Button btnExport;
        private Label lblValidation;
        private Button btnOK;
        private Button btnCancel;
        private Button btnApply;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            components = new Container();
            toolTip = new ToolTip(components);
            tbTemperature = new TrackBar();
            nudTemperature = new NumericUpDown();
            tbTopP = new TrackBar();
            nudTopP = new NumericUpDown();
            tbTopK = new TrackBar();
            nudTopK = new NumericUpDown();
            tbDepth = new TrackBar();
            nudDepth = new NumericUpDown();
            tbSpeed = new TrackBar();
            nudSpeed = new NumericUpDown();
            tbRepeatPenalty = new TrackBar();
            nudRepeatPenalty = new NumericUpDown();
            tbNumPredict = new TrackBar();
            nudNumPredict = new NumericUpDown();
            tbNumCtx = new TrackBar();
            nudNumCtx = new NumericUpDown();
            cmbMirostat = new ComboBox();
            tbMirostatTau = new TrackBar();
            nudMirostatTau = new NumericUpDown();
            tbMirostatEta = new TrackBar();
            nudMirostatEta = new NumericUpDown();
            nudSeed = new NumericUpDown();
            btnDefaults = new Button();
            btnImport = new Button();
            btnExport = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            btnApply = new Button();
            errorProvider = new ErrorProvider(components);
            bindingSourceOptions = new BindingSource(components);
            tabMain = new TabControl();
            tabSimple = new TabPage();
            tableSimple = new TableLayoutPanel();
            presetRow = new TableLayoutPanel();
            lblPreset = new Label();
            cmbPreset = new ComboBox();
            depthRow = new TableLayoutPanel();
            lblDepth = new Label();
            speedRow = new TableLayoutPanel();
            lblSpeed = new Label();
            grpQuickTest = new GroupBox();
            tableQuickTest = new TableLayoutPanel();
            txtSamplePrompt = new TextBox();
            tableQuickActions = new TableLayoutPanel();
            btnTestSample = new Button();
            progressTest = new ProgressBar();
            rtbSampleOutput = new RichTextBox();
            tabAdvanced = new TabPage();
            tableAdvanced = new TableLayoutPanel();
            tableAdvancedTop = new TableLayoutPanel();
            grpSampling = new GroupBox();
            tableSampling = new TableLayoutPanel();
            lblTemperature = new Label();
            lblTopP = new Label();
            lblTopK = new Label();
            grpLength = new GroupBox();
            tableLength = new TableLayoutPanel();
            lblNumPredict = new Label();
            lblNumCtx = new Label();
            tableAdvancedMid = new TableLayoutPanel();
            grpAntiRepeat = new GroupBox();
            tableAntiRepeat = new TableLayoutPanel();
            lblRepeatPenalty = new Label();
            grpMirostat = new GroupBox();
            tableMirostat = new TableLayoutPanel();
            lblMirostat = new Label();
            lblMirostatTau = new Label();
            lblMirostatEta = new Label();
            tableAdvancedBottom = new TableLayoutPanel();
            grpSystem = new GroupBox();
            tableSystem = new TableLayoutPanel();
            lblSystemPrompt = new Label();
            txtSystemPrompt = new TextBox();
            lblStopSeq = new Label();
            tableStopRow = new TableLayoutPanel();
            lstStopSequences = new ListBox();
            tableStopActions = new TableLayoutPanel();
            txtNewStopSequence = new TextBox();
            btnAddStopSequence = new Button();
            btnRemoveStopSequence = new Button();
            grpDeterminism = new GroupBox();
            tableDeterminism = new TableLayoutPanel();
            lblSeed = new Label();
            chkRandomSeedEachRun = new CheckBox();
            commandBar = new TableLayoutPanel();
            lblValidation = new Label();
            layoutRoot = new TableLayoutPanel();
            ((ISupportInitialize)tbTemperature).BeginInit();
            ((ISupportInitialize)nudTemperature).BeginInit();
            ((ISupportInitialize)tbTopP).BeginInit();
            ((ISupportInitialize)nudTopP).BeginInit();
            ((ISupportInitialize)tbTopK).BeginInit();
            ((ISupportInitialize)nudTopK).BeginInit();
            ((ISupportInitialize)tbDepth).BeginInit();
            ((ISupportInitialize)nudDepth).BeginInit();
            ((ISupportInitialize)tbSpeed).BeginInit();
            ((ISupportInitialize)nudSpeed).BeginInit();
            ((ISupportInitialize)tbRepeatPenalty).BeginInit();
            ((ISupportInitialize)nudRepeatPenalty).BeginInit();
            ((ISupportInitialize)tbNumPredict).BeginInit();
            ((ISupportInitialize)nudNumPredict).BeginInit();
            ((ISupportInitialize)tbNumCtx).BeginInit();
            ((ISupportInitialize)nudNumCtx).BeginInit();
            ((ISupportInitialize)tbMirostatTau).BeginInit();
            ((ISupportInitialize)nudMirostatTau).BeginInit();
            ((ISupportInitialize)tbMirostatEta).BeginInit();
            ((ISupportInitialize)nudMirostatEta).BeginInit();
            ((ISupportInitialize)nudSeed).BeginInit();
            ((ISupportInitialize)errorProvider).BeginInit();
            ((ISupportInitialize)bindingSourceOptions).BeginInit();
            tabMain.SuspendLayout();
            tabSimple.SuspendLayout();
            tableSimple.SuspendLayout();
            presetRow.SuspendLayout();
            depthRow.SuspendLayout();
            speedRow.SuspendLayout();
            grpQuickTest.SuspendLayout();
            tableQuickTest.SuspendLayout();
            tableQuickActions.SuspendLayout();
            tabAdvanced.SuspendLayout();
            tableAdvanced.SuspendLayout();
            tableAdvancedTop.SuspendLayout();
            grpSampling.SuspendLayout();
            tableSampling.SuspendLayout();
            grpLength.SuspendLayout();
            tableLength.SuspendLayout();
            tableAdvancedMid.SuspendLayout();
            grpAntiRepeat.SuspendLayout();
            tableAntiRepeat.SuspendLayout();
            grpMirostat.SuspendLayout();
            tableMirostat.SuspendLayout();
            tableAdvancedBottom.SuspendLayout();
            grpSystem.SuspendLayout();
            tableSystem.SuspendLayout();
            tableStopRow.SuspendLayout();
            tableStopActions.SuspendLayout();
            grpDeterminism.SuspendLayout();
            tableDeterminism.SuspendLayout();
            commandBar.SuspendLayout();
            layoutRoot.SuspendLayout();
            SuspendLayout();
            // 
            // tbTemperature
            // 
            tbTemperature.Dock = DockStyle.Fill;
            tbTemperature.Location = new Point(98, 12);
            tbTemperature.Margin = new Padding(0, 0, 12, 0);
            tbTemperature.Maximum = 200;
            tbTemperature.Name = "tbTemperature";
            tbTemperature.Size = new Size(238, 56);
            tbTemperature.TabIndex = 1;
            tbTemperature.TickFrequency = 20;
            toolTip.SetToolTip(tbTemperature, "Higher = more creative, lower = more deterministic.");
            tbTemperature.Value = 70;
            // 
            // nudTemperature
            // 
            nudTemperature.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudTemperature.DecimalPlaces = 2;
            nudTemperature.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudTemperature.Location = new Point(348, 16);
            nudTemperature.Margin = new Padding(0, 4, 0, 0);
            nudTemperature.Maximum = new decimal(new int[] { 200, 0, 0, 131072 });
            nudTemperature.Name = "nudTemperature";
            nudTemperature.Size = new Size(80, 23);
            nudTemperature.TabIndex = 2;
            toolTip.SetToolTip(nudTemperature, "Higher = more creative, lower = more deterministic.");
            nudTemperature.Value = new decimal(new int[] { 70, 0, 0, 131072 });
            // 
            // tbTopP
            // 
            tbTopP.Dock = DockStyle.Fill;
            tbTopP.Location = new Point(98, 68);
            tbTopP.Margin = new Padding(0, 0, 12, 0);
            tbTopP.Maximum = 100;
            tbTopP.Name = "tbTopP";
            tbTopP.Size = new Size(238, 56);
            tbTopP.TabIndex = 4;
            tbTopP.TickFrequency = 10;
            toolTip.SetToolTip(tbTopP, "Nucleus sampling; 1.0 disables.");
            tbTopP.Value = 90;
            // 
            // nudTopP
            // 
            nudTopP.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudTopP.DecimalPlaces = 2;
            nudTopP.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudTopP.Location = new Point(348, 72);
            nudTopP.Margin = new Padding(0, 4, 0, 0);
            nudTopP.Maximum = new decimal(new int[] { 100, 0, 0, 131072 });
            nudTopP.Name = "nudTopP";
            nudTopP.Size = new Size(80, 23);
            nudTopP.TabIndex = 5;
            toolTip.SetToolTip(nudTopP, "Nucleus sampling; 1.0 disables.");
            nudTopP.Value = new decimal(new int[] { 90, 0, 0, 131072 });
            // 
            // tbTopK
            // 
            tbTopK.Dock = DockStyle.Fill;
            tbTopK.Location = new Point(98, 124);
            tbTopK.Margin = new Padding(0, 0, 12, 0);
            tbTopK.Maximum = 100;
            tbTopK.Name = "tbTopK";
            tbTopK.Size = new Size(238, 56);
            tbTopK.TabIndex = 7;
            tbTopK.TickFrequency = 10;
            toolTip.SetToolTip(tbTopK, "Candidate shortlist; 0 disables.");
            tbTopK.Value = 40;
            // 
            // nudTopK
            // 
            nudTopK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudTopK.Location = new Point(348, 128);
            nudTopK.Margin = new Padding(0, 4, 0, 0);
            nudTopK.Name = "nudTopK";
            nudTopK.Size = new Size(80, 23);
            nudTopK.TabIndex = 8;
            toolTip.SetToolTip(nudTopK, "Candidate shortlist; 0 disables.");
            nudTopK.Value = new decimal(new int[] { 40, 0, 0, 0 });
            // 
            // tbDepth
            // 
            tbDepth.Dock = DockStyle.Fill;
            tbDepth.Location = new Point(50, 6);
            tbDepth.Margin = new Padding(0, 6, 0, 6);
            tbDepth.Maximum = 100;
            tbDepth.Name = "tbDepth";
            tbDepth.Size = new Size(778, 36);
            tbDepth.TabIndex = 1;
            tbDepth.TickFrequency = 10;
            toolTip.SetToolTip(tbDepth, "Increase to allow longer, more detailed responses.");
            tbDepth.Value = 50;
            // 
            // nudDepth
            // 
            nudDepth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudDepth.Location = new Point(836, 10);
            nudDepth.Margin = new Padding(8, 10, 0, 6);
            nudDepth.Name = "nudDepth";
            nudDepth.Size = new Size(80, 23);
            nudDepth.TabIndex = 2;
            toolTip.SetToolTip(nudDepth, "Increase to allow longer, more detailed responses.");
            nudDepth.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // tbSpeed
            // 
            tbSpeed.Dock = DockStyle.Fill;
            tbSpeed.Location = new Point(50, 6);
            tbSpeed.Margin = new Padding(0, 6, 0, 6);
            tbSpeed.Maximum = 100;
            tbSpeed.Name = "tbSpeed";
            tbSpeed.Size = new Size(778, 36);
            tbSpeed.TabIndex = 1;
            tbSpeed.TickFrequency = 10;
            toolTip.SetToolTip(tbSpeed, "Lower values trade speed for additional reasoning steps.");
            tbSpeed.Value = 50;
            // 
            // nudSpeed
            // 
            nudSpeed.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSpeed.Location = new Point(836, 10);
            nudSpeed.Margin = new Padding(8, 10, 0, 6);
            nudSpeed.Name = "nudSpeed";
            nudSpeed.Size = new Size(80, 23);
            nudSpeed.TabIndex = 2;
            toolTip.SetToolTip(nudSpeed, "Lower values trade speed for additional reasoning steps.");
            nudSpeed.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // tbRepeatPenalty
            // 
            tbRepeatPenalty.Dock = DockStyle.Fill;
            tbRepeatPenalty.Location = new Point(109, 12);
            tbRepeatPenalty.Margin = new Padding(0, 0, 12, 0);
            tbRepeatPenalty.Maximum = 140;
            tbRepeatPenalty.Minimum = 80;
            tbRepeatPenalty.Name = "tbRepeatPenalty";
            tbRepeatPenalty.Size = new Size(227, 162);
            tbRepeatPenalty.TabIndex = 1;
            tbRepeatPenalty.TickFrequency = 5;
            toolTip.SetToolTip(tbRepeatPenalty, "Discourage repetition; >1.0 penalizes repeats.");
            tbRepeatPenalty.Value = 110;
            // 
            // nudRepeatPenalty
            // 
            nudRepeatPenalty.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudRepeatPenalty.DecimalPlaces = 2;
            nudRepeatPenalty.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudRepeatPenalty.Location = new Point(348, 16);
            nudRepeatPenalty.Margin = new Padding(0, 4, 0, 0);
            nudRepeatPenalty.Maximum = new decimal(new int[] { 140, 0, 0, 131072 });
            nudRepeatPenalty.Minimum = new decimal(new int[] { 80, 0, 0, 131072 });
            nudRepeatPenalty.Name = "nudRepeatPenalty";
            nudRepeatPenalty.Size = new Size(80, 23);
            nudRepeatPenalty.TabIndex = 2;
            toolTip.SetToolTip(nudRepeatPenalty, "Discourage repetition; >1.0 penalizes repeats.");
            nudRepeatPenalty.Value = new decimal(new int[] { 110, 0, 0, 131072 });
            // 
            // tbNumPredict
            // 
            tbNumPredict.Dock = DockStyle.Fill;
            tbNumPredict.Location = new Point(98, 12);
            tbNumPredict.Margin = new Padding(0, 0, 12, 0);
            tbNumPredict.Maximum = 2048;
            tbNumPredict.Minimum = 32;
            tbNumPredict.Name = "tbNumPredict";
            tbNumPredict.Size = new Size(238, 56);
            tbNumPredict.TabIndex = 1;
            tbNumPredict.TickFrequency = 128;
            toolTip.SetToolTip(tbNumPredict, "Max new tokens to generate.");
            tbNumPredict.Value = 512;
            // 
            // nudNumPredict
            // 
            nudNumPredict.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudNumPredict.Increment = new decimal(new int[] { 32, 0, 0, 0 });
            nudNumPredict.Location = new Point(348, 16);
            nudNumPredict.Margin = new Padding(0, 4, 0, 0);
            nudNumPredict.Maximum = new decimal(new int[] { 2048, 0, 0, 0 });
            nudNumPredict.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            nudNumPredict.Name = "nudNumPredict";
            nudNumPredict.Size = new Size(80, 23);
            nudNumPredict.TabIndex = 2;
            toolTip.SetToolTip(nudNumPredict, "Max new tokens to generate.");
            nudNumPredict.Value = new decimal(new int[] { 512, 0, 0, 0 });
            // 
            // tbNumCtx
            // 
            tbNumCtx.Dock = DockStyle.Fill;
            tbNumCtx.Location = new Point(98, 68);
            tbNumCtx.Margin = new Padding(0, 0, 12, 0);
            tbNumCtx.Maximum = 32768;
            tbNumCtx.Minimum = 512;
            tbNumCtx.Name = "tbNumCtx";
            tbNumCtx.Size = new Size(238, 106);
            tbNumCtx.TabIndex = 4;
            tbNumCtx.TickFrequency = 1024;
            toolTip.SetToolTip(tbNumCtx, "Context window size (depends on model).");
            tbNumCtx.Value = 4096;
            // 
            // nudNumCtx
            // 
            nudNumCtx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudNumCtx.Increment = new decimal(new int[] { 256, 0, 0, 0 });
            nudNumCtx.Location = new Point(348, 72);
            nudNumCtx.Margin = new Padding(0, 4, 0, 0);
            nudNumCtx.Maximum = new decimal(new int[] { 32768, 0, 0, 0 });
            nudNumCtx.Minimum = new decimal(new int[] { 512, 0, 0, 0 });
            nudNumCtx.Name = "nudNumCtx";
            nudNumCtx.Size = new Size(80, 23);
            nudNumCtx.TabIndex = 5;
            toolTip.SetToolTip(nudNumCtx, "Context window size (depends on model).");
            nudNumCtx.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            // 
            // cmbMirostat
            // 
            tableMirostat.SetColumnSpan(cmbMirostat, 2);
            cmbMirostat.Dock = DockStyle.Fill;
            cmbMirostat.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMirostat.Items.AddRange(new object[] { "Off (0)", "V1 (1)", "V2 (2)" });
            cmbMirostat.Location = new Point(62, 14);
            cmbMirostat.Margin = new Padding(0, 2, 0, 12);
            cmbMirostat.MinimumSize = new Size(160, 0);
            cmbMirostat.Name = "cmbMirostat";
            cmbMirostat.Size = new Size(366, 23);
            cmbMirostat.TabIndex = 1;
            toolTip.SetToolTip(cmbMirostat, "Entropy-based sampling: Off, V1, or V2.");
            // 
            // tbMirostatTau
            // 
            tbMirostatTau.Dock = DockStyle.Fill;
            tbMirostatTau.Location = new Point(62, 68);
            tbMirostatTau.Margin = new Padding(0, 0, 12, 0);
            tbMirostatTau.Maximum = 100;
            tbMirostatTau.Minimum = 1;
            tbMirostatTau.Name = "tbMirostatTau";
            tbMirostatTau.Size = new Size(274, 56);
            tbMirostatTau.TabIndex = 3;
            tbMirostatTau.TickFrequency = 5;
            toolTip.SetToolTip(tbMirostatTau, "Target entropy (higher = more random).");
            tbMirostatTau.Value = 50;
            // 
            // nudMirostatTau
            // 
            nudMirostatTau.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudMirostatTau.DecimalPlaces = 2;
            nudMirostatTau.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            nudMirostatTau.Location = new Point(348, 72);
            nudMirostatTau.Margin = new Padding(0, 4, 0, 0);
            nudMirostatTau.Maximum = new decimal(new int[] { 1000, 0, 0, 131072 });
            nudMirostatTau.Minimum = new decimal(new int[] { 10, 0, 0, 131072 });
            nudMirostatTau.Name = "nudMirostatTau";
            nudMirostatTau.Size = new Size(80, 23);
            nudMirostatTau.TabIndex = 4;
            toolTip.SetToolTip(nudMirostatTau, "Target entropy (higher = more random).");
            nudMirostatTau.Value = new decimal(new int[] { 500, 0, 0, 131072 });
            // 
            // tbMirostatEta
            // 
            tbMirostatEta.Dock = DockStyle.Fill;
            tbMirostatEta.Location = new Point(62, 124);
            tbMirostatEta.Margin = new Padding(0, 0, 12, 0);
            tbMirostatEta.Maximum = 1000;
            tbMirostatEta.Minimum = 1;
            tbMirostatEta.Name = "tbMirostatEta";
            tbMirostatEta.Size = new Size(274, 56);
            tbMirostatEta.TabIndex = 6;
            tbMirostatEta.TickFrequency = 50;
            toolTip.SetToolTip(tbMirostatEta, "Learning rate for Mirostat.");
            tbMirostatEta.Value = 100;
            // 
            // nudMirostatEta
            // 
            nudMirostatEta.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudMirostatEta.DecimalPlaces = 3;
            nudMirostatEta.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
            nudMirostatEta.Location = new Point(348, 128);
            nudMirostatEta.Margin = new Padding(0, 4, 0, 0);
            nudMirostatEta.Maximum = new decimal(new int[] { 1000, 0, 0, 196608 });
            nudMirostatEta.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
            nudMirostatEta.Name = "nudMirostatEta";
            nudMirostatEta.Size = new Size(80, 23);
            nudMirostatEta.TabIndex = 7;
            toolTip.SetToolTip(nudMirostatEta, "Learning rate for Mirostat.");
            nudMirostatEta.Value = new decimal(new int[] { 100, 0, 0, 196608 });
            // 
            // nudSeed
            // 
            nudSeed.Location = new Point(56, 14);
            nudSeed.Margin = new Padding(0, 2, 0, 6);
            nudSeed.Maximum = new decimal(new int[] { int.MaxValue, 0, 0, 0 });
            nudSeed.Minimum = new decimal(new int[] { int.MinValue, 0, 0, int.MinValue });
            nudSeed.Name = "nudSeed";
            nudSeed.Size = new Size(120, 23);
            nudSeed.TabIndex = 1;
            toolTip.SetToolTip(nudSeed, "Fixed seed for reproducibility; negative = randomized.");
            // 
            // btnDefaults
            // 
            btnDefaults.AutoSize = true;
            btnDefaults.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnDefaults.Location = new Point(0, 8);
            btnDefaults.Margin = new Padding(0, 0, 8, 0);
            btnDefaults.MinimumSize = new Size(130, 36);
            btnDefaults.Name = "btnDefaults";
            btnDefaults.Size = new Size(130, 36);
            btnDefaults.TabIndex = 0;
            btnDefaults.Text = "Reset to defaults";
            toolTip.SetToolTip(btnDefaults, "Restore the recommended defaults for all options.");
            // 
            // btnImport
            // 
            btnImport.AutoSize = true;
            btnImport.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnImport.Location = new Point(138, 8);
            btnImport.Margin = new Padding(0, 0, 8, 0);
            btnImport.MinimumSize = new Size(100, 36);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(100, 36);
            btnImport.TabIndex = 1;
            btnImport.Text = "Import…";
            toolTip.SetToolTip(btnImport, "Load options from a JSON file.");
            // 
            // btnExport
            // 
            btnExport.AutoSize = true;
            btnExport.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnExport.Location = new Point(246, 8);
            btnExport.Margin = new Padding(0, 0, 12, 0);
            btnExport.MinimumSize = new Size(100, 36);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(100, 36);
            btnExport.TabIndex = 2;
            btnExport.Text = "Export…";
            toolTip.SetToolTip(btnExport, "Save the current options to a JSON file.");
            // 
            // btnOK
            // 
            btnOK.AutoSize = true;
            btnOK.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(670, 8);
            btnOK.Margin = new Padding(12, 0, 0, 0);
            btnOK.MinimumSize = new Size(90, 36);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 36);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            toolTip.SetToolTip(btnOK, "Apply changes and close the tuner.");
            // 
            // btnCancel
            // 
            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(768, 8);
            btnCancel.Margin = new Padding(8, 0, 0, 0);
            btnCancel.MinimumSize = new Size(90, 36);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(90, 36);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "Cancel";
            toolTip.SetToolTip(btnCancel, "Close without applying changes.");
            // 
            // btnApply
            // 
            btnApply.AutoSize = true;
            btnApply.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnApply.Location = new Point(866, 8);
            btnApply.Margin = new Padding(8, 0, 0, 0);
            btnApply.MinimumSize = new Size(90, 36);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(90, 36);
            btnApply.TabIndex = 6;
            btnApply.Text = "Apply";
            toolTip.SetToolTip(btnApply, "Apply changes and keep the tuner open.");
            // 
            // errorProvider
            // 
            errorProvider.ContainerControl = this;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabSimple);
            tabMain.Controls.Add(tabAdvanced);
            tabMain.Dock = DockStyle.Fill;
            tabMain.Location = new Point(0, 0);
            tabMain.Margin = new Padding(0);
            tabMain.Name = "tabMain";
            tabMain.SelectedIndex = 0;
            tabMain.Size = new Size(956, 652);
            tabMain.TabIndex = 0;
            // 
            // tabSimple
            // 
            tabSimple.AutoScroll = true;
            tabSimple.Controls.Add(tableSimple);
            tabSimple.Location = new Point(4, 24);
            tabSimple.Name = "tabSimple";
            tabSimple.Padding = new Padding(10);
            tabSimple.Size = new Size(948, 624);
            tabSimple.TabIndex = 0;
            tabSimple.Text = "Simple";
            // 
            // tableSimple
            // 
            tableSimple.AutoSize = true;
            tableSimple.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableSimple.ColumnCount = 1;
            tableSimple.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableSimple.Controls.Add(presetRow, 0, 0);
            tableSimple.Controls.Add(depthRow, 0, 1);
            tableSimple.Controls.Add(speedRow, 0, 2);
            tableSimple.Controls.Add(grpQuickTest, 0, 3);
            tableSimple.Dock = DockStyle.Fill;
            tableSimple.Location = new Point(10, 10);
            tableSimple.Margin = new Padding(0);
            tableSimple.Name = "tableSimple";
            tableSimple.Padding = new Padding(6, 6, 6, 0);
            tableSimple.RowCount = 4;
            tableSimple.RowStyles.Add(new RowStyle());
            tableSimple.RowStyles.Add(new RowStyle());
            tableSimple.RowStyles.Add(new RowStyle());
            tableSimple.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableSimple.Size = new Size(928, 604);
            tableSimple.TabIndex = 0;
            // 
            // presetRow
            // 
            presetRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            presetRow.ColumnCount = 2;
            presetRow.ColumnStyles.Add(new ColumnStyle());
            presetRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            presetRow.Controls.Add(lblPreset, 0, 0);
            presetRow.Controls.Add(cmbPreset, 1, 0);
            presetRow.Dock = DockStyle.Fill;
            presetRow.Location = new Point(6, 6);
            presetRow.Margin = new Padding(0, 0, 0, 12);
            presetRow.Name = "presetRow";
            presetRow.RowCount = 1;
            presetRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            presetRow.Size = new Size(916, 130);
            presetRow.TabIndex = 0;
            // 
            // lblPreset
            // 
            lblPreset.AutoSize = true;
            lblPreset.Location = new Point(0, 6);
            lblPreset.Margin = new Padding(0, 6, 8, 0);
            lblPreset.Name = "lblPreset";
            lblPreset.Size = new Size(42, 15);
            lblPreset.TabIndex = 0;
            lblPreset.Text = "Preset:";
            // 
            // cmbPreset
            // 
            cmbPreset.Dock = DockStyle.Fill;
            cmbPreset.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPreset.Items.AddRange(new object[] { "Speed", "Balanced", "Depth", "Creative", "Deterministic" });
            cmbPreset.Location = new Point(50, 2);
            cmbPreset.Margin = new Padding(0, 2, 0, 0);
            cmbPreset.Name = "cmbPreset";
            cmbPreset.Size = new Size(866, 23);
            cmbPreset.TabIndex = 1;
            // 
            // depthRow
            // 
            depthRow.AutoSize = true;
            depthRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            depthRow.ColumnCount = 3;
            depthRow.ColumnStyles.Add(new ColumnStyle());
            depthRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            depthRow.ColumnStyles.Add(new ColumnStyle());
            depthRow.Controls.Add(lblDepth, 0, 0);
            depthRow.Controls.Add(tbDepth, 1, 0);
            depthRow.Controls.Add(nudDepth, 2, 0);
            depthRow.Dock = DockStyle.Fill;
            depthRow.Location = new Point(6, 148);
            depthRow.Margin = new Padding(0, 0, 0, 12);
            depthRow.Name = "depthRow";
            depthRow.RowCount = 1;
            depthRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            depthRow.Size = new Size(916, 48);
            depthRow.TabIndex = 1;
            // 
            // lblDepth
            // 
            lblDepth.AutoSize = true;
            lblDepth.Location = new Point(0, 6);
            lblDepth.Margin = new Padding(0, 6, 8, 0);
            lblDepth.Name = "lblDepth";
            lblDepth.Size = new Size(42, 15);
            lblDepth.TabIndex = 0;
            lblDepth.Text = "Depth:";
            // 
            // speedRow
            // 
            speedRow.AutoSize = true;
            speedRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            speedRow.ColumnCount = 3;
            speedRow.ColumnStyles.Add(new ColumnStyle());
            speedRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            speedRow.ColumnStyles.Add(new ColumnStyle());
            speedRow.Controls.Add(tbSpeed, 1, 0);
            speedRow.Controls.Add(nudSpeed, 2, 0);
            speedRow.Controls.Add(lblSpeed, 0, 0);
            speedRow.Dock = DockStyle.Fill;
            speedRow.Location = new Point(6, 208);
            speedRow.Margin = new Padding(0, 0, 0, 12);
            speedRow.Name = "speedRow";
            speedRow.RowCount = 1;
            speedRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            speedRow.Size = new Size(916, 48);
            speedRow.TabIndex = 2;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(0, 6);
            lblSpeed.Margin = new Padding(0, 6, 8, 0);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(42, 15);
            lblSpeed.TabIndex = 0;
            lblSpeed.Text = "Speed:";
            // 
            // grpQuickTest
            // 
            grpQuickTest.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            grpQuickTest.Controls.Add(tableQuickTest);
            grpQuickTest.Location = new Point(6, 306);
            grpQuickTest.Margin = new Padding(0);
            grpQuickTest.MinimumSize = new Size(0, 260);
            grpQuickTest.Name = "grpQuickTest";
            grpQuickTest.Padding = new Padding(12);
            grpQuickTest.Size = new Size(916, 260);
            grpQuickTest.TabIndex = 3;
            grpQuickTest.TabStop = false;
            grpQuickTest.Text = "Quick sanity test";
            // 
            // tableQuickTest
            // 
            tableQuickTest.ColumnCount = 2;
            tableQuickTest.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableQuickTest.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
            tableQuickTest.Controls.Add(txtSamplePrompt, 0, 0);
            tableQuickTest.Controls.Add(tableQuickActions, 1, 0);
            tableQuickTest.Controls.Add(rtbSampleOutput, 0, 1);
            tableQuickTest.Location = new Point(12, 28);
            tableQuickTest.Margin = new Padding(0);
            tableQuickTest.Name = "tableQuickTest";
            tableQuickTest.Padding = new Padding(8, 12, 8, 8);
            tableQuickTest.RowCount = 2;
            tableQuickTest.RowStyles.Add(new RowStyle());
            tableQuickTest.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableQuickTest.Size = new Size(892, 220);
            tableQuickTest.TabIndex = 0;
            // 
            // txtSamplePrompt
            // 
            txtSamplePrompt.Dock = DockStyle.Fill;
            txtSamplePrompt.Location = new Point(11, 15);
            txtSamplePrompt.Multiline = true;
            txtSamplePrompt.Name = "txtSamplePrompt";
            txtSamplePrompt.ScrollBars = ScrollBars.Vertical;
            txtSamplePrompt.Size = new Size(740, 94);
            txtSamplePrompt.TabIndex = 0;
            txtSamplePrompt.Text = "Summarize why temperature affects creativity.";
            // 
            // tableQuickActions
            // 
            tableQuickActions.ColumnCount = 1;
            tableQuickActions.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableQuickActions.Controls.Add(btnTestSample, 0, 0);
            tableQuickActions.Controls.Add(progressTest, 0, 1);
            tableQuickActions.Dock = DockStyle.Fill;
            tableQuickActions.Location = new Point(762, 12);
            tableQuickActions.Margin = new Padding(8, 0, 0, 0);
            tableQuickActions.Name = "tableQuickActions";
            tableQuickActions.RowCount = 2;
            tableQuickActions.RowStyles.Add(new RowStyle());
            tableQuickActions.RowStyles.Add(new RowStyle());
            tableQuickActions.Size = new Size(122, 100);
            tableQuickActions.TabIndex = 1;
            // 
            // btnTestSample
            // 
            btnTestSample.Dock = DockStyle.Fill;
            btnTestSample.Location = new Point(0, 0);
            btnTestSample.Margin = new Padding(0, 0, 0, 6);
            btnTestSample.Name = "btnTestSample";
            btnTestSample.Size = new Size(122, 30);
            btnTestSample.TabIndex = 0;
            btnTestSample.Text = "Test";
            // 
            // progressTest
            // 
            progressTest.Dock = DockStyle.Fill;
            progressTest.Location = new Point(3, 39);
            progressTest.Name = "progressTest";
            progressTest.Size = new Size(116, 58);
            progressTest.TabIndex = 1;
            // 
            // rtbSampleOutput
            // 
            tableQuickTest.SetColumnSpan(rtbSampleOutput, 2);
            rtbSampleOutput.Location = new Point(11, 115);
            rtbSampleOutput.Name = "rtbSampleOutput";
            rtbSampleOutput.ReadOnly = true;
            rtbSampleOutput.Size = new Size(870, 48);
            rtbSampleOutput.TabIndex = 2;
            rtbSampleOutput.Text = "";
            // 
            // tabAdvanced
            // 
            tabAdvanced.AutoScroll = true;
            tabAdvanced.Controls.Add(tableAdvanced);
            tabAdvanced.Location = new Point(4, 24);
            tabAdvanced.Name = "tabAdvanced";
            tabAdvanced.Padding = new Padding(10);
            tabAdvanced.Size = new Size(948, 624);
            tabAdvanced.TabIndex = 1;
            tabAdvanced.Text = "Advanced";
            // 
            // tableAdvanced
            // 
            tableAdvanced.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableAdvanced.ColumnCount = 1;
            tableAdvanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableAdvanced.Controls.Add(tableAdvancedTop, 0, 0);
            tableAdvanced.Controls.Add(tableAdvancedMid, 0, 1);
            tableAdvanced.Controls.Add(tableAdvancedBottom, 0, 2);
            tableAdvanced.Dock = DockStyle.Fill;
            tableAdvanced.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableAdvanced.Location = new Point(10, 10);
            tableAdvanced.Margin = new Padding(0);
            tableAdvanced.Name = "tableAdvanced";
            tableAdvanced.Padding = new Padding(6, 6, 6, 0);
            tableAdvanced.RowCount = 3;
            tableAdvanced.RowStyles.Add(new RowStyle());
            tableAdvanced.RowStyles.Add(new RowStyle());
            tableAdvanced.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableAdvanced.Size = new Size(928, 604);
            tableAdvanced.TabIndex = 0;
            // 
            // tableAdvancedTop
            // 
            tableAdvancedTop.AutoSize = true;
            tableAdvancedTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableAdvancedTop.ColumnCount = 2;
            tableAdvancedTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableAdvancedTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableAdvancedTop.Controls.Add(grpSampling, 0, 0);
            tableAdvancedTop.Controls.Add(grpLength, 1, 0);
            tableAdvancedTop.Dock = DockStyle.Fill;
            tableAdvancedTop.Location = new Point(6, 6);
            tableAdvancedTop.Margin = new Padding(0, 0, 0, 12);
            tableAdvancedTop.Name = "tableAdvancedTop";
            tableAdvancedTop.RowCount = 1;
            tableAdvancedTop.RowStyles.Add(new RowStyle());
            tableAdvancedTop.Size = new Size(916, 216);
            tableAdvancedTop.TabIndex = 0;
            // 
            // grpSampling
            // 
            grpSampling.Controls.Add(tableSampling);
            grpSampling.Dock = DockStyle.Fill;
            grpSampling.Location = new Point(0, 0);
            grpSampling.Margin = new Padding(0, 0, 12, 12);
            grpSampling.Name = "grpSampling";
            grpSampling.Size = new Size(446, 204);
            grpSampling.TabIndex = 0;
            grpSampling.TabStop = false;
            grpSampling.Text = "Sampling";
            // 
            // tableSampling
            // 
            tableSampling.AutoSize = true;
            tableSampling.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableSampling.ColumnCount = 3;
            tableSampling.ColumnStyles.Add(new ColumnStyle());
            tableSampling.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableSampling.ColumnStyles.Add(new ColumnStyle());
            tableSampling.Controls.Add(lblTemperature, 0, 0);
            tableSampling.Controls.Add(tbTemperature, 1, 0);
            tableSampling.Controls.Add(nudTemperature, 2, 0);
            tableSampling.Controls.Add(lblTopP, 0, 1);
            tableSampling.Controls.Add(tbTopP, 1, 1);
            tableSampling.Controls.Add(nudTopP, 2, 1);
            tableSampling.Controls.Add(lblTopK, 0, 2);
            tableSampling.Controls.Add(tbTopK, 1, 2);
            tableSampling.Controls.Add(nudTopK, 2, 2);
            tableSampling.Dock = DockStyle.Fill;
            tableSampling.Location = new Point(3, 19);
            tableSampling.Name = "tableSampling";
            tableSampling.Padding = new Padding(12, 12, 12, 8);
            tableSampling.RowCount = 3;
            tableSampling.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableSampling.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableSampling.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableSampling.Size = new Size(440, 182);
            tableSampling.TabIndex = 0;
            // 
            // lblTemperature
            // 
            lblTemperature.AutoSize = true;
            lblTemperature.Location = new Point(12, 18);
            lblTemperature.Margin = new Padding(0, 6, 12, 0);
            lblTemperature.Name = "lblTemperature";
            lblTemperature.Size = new Size(74, 15);
            lblTemperature.TabIndex = 0;
            lblTemperature.Text = "Temperature";
            // 
            // lblTopP
            // 
            lblTopP.AutoSize = true;
            lblTopP.Location = new Point(12, 74);
            lblTopP.Margin = new Padding(0, 6, 12, 0);
            lblTopP.Name = "lblTopP";
            lblTopP.Size = new Size(37, 15);
            lblTopP.TabIndex = 3;
            lblTopP.Text = "Top P";
            // 
            // lblTopK
            // 
            lblTopK.AutoSize = true;
            lblTopK.Location = new Point(12, 130);
            lblTopK.Margin = new Padding(0, 6, 12, 0);
            lblTopK.Name = "lblTopK";
            lblTopK.Size = new Size(37, 15);
            lblTopK.TabIndex = 6;
            lblTopK.Text = "Top K";
            // 
            // grpLength
            // 
            grpLength.Controls.Add(tableLength);
            grpLength.Dock = DockStyle.Fill;
            grpLength.Location = new Point(470, 0);
            grpLength.Margin = new Padding(12, 0, 0, 12);
            grpLength.Name = "grpLength";
            grpLength.Size = new Size(446, 204);
            grpLength.TabIndex = 1;
            grpLength.TabStop = false;
            grpLength.Text = "Length & context";
            // 
            // tableLength
            // 
            tableLength.AutoSize = true;
            tableLength.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLength.ColumnCount = 3;
            tableLength.ColumnStyles.Add(new ColumnStyle());
            tableLength.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLength.ColumnStyles.Add(new ColumnStyle());
            tableLength.Controls.Add(lblNumPredict, 0, 0);
            tableLength.Controls.Add(tbNumPredict, 1, 0);
            tableLength.Controls.Add(nudNumPredict, 2, 0);
            tableLength.Controls.Add(lblNumCtx, 0, 1);
            tableLength.Controls.Add(tbNumCtx, 1, 1);
            tableLength.Controls.Add(nudNumCtx, 2, 1);
            tableLength.Dock = DockStyle.Fill;
            tableLength.Location = new Point(3, 19);
            tableLength.Name = "tableLength";
            tableLength.Padding = new Padding(12, 12, 12, 8);
            tableLength.RowCount = 2;
            tableLength.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableLength.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableLength.Size = new Size(440, 182);
            tableLength.TabIndex = 0;
            // 
            // lblNumPredict
            // 
            lblNumPredict.AutoSize = true;
            lblNumPredict.Location = new Point(12, 18);
            lblNumPredict.Margin = new Padding(0, 6, 12, 0);
            lblNumPredict.Name = "lblNumPredict";
            lblNumPredict.Size = new Size(74, 15);
            lblNumPredict.TabIndex = 0;
            lblNumPredict.Text = "num_predict";
            // 
            // lblNumCtx
            // 
            lblNumCtx.AutoSize = true;
            lblNumCtx.Location = new Point(12, 74);
            lblNumCtx.Margin = new Padding(0, 6, 12, 0);
            lblNumCtx.Name = "lblNumCtx";
            lblNumCtx.Size = new Size(52, 15);
            lblNumCtx.TabIndex = 3;
            lblNumCtx.Text = "num_ctx";
            // 
            // tableAdvancedMid
            // 
            tableAdvancedMid.AutoSize = true;
            tableAdvancedMid.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableAdvancedMid.ColumnCount = 2;
            tableAdvancedMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableAdvancedMid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableAdvancedMid.Controls.Add(grpAntiRepeat, 0, 0);
            tableAdvancedMid.Controls.Add(grpMirostat, 1, 0);
            tableAdvancedMid.Dock = DockStyle.Fill;
            tableAdvancedMid.Location = new Point(6, 234);
            tableAdvancedMid.Margin = new Padding(0, 0, 0, 12);
            tableAdvancedMid.Name = "tableAdvancedMid";
            tableAdvancedMid.RowCount = 1;
            tableAdvancedMid.RowStyles.Add(new RowStyle());
            tableAdvancedMid.Size = new Size(916, 216);
            tableAdvancedMid.TabIndex = 1;
            // 
            // grpAntiRepeat
            // 
            grpAntiRepeat.Controls.Add(tableAntiRepeat);
            grpAntiRepeat.Dock = DockStyle.Fill;
            grpAntiRepeat.Location = new Point(0, 0);
            grpAntiRepeat.Margin = new Padding(0, 0, 12, 12);
            grpAntiRepeat.Name = "grpAntiRepeat";
            grpAntiRepeat.Size = new Size(446, 204);
            grpAntiRepeat.TabIndex = 2;
            grpAntiRepeat.TabStop = false;
            grpAntiRepeat.Text = "Anti-repeat";
            // 
            // tableAntiRepeat
            // 
            tableAntiRepeat.AutoSize = true;
            tableAntiRepeat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableAntiRepeat.ColumnCount = 3;
            tableAntiRepeat.ColumnStyles.Add(new ColumnStyle());
            tableAntiRepeat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableAntiRepeat.ColumnStyles.Add(new ColumnStyle());
            tableAntiRepeat.Controls.Add(lblRepeatPenalty, 0, 0);
            tableAntiRepeat.Controls.Add(tbRepeatPenalty, 1, 0);
            tableAntiRepeat.Controls.Add(nudRepeatPenalty, 2, 0);
            tableAntiRepeat.Dock = DockStyle.Fill;
            tableAntiRepeat.Location = new Point(3, 19);
            tableAntiRepeat.Name = "tableAntiRepeat";
            tableAntiRepeat.Padding = new Padding(12, 12, 12, 8);
            tableAntiRepeat.RowCount = 1;
            tableAntiRepeat.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableAntiRepeat.Size = new Size(440, 182);
            tableAntiRepeat.TabIndex = 0;
            // 
            // lblRepeatPenalty
            // 
            lblRepeatPenalty.AutoSize = true;
            lblRepeatPenalty.Location = new Point(12, 18);
            lblRepeatPenalty.Margin = new Padding(0, 6, 12, 0);
            lblRepeatPenalty.Name = "lblRepeatPenalty";
            lblRepeatPenalty.Size = new Size(85, 15);
            lblRepeatPenalty.TabIndex = 0;
            lblRepeatPenalty.Text = "Repeat Penalty";
            // 
            // grpMirostat
            // 
            grpMirostat.Controls.Add(tableMirostat);
            grpMirostat.Dock = DockStyle.Fill;
            grpMirostat.Location = new Point(470, 0);
            grpMirostat.Margin = new Padding(12, 0, 0, 12);
            grpMirostat.Name = "grpMirostat";
            grpMirostat.Size = new Size(446, 204);
            grpMirostat.TabIndex = 3;
            grpMirostat.TabStop = false;
            grpMirostat.Text = "Mirostat";
            // 
            // tableMirostat
            // 
            tableMirostat.AutoSize = true;
            tableMirostat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableMirostat.ColumnCount = 3;
            tableMirostat.ColumnStyles.Add(new ColumnStyle());
            tableMirostat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableMirostat.ColumnStyles.Add(new ColumnStyle());
            tableMirostat.Controls.Add(lblMirostat, 0, 0);
            tableMirostat.Controls.Add(cmbMirostat, 1, 0);
            tableMirostat.Controls.Add(lblMirostatTau, 0, 1);
            tableMirostat.Controls.Add(tbMirostatTau, 1, 1);
            tableMirostat.Controls.Add(nudMirostatTau, 2, 1);
            tableMirostat.Controls.Add(lblMirostatEta, 0, 2);
            tableMirostat.Controls.Add(tbMirostatEta, 1, 2);
            tableMirostat.Controls.Add(nudMirostatEta, 2, 2);
            tableMirostat.Dock = DockStyle.Fill;
            tableMirostat.Location = new Point(3, 19);
            tableMirostat.Name = "tableMirostat";
            tableMirostat.Padding = new Padding(12, 12, 12, 8);
            tableMirostat.RowCount = 3;
            tableMirostat.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableMirostat.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableMirostat.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableMirostat.Size = new Size(440, 182);
            tableMirostat.TabIndex = 0;
            // 
            // lblMirostat
            // 
            lblMirostat.AutoSize = true;
            lblMirostat.Location = new Point(12, 18);
            lblMirostat.Margin = new Padding(0, 6, 12, 0);
            lblMirostat.Name = "lblMirostat";
            lblMirostat.Size = new Size(38, 15);
            lblMirostat.TabIndex = 0;
            lblMirostat.Text = "Mode";
            // 
            // lblMirostatTau
            // 
            lblMirostatTau.AutoSize = true;
            lblMirostatTau.Location = new Point(12, 74);
            lblMirostatTau.Margin = new Padding(0, 6, 12, 0);
            lblMirostatTau.Name = "lblMirostatTau";
            lblMirostatTau.Size = new Size(26, 15);
            lblMirostatTau.TabIndex = 2;
            lblMirostatTau.Text = "Tau";
            // 
            // lblMirostatEta
            // 
            lblMirostatEta.AutoSize = true;
            lblMirostatEta.Location = new Point(12, 130);
            lblMirostatEta.Margin = new Padding(0, 6, 12, 0);
            lblMirostatEta.Name = "lblMirostatEta";
            lblMirostatEta.Size = new Size(23, 15);
            lblMirostatEta.TabIndex = 5;
            lblMirostatEta.Text = "Eta";
            // 
            // tableAdvancedBottom
            // 
            tableAdvancedBottom.ColumnCount = 2;
            tableAdvancedBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableAdvancedBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableAdvancedBottom.Controls.Add(grpSystem, 0, 0);
            tableAdvancedBottom.Controls.Add(grpDeterminism, 1, 0);
            tableAdvancedBottom.Dock = DockStyle.Fill;
            tableAdvancedBottom.Location = new Point(6, 462);
            tableAdvancedBottom.Margin = new Padding(0);
            tableAdvancedBottom.Name = "tableAdvancedBottom";
            tableAdvancedBottom.RowCount = 1;
            tableAdvancedBottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableAdvancedBottom.Size = new Size(916, 142);
            tableAdvancedBottom.TabIndex = 1;
            // 
            // grpSystem
            // 
            grpSystem.Controls.Add(tableSystem);
            grpSystem.Dock = DockStyle.Fill;
            grpSystem.Location = new Point(0, 0);
            grpSystem.Margin = new Padding(0, 0, 12, 0);
            grpSystem.Name = "grpSystem";
            grpSystem.Padding = new Padding(12);
            grpSystem.Size = new Size(537, 142);
            grpSystem.TabIndex = 0;
            grpSystem.TabStop = false;
            grpSystem.Text = "System & stops";
            // 
            // tableSystem
            // 
            tableSystem.ColumnCount = 1;
            tableSystem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableSystem.Controls.Add(lblSystemPrompt, 0, 0);
            tableSystem.Controls.Add(txtSystemPrompt, 0, 1);
            tableSystem.Controls.Add(lblStopSeq, 0, 2);
            tableSystem.Controls.Add(tableStopRow, 0, 3);
            tableSystem.Dock = DockStyle.Fill;
            tableSystem.Location = new Point(12, 28);
            tableSystem.Margin = new Padding(0);
            tableSystem.Name = "tableSystem";
            tableSystem.Padding = new Padding(12);
            tableSystem.RowCount = 4;
            tableSystem.RowStyles.Add(new RowStyle());
            tableSystem.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
            tableSystem.RowStyles.Add(new RowStyle());
            tableSystem.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableSystem.Size = new Size(513, 102);
            tableSystem.TabIndex = 0;
            // 
            // lblSystemPrompt
            // 
            lblSystemPrompt.AutoSize = true;
            lblSystemPrompt.Location = new Point(12, 12);
            lblSystemPrompt.Margin = new Padding(0, 0, 0, 6);
            lblSystemPrompt.Name = "lblSystemPrompt";
            lblSystemPrompt.Size = new Size(88, 15);
            lblSystemPrompt.TabIndex = 0;
            lblSystemPrompt.Text = "System Prompt";
            // 
            // txtSystemPrompt
            // 
            txtSystemPrompt.Dock = DockStyle.Fill;
            txtSystemPrompt.Location = new Point(12, 33);
            txtSystemPrompt.Margin = new Padding(0, 0, 0, 12);
            txtSystemPrompt.MinimumSize = new Size(0, 80);
            txtSystemPrompt.Multiline = true;
            txtSystemPrompt.Name = "txtSystemPrompt";
            txtSystemPrompt.ScrollBars = ScrollBars.Vertical;
            txtSystemPrompt.Size = new Size(489, 84);
            txtSystemPrompt.TabIndex = 1;
            // 
            // lblStopSeq
            // 
            lblStopSeq.AutoSize = true;
            lblStopSeq.Location = new Point(12, 129);
            lblStopSeq.Margin = new Padding(0, 0, 0, 6);
            lblStopSeq.Name = "lblStopSeq";
            lblStopSeq.Size = new Size(90, 15);
            lblStopSeq.TabIndex = 2;
            lblStopSeq.Text = "Stop Sequences";
            // 
            // tableStopRow
            // 
            tableStopRow.ColumnCount = 2;
            tableStopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableStopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableStopRow.Controls.Add(lstStopSequences, 0, 0);
            tableStopRow.Controls.Add(tableStopActions, 1, 0);
            tableStopRow.Dock = DockStyle.Fill;
            tableStopRow.Location = new Point(15, 153);
            tableStopRow.MinimumSize = new Size(0, 120);
            tableStopRow.Name = "tableStopRow";
            tableStopRow.Padding = new Padding(0, 0, 0, 8);
            tableStopRow.RowCount = 1;
            tableStopRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableStopRow.Size = new Size(483, 120);
            tableStopRow.TabIndex = 3;
            // 
            // lstStopSequences
            // 
            lstStopSequences.Dock = DockStyle.Fill;
            lstStopSequences.IntegralHeight = false;
            lstStopSequences.ItemHeight = 15;
            lstStopSequences.Location = new Point(0, 0);
            lstStopSequences.Margin = new Padding(0, 0, 12, 8);
            lstStopSequences.MinimumSize = new Size(0, 100);
            lstStopSequences.Name = "lstStopSequences";
            lstStopSequences.Size = new Size(271, 104);
            lstStopSequences.TabIndex = 0;
            // 
            // tableStopActions
            // 
            tableStopActions.ColumnCount = 1;
            tableStopActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableStopActions.Controls.Add(txtNewStopSequence, 0, 0);
            tableStopActions.Controls.Add(btnAddStopSequence, 0, 1);
            tableStopActions.Controls.Add(btnRemoveStopSequence, 0, 2);
            tableStopActions.Dock = DockStyle.Fill;
            tableStopActions.Location = new Point(283, 0);
            tableStopActions.Margin = new Padding(0, 0, 0, 8);
            tableStopActions.Name = "tableStopActions";
            tableStopActions.Padding = new Padding(0, 0, 0, 4);
            tableStopActions.RowCount = 4;
            tableStopActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableStopActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableStopActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tableStopActions.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableStopActions.Size = new Size(200, 104);
            tableStopActions.TabIndex = 1;
            // 
            // txtNewStopSequence
            // 
            txtNewStopSequence.Dock = DockStyle.Fill;
            txtNewStopSequence.Location = new Point(0, 0);
            txtNewStopSequence.Margin = new Padding(0, 0, 0, 6);
            txtNewStopSequence.MinimumSize = new Size(0, 24);
            txtNewStopSequence.Name = "txtNewStopSequence";
            txtNewStopSequence.Size = new Size(200, 24);
            txtNewStopSequence.TabIndex = 0;
            // 
            // btnAddStopSequence
            // 
            btnAddStopSequence.Dock = DockStyle.Top;
            btnAddStopSequence.Location = new Point(0, 32);
            btnAddStopSequence.Margin = new Padding(0, 0, 0, 6);
            btnAddStopSequence.MinimumSize = new Size(0, 28);
            btnAddStopSequence.Name = "btnAddStopSequence";
            btnAddStopSequence.Size = new Size(200, 30);
            btnAddStopSequence.TabIndex = 1;
            btnAddStopSequence.Text = "Add";
            // 
            // btnRemoveStopSequence
            // 
            btnRemoveStopSequence.Dock = DockStyle.Top;
            btnRemoveStopSequence.Location = new Point(0, 68);
            btnRemoveStopSequence.Margin = new Padding(0, 0, 0, 6);
            btnRemoveStopSequence.MinimumSize = new Size(0, 28);
            btnRemoveStopSequence.Name = "btnRemoveStopSequence";
            btnRemoveStopSequence.Size = new Size(200, 30);
            btnRemoveStopSequence.TabIndex = 2;
            btnRemoveStopSequence.Text = "Remove";
            // 
            // grpDeterminism
            // 
            grpDeterminism.Controls.Add(tableDeterminism);
            grpDeterminism.Dock = DockStyle.Fill;
            grpDeterminism.Location = new Point(561, 0);
            grpDeterminism.Margin = new Padding(12, 0, 0, 0);
            grpDeterminism.MinimumSize = new Size(0, 120);
            grpDeterminism.Name = "grpDeterminism";
            grpDeterminism.Padding = new Padding(12);
            grpDeterminism.Size = new Size(355, 142);
            grpDeterminism.TabIndex = 1;
            grpDeterminism.TabStop = false;
            grpDeterminism.Text = "Determinism";
            // 
            // tableDeterminism
            // 
            tableDeterminism.AutoSize = true;
            tableDeterminism.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableDeterminism.ColumnCount = 2;
            tableDeterminism.ColumnStyles.Add(new ColumnStyle());
            tableDeterminism.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableDeterminism.Controls.Add(lblSeed, 0, 0);
            tableDeterminism.Controls.Add(nudSeed, 1, 0);
            tableDeterminism.Controls.Add(chkRandomSeedEachRun, 0, 1);
            tableDeterminism.Dock = DockStyle.Fill;
            tableDeterminism.Location = new Point(12, 28);
            tableDeterminism.Name = "tableDeterminism";
            tableDeterminism.Padding = new Padding(12, 12, 12, 8);
            tableDeterminism.RowCount = 2;
            tableDeterminism.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
            tableDeterminism.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            tableDeterminism.Size = new Size(331, 102);
            tableDeterminism.TabIndex = 0;
            // 
            // lblSeed
            // 
            lblSeed.AutoSize = true;
            lblSeed.Location = new Point(12, 18);
            lblSeed.Margin = new Padding(0, 6, 12, 0);
            lblSeed.Name = "lblSeed";
            lblSeed.Size = new Size(32, 15);
            lblSeed.TabIndex = 0;
            lblSeed.Text = "Seed";
            // 
            // chkRandomSeedEachRun
            // 
            chkRandomSeedEachRun.AutoSize = true;
            tableDeterminism.SetColumnSpan(chkRandomSeedEachRun, 2);
            chkRandomSeedEachRun.Location = new Point(12, 72);
            chkRandomSeedEachRun.Margin = new Padding(0, 4, 0, 0);
            chkRandomSeedEachRun.Name = "chkRandomSeedEachRun";
            chkRandomSeedEachRun.Size = new Size(214, 19);
            chkRandomSeedEachRun.TabIndex = 2;
            chkRandomSeedEachRun.Text = "Randomize each run when Seed < 0";
            // 
            // commandBar
            // 
            commandBar.AutoSize = true;
            commandBar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            commandBar.ColumnCount = 7;
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.ColumnStyles.Add(new ColumnStyle());
            commandBar.Controls.Add(btnDefaults, 0, 0);
            commandBar.Controls.Add(btnImport, 1, 0);
            commandBar.Controls.Add(btnExport, 2, 0);
            commandBar.Controls.Add(lblValidation, 3, 0);
            commandBar.Controls.Add(btnOK, 4, 0);
            commandBar.Controls.Add(btnCancel, 5, 0);
            commandBar.Controls.Add(btnApply, 6, 0);
            commandBar.Dock = DockStyle.Bottom;
            commandBar.Location = new Point(0, 652);
            commandBar.Margin = new Padding(0);
            commandBar.Name = "commandBar";
            commandBar.Padding = new Padding(0, 8, 0, 0);
            commandBar.RowCount = 1;
            commandBar.RowStyles.Add(new RowStyle());
            commandBar.Size = new Size(956, 44);
            commandBar.TabIndex = 1;
            // 
            // lblValidation
            // 
            lblValidation.AutoEllipsis = true;
            lblValidation.Dock = DockStyle.Fill;
            lblValidation.Location = new Point(358, 16);
            lblValidation.Margin = new Padding(0, 8, 0, 0);
            lblValidation.MinimumSize = new Size(0, 28);
            lblValidation.Name = "lblValidation";
            lblValidation.Size = new Size(300, 28);
            lblValidation.TabIndex = 3;
            lblValidation.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // layoutRoot
            // 
            layoutRoot.ColumnCount = 1;
            layoutRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutRoot.Controls.Add(tabMain, 0, 0);
            layoutRoot.Controls.Add(commandBar, 0, 1);
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Location = new Point(12, 12);
            layoutRoot.Margin = new Padding(0);
            layoutRoot.Name = "layoutRoot";
            layoutRoot.RowCount = 2;
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutRoot.RowStyles.Add(new RowStyle());
            layoutRoot.Size = new Size(956, 696);
            layoutRoot.TabIndex = 0;
            // 
            // ModelTuner
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(980, 720);
            Controls.Add(layoutRoot);
            Name = "ModelTuner";
            Padding = new Padding(12);
            StartPosition = FormStartPosition.CenterParent;
            Text = "Model Tuner";
            ((ISupportInitialize)tbTemperature).EndInit();
            ((ISupportInitialize)nudTemperature).EndInit();
            ((ISupportInitialize)tbTopP).EndInit();
            ((ISupportInitialize)nudTopP).EndInit();
            ((ISupportInitialize)tbTopK).EndInit();
            ((ISupportInitialize)nudTopK).EndInit();
            ((ISupportInitialize)tbDepth).EndInit();
            ((ISupportInitialize)nudDepth).EndInit();
            ((ISupportInitialize)tbSpeed).EndInit();
            ((ISupportInitialize)nudSpeed).EndInit();
            ((ISupportInitialize)tbRepeatPenalty).EndInit();
            ((ISupportInitialize)nudRepeatPenalty).EndInit();
            ((ISupportInitialize)tbNumPredict).EndInit();
            ((ISupportInitialize)nudNumPredict).EndInit();
            ((ISupportInitialize)tbNumCtx).EndInit();
            ((ISupportInitialize)nudNumCtx).EndInit();
            ((ISupportInitialize)tbMirostatTau).EndInit();
            ((ISupportInitialize)nudMirostatTau).EndInit();
            ((ISupportInitialize)tbMirostatEta).EndInit();
            ((ISupportInitialize)nudMirostatEta).EndInit();
            ((ISupportInitialize)nudSeed).EndInit();
            ((ISupportInitialize)errorProvider).EndInit();
            ((ISupportInitialize)bindingSourceOptions).EndInit();
            tabMain.ResumeLayout(false);
            tabSimple.ResumeLayout(false);
            tabSimple.PerformLayout();
            tableSimple.ResumeLayout(false);
            tableSimple.PerformLayout();
            presetRow.ResumeLayout(false);
            presetRow.PerformLayout();
            depthRow.ResumeLayout(false);
            depthRow.PerformLayout();
            speedRow.ResumeLayout(false);
            speedRow.PerformLayout();
            grpQuickTest.ResumeLayout(false);
            tableQuickTest.ResumeLayout(false);
            tableQuickTest.PerformLayout();
            tableQuickActions.ResumeLayout(false);
            tabAdvanced.ResumeLayout(false);
            tableAdvanced.ResumeLayout(false);
            tableAdvanced.PerformLayout();
            tableAdvancedTop.ResumeLayout(false);
            grpSampling.ResumeLayout(false);
            grpSampling.PerformLayout();
            tableSampling.ResumeLayout(false);
            tableSampling.PerformLayout();
            grpLength.ResumeLayout(false);
            grpLength.PerformLayout();
            tableLength.ResumeLayout(false);
            tableLength.PerformLayout();
            tableAdvancedMid.ResumeLayout(false);
            grpAntiRepeat.ResumeLayout(false);
            grpAntiRepeat.PerformLayout();
            tableAntiRepeat.ResumeLayout(false);
            tableAntiRepeat.PerformLayout();
            grpMirostat.ResumeLayout(false);
            grpMirostat.PerformLayout();
            tableMirostat.ResumeLayout(false);
            tableMirostat.PerformLayout();
            tableAdvancedBottom.ResumeLayout(false);
            grpSystem.ResumeLayout(false);
            tableSystem.ResumeLayout(false);
            tableSystem.PerformLayout();
            tableStopRow.ResumeLayout(false);
            tableStopActions.ResumeLayout(false);
            tableStopActions.PerformLayout();
            grpDeterminism.ResumeLayout(false);
            grpDeterminism.PerformLayout();
            tableDeterminism.ResumeLayout(false);
            tableDeterminism.PerformLayout();
            commandBar.ResumeLayout(false);
            commandBar.PerformLayout();
            layoutRoot.ResumeLayout(false);
            layoutRoot.PerformLayout();
            ResumeLayout(false);
        }
        #endregion

        private TableLayoutPanel layoutRoot;
    }
}
//DONE
