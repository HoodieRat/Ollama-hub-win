//START
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.AI;
using OllamaSharp;
using MyOllamaHub3.Models;

namespace MyOllamaHub3
{
    public partial class ModelTuner : Form
    {
        private const string CustomPresetName = "Custom";

        private sealed record PresetDefinition(
            string Name,
            double Temperature,
            double TopP,
            int TopK,
            double RepeatPenalty,
            int NumPredict,
            int NumCtx,
            bool RandomSeedEachRun = true,
            int? Seed = null,
            int MirostatMode = 0,
            double MirostatTau = 5.0,
            double MirostatEta = 0.1,
            int DepthDial = 50,
            int SpeedDial = 50);

        private static readonly PresetDefinition[] PresetDefinitions =
        {
            new("Speed", 0.3, 0.8, 20, 1.15, 256, 4096, true, null, 0, 5.0, 0.1, 30, 85),
            new("Balanced", 0.7, 0.9, 40, 1.10, 512, 4096, true, null, 0, 5.0, 0.1, 50, 50),
            new("Depth", 1.0, 0.95, 60, 1.05, 1024, 8192, true, null, 0, 5.0, 0.1, 85, 30),
            new("Creative", 1.3, 0.98, 80, 1.02, 1024, 8192, true, null, 0, 5.0, 0.1, 75, 35),
            new("Deterministic", 0.2, 0.8, 20, 1.15, 512, 4096, false, 42, 0, 5.0, 0.1, 45, 25)
        };

        private static readonly Uri OllamaBase = new Uri("http://127.0.0.1:11434/");
        private static readonly JsonSerializerOptions JsonReadOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions JsonWriteOptions = new JsonSerializerOptions { WriteIndented = true };

        public OllamaOptions ResultOptions { get; private set; }

        private OllamaOptions _working;
        private CancellationTokenSource? _testCts;
        private bool _suppressEvents;

        public string? ActiveModelName { get; set; }

        public event Action<OllamaOptions>? OptionsApplied;

        public ModelTuner() : this(OllamaOptions.Default()) { }

        public ModelTuner(OllamaOptions options)
        {
            InitializeComponent();
            if (!IsDesignSurface(this))
                ThemeApplier.Apply(this);
            EnsurePresetItems();
            _working = (options ?? OllamaOptions.Default()).Clone();
            ResultOptions = _working.Clone();
            WireEvents();
            LoadFromOptions(_working);
        }

        private void WireEvents()
        {
            cmbPreset.SelectedIndexChanged += (_, __) =>
            {
                if (_suppressEvents) return;
                if (ApplyPresetBySelection())
                {
                    var selectedName = cmbPreset?.SelectedItem as string;
                    var appliedLabel = string.IsNullOrWhiteSpace(selectedName) ? "Preset" : selectedName!;
                    CommitOptions($"{appliedLabel} preset applied.");
                }
                else if (string.Equals(cmbPreset?.SelectedItem as string, CustomPresetName, StringComparison.OrdinalIgnoreCase))
                {
                    lblValidation.Text = "Custom tuning active.";
                }
            };

            btnDefaults.Click += btnDefaults_Click;
            btnApply.Click += btnApply_Click;
            btnOK.Click += btnOK_Click;
            btnCancel.Click += btnCancel_Click;
            btnExport.Click += btnExport_Click;
            btnImport.Click += btnImport_Click;
            btnTestSample.Click += async (_, __) => await RunQuickTestAsync();

            btnAddStopSequence.Click += btnAddStopSequence_Click;
            btnRemoveStopSequence.Click += btnRemoveStopSequence_Click;
            txtNewStopSequence.KeyDown += txtNewStopSequence_KeyDown;
            lstStopSequences.KeyDown += lstStopSequences_KeyDown;

            this.FormClosing += ModelTuner_FormClosing;

            // Sampling
            tbTemperature.Scroll += (_, __) => nudTemperature.Value = (decimal)(tbTemperature.Value / 100.0);
            nudTemperature.ValueChanged += (_, __) =>
            {
                tbTemperature.Value = Clamp((int)Math.Round((double)nudTemperature.Value * 100), tbTemperature.Minimum, tbTemperature.Maximum);
                OnWorkingValueChanged();
            };

            tbTopP.Scroll += (_, __) => nudTopP.Value = (decimal)(tbTopP.Value / 100.0);
            nudTopP.ValueChanged += (_, __) =>
            {
                tbTopP.Value = Clamp((int)Math.Round((double)nudTopP.Value * 100), tbTopP.Minimum, tbTopP.Maximum);
                OnWorkingValueChanged();
            };

            tbTopK.Scroll += (_, __) => nudTopK.Value = tbTopK.Value;
            nudTopK.ValueChanged += (_, __) =>
            {
                tbTopK.Value = Clamp((int)nudTopK.Value, tbTopK.Minimum, tbTopK.Maximum);
                OnWorkingValueChanged();
            };

            // Anti-repeat
            tbRepeatPenalty.Scroll += (_, __) => nudRepeatPenalty.Value = (decimal)(tbRepeatPenalty.Value / 100.0);
            nudRepeatPenalty.ValueChanged += (_, __) =>
            {
                tbRepeatPenalty.Value = Clamp((int)Math.Round((double)nudRepeatPenalty.Value * 100), tbRepeatPenalty.Minimum, tbRepeatPenalty.Maximum);
                OnWorkingValueChanged();
            };

            // Length & context
            tbNumPredict.Scroll += (_, __) => nudNumPredict.Value = tbNumPredict.Value;
            nudNumPredict.ValueChanged += (_, __) =>
            {
                tbNumPredict.Value = Clamp((int)nudNumPredict.Value, tbNumPredict.Minimum, tbNumPredict.Maximum);
                OnWorkingValueChanged();
            };

            tbNumCtx.Scroll += (_, __) => nudNumCtx.Value = tbNumCtx.Value;
            nudNumCtx.ValueChanged += (_, __) =>
            {
                tbNumCtx.Value = Clamp((int)nudNumCtx.Value, tbNumCtx.Minimum, tbNumCtx.Maximum);
                OnWorkingValueChanged();
            };

            // Mirostat
            cmbMirostat.SelectedIndexChanged += (_, __) => OnWorkingValueChanged();
            tbMirostatTau.Scroll += (_, __) => nudMirostatTau.Value = (decimal)(tbMirostatTau.Value / 10.0);
            nudMirostatTau.ValueChanged += (_, __) =>
            {
                tbMirostatTau.Value = Clamp((int)Math.Round((double)nudMirostatTau.Value * 10), tbMirostatTau.Minimum, tbMirostatTau.Maximum);
                OnWorkingValueChanged();
            };

            tbMirostatEta.Scroll += (_, __) => nudMirostatEta.Value = (decimal)(tbMirostatEta.Value / 1000.0);
            nudMirostatEta.ValueChanged += (_, __) =>
            {
                tbMirostatEta.Value = Clamp((int)Math.Round((double)nudMirostatEta.Value * 1000), tbMirostatEta.Minimum, tbMirostatEta.Maximum);
                OnWorkingValueChanged();
            };

            // Determinism
            chkRandomSeedEachRun.CheckedChanged += (_, __) =>
            {
                nudSeed.Enabled = !chkRandomSeedEachRun.Checked;
                OnWorkingValueChanged();
            };

            nudSeed.ValueChanged += (_, __) => OnWorkingValueChanged();
        }

        private void OnWorkingValueChanged()
        {
            if (_suppressEvents) return;
            if (!TryCommit()) return;

            _working = ResultOptions.Clone();
            UpdatePresetSelectionFromOptions(ResultOptions);
            OptionsApplied?.Invoke(ResultOptions.Clone());
        }

        private bool ApplyPresetBySelection()
        {
            if (_suppressEvents) return false;

            var name = cmbPreset?.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(name) || string.Equals(name, CustomPresetName, StringComparison.OrdinalIgnoreCase))
                return false;

            var preset = GetPresetByName(name);
            if (preset == null)
                return false;

            ApplyPresetDefinition(preset);
            return true;
        }

        private void ApplyPresetDefinition(PresetDefinition preset)
        {
            _suppressEvents = true;
            try
            {
                SetSampling(preset.Temperature, preset.TopP, preset.TopK, preset.RepeatPenalty);
                SetLenCtx(preset.NumPredict, preset.NumCtx);
                SetMirostatControls(preset.MirostatMode, preset.MirostatTau, preset.MirostatEta);

                if (preset.RandomSeedEachRun)
                {
                    chkRandomSeedEachRun.Checked = true;
                    nudSeed.Value = 0;
                }
                else
                {
                    chkRandomSeedEachRun.Checked = false;
                    var seedValue = preset.Seed ?? 0;
                    seedValue = Clamp(seedValue, (int)nudSeed.Minimum, (int)nudSeed.Maximum);
                    nudSeed.Value = seedValue;
                }

                SetSimpleDialValues(preset.DepthDial, preset.SpeedDial);
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private void SetSampling(double temperature, double topP, int topK, double repeatPenalty)
        {
            nudTemperature.Value = (decimal)temperature;
            nudTopP.Value = (decimal)topP;
            nudTopK.Value = topK;
            nudRepeatPenalty.Value = (decimal)repeatPenalty;
        }

        private void SetLenCtx(int numPredict, int numCtx)
        {
            numPredict = Math.Max(tbNumPredict.Minimum, Math.Min(tbNumPredict.Maximum, numPredict));
            numCtx = Math.Max(tbNumCtx.Minimum, Math.Min(tbNumCtx.Maximum, numCtx));
            nudNumPredict.Value = numPredict;
            nudNumCtx.Value = numCtx;
        }

        private void SetMirostatControls(int mode, double tau, double eta)
        {
            var label = mode switch
            {
                1 => "V1 (1)",
                2 => "V2 (2)",
                _ => "Off (0)"
            };

            if (!string.Equals(cmbMirostat.SelectedItem as string, label, StringComparison.Ordinal))
                cmbMirostat.SelectedItem = label;

            nudMirostatTau.Value = (decimal)tau;
            nudMirostatEta.Value = (decimal)eta;
        }

        private void EnsurePresetItems()
        {
            if (cmbPreset == null) return;

            if (!ContainsPreset(CustomPresetName))
            {
                cmbPreset.Items.Insert(0, CustomPresetName);
            }
        }

        private bool ContainsPreset(string name)
        {
            if (cmbPreset == null) return false;

            foreach (var item in cmbPreset.Items)
            {
                if (string.Equals(item?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private PresetDefinition? GetPresetByName(string? name)
            => PresetDefinitions.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

        private PresetDefinition? DetectPreset(OllamaOptions options)
        {
            foreach (var preset in PresetDefinitions)
            {
                if (!Approximately(options.Temperature, preset.Temperature, 0.05)) continue;
                if (!Approximately(options.TopP, preset.TopP, 0.05)) continue;
                if (!Approximately(options.TopK ?? preset.TopK, preset.TopK, 2)) continue;
                if (!Approximately(options.RepeatPenalty ?? preset.RepeatPenalty, preset.RepeatPenalty, 0.05)) continue;
                if (!Approximately(options.NumPredict, preset.NumPredict, 64)) continue;
                if (!Approximately(options.NumCtx ?? preset.NumCtx, preset.NumCtx, 256)) continue;
                if ((options.Mirostat ?? 0) != preset.MirostatMode) continue;

                var expectsRandom = preset.RandomSeedEachRun;
                if (expectsRandom)
                {
                    if (options.Seed.HasValue) continue;
                }
                else
                {
                    if (!options.Seed.HasValue || options.Seed.Value != preset.Seed) continue;
                }

                return preset;
            }

            return null;
        }

        private void UpdatePresetSelectionFromOptions(OllamaOptions options)
        {
            EnsurePresetItems();

            var matched = DetectPreset(options);
            var targetName = matched?.Name ?? CustomPresetName;

            _suppressEvents = true;
            try
            {
                SetSelectedPreset(targetName);
                if (matched != null)
                {
                    SetSimpleDialValues(matched.DepthDial, matched.SpeedDial);
                }
                else
                {
                    SetSimpleDialValuesForCustom(options);
                }
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private void SetSelectedPreset(string name)
        {
            if (cmbPreset == null) return;

            foreach (var item in cmbPreset.Items)
            {
                if (string.Equals(item?.ToString(), name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(cmbPreset.SelectedItem as string, item?.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        cmbPreset.SelectedItem = item;
                    }
                    return;
                }
            }

            var index = cmbPreset.Items.Add(name);
            cmbPreset.SelectedIndex = index;
        }

        private void SetSimpleDialValues(int depthValue, int speedValue)
        {
            depthValue = Clamp(depthValue, tbDepth.Minimum, tbDepth.Maximum);
            speedValue = Clamp(speedValue, tbSpeed.Minimum, tbSpeed.Maximum);

            if (tbDepth.Value != depthValue)
                tbDepth.Value = depthValue;
            if ((int)nudDepth.Value != depthValue)
                nudDepth.Value = depthValue;

            if (tbSpeed.Value != speedValue)
                tbSpeed.Value = speedValue;
            if ((int)nudSpeed.Value != speedValue)
                nudSpeed.Value = speedValue;
        }

        private void SetSimpleDialValuesForCustom(OllamaOptions options)
        {
            var depth = ScaleToPercentage(options.NumPredict, (int)nudNumPredict.Minimum, Math.Min(1024, (int)nudNumPredict.Maximum));
            var speed = ScaleToPercentage(options.Temperature, 0.2, 1.3, invert: true);
            SetSimpleDialValues(depth, speed);
        }

        private static int ScaleToPercentage(double value, double min, double max, bool invert = false)
        {
            if (max <= min)
                return 50;

            var clamped = Math.Clamp(value, min, max);
            var ratio = (clamped - min) / (max - min);
            if (invert)
                ratio = 1.0 - ratio;

            return Clamp((int)Math.Round(ratio * 100), 0, 100);
        }

        private static bool Approximately(double value, double target, double tolerance)
            => Math.Abs(value - target) <= tolerance;

        private static bool Approximately(int value, int target, int tolerance)
            => Math.Abs(value - target) <= tolerance;

        private static bool Approximately(int? value, int target, int tolerance)
            => value.HasValue && Approximately(value.Value, target, tolerance);

        private void LoadFromOptions(OllamaOptions o)
        {
            if (o == null) return;

            _suppressEvents = true;
            try
            {
                errorProvider.Clear();

                // Advanced controls
                nudTemperature.Value = (decimal)o.Temperature;
                nudTopP.Value = (decimal)o.TopP;
                nudTopK.Value = Clamp(o.TopK ?? 40, (int)nudTopK.Minimum, (int)nudTopK.Maximum);
                nudRepeatPenalty.Value = (decimal)(o.RepeatPenalty ?? 1.1);

                nudNumPredict.Value = Clamp(o.NumPredict, (int)nudNumPredict.Minimum, (int)nudNumPredict.Maximum);
                nudNumCtx.Value = Clamp(o.NumCtx ?? 4096, (int)nudNumCtx.Minimum, (int)nudNumCtx.Maximum);

                cmbMirostat.SelectedItem = (o.Mirostat ?? 0) switch { 1 => "V1 (1)", 2 => "V2 (2)", _ => "Off (0)" };
                nudMirostatTau.Value = (decimal)(o.MirostatTau ?? 5.0);
                nudMirostatEta.Value = (decimal)(o.MirostatEta ?? 0.1);

                if (o.Seed.HasValue && o.Seed.Value >= 0)
                {
                    chkRandomSeedEachRun.Checked = false;
                    nudSeed.Value = Clamp(o.Seed.Value, (int)nudSeed.Minimum, (int)nudSeed.Maximum);
                }
                else
                {
                    chkRandomSeedEachRun.Checked = true;
                    nudSeed.Value = 0;
                }

                txtSystemPrompt.Text = o.SystemPrompt ?? string.Empty;

                lstStopSequences.Items.Clear();
                if (o.StopSequences != null)
                {
                    foreach (var entry in o.StopSequences)
                    {
                        if (string.IsNullOrWhiteSpace(entry)) continue;
                        lstStopSequences.Items.Add(entry);
                    }
                }

                txtNewStopSequence.Clear();
            }
            finally
            {
                _suppressEvents = false;
            }

            ResultOptions = o.Clone();
            UpdatePresetSelectionFromOptions(o);
        }

        private bool TryCommit()
        {
            errorProvider.Clear();

            var temperature = (double)nudTemperature.Value;
            var topP = (double)nudTopP.Value;
            var topK = (int)nudTopK.Value;
            var repeatPenalty = (double)nudRepeatPenalty.Value;
            var numPredict = (int)nudNumPredict.Value;
            var numCtx = (int)nudNumCtx.Value;

            var mirostat = cmbMirostat.SelectedItem as string;
            int mirostatMode = 0;
            if (mirostat == "V1 (1)") mirostatMode = 1;
            else if (mirostat == "V2 (2)") mirostatMode = 2;

            var tau = (double)nudMirostatTau.Value;
            var eta = (double)nudMirostatEta.Value;

            int? seed = chkRandomSeedEachRun.Checked ? (int?)null : (int)nudSeed.Value;

            if (temperature < 0 || temperature > 2.0) return Fail(nudTemperature, "Temperature must be 0–2.");
            if (topP < 0 || topP > 1.0) return Fail(nudTopP, "TopP must be 0–1.");
            if (repeatPenalty < 0.5 || repeatPenalty > 2.0) return Fail(nudRepeatPenalty, "Repeat penalty looks odd.");
            if (numPredict < 1) return Fail(nudNumPredict, "num_predict must be >= 1.");
            if (numCtx < 256) return Fail(nudNumCtx, "num_ctx must be >= 256.");

            var systemPrompt = string.IsNullOrWhiteSpace(txtSystemPrompt.Text) ? null : txtSystemPrompt.Text.Trim();
            var stopSequences = GetStopSequencesFromUi();

            ResultOptions = new OllamaOptions
            {
                Temperature = temperature,
                TopP = topP,
                TopK = topK,
                RepeatPenalty = repeatPenalty,
                NumPredict = numPredict,
                NumCtx = numCtx,
                Seed = seed,
                Mirostat = mirostatMode,
                MirostatTau = tau,
                MirostatEta = eta,
                SystemPrompt = systemPrompt,
                StopSequences = stopSequences
            };
            lblValidation.Text = string.Empty;
            return true;
        }

        private bool Fail(Control c, string message)
        {
            errorProvider.SetError(c, message);
            lblValidation.Text = message;
            return false;
        }

        private List<string> GetStopSequencesFromUi()
        {
            if (lstStopSequences == null || lstStopSequences.Items.Count == 0)
                return new List<string>();

            return lstStopSequences.Items
                .Cast<object>()
                .Select(item => item?.ToString()?.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.Ordinal)
                .ToList()!;
        }

        private bool CommitOptions(string? statusMessage)
        {
            if (!TryCommit()) return false;

            _working = ResultOptions.Clone();

            UpdatePresetSelectionFromOptions(ResultOptions);

            OptionsApplied?.Invoke(ResultOptions.Clone());

            if (!string.IsNullOrWhiteSpace(statusMessage))
                lblValidation.Text = statusMessage;
            else
                lblValidation.Text = "Changes applied.";

            return true;
        }

        private void btnDefaults_Click(object? sender, EventArgs e)
        {
            _working = OllamaOptions.Default();
            LoadFromOptions(_working);
            CommitOptions("Defaults restored.");
        }

        private void btnApply_Click(object? sender, EventArgs e)
        {
            CommitOptions("Changes applied.");
        }

        private void btnOK_Click(object? sender, EventArgs e)
        {
            if (CommitOptions(null))
                DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            CancelQuickTest();
            DialogResult = DialogResult.Cancel;
        }

        private void btnExport_Click(object? sender, EventArgs e) => ExportJson();

        private void btnImport_Click(object? sender, EventArgs e)
        {
            var status = ImportJson();
            if (!string.IsNullOrWhiteSpace(status))
                CommitOptions(status);
        }

        private void btnAddStopSequence_Click(object? sender, EventArgs e)
        {
            var value = txtNewStopSequence.Text?.Trim();
            if (string.IsNullOrWhiteSpace(value)) return;

            if (!lstStopSequences.Items.Cast<object>().Any(item => string.Equals(item?.ToString(), value, StringComparison.Ordinal)))
            {
                lstStopSequences.Items.Add(value);
            }

            txtNewStopSequence.Clear();
            CommitOptions("Stop sequence added.");
        }

        private void btnRemoveStopSequence_Click(object? sender, EventArgs e)
        {
            if (lstStopSequences.SelectedItems.Count == 0) return;

            while (lstStopSequences.SelectedItems.Count > 0)
            {
                var selected = lstStopSequences.SelectedItems[0];
                if (selected is null) break;
                lstStopSequences.Items.Remove(selected);
            }

            CommitOptions("Stop sequence removed.");
        }

        private void txtNewStopSequence_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            btnAddStopSequence_Click(sender, EventArgs.Empty);
        }

        private void lstStopSequences_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                e.Handled = true;
                btnRemoveStopSequence_Click(sender, EventArgs.Empty);
            }
        }

        private void ExportJson()
        {
            if (!CommitOptions(null)) return;

            using var sfd = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = "ollama-options.json" };
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    var json = JsonSerializer.Serialize(ResultOptions, JsonWriteOptions);
                    File.WriteAllText(sfd.FileName, json);
                    lblValidation.Text = $"Exported to {sfd.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string? ImportJson()
        {
            using var ofd = new OpenFileDialog { Filter = "JSON (*.json)|*.json" };
            if (ofd.ShowDialog(this) != DialogResult.OK) return null;

            try
            {
                var json = File.ReadAllText(ofd.FileName);
                var o = JsonSerializer.Deserialize<OllamaOptions>(json, JsonReadOptions);
                if (o == null) throw new InvalidDataException("The selected file does not contain valid options.");

                _working = o.Clone();
                LoadFromOptions(_working);
                ResultOptions = _working.Clone();

                var fileName = Path.GetFileName(ofd.FileName);
                return $"Imported from {fileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async Task RunQuickTestAsync()
        {
            if (!CommitOptions(null)) return;

            var prompt = txtSamplePrompt.Text?.Trim();
            if (string.IsNullOrWhiteSpace(prompt))
            {
                lblValidation.Text = "Enter a sample prompt first.";
                return;
            }

            var model = ActiveModelName;
            if (string.IsNullOrWhiteSpace(model))
            {
                lblValidation.Text = "Select an active model in the main window before running a quick test.";
                return;
            }

            CancelQuickTest();
            _testCts = new CancellationTokenSource();
            var token = _testCts.Token;

            btnTestSample.Enabled = false;
            progressTest.Style = ProgressBarStyle.Marquee;
            progressTest.MarqueeAnimationSpeed = 30;
            progressTest.Value = 0;
            rtbSampleOutput.Clear();
            lblValidation.Text = $"Testing '{model}'...";

            try
            {
                var history = new List<ChatMessage>();
                if (!string.IsNullOrWhiteSpace(ResultOptions.SystemPrompt))
                    history.Add(new ChatMessage(ChatRole.System, ResultOptions.SystemPrompt));

                history.Add(new ChatMessage(ChatRole.User, prompt));

                IChatClient chatClient = new OllamaApiClient(OllamaBase, model);
                var options = ResultOptions.ToChatOptions(out var optionsWarning);
                var totalChars = 0;
                DateTimeOffset? lastDeltaAtUtc = null;

                await foreach (var update in chatClient.GetStreamingResponseAsync(history, options: options, cancellationToken: token))
                {
                    var delta = StreamingUpdateHelper.ExtractText(update);
                    if (string.IsNullOrEmpty(delta)) continue;

                    totalChars += delta.Length;
                    AppendSampleOutput(delta);
                    lastDeltaAtUtc = DateTimeOffset.UtcNow;
                }

                var statusMessages = new List<string>();
                if (!string.IsNullOrWhiteSpace(optionsWarning))
                    statusMessages.Add(optionsWarning);

                if (totalChars > 0)
                {
                    var message = $"Quick test completed ({totalChars} chars).";
                    if (lastDeltaAtUtc.HasValue)
                        message += $" Last delta at {lastDeltaAtUtc:HH:mm:ss} UTC.";
                    statusMessages.Add(message);
                }
                else
                {
                    var lastInfo = lastDeltaAtUtc.HasValue
                        ? $"last delta at {lastDeltaAtUtc:HH:mm:ss} UTC"
                        : "no streaming tokens were received";
                    statusMessages.Add($"Ollama ({model}) returned no content; {lastInfo}.");
                }

                lblValidation.Text = string.Join(" ", statusMessages);
            }
            catch (OperationCanceledException)
            {
                lblValidation.Text = "Quick test canceled.";
            }
            catch (HttpRequestException ex)
            {
                lblValidation.Text = $"Quick test failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                lblValidation.Text = $"Quick test error: {ex.Message}";
            }
            finally
            {
                progressTest.MarqueeAnimationSpeed = 0;
                progressTest.Style = ProgressBarStyle.Blocks;
                progressTest.Value = 0;
                btnTestSample.Enabled = true;
                _testCts?.Dispose();
                _testCts = null;
            }
        }

        private void AppendSampleOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (rtbSampleOutput.InvokeRequired)
            {
                rtbSampleOutput.BeginInvoke(new Action<string>(AppendSampleOutput), text);
                return;
            }

            rtbSampleOutput.AppendText(text);
            rtbSampleOutput.ScrollToCaret();
        }

        private void CancelQuickTest()
        {
            var cts = _testCts;
            if (cts == null) return;
            if (!cts.IsCancellationRequested)
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { }
            }
        }

        private void ModelTuner_FormClosing(object? sender, FormClosingEventArgs e) => CancelQuickTest();

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        private static bool IsDesignSurface(Control? control)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            while (control != null)
            {
                if (control.Site?.DesignMode == true)
                    return true;
                control = control.Parent;
            }

            return false;
        }
    }
}
//DONE
