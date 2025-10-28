using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Extensions.AI;
using OllamaSharp;
using MyOllamaHub3.Models;
using MyOllamaHub3.Services;

namespace MyOllamaHub3
{
    public partial class Form1 : Form
    {
        private const string OllamaBase = "http://localhost:11434/";
        private const string DefaultPersonality = "Helpful Assistant";
        private const int DefaultHistoryTurnLimit = 12;
        private const int DefaultContinuationAttempts = 2;
        private const int MaxDiagnosticEntries = 200;
        private const int ContinuationMinPredict = 64;
        private const int ContinuationMaxPredict = 2048;
        private const int ContinuationMinCtx = 4096;
        private const int ContinuationMaxCtx = 32768;
        private const int AutoModelRoutingAttempts = 2;

        private static readonly TimeSpan PromptShortcutDebounce = TimeSpan.FromMilliseconds(350);
        private static readonly TimeSpan SendPromptTimeout = TimeSpan.FromSeconds(120);
        private static readonly TimeSpan AutoModelRoutingTimeout = TimeSpan.FromSeconds(12);
        private static readonly TimeSpan AutoModelRoutingRetryDelay = TimeSpan.FromSeconds(2);

        private static readonly string[] CreativeKeywords =
        {
            "brainstorm", "creative", "story", "plot", "poem", "slogan", "idea", "narrative", "tagline", "lyrics", "script"
        };

        private static readonly string[] CodingKeywords =
        {
            "code", "bug", "exception", "stack trace", "refactor", "function", "class", "api", "compile", "debug"
        };

        private static readonly string[] ResearchKeywords =
        {
            "research", "study", "whitepaper", "dataset", "evidence", "cite", "source", "statistics", "analysis", "report"
        };

        private static readonly string[] LongFormKeywords =
        {
            "article", "essay", "chapter", "documentation", "long form", "white paper", "guide", "tutorial", "transcript"
        };

        private static readonly string[] FastKeywords =
        {
            "quick", "summary", "summarize", "tl;dr", "short", "fast", "bullet", "recap"
        };

        private static readonly string[] AnalyticalKeywords =
        {
            "analyse", "analyze", "compare", "contrast", "reason", "evaluate", "explain", "logic"
        };

        private static readonly string[] AccuracyKeywords =
        {
            "accuracy", "accurate", "precise", "fact", "factual", "verify", "double-check", "truth"
        };

        private static readonly Regex CollapseWhitespaceRegex = new Regex("\\s+", RegexOptions.Compiled);
        private static readonly Regex TruncatedTailRegex = new Regex(@"(\\.\\.\\.|…|--|—)$", RegexOptions.Compiled);
        private static readonly Regex DiagnosticsUrlRegex = new Regex(@"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly HttpClient Http = new HttpClient { BaseAddress = new Uri(OllamaBase) };
    private static readonly JsonSerializerOptions ListTagsSerializerOptions = new() { PropertyNameCaseInsensitive = true };

        private static readonly IReadOnlyList<string> PersonalityPresetOrder = new[]
        {
            "Helpful Assistant",
            "Creative Partner",
            "Code Review Buddy",
            "Debate Coach",
            "Product Manager"
        };

        private static readonly Dictionary<string, string> DefaultPersonalityPrompts = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Helpful Assistant"] = "You are a concise but friendly assistant who cites tools when used.",
            ["Creative Partner"] = "Adopt a playful tone and help the user ideate and explore novel angles.",
            ["Code Review Buddy"] = "Act as a precise reviewer. Highlight bugs, edge cases, and suggest improvements.",
            ["Debate Coach"] = "Challenge the user respectfully, pointing out flaws and asking guiding questions.",
            ["Product Manager"] = "Summarise requirements, clarify stakeholders, and negotiate next steps."
        };

    private readonly Dictionary<string, string> _personalityPresets = new(DefaultPersonalityPrompts, StringComparer.OrdinalIgnoreCase);
    private readonly IReadOnlyList<GenerationPreset> _generationPresets = ModelCatalog.GenerationPresets;
        private readonly List<ChatMessage> _chatHistory = new();
        private readonly DiagnosticsBuffer _diagnosticsEntries = new(MaxDiagnosticEntries);
        private readonly ToolRegistry _toolRegistry = new();
        private readonly SessionManager _session = new();
        private readonly List<SessionRecord> _sessions = new();
        private readonly object _settingsWriteLock = new();
        private readonly object _sessionsWriteLock = new();
        private readonly object _chatClientLock = new();
        private readonly object _autoModelSelectionLock = new();
        private readonly SemaphoreSlim _modelRefreshLock = new(1, 1);
        private readonly WarmupManager _warmupManager;
        private readonly DebouncedSaver _settingsSaveDebouncer;
        private readonly DebouncedSaver _sessionSaveDebouncer;
        private readonly System.Windows.Forms.Timer _warmupAnimationTimer;

        private ThemedMarkdownView markdownView = null!;
        private OllamaOptions _options = OllamaOptions.Default();
        private string? _tunerSystemPrompt;
        private string? _selectedPersonality = DefaultPersonality;
        private string? _lastAutoSelectedModel;
        private string? _cachedChatModel;
        private string? _currentSessionId;
        private string _settingsPath = string.Empty;
        private string _sessionStorePath = string.Empty;
        private string? _pendingSettingsJson;
        private string? _pendingSessionsJson;
        private string? _lastSavedSettingsJson;
        private CancellationTokenSource? _cts;
        private CancellationTokenSource? _autoModelSelectionCts;
        private Task _autoModelSelectionTask = Task.CompletedTask;
        private IChatClient? _cachedChatClient;
        private WarmupState _warmupState = WarmupState.Hidden;
        private string? _warmupBaseText;
        private DateTime _lastPromptSubmitUtc;
    private bool _autoModelSelectionEnabled;
        private bool _suppressSessionSelection;
        private bool _suppressSettingsSave;
        private bool _sendInProgress;
    private bool _enableWebTrace;
        private int _historyTurnLimit = DefaultHistoryTurnLimit;
        private int _maxContinuationAttempts = DefaultContinuationAttempts;
        private int _modelSortColumn = -1;
        private int _warmupEllipsisPhase;
        private SortOrder _modelSortOrder = SortOrder.None;
        private int? _modelTableSnapshotHash;
    private bool _modelTableUsingFallback;
        private int? _installedModelsHash;
    private bool _suppressGenerationPresetEvents;
    private OllamaOptions? _lastAdvancedOptions;
    private bool _pendingInitialTranscriptRefresh = true;

        public Form1()
        {
            InitializeComponent();
            ThemeApplier.Apply(this);

            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyOllamaHub3");
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            _settingsPath = Path.Combine(appData, "settings.json");
            _sessionStorePath = Path.Combine(appData, "sessions.json");

            _settingsSaveDebouncer = new DebouncedSaver(TimeSpan.FromMilliseconds(600), SaveSettingsJsonAsync, ex => LogBackgroundError($"Settings save failed: {ex.Message}"));
            _sessionSaveDebouncer = new DebouncedSaver(TimeSpan.FromMilliseconds(600), SaveSessionsJsonAsync, ex => LogBackgroundError($"Session save failed: {ex.Message}"));

            _warmupAnimationTimer = new System.Windows.Forms.Timer { Interval = 350 };
            _warmupAnimationTimer.Tick += WarmupAnimationTimer_Tick;

            _warmupManager = new WarmupManager(WarmModelAsync);
            _warmupManager.StateChanged += WarmupManager_StateChanged;

            InitializeDiagnosticsUi();
            InitializeModelClassificationUi();
            InitializeTools();

            if (modelsComboBox != null)
            {
                modelsComboBox.DisplayMember = nameof(ModelListItem.DisplayText);
                modelsComboBox.ValueMember = nameof(ModelListItem.ModelId);
                modelsComboBox.SelectedIndexChanged += ModelsComboBox_SelectedIndexChanged;
            }

            if (generationPresetComboBox != null)
            {
                generationPresetComboBox.DisplayMember = nameof(GenerationPreset.Name);
                generationPresetComboBox.SelectedIndexChanged += GenerationPresetComboBox_SelectedIndexChanged;
            }

            UpdateSessionMenuState();

            FormClosing += Form1_FormClosing;
        }

        private async void sendPromptBtn_Click(object? sender, EventArgs e)
        {
            try
            {
                await TrySendPromptAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                LogBackgroundError($"Send failed: {ex.Message}");
            }
        }

        private async Task TrySendPromptAsync()
        {
            if (_sendInProgress)
            {
                SetDiagnostics("A request is already in progress. Cancel it or wait for it to complete.");
                return;
            }

            if (userPromptTxt == null)
            {
                SetDiagnostics("Prompt input is unavailable.");
                return;
            }

            var rawPrompt = userPromptTxt.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rawPrompt))
            {
                SetDiagnostics("Enter a prompt before sending.");
                return;
            }

            var prompt = rawPrompt.Trim();
            if (!TryEnsureActiveModel(out var model))
                return;

            EnsureSystemMessage();

            _sendInProgress = true;
            DisableUiDuringSend(true);

            _cts = new CancellationTokenSource();
            using var timeoutCts = new CancellationTokenSource(SendPromptTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, timeoutCts.Token);
            var cancellationToken = linkedCts.Token;

            var stopwatch = Stopwatch.StartNew();
            var sendCompleted = false;
            var cancelToolLogged = false;
            DateTime? lastDeltaAtUtc = null;
            var combinedResponse = new StringBuilder();

            try
            {
                var toolProcessing = await ProcessToolCommandsAsync(prompt, cancellationToken).ConfigureAwait(true);

                if (toolProcessing.Invocations.Count > 0)
                {
                    foreach (var invocation in toolProcessing.Invocations)
                    {
                        AppendToolMessageToView(invocation.ToolName, invocation.Output, invocation.Success);
                        _session.AppendTool(invocation.ToolName, invocation.Output, invocation.Success);
                        _chatHistory.Add(new ChatMessage(ChatRole.System, invocation.SystemMessage));
                    }
                }

                if (!string.IsNullOrWhiteSpace(toolProcessing.Diagnostics))
                    SetDiagnostics(toolProcessing.Diagnostics);

                prompt = toolProcessing.Prompt;
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    sendCompleted = true;
                    markdownView.EndAssistantMessage();
                    return;
                }

                _chatHistory.Add(new ChatMessage(ChatRole.User, prompt));
                _session.AppendUser(prompt, prompt);
                AppendUserMessageToView(prompt);

                userPromptTxt.Clear();

                var client = GetOrCreateChatClient(model);
                var options = MakeChatOptions(_options, out var optionWarning);
                if (!string.IsNullOrWhiteSpace(optionWarning))
                    SetDiagnostics(optionWarning);

                var historySnapshot = new List<ChatMessage>(_chatHistory);

                await foreach (var update in client.GetStreamingResponseAsync(historySnapshot, options: options, cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    var delta = StreamingUpdateHelper.ExtractText(update);
                    if (string.IsNullOrEmpty(delta))
                        continue;

                    lastDeltaAtUtc = DateTime.UtcNow;
                    combinedResponse.Append(delta);
                    AppendAssistantDelta(delta);
                }

                markdownView.EndAssistantMessage();

                sendCompleted = true;

                var finalResponseRaw = combinedResponse.ToString();
                var finalResponse = StreamingUpdateHelper.StripHiddenSections(finalResponseRaw);
                if (!string.IsNullOrEmpty(finalResponse))
                {
                    _chatHistory.Add(new ChatMessage(ChatRole.Assistant, finalResponse));
                    UpdateAssistantTranscripts(finalResponse);
                }
                else
                {
                    UpdateAssistantTranscripts(string.Empty);
                }

                stopwatch.Stop();

                var chars = finalResponse.Length;
                if (chars > 0)
                {
                    var seconds = Math.Max(0.01, stopwatch.Elapsed.TotalSeconds);
                    var estimatedTokensPerSecond = (chars / 4.0) / seconds;
                    ModelClassifier.UpdateMeasurement(model, estimatedTokensPerSecond);
                    _modelTableSnapshotHash = null;
                    RefreshModelTableEntries();
                }

                if (chars > 0)
                {
                    var message = $"Completed in {stopwatch.ElapsedMilliseconds} ms ({chars} chars).";
                    if (lastDeltaAtUtc.HasValue)
                        message += $" Last delta at {lastDeltaAtUtc:HH:mm:ss} UTC.";
                    SetDiagnostics(message);
                }
                else
                {
                    var info = lastDeltaAtUtc.HasValue
                        ? $"last delta at {lastDeltaAtUtc:HH:mm:ss} UTC"
                        : "no streaming tokens were received";
                    SetDiagnostics($"Ollama ({model}) returned no content after {stopwatch.ElapsedMilliseconds} ms; {info}.");
                }
            }
            catch (TaskCanceledException)
            {
                if (_cts != null && _cts.IsCancellationRequested && !timeoutCts.IsCancellationRequested)
                    throw;

                InvalidateChatClient();

                if (timeoutCts.IsCancellationRequested && (_cts == null || !_cts.IsCancellationRequested))
                {
                    var limit = SendPromptTimeout.TotalSeconds.ToString("F0", CultureInfo.InvariantCulture);
                    SetDiagnostics($"Request exceeded the {limit}s processing limit and was canceled. Try simplifying the prompt or retrying.");
                }
                else
                {
                    SetDiagnostics("Ollama request timed out. Connection reset; try again.");
                }
            }
            catch (OperationCanceledException)
            {
                if (!cancelToolLogged)
                {
                    LogToolMessage("System", "Request canceled before completion; no assistant response recorded.", false);
                    cancelToolLogged = true;
                }
                SetDiagnostics("Request canceled.");
            }
            catch (TimeoutException)
            {
                InvalidateChatClient();
                SetDiagnostics("The request timed out. Connection reset; try again or adjust timeout settings.");
            }
            catch (ObjectDisposedException)
            {
                InvalidateChatClient();
                SetDiagnostics("Ollama connection became invalid. Resetting client; please retry.");
            }
            catch (HttpRequestException hex)
            {
                InvalidateChatClient();
                SetDiagnostics($"Could not reach Ollama at {OllamaBase}. {hex.Message} (connection reset)");
            }
            catch (Exception ex)
            {
                InvalidateChatClient();
                SetDiagnostics($"Unexpected error: {ex.Message}");
            }
            finally
            {
                stopwatch.Stop();
                CleanupAfterSend(sendCompleted);
            }
        }

        private static bool IsLikelyUncensoredModelName(string? modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return false;

            var normalized = modelName.Trim().ToLowerInvariant();
            return normalized.Contains("uncensored") ||
                   normalized.Contains("ungated") ||
                   normalized.Contains("nolimit") ||
                   normalized.Contains("no-guard") ||
                   normalized.Contains("devv") ||
                   normalized.Contains("raw");
        }

        [Flags]
        private enum PromptIntent
        {
            None = 0,
            Analytical = 1 << 0,
            Creative = 1 << 1,
            Accuracy = 1 << 2,
            Coding = 1 << 3,
            LongForm = 1 << 4,
            FastTurnaround = 1 << 5
        }

        private void EnsureSystemMessage()
        {
            var previousPrompt = _options.SystemPrompt;
            var systemPrompt = ComposeSystemPrompt();
            _options.SystemPrompt = systemPrompt;

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                var firstMatch = -1;
                for (var i = 0; i < _chatHistory.Count; i++)
                {
                    var message = _chatHistory[i];
                    if (message.Role != ChatRole.System)
                        continue;

                    if (string.Equals(message.Text, systemPrompt, StringComparison.Ordinal))
                    {
                        if (firstMatch == -1)
                        {
                            firstMatch = i;
                        }
                        else
                        {
                            _chatHistory.RemoveAt(i);
                            i--;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(previousPrompt) && string.Equals(message.Text, previousPrompt, StringComparison.Ordinal))
                    {
                        _chatHistory.RemoveAt(i);
                        i--;
                    }
                }

                if (firstMatch == -1)
                {
                    _chatHistory.Insert(0, new ChatMessage(ChatRole.System, systemPrompt));
                    firstMatch = 0;
                }
                else if (firstMatch != 0)
                {
                    var message = _chatHistory[firstMatch];
                    _chatHistory.RemoveAt(firstMatch);
                    _chatHistory.Insert(0, message);
                    firstMatch = 0;
                }
            }
            else if (!string.IsNullOrWhiteSpace(previousPrompt))
            {
                for (var i = _chatHistory.Count - 1; i >= 0; i--)
                {
                    var message = _chatHistory[i];
                    if (message.Role != ChatRole.System)
                        continue;

                    if (string.Equals(message.Text, previousPrompt, StringComparison.Ordinal))
                        _chatHistory.RemoveAt(i);
                }
            }

            TrimChatHistory();
        }

        private void ApplyModelOptions(OllamaOptions newOptions, string diagnosticsMessage)
        {
            if (newOptions == null) return;

            _options = newOptions.Clone();
            _tunerSystemPrompt = _options.SystemPrompt;

            UpdateAdvancedPresetSnapshot(_options);
            UpdateGenerationPresetSelectionFromOptions(_options);

            EnsureSystemMessage();
            RefreshModelTableEntries();

            if (!string.IsNullOrWhiteSpace(diagnosticsMessage))
                SetDiagnostics(diagnosticsMessage);

            SaveAppSettings();
        }

        private string? ComposeSystemPrompt()
        {
            var builder = new System.Text.StringBuilder();

            if (!string.IsNullOrWhiteSpace(_tunerSystemPrompt))
            {
                builder.Append(_tunerSystemPrompt.Trim());
            }

            var personaKey = _selectedPersonality;
            if (string.IsNullOrWhiteSpace(personaKey) || !_personalityPresets.TryGetValue(personaKey, out var personaPrompt))
            {
                personaKey = DefaultPersonality;
                personaPrompt = _personalityPresets[DefaultPersonality];
            }

            if (!string.IsNullOrWhiteSpace(personaPrompt))
            {
                if (builder.Length > 0) builder.AppendLine();
                builder.Append(personaPrompt.Trim());
            }

            var supplementalInstructions = richTextBox1?.Text;
            if (!string.IsNullOrWhiteSpace(supplementalInstructions))
            {
                if (builder.Length > 0) builder.AppendLine();
                builder.Append(supplementalInstructions.Trim());
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        private void InitializeAssistantSettings()
        {
            if (personalityComboBox == null) return;

            personalityComboBox.SelectedIndexChanged -= PersonalityComboBox_SelectedIndexChanged;
            personalityComboBox.SelectedIndexChanged += PersonalityComboBox_SelectedIndexChanged;

            if (richTextBox1 != null)
            {
                richTextBox1.TextChanged -= AssistantInstructions_TextChanged;
                richTextBox1.TextChanged += AssistantInstructions_TextChanged;
            }

            if (userPromptTxt != null)
            {
                userPromptTxt.KeyDown -= UserPromptTxt_KeyDown;
                userPromptTxt.KeyDown += UserPromptTxt_KeyDown;
                userPromptTxt.TextChanged -= UserPromptTxt_TextChanged;
                userPromptTxt.TextChanged += UserPromptTxt_TextChanged;
            }

            if (autoModelCheckBox != null)
            {
                autoModelCheckBox.CheckedChanged -= autoModelCheckBox_CheckedChanged;
                autoModelCheckBox.Checked = _autoModelSelectionEnabled;
                if (modelsComboBox != null)
                    modelsComboBox.Enabled = !_autoModelSelectionEnabled;
                autoModelCheckBox.CheckedChanged += autoModelCheckBox_CheckedChanged;
            }

            personalityComboBox.BeginUpdate();
            personalityComboBox.Items.Clear();
            foreach (var presetName in PersonalityPresetOrder)
            {
                if (_personalityPresets.ContainsKey(presetName))
                    personalityComboBox.Items.Add(presetName);
            }
            personalityComboBox.EndUpdate();

            if (personalityComboBox.Items.Count > 0)
            {
                var targetIndex = personalityComboBox.Items.IndexOf(_selectedPersonality);
                if (targetIndex < 0)
                    targetIndex = personalityComboBox.Items.IndexOf(DefaultPersonality);
                if (targetIndex < 0)
                    targetIndex = 0;

                personalityComboBox.SelectedIndex = targetIndex;
            }

            RefreshDiagnosticsDisplay();
        }

        private void InitializeGenerationPresetSelector()
        {
            if (generationPresetComboBox == null)
                return;

            UpdateAdvancedPresetSnapshot(_options);

            _suppressGenerationPresetEvents = true;
            try
            {
                generationPresetComboBox.BeginUpdate();
                generationPresetComboBox.Items.Clear();

                foreach (var preset in _generationPresets)
                    generationPresetComboBox.Items.Add(preset);

                var match = GenerationPresetState.FindMatch(_generationPresets, _options);
                if (match != null)
                {
                    generationPresetComboBox.SelectedItem = match;
                }
                else
                {
                    var advanced = GetAdvancedPreset();
                    if (advanced != null)
                    {
                        generationPresetComboBox.SelectedItem = advanced;
                        _lastAdvancedOptions ??= _options.Clone();
                    }
                }
            }
            finally
            {
                generationPresetComboBox.EndUpdate();
                _suppressGenerationPresetEvents = false;
            }
        }

        private void UpdateGenerationPresetSelectionFromOptions(OllamaOptions options)
        {
            if (generationPresetComboBox == null || generationPresetComboBox.Items.Count == 0)
                return;

            _suppressGenerationPresetEvents = true;
            try
            {
                var match = GenerationPresetState.FindMatch(_generationPresets, options);
                if (match != null && generationPresetComboBox.Items.Contains(match))
                {
                    generationPresetComboBox.SelectedItem = match;
                    return;
                }

                var advanced = GetAdvancedPreset();
                if (advanced != null && generationPresetComboBox.Items.Contains(advanced))
                {
                    if (!ReferenceEquals(generationPresetComboBox.SelectedItem, advanced))
                        generationPresetComboBox.SelectedItem = advanced;
                }
            }
            finally
            {
                _suppressGenerationPresetEvents = false;
            }
        }

        private void UpdateAdvancedPresetSnapshot(OllamaOptions currentOptions)
        {
            if (currentOptions == null)
                return;

            if (GenerationPresetState.ShouldCaptureAdvancedSnapshot(_generationPresets, currentOptions, _lastAdvancedOptions))
            {
                _lastAdvancedOptions = currentOptions.Clone();
            }
        }

        private GenerationPreset? GetAdvancedPreset()
            => _generationPresets.FirstOrDefault(p => p.IsCustom);

        private void GenerationPresetComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressGenerationPresetEvents)
                return;

            if (generationPresetComboBox?.SelectedItem is not GenerationPreset preset)
                return;

            if (preset.IsCustom)
            {
                var advanced = _lastAdvancedOptions?.Clone() ?? _options.Clone();
                ApplyModelOptions(advanced, string.Empty);
            }
            else
            {
                var updated = _options.Clone();
                preset.ApplyTo(updated);
                ApplyModelOptions(updated, $"{preset.Name} preset applied.");
            }
        }

        private void InitializeModelClassificationUi()
        {
            if (modelTableListView == null || modelLegendTextBox == null || modelSummaryListView == null)
                return;

            modelTableListView.ColumnClick -= modelTableListView_ColumnClick;
            modelTableListView.ColumnClick += modelTableListView_ColumnClick;
            modelTableListView.HideSelection = false;

            modelLegendTextBox.Text = ModelCatalog.ModelLegendText;
            modelLegendTextBox.SelectionStart = 0;
            modelLegendTextBox.SelectionLength = 0;

            ConfigureModelsTabLayout();

            modelSummaryListView.BeginUpdate();
            modelSummaryListView.Items.Clear();
            foreach (var summary in ModelCatalog.ModelSummaryEntries)
            {
                var item = new ListViewItem(summary.Category)
                {
                    Tag = summary
                };

                item.SubItems.Add(summary.BestModels);
                item.SubItems.Add(summary.Notes);
                modelSummaryListView.Items.Add(item);
            }
            modelSummaryListView.EndUpdate();
            modelSummaryListView.HideSelection = false;

            UpdateModelSummaryColumnWidths();

            RefreshModelTableEntries();
        }

    private async Task RefreshInstalledModelsAsync(bool showDiagnostics = true, CancellationToken cancellationToken = default)
        {
            if (modelsComboBox == null)
                return;

            await _modelRefreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var installed = new List<string>();
                string? diagnostics = null;

                try
                {
                    using var response = await Http.GetAsync("api/tags", cancellationToken).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                        var payload = await JsonSerializer.DeserializeAsync<ListTagsResponse>(stream, ListTagsSerializerOptions, cancellationToken).ConfigureAwait(false);
                        if (payload?.models != null)
                        {
                            installed = payload.models
                                .Select(model => model?.name)
                                .Where(name => !string.IsNullOrWhiteSpace(name))
                                .Select(name => name!.Trim())
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                                .ToList();
                        }
                    }
                    else if (showDiagnostics)
                    {
                        diagnostics = $"Ollama returned {(int)response.StatusCode} when listing models. Showing built-in catalog.";
                    }
                }
                catch (Exception ex) when (showDiagnostics && !cancellationToken.IsCancellationRequested)
                {
                    diagnostics = $"Unable to list models from Ollama: {ex.Message}";
                }

                if (installed.Count == 0)
                {
                    installed = ModelCatalog.BuiltInModelProfiles
                        .Select(profile => profile.ModelId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (showDiagnostics)
                    {
                        var fallbackNotice = "No local models detected. Showing built-in catalog.";
                        diagnostics = string.IsNullOrWhiteSpace(diagnostics)
                            ? fallbackNotice
                            : diagnostics + " " + fallbackNotice;
                    }
                }

                var listHash = ComputeModelListHash(installed);

                void Apply()
                {
                    if (modelsComboBox == null || IsDisposed)
                        return;

                    var existing = modelsComboBox.Items.Cast<object>()
                        .Select(GetModelIdFromItem)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Select(name => name!)
                        .ToList();

                    var matchesExisting = _installedModelsHash.HasValue
                        && _installedModelsHash.Value == listHash
                        && existing.Count == installed.Count;

                    if (matchesExisting)
                    {
                        for (var i = 0; i < installed.Count; i++)
                        {
                            if (!string.Equals(existing[i], installed[i], StringComparison.OrdinalIgnoreCase))
                            {
                                matchesExisting = false;
                                break;
                            }
                        }
                    }

                    var previous = GetSelectedModelId();
                    var autoSelected = _lastAutoSelectedModel;
                    var warmed = _cachedChatModel;

                    if (!matchesExisting)
                    {
                        modelsComboBox.BeginUpdate();
                        modelsComboBox.Items.Clear();
                        foreach (var name in installed)
                            modelsComboBox.Items.Add(CreateModelListItem(name));
                        modelsComboBox.EndUpdate();

                        _modelTableSnapshotHash = null;
                    }

                    string? target = null;
                    if (!string.IsNullOrWhiteSpace(previous))
                        target = installed.FirstOrDefault(name => string.Equals(name, previous, StringComparison.OrdinalIgnoreCase));

                    if (target == null && !string.IsNullOrWhiteSpace(autoSelected))
                        target = installed.FirstOrDefault(name => string.Equals(name, autoSelected, StringComparison.OrdinalIgnoreCase));

                    if (target == null && !string.IsNullOrWhiteSpace(warmed))
                        target = installed.FirstOrDefault(name => string.Equals(name, warmed, StringComparison.OrdinalIgnoreCase));

                    if (target == null && installed.Count > 0)
                        target = installed[0];

                    if (target != null)
                    {
                        var index = installed.FindIndex(name => string.Equals(name, target, StringComparison.OrdinalIgnoreCase));
                        if (index >= 0)
                        {
                            if (modelsComboBox.SelectedIndex != index)
                                modelsComboBox.SelectedIndex = index;
                            else if (string.IsNullOrWhiteSpace(GetSelectedModelId()))
                                modelsComboBox.SelectedIndex = index;
                        }
                    }
                    else
                    {
                        if (modelsComboBox.SelectedIndex != -1)
                            modelsComboBox.SelectedIndex = -1;
                    }

                    if (!matchesExisting)
                        RefreshModelTableEntries();
                    else
                        SyncModelTableSelection();

                    _installedModelsHash = listHash;

                    if (showDiagnostics && !string.IsNullOrWhiteSpace(diagnostics))
                        SetDiagnostics(diagnostics);
                }

                if (IsDisposed)
                    return;

                if (!IsHandleCreated || !InvokeRequired)
                    Apply();
                else
                    BeginInvoke((Action)Apply);
            }
            finally
            {
                _modelRefreshLock.Release();
            }
        }

        private void ConfigureModelsTabLayout()
        {
            if (modelsTabPage == null || modelTableListView == null || modelLegendLabel == null || modelLegendTextBox == null || modelSummaryLabel == null || modelSummaryListView == null)
                return;

            modelsTabPage.Padding = new Padding(12);
            modelsTabPage.Resize -= ModelsTabPage_Resize;
            modelsTabPage.Resize += ModelsTabPage_Resize;
            modelSummaryListView.Resize -= ModelSummaryListView_Resize;
            modelSummaryListView.Resize += ModelSummaryListView_Resize;
            modelTableListView.Resize -= ModelTableListView_Resize;
            modelTableListView.Resize += ModelTableListView_Resize;
            ModelsTabPage_Resize(modelsTabPage, EventArgs.Empty);
            UpdateModelSummaryColumnWidths();
        }

        private void ModelsTabPage_Resize(object? sender, EventArgs e)
        {
            if (modelsTabPage == null || modelTableListView == null || modelLegendLabel == null || modelLegendTextBox == null || modelSummaryLabel == null || modelSummaryListView == null)
                return;

            var padding = modelsTabPage.Padding;
            var client = modelsTabPage.ClientSize;
            var width = Math.Max(0, client.Width - padding.Horizontal);
            var x = padding.Left;
            var y = padding.Top;
            if (width <= 0)
                width = client.Width;

            var availableHeight = Math.Max(240, client.Height - padding.Vertical);
            var tableHeight = Math.Max(220, (int)(availableHeight * 0.5));
            modelTableListView.Bounds = new Rectangle(x, y, width, tableHeight);
            modelTableListView.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            y = modelTableListView.Bottom + 12;
            modelLegendLabel.Location = new Point(x, y);
            modelLegendLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            y = modelLegendLabel.Bottom + 6;
            var legendHeight = 96;
            modelLegendTextBox.Bounds = new Rectangle(x, y, width, legendHeight);
            modelLegendTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            y = modelLegendTextBox.Bottom + 12;
            modelSummaryLabel.Location = new Point(x, y);
            modelSummaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            y = modelSummaryLabel.Bottom + 6;
            var summaryHeight = Math.Max(160, client.Height - padding.Bottom - y);
            modelSummaryListView.Bounds = new Rectangle(x, y, width, summaryHeight);
            modelSummaryListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            UpdateModelTableColumnWidths();
            UpdateModelSummaryColumnWidths();
        }

        private void ModelSummaryListView_Resize(object? sender, EventArgs e)
            => UpdateModelSummaryColumnWidths();

        private void ModelTableListView_Resize(object? sender, EventArgs e)
            => UpdateModelTableColumnWidths();

        private void UpdateModelSummaryColumnWidths()
        {
            if (modelSummaryListView == null || modelSummaryListView.Columns.Count < 3)
                return;

            var clientWidth = Math.Max(0, modelSummaryListView.ClientSize.Width);
            if (clientWidth <= 0)
                return;

            const int minCategoryWidth = 120;
            const int minBestWidth = 150;
            const int minNotesWidth = 220;

            var baseTotal = minCategoryWidth + minBestWidth + minNotesWidth;
            var extra = Math.Max(0, clientWidth - baseTotal);

            var categoryWidth = minCategoryWidth + (int)Math.Round(extra * 0.2);
            var bestWidth = minBestWidth + (int)Math.Round(extra * 0.25);
            var notesWidth = Math.Max(minNotesWidth, clientWidth - categoryWidth - bestWidth);

            var widths = new[] { categoryWidth, bestWidth, notesWidth };
            var mins = new[] { minCategoryWidth, minBestWidth, minNotesWidth };

            var total = widths.Sum();
            var overflow = total - clientWidth;
            if (overflow > 0)
            {
                for (var i = widths.Length - 1; i >= 0 && overflow > 0; i--)
                {
                    var reducible = widths[i] - mins[i];
                    if (reducible <= 0)
                        continue;

                    var reduction = Math.Min(reducible, overflow);
                    widths[i] -= reduction;
                    overflow -= reduction;
                }
            }

            total = widths.Sum();
            var remainder = clientWidth - total;
            if (remainder != 0)
            {
                widths[^1] = Math.Max(mins[^1], widths[^1] + remainder);
            }

            modelSummaryListView.BeginUpdate();
            try
            {
                for (var i = 0; i < widths.Length; i++)
                    modelSummaryListView.Columns[i].Width = widths[i];
            }
            finally
            {
                modelSummaryListView.EndUpdate();
            }
        }

        private void UpdateModelTableColumnWidths()
        {
            if (modelTableListView == null || modelTableListView.Columns.Count < 9)
                return;

            var clientWidth = Math.Max(0, modelTableListView.ClientSize.Width);
            if (clientWidth <= 0)
                return;

            var minWidths = new[] { 150, 130, 150, 80, 70, 80, 80, 80, 220 };
            var weights = new[] { 0.20, 0.12, 0.16, 0.06, 0.06, 0.08, 0.08, 0.08, 0.16 };

            var widths = (int[])minWidths.Clone();
            var baseTotal = minWidths.Sum();
            var extra = clientWidth - baseTotal;

            if (extra > 0)
            {
                for (var i = 0; i < widths.Length; i++)
                {
                    var allocation = (int)Math.Round(extra * weights[i]);
                    widths[i] += allocation;
                }
            }

            var total = widths.Sum();
            var remainder = clientWidth - total;
            if (remainder != 0)
            {
                widths[^1] = Math.Max(minWidths[^1], widths[^1] + remainder);
            }

            modelTableListView.BeginUpdate();
            try
            {
                for (var i = 0; i < widths.Length && i < modelTableListView.Columns.Count; i++)
                    modelTableListView.Columns[i].Width = widths[i];
            }
            finally
            {
                modelTableListView.EndUpdate();
            }
        }

        private static string FormatRank(int score)
        {
            var clamped = Math.Max(0, Math.Min(5, score));
            return clamped.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatOutput(ModelProfile profile, ModelClassification? classification = null)
        {
            var score = classification?.OutputScore > 0 ? classification!.OutputScore : profile.OutputLengthScore;
            var label = !string.IsNullOrWhiteSpace(classification?.OutputLabel) ? classification!.OutputLabel : profile.OutputLengthLabel;
            return string.Create(CultureInfo.InvariantCulture, $"{score} – {label}");
        }

        private static string FormatSpeed(ModelProfile profile, ModelClassification? classification = null)
        {
            var score = classification?.SpeedScore > 0 ? classification!.SpeedScore : profile.SpeedScore;
            var label = !string.IsNullOrWhiteSpace(classification?.SpeedLabel) ? classification!.SpeedLabel : profile.SpeedLabel;
            return string.Create(CultureInfo.InvariantCulture, $"{score} – {label}");
        }

        private static string MergeNotes(string? primary, string? secondary)
        {
            var baseText = (primary ?? string.Empty).Trim();
            var extraText = (secondary ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(extraText))
                return baseText;

            if (string.IsNullOrEmpty(baseText))
                return extraText;

            if (baseText.IndexOf(extraText, StringComparison.OrdinalIgnoreCase) >= 0)
                return baseText;

            return string.Create(CultureInfo.InvariantCulture, $"{baseText} | {extraText}");
        }

        private void SyncModelTableSelection()
        {
            if (modelTableListView == null)
                return;

            var selectedModel = GetSelectedModelId();
            if (string.IsNullOrWhiteSpace(selectedModel))
            {
                modelTableListView.SelectedIndices.Clear();
                return;
            }

            ListViewItem? match = null;
            foreach (ListViewItem item in modelTableListView.Items)
            {
                if (item.Tag is ModelListViewEntry entry && entry.Matches(selectedModel))
                {
                    match = item;
                    break;
                }
            }

            modelTableListView.SelectedIndices.Clear();
            if (match != null)
            {
                match.Selected = true;
                match.Focused = true;
                match.EnsureVisible();
            }
        }

        private void RefreshModelTableEntries()
        {
            if (modelTableListView == null)
                return;

            if (modelTableListView.InvokeRequired)
            {
                modelTableListView.BeginInvoke(new Action(RefreshModelTableEntries));
                return;
            }

            var installedNames = modelsComboBox?.Items.Cast<object>()
                .Select(GetModelIdFromItem)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var usingFallback = installedNames == null || installedNames.Count == 0;
            var snapshotHash = usingFallback
                ? ModelCatalog.BuiltInModelProfiles.Count
                : ComputeModelListHash(installedNames!);

            if (_modelTableSnapshotHash.HasValue
                && _modelTableSnapshotHash.Value == snapshotHash
                && _modelTableUsingFallback == usingFallback
                && (!usingFallback ? modelTableListView.Items.Count == installedNames!.Count : modelTableListView.Items.Count == ModelCatalog.BuiltInModelProfiles.Count))
            {
                SyncModelTableSelection();
                return;
            }

            modelTableListView.BeginUpdate();
            try
            {
                modelTableListView.Items.Clear();

                if (!usingFallback && installedNames != null)
                {
                    foreach (var modelName in installedNames)
                    {
                        if (string.IsNullOrWhiteSpace(modelName))
                            continue;

                        var profile = ResolveProfile(modelName);
                        var classification = ModelClassifier.Classify(modelName, profile);
                        modelTableListView.Items.Add(CreateModelListViewItem(profile, modelName, classification));
                    }
                }
                else
                {
                    foreach (var profile in ModelCatalog.BuiltInModelProfiles.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase))
                    {
                        var classification = ModelClassifier.Classify(profile.ModelId, profile);
                        modelTableListView.Items.Add(CreateModelListViewItem(profile, profile.ModelId, classification));
                    }
                }
            }
            finally
            {
                modelTableListView.EndUpdate();
            }

            _modelTableSnapshotHash = snapshotHash;
            _modelTableUsingFallback = usingFallback;

            ApplyModelTableSort();
            SyncModelTableSelection();
            UpdateModelTableColumnWidths();
        }

        private static int ComputeModelListHash(IReadOnlyList<string> names)
        {
            var hash = new HashCode();
            hash.Add(names.Count);
            for (var i = 0; i < names.Count; i++)
            {
                hash.Add(names[i], StringComparer.OrdinalIgnoreCase);
            }
            return hash.ToHashCode();
        }

        private static ListViewItem CreateModelListViewItem(ModelProfile profile, string modelLabel, ModelClassification classification)
        {
            var display = string.IsNullOrWhiteSpace(modelLabel)
                ? profile.DisplayName
                : modelLabel;

            if (string.Equals(modelLabel, profile.ModelId, StringComparison.OrdinalIgnoreCase))
                display = profile.DisplayName;

            if (profile.IsUncensored && display.IndexOf("uncensored", StringComparison.OrdinalIgnoreCase) < 0)
                display += " (Uncensored)";

            var item = new ListViewItem(display)
            {
                Tag = new ModelListViewEntry(modelLabel, profile, classification)
            };

            var typeLabel = !string.IsNullOrWhiteSpace(classification.TypeLabel) ? classification.TypeLabel : profile.Type;
            if (profile.IsUncensored && typeLabel.IndexOf("uncensored", StringComparison.OrdinalIgnoreCase) < 0)
            {
                typeLabel = string.IsNullOrWhiteSpace(typeLabel)
                    ? "Uncensored"
                    : typeLabel + " (Uncensored)";
            }

            item.SubItems.Add(typeLabel);
            item.SubItems.Add(string.IsNullOrWhiteSpace(classification.IdealUse) ? profile.IdealUse : classification.IdealUse);
            item.SubItems.Add(FormatOutput(profile, classification));
            item.SubItems.Add(FormatSpeed(profile, classification));
            item.SubItems.Add(FormatRank(classification.AnalyticalScore > 0 ? classification.AnalyticalScore : profile.AnalyticalScore));
            item.SubItems.Add(FormatRank(classification.CreativityScore > 0 ? classification.CreativityScore : profile.CreativityScore));
            item.SubItems.Add(FormatRank(classification.AccuracyScore > 0 ? classification.AccuracyScore : profile.AccuracyScore));

            var notes = string.IsNullOrWhiteSpace(classification.Notes) ? profile.Notes : MergeNotes(profile.Notes, classification.Notes);
            item.SubItems.Add(notes);

            return item;
        }

        private Task ApplyAutoModelSelectionAsync(string? promptSource, bool suppressDiagnostics = false)
        {
            if (autoModelCheckBox?.Checked != true || modelsComboBox == null)
                return Task.CompletedTask;

            var candidates = CollectModelCandidates();
            if (candidates.Count == 0)
                return Task.CompletedTask;

            CancellationTokenSource cts;
            lock (_autoModelSelectionLock)
            {
                _autoModelSelectionCts?.Cancel();
                cts = new CancellationTokenSource();
                _autoModelSelectionCts = cts;
                _autoModelSelectionTask = PerformAutoModelSelectionAsync(promptSource, candidates, suppressDiagnostics, cts);
                return _autoModelSelectionTask;
            }
        }

        private List<(string Name, ModelProfile Profile)> CollectModelCandidates()
        {
            var results = new List<(string Name, ModelProfile Profile)>();
            if (modelsComboBox == null)
                return results;

            foreach (var item in modelsComboBox.Items)
            {
                if (item is ModelListItem listItem)
                {
                    results.Add((listItem.ModelId, listItem.Profile));
                    continue;
                }

                var name = GetModelIdFromItem(item);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var profile = ResolveProfile(name);
                results.Add((name, profile));
            }

            return results;
        }

        private async Task PerformAutoModelSelectionAsync(string? promptSource, IReadOnlyList<(string Name, ModelProfile Profile)> candidates, bool suppressDiagnostics, CancellationTokenSource cts)
        {
            var token = cts.Token;
            var intent = AnalyzePromptIntent(promptSource);

            try
            {
                var selection = await SelectModelWithAssistantAsync(promptSource, intent, candidates, token).ConfigureAwait(false)
                    ?? SelectBestModelForPrompt(promptSource, candidates, intent);

                if (selection == null || token.IsCancellationRequested)
                    return;

                void ApplySelection() => ApplyResolvedModelSelection(selection.Value.ModelName, selection.Value.Profile, selection.Value.Intent, suppressDiagnostics);

                if (IsHandleCreated && !IsDisposed && InvokeRequired)
                    Invoke((Action)ApplySelection);
                else
                    ApplySelection();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!suppressDiagnostics)
                {
                    void Report() => SetDiagnostics($"Auto-pick routing failed: {ex.Message}");

                    if (IsHandleCreated && !IsDisposed && InvokeRequired)
                        Invoke((Action)Report);
                    else
                        Report();
                }

                if (token.IsCancellationRequested)
                    return;

                var fallback = SelectBestModelForPrompt(promptSource, candidates, intent);
                if (fallback == null)
                    return;

                void ApplyFallback() => ApplyResolvedModelSelection(fallback.Value.ModelName, fallback.Value.Profile, fallback.Value.Intent, suppressDiagnostics: true);

                if (IsHandleCreated && !IsDisposed && InvokeRequired)
                    Invoke((Action)ApplyFallback);
                else
                    ApplyFallback();
            }
            finally
            {
                cts.Dispose();

                lock (_autoModelSelectionLock)
                {
                    if (ReferenceEquals(_autoModelSelectionCts, cts))
                    {
                        _autoModelSelectionCts = null;
                        _autoModelSelectionTask = Task.CompletedTask;
                    }
                }
            }
        }

        private void ApplyResolvedModelSelection(string modelName, ModelProfile profile, PromptIntent intent, bool suppressDiagnostics)
        {
            if (modelsComboBox == null)
                return;

            if (!profile.IsUncensored && IsLikelyUncensoredModelName(modelName))
                profile = profile.WithUncensoredFlag(true);

            var previous = GetSelectedModelId();
            var targetIndex = FindModelIndex(modelName);

            if (targetIndex < 0)
            {
                if (!suppressDiagnostics)
                    SetDiagnostics($"Auto-pick suggested '{modelName}', but that model isn't installed.");
                return;
            }

            if (targetIndex >= 0 && modelsComboBox.SelectedIndex != targetIndex)
                modelsComboBox.SelectedIndex = targetIndex;

            SyncModelTableSelection();

            var selectionChanged = !string.Equals(previous, modelName, StringComparison.OrdinalIgnoreCase);
            if (!suppressDiagnostics && (selectionChanged || string.IsNullOrEmpty(_lastAutoSelectedModel)))
            {
                var description = DescribeIntent(intent);
                var profileLabel = profile.Type;
                if (profile.IsUncensored && profileLabel.IndexOf("uncensored", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    profileLabel = string.IsNullOrWhiteSpace(profileLabel)
                        ? "Uncensored"
                        : profileLabel + " (Uncensored)";
                }

                if (!string.IsNullOrWhiteSpace(description))
                    SetDiagnostics($"Auto-picked '{modelName}' ({profileLabel}) for {description}.");
                else
                    SetDiagnostics($"Auto-picked '{modelName}' ({profileLabel}).");
            }

            if (selectionChanged || string.IsNullOrEmpty(_lastAutoSelectedModel))
                SetActiveModel(modelName);

            _lastAutoSelectedModel = modelName;
        }

        private async Task<(string ModelName, ModelProfile Profile, PromptIntent Intent)?> SelectModelWithAssistantAsync(
            string? promptSource,
            PromptIntent intent,
            IReadOnlyList<(string Name, ModelProfile Profile)> candidates,
            CancellationToken cancellationToken)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var routingModel = ResolveRoutingModelName(candidates);
            if (string.IsNullOrWhiteSpace(routingModel))
            {
                SetDiagnostics("Auto-pick router skipped: no routing model available; using heuristic scores.");
                return null;
            }

            var normalizedPrompt = string.IsNullOrWhiteSpace(promptSource)
                ? "(empty prompt)"
                : promptSource.Trim();

            var rosterBuilder = new StringBuilder();
            for (var i = 0; i < candidates.Count; i++)
            {
                var (name, profile) = candidates[i];
                rosterBuilder.Append(i + 1).Append(". ").Append(name)
                    .Append(" — ").Append(profile.Type);

                if (!string.IsNullOrWhiteSpace(profile.IdealUse))
                    rosterBuilder.Append(" | Ideal: ").Append(profile.IdealUse);

                rosterBuilder.Append(" | Speed ").Append(profile.SpeedScore).Append("/5");
                rosterBuilder.Append(", Analytical ").Append(profile.AnalyticalScore).Append("/5");
                rosterBuilder.Append(", Creativity ").Append(profile.CreativityScore).Append("/5");
                rosterBuilder.Append(", Accuracy ").Append(profile.AccuracyScore).Append("/5");
                rosterBuilder.Append(", Output ").Append(profile.OutputLengthLabel);
                rosterBuilder.Append(", Moderation ").Append(profile.IsUncensored ? "Uncensored" : "Standard guardrails");

                if (!string.IsNullOrWhiteSpace(profile.Notes))
                    rosterBuilder.Append(" | ").Append(profile.Notes);

                rosterBuilder.AppendLine();
            }

            var systemMessage = "You route prompts to the strongest local model. Respond with exactly one model id from the provided list. Do not add explanations. When multiple options fit, prefer models marked Moderation Uncensored so the user is not blocked by guardrails.";
            var userMessage = new StringBuilder()
                .AppendLine("Decide which model should answer the user's prompt.")
                .AppendLine()
                .AppendLine("Models:")
                .Append(rosterBuilder)
                .AppendLine("(If the prompt needs fewer restrictions, pick an uncensored option.)")
                .AppendLine("User prompt:")
                .AppendLine(normalizedPrompt)
                .AppendLine()
                .Append("Return only the single model id from the list above.");

            var history = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemMessage),
                new ChatMessage(ChatRole.User, userMessage.ToString())
            };

            var options = new ChatOptions
            {
                Temperature = 0.1f,
                MaxOutputTokens = 32
            };

            for (var attempt = 1; attempt <= AutoModelRoutingAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var clientInstance = new OllamaApiClient(OllamaBase, routingModel);
                IChatClient client = clientInstance;
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(AutoModelRoutingTimeout);

                try
                {
                    var responseBuilder = new StringBuilder();
                    await foreach (var update in client.GetStreamingResponseAsync(history, options: options, cancellationToken: timeoutCts.Token).ConfigureAwait(false))
                    {
                        var text = StreamingUpdateHelper.ExtractText(update);
                        if (!string.IsNullOrWhiteSpace(text))
                            responseBuilder.Append(text);
                    }

                    var response = CollapseWhitespaceRegex.Replace(responseBuilder.ToString(), " ").Trim().Trim('"', '\'', '`');
                    if (string.IsNullOrWhiteSpace(response))
                        throw new InvalidOperationException("Router returned an empty response.");

                    var match = MatchModelFromResponse(response, candidates);
                    if (match == null)
                        throw new InvalidOperationException($"Router response '{response}' did not match any installed model.");

                    return (match.Value.Name, match.Value.Profile, intent);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutCts.IsCancellationRequested)
                {
                    if (attempt >= AutoModelRoutingAttempts)
                    {
                        SetDiagnostics("Auto-pick router timed out; using heuristic scores.");
                        return null;
                    }

                    SetDiagnostics($"Auto-pick routing attempt {attempt} timed out; retrying...", isTrace: true);
                    await Task.Delay(AutoModelRoutingRetryDelay, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    if (attempt >= AutoModelRoutingAttempts)
                    {
                        SetDiagnostics($"Auto-pick router failed: {ex.Message}; using heuristic scores.");
                        return null;
                    }

                    SetDiagnostics($"Auto-pick routing attempt {attempt} failed ({ex.Message}); retrying...", isTrace: true);
                    await Task.Delay(AutoModelRoutingRetryDelay, cancellationToken).ConfigureAwait(false);
                }
            }

            return null;
        }

        private static (string Name, ModelProfile Profile)? MatchModelFromResponse(string response, IReadOnlyList<(string Name, ModelProfile Profile)> candidates)
        {
            if (string.IsNullOrWhiteSpace(response) || candidates == null || candidates.Count == 0)
                return null;

            var trimmed = response.Trim();

            foreach (var candidate in candidates)
            {
                if (string.Equals(trimmed, candidate.Name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, candidate.Profile.ModelId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, candidate.Profile.DisplayName, StringComparison.OrdinalIgnoreCase))
                    return candidate;
            }

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate.Name) && trimmed.IndexOf(candidate.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;

                if (!string.IsNullOrWhiteSpace(candidate.Profile.ModelId) && trimmed.IndexOf(candidate.Profile.ModelId, StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;

                if (!string.IsNullOrWhiteSpace(candidate.Profile.DisplayName) && trimmed.IndexOf(candidate.Profile.DisplayName, StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;
            }

            var tokens = trimmed.Split(new[] { ' ', '\r', '\n', '\t', ',', ';', ':', '|', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                foreach (var candidate in candidates)
                {
                    if (string.Equals(token, candidate.Name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(token, candidate.Profile.ModelId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(token, candidate.Profile.DisplayName, StringComparison.OrdinalIgnoreCase))
                        return candidate;
                }
            }

            return null;
        }

        private string? ResolveRoutingModelName(IReadOnlyList<(string Name, ModelProfile Profile)> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            var preferred = new[]
            {
                "llama3.1:8b",
                "llama3.1:8b-instruct",
                "llama3:8b",
                "llama3.1:8b-instruct-q4_K_M",
                "llama3.2:latest",
                "llama3:latest",
                "llama3",
                "mistral:7b",
                "mistral-small:latest"
            };

            var uncensoredPreferred = candidates.FirstOrDefault(c =>
                c.Profile != null && c.Profile.IsUncensored && preferred.Any(p => string.Equals(c.Name, p, StringComparison.OrdinalIgnoreCase) || c.Profile.Matches(p)));
            if (!string.IsNullOrWhiteSpace(uncensoredPreferred.Name))
                return uncensoredPreferred.Name;

            var anyUncensored = candidates.FirstOrDefault(c => c.Profile != null && c.Profile.IsUncensored);
            if (!string.IsNullOrWhiteSpace(anyUncensored.Name))
                return anyUncensored.Name;

            foreach (var candidate in preferred)
            {
                var match = candidates.FirstOrDefault(c =>
                    string.Equals(c.Name, candidate, StringComparison.OrdinalIgnoreCase) ||
                    (c.Profile != null && c.Profile.Matches(candidate)));
                if (!string.IsNullOrWhiteSpace(match.Name))
                    return match.Name;
            }

            var generalist = candidates.FirstOrDefault(c => c.Profile.Type.IndexOf("general", StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrWhiteSpace(generalist.Name))
                return generalist.Name;

            return candidates[0].Name;
        }

        private void CancelAutoModelSelection()
        {
            CancellationTokenSource? toCancel = null;

            lock (_autoModelSelectionLock)
            {
                if (_autoModelSelectionCts != null)
                {
                    toCancel = _autoModelSelectionCts;
                    _autoModelSelectionCts = null;
                    _autoModelSelectionTask = Task.CompletedTask;
                }
            }

            toCancel?.Cancel();
        }

        private (string ModelName, ModelProfile Profile, PromptIntent Intent)? SelectBestModelForPrompt(string? prompt, IReadOnlyList<(string Name, ModelProfile Profile)>? candidates = null, PromptIntent? knownIntent = null)
        {
            var source = candidates ?? CollectModelCandidates();
            if (source == null || source.Count == 0)
                return null;

            var intent = knownIntent ?? AnalyzePromptIntent(prompt);
            var evaluated = new List<(string Name, ModelProfile Profile, double Score)>(source.Count);

            foreach (var entry in source)
            {
                var score = ScoreProfile(entry.Profile, intent);
                evaluated.Add((entry.Name, entry.Profile, score));
            }

            if (evaluated.Count == 0)
                return null;

            (string Name, ModelProfile Profile, double Score)? best = null;
            foreach (var entry in evaluated)
            {
                if (best == null)
                {
                    best = entry;
                    continue;
                }

                var current = best.Value;
                if (entry.Score > current.Score + 0.0001)
                {
                    best = entry;
                    continue;
                }

                var scoreDelta = Math.Abs(entry.Score - current.Score);
                if (scoreDelta <= 0.0001)
                {
                    if (entry.Profile.IsUncensored && !current.Profile.IsUncensored)
                    {
                        best = entry;
                        continue;
                    }

                    if (!entry.Profile.IsUncensored && current.Profile.IsUncensored)
                        continue;

                    if (entry.Profile.AccuracyScore > current.Profile.AccuracyScore)
                    {
                        best = entry;
                        continue;
                    }

                    if (entry.Profile.AccuracyScore == current.Profile.AccuracyScore && entry.Profile.AnalyticalScore > current.Profile.AnalyticalScore)
                    {
                        best = entry;
                        continue;
                    }

                    if (entry.Profile.AccuracyScore == current.Profile.AccuracyScore && entry.Profile.AnalyticalScore == current.Profile.AnalyticalScore && entry.Profile.BaseScore > current.Profile.BaseScore)
                    {
                        best = entry;
                        continue;
                    }
                }
            }

            if (best == null || string.IsNullOrWhiteSpace(best.Value.Name))
                return null;

            return (best.Value.Name, best.Value.Profile, intent);
        }

        private ModelProfile? FindProfileForName(string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return null;

            foreach (var profile in ModelCatalog.BuiltInModelProfiles)
            {
                if (profile.Matches(modelName))
                    return profile;
            }

            return null;
        }

        private ModelProfile ResolveProfile(string modelName)
        {
            var baseline = FindProfileForName(modelName);
            ModelProfile profile;

            if (baseline == null)
            {
                profile = ModelProfile.CreateFallback(modelName);
            }
            else
            {
                var classification = ModelClassifier.Classify(modelName, baseline);

                if (classification.HasMeasuredSpeed || !string.IsNullOrWhiteSpace(classification.Notes))
                {
                    var mergedNotes = MergeNotes(baseline.Notes, classification.Notes);
                    profile = baseline.WithOverrides(
                        speedScore: classification.SpeedScore > 0 ? classification.SpeedScore : baseline.SpeedScore,
                        speedLabel: !string.IsNullOrWhiteSpace(classification.SpeedLabel) ? classification.SpeedLabel : baseline.SpeedLabel,
                        notes: mergedNotes,
                        isUncensored: baseline.IsUncensored || classification.IsUncensored);
                }
                else
                {
                    profile = baseline.WithOverrides(
                        isUncensored: baseline.IsUncensored || classification.IsUncensored);
                }
            }

            if (!profile.IsUncensored && IsLikelyUncensoredModelName(modelName))
                profile = profile.WithUncensoredFlag(true);

            return profile;
        }

        private double ScoreProfile(ModelProfile profile, PromptIntent intent)
        {
            var score = profile.BaseScore;

            if (profile.IsUncensored)
                score += 2.5;

            score += profile.SpeedScore * 0.25;
            score += profile.AnalyticalScore * 0.35;
            score += profile.CreativityScore * 0.25;
            score += profile.AccuracyScore * 0.4;
            score += profile.OutputLengthScore * 0.25;

            if (intent.HasFlag(PromptIntent.Analytical))
                score += profile.AnalyticalScore * 0.6;

            if (intent.HasFlag(PromptIntent.Accuracy))
                score += profile.AccuracyScore * 0.75;

            if (intent.HasFlag(PromptIntent.Creative))
                score += profile.CreativityScore * 0.7;

            if (intent.HasFlag(PromptIntent.Coding))
                score += (profile.AnalyticalScore + profile.AccuracyScore) * 0.55 + profile.SpeedScore * 0.2;

            if (intent.HasFlag(PromptIntent.LongForm))
                score += profile.OutputLengthScore * 0.65;

            if (intent.HasFlag(PromptIntent.FastTurnaround))
                score += profile.SpeedScore * 0.85;

            return score;
        }

        private PromptIntent AnalyzePromptIntent(string? promptText)
        {
            if (string.IsNullOrWhiteSpace(promptText))
                return PromptIntent.None;

            var text = promptText.Trim();
            var intent = PromptIntent.None;

            if (ContainsAny(text, CodingKeywords))
                intent |= PromptIntent.Coding | PromptIntent.Analytical;

            if (ContainsAny(text, CreativeKeywords))
                intent |= PromptIntent.Creative;

            if (ContainsAny(text, ResearchKeywords))
                intent |= PromptIntent.Accuracy | PromptIntent.Analytical;

            if (ContainsAny(text, LongFormKeywords) || text.Length > 400)
                intent |= PromptIntent.LongForm;

            if (ContainsAny(text, FastKeywords))
                intent |= PromptIntent.FastTurnaround;

            if (ContainsAny(text, AnalyticalKeywords))
                intent |= PromptIntent.Analytical;

            if (ContainsAny(text, AccuracyKeywords))
                intent |= PromptIntent.Accuracy;

            return intent;
        }

        private static bool ContainsAny(string text, IReadOnlyList<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(text) || keywords == null || keywords.Count == 0)
                return false;

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    continue;

                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static string DescribeIntent(PromptIntent intent)
        {
            if (intent == PromptIntent.None)
                return "general-purpose prompts";

            var parts = new List<string>();

            if (intent.HasFlag(PromptIntent.Coding))
                parts.Add("coding/debugging");

            if (intent.HasFlag(PromptIntent.Analytical) && !intent.HasFlag(PromptIntent.Coding))
                parts.Add("analytical reasoning");

            if (intent.HasFlag(PromptIntent.Accuracy))
                parts.Add("high-accuracy work");

            if (intent.HasFlag(PromptIntent.Creative))
                parts.Add("creative exploration");

            if (intent.HasFlag(PromptIntent.LongForm))
                parts.Add("long-form drafting");

            if (intent.HasFlag(PromptIntent.FastTurnaround))
                parts.Add("fast turnaround");

            if (parts.Count == 0)
                return string.Empty;

            if (parts.Count == 1)
                return parts[0];

            if (parts.Count == 2)
                return parts[0] + " and " + parts[1];

            return string.Join(", ", parts.Take(parts.Count - 1)) + " and " + parts[^1];
        }

        private int FindModelIndex(string modelName)
        {
            if (modelsComboBox == null || string.IsNullOrWhiteSpace(modelName))
                return -1;

            for (var i = 0; i < modelsComboBox.Items.Count; i++)
            {
                var candidate = GetModelIdFromItem(modelsComboBox.Items[i]);
                if (string.Equals(candidate, modelName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void ApplyModelTableSort()
        {
            if (modelTableListView == null)
                return;

            if (_modelSortColumn >= 0 && _modelSortOrder != SortOrder.None)
            {
                modelTableListView.ListViewItemSorter = new ModelColumnComparer(_modelSortColumn, _modelSortOrder);
                modelTableListView.Sort();
            }
            else
            {
                modelTableListView.ListViewItemSorter = null;
            }
        }

        private void modelTableListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            if (modelTableListView == null)
                return;

            if (_modelSortColumn == e.Column)
            {
                _modelSortOrder = _modelSortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _modelSortColumn = e.Column;
                _modelSortOrder = SortOrder.Ascending;
            }

            ApplyModelTableSort();
            SyncModelTableSelection();
        }

        private static (double? Numeric, string Text) GetModelColumnSortValue(ListViewItem item, int column)
        {
            if (item.Tag is ModelListViewEntry entry)
            {
                var profile = entry.Profile;
                var classification = entry.Classification;

                var analytical = classification.AnalyticalScore > 0 ? classification.AnalyticalScore : profile.AnalyticalScore;
                var creativity = classification.CreativityScore > 0 ? classification.CreativityScore : profile.CreativityScore;
                var accuracy = classification.AccuracyScore > 0 ? classification.AccuracyScore : profile.AccuracyScore;
                var outputScore = classification.OutputScore > 0 ? classification.OutputScore : profile.OutputLengthScore;
                var speedScore = classification.SpeedScore > 0 ? classification.SpeedScore : profile.SpeedScore;

                return column switch
                {
                    0 => (null, item.Text ?? profile.DisplayName),
                    1 => (null, string.IsNullOrWhiteSpace(classification.TypeLabel) ? profile.Type : classification.TypeLabel),
                    2 => (null, string.IsNullOrWhiteSpace(classification.IdealUse) ? profile.IdealUse : classification.IdealUse),
                    3 => (outputScore, FormatOutput(profile, classification)),
                    4 => (speedScore, FormatSpeed(profile, classification)),
                    5 => (analytical, FormatRank(analytical)),
                    6 => (creativity, FormatRank(creativity)),
                    7 => (accuracy, FormatRank(accuracy)),
                    8 => (null, MergeNotes(profile.Notes, classification.Notes)),
                    _ => (null, GetSubItemText(item, column))
                };
            }

            return (null, GetSubItemText(item, column));
        }

        private static string GetSubItemText(ListViewItem item, int column)
        {
            if (column < 0)
                return item.Text ?? string.Empty;

            if (column < item.SubItems.Count)
                return item.SubItems[column].Text ?? string.Empty;

            return item.Text ?? string.Empty;
        }

        private static int CompareValues(double? xNumeric, string xText, double? yNumeric, string yText)
        {
            if (xNumeric.HasValue || yNumeric.HasValue)
            {
                if (xNumeric.HasValue && yNumeric.HasValue)
                {
                    var numericComparison = xNumeric.Value.CompareTo(yNumeric.Value);
                    if (numericComparison != 0)
                        return numericComparison;
                }
                else if (xNumeric.HasValue)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }

            return CompareText(xText, yText);
        }

        private static int CompareText(string? left, string? right)
        {
            var comparer = CultureInfo.CurrentCulture.CompareInfo;
            return comparer.Compare(left ?? string.Empty, right ?? string.Empty, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols);
        }

        private sealed class ModelColumnComparer : IComparer
        {
            private readonly int _column;
            private readonly SortOrder _order;

            public ModelColumnComparer(int column, SortOrder order)
            {
                _column = column;
                _order = order;
            }

            public int Compare(object? x, object? y)
            {
                if (x is not ListViewItem itemX || y is not ListViewItem itemY)
                    return 0;

                var (numericX, textX) = GetModelColumnSortValue(itemX, _column);
                var (numericY, textY) = GetModelColumnSortValue(itemY, _column);
                var comparison = CompareValues(numericX, textX, numericY, textY);

                if (comparison == 0)
                {
                    var (fallbackNumericX, fallbackTextX) = GetModelColumnSortValue(itemX, 0);
                    var (fallbackNumericY, fallbackTextY) = GetModelColumnSortValue(itemY, 0);
                    comparison = CompareValues(fallbackNumericX, fallbackTextX, fallbackNumericY, fallbackTextY);
                }

                if (_order == SortOrder.Descending)
                    comparison = -comparison;

                return comparison;
            }
        }

        private void PersonalityComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var selected = personalityComboBox?.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selected) || !_personalityPresets.ContainsKey(selected))
            {
                _selectedPersonality = DefaultPersonality;
            }
            else
            {
                _selectedPersonality = selected;
            }

            ApplyAssistantSettingsToOptions();
        }

        private void AssistantInstructions_TextChanged(object? sender, EventArgs e)
        {
            ApplyAssistantSettingsToOptions();
        }

        private void UserPromptTxt_KeyDown(object? sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;
            e.Handled = true;

            if (sendPromptBtn != null && sendPromptBtn.Enabled)
            {
                var nowUtc = DateTime.UtcNow;
                if (_lastPromptSubmitUtc != DateTime.MinValue && nowUtc - _lastPromptSubmitUtc < PromptShortcutDebounce)
                    return;

                _lastPromptSubmitUtc = nowUtc;
                sendPromptBtn.PerformClick();
            }
        }

        private void ApplyAssistantSettingsToOptions()
        {
            var composed = ComposeSystemPrompt();
            _options.SystemPrompt = composed;

            if (sendPromptBtn != null && sendPromptBtn.Enabled)
            {
                EnsureSystemMessage();
            }

            SaveAppSettings();
        }

        private void UserPromptTxt_TextChanged(object? sender, EventArgs e)
        {
            if (autoModelCheckBox?.Checked == true)
            {
                var text = userPromptTxt?.Text;
                _ = ApplyAutoModelSelectionAsync(text, suppressDiagnostics: true);
            }
        }

        // Build ChatOptions. Unknown/unsupported keys are harmlessly ignored by providers.
        private static Microsoft.Extensions.AI.ChatOptions MakeChatOptions(OllamaOptions opts, out string? warning)
            => (opts ?? OllamaOptions.Default()).ToChatOptions(out warning);



        private void AppendAssistantDelta(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (aiOutputTxt.InvokeRequired)
            {
                aiOutputTxt.BeginInvoke(new Action<string>(AppendAssistantDelta), text);
                return;
            }

            // Route streaming output exclusively through the markdown WebView
            markdownView.AppendTextSafe(text);
        }

        private void AiOutputTxt_LinkClicked(object? sender, LinkClickedEventArgs e)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.LinkText))
                return;

            var link = e.LinkText.Trim();

            if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
            {
                if (!link.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !link.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var normalized = "https://" + link.TrimStart('/');
                    if (!Uri.TryCreate(normalized, UriKind.Absolute, out uri))
                    {
                        MessageBox.Show(this, "Unable to open the selected link.", "Open Link",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show(this, "Unable to open the selected link.", "Open Link",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            if (!BrowserLauncher.TryOpenUrl(uri.ToString(), out var error))
            {
                MessageBox.Show(this, error ?? "Unable to open the selected link.", "Open Link",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #region Tools / MCP

        private void InitializeTools()
        {
            _toolRegistry.Register(new GmailTool());
            _toolRegistry.Register(new GoogleDocsTool());

            foreach (var tool in _toolRegistry.AllTools)
            {
                _toolRegistry.SetEnabled(tool.Name, true);
            }
        }

        private async Task<ToolProcessingResult> ProcessToolCommandsAsync(string prompt, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(prompt) || !_toolRegistry.HasEnabledTools)
                return ToolProcessingResult.PassThrough(prompt);

            var lines = prompt.Replace("\r\n", "\n").Split('\n');
            var remaining = new List<string>(lines.Length);
            var diagnostics = new StringBuilder();
            var result = new ToolProcessingResult { Prompt = prompt };

            foreach (var rawLine in lines)
            {
                if (TryExtractCommand(rawLine, out var trigger, out var arguments) &&
                    _toolRegistry.TryGetToolByTrigger(trigger, out var tool))
                {
                    if (!_toolRegistry.IsEnabled(tool.Name))
                    {
                        remaining.Add(rawLine);
                        diagnostics.AppendLine($"{tool.Name} is disabled. Enable it before use.");
                        continue;
                    }

                    try
                    {
                        var invocation = await tool.ExecuteAsync(arguments, cancellationToken);
                        var content = invocation.Success ? invocation.Output : invocation.ErrorMessage ?? "Tool failed.";
                        var systemMessage = SessionMessage.FormatToolSystemMessage(tool.Name, invocation.Success, content);
                        result.Invocations.Add(new ToolInvocationLog(tool.Name, content, invocation.Success, systemMessage));
                        diagnostics.AppendLine($"{tool.Name}: {(invocation.Success ? "completed" : "failed")}.");
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var errorText = ex.Message;
                        var systemMessage = SessionMessage.FormatToolSystemMessage(tool.Name, false, errorText);
                        result.Invocations.Add(new ToolInvocationLog(tool.Name, errorText, false, systemMessage));
                        diagnostics.AppendLine($"{tool.Name}: failed ({ex.Message}).");
                    }

                    continue;
                }

                remaining.Add(rawLine);
            }

            result.Prompt = string.Join(Environment.NewLine, remaining).Trim();

            var diagText = diagnostics.ToString().Trim();
            result.Diagnostics = string.IsNullOrEmpty(diagText) ? null : diagText;

            return result;
        }

        private static bool TryExtractCommand(string line, out string trigger, out string arguments)
        {
            trigger = string.Empty;
            arguments = string.Empty;

            if (string.IsNullOrWhiteSpace(line)) return false;

            var trimmed = line.TrimStart();
            if (!trimmed.StartsWith("/", StringComparison.Ordinal)) return false;

            var spaceIndex = trimmed.IndexOf(' ');
            if (spaceIndex < 0)
            {
                trigger = trimmed;
                return true;
            }

            trigger = trimmed.Substring(0, spaceIndex);
            arguments = trimmed.Substring(spaceIndex + 1).Trim();
            return true;
        }

        private sealed class ToolProcessingResult
        {
            public string Prompt { get; set; } = string.Empty;
            public List<ToolInvocationLog> Invocations { get; } = new List<ToolInvocationLog>();
            public string? Diagnostics { get; set; }

            public static ToolProcessingResult PassThrough(string prompt)
                => new ToolProcessingResult { Prompt = prompt ?? string.Empty };
        }

        private sealed class ToolInvocationLog
        {
            public ToolInvocationLog(string toolName, string output, bool success, string systemMessage)
            {
                ToolName = toolName;
                Output = output ?? string.Empty;
                Success = success;
                SystemMessage = systemMessage ?? string.Empty;
            }

            public string ToolName { get; }
            public string Output { get; }
            public bool Success { get; }
            public string SystemMessage { get; }
        }

        // Wraps a model entry with descriptive text and profile metadata for the combo box.
        private sealed class ModelListItem
        {
            public ModelListItem(string modelId, string displayText, ModelProfile profile, ModelClassification classification)
            {
                ModelId = modelId;
                DisplayText = displayText;
                Profile = profile;
                Classification = classification;
            }

            public string ModelId { get; }
            public string DisplayText { get; }
            public ModelProfile Profile { get; }
            public ModelClassification Classification { get; }

            public override string ToString() => DisplayText;
        }

        private sealed class ModelListViewEntry
        {
            public ModelListViewEntry(string modelId, ModelProfile profile, ModelClassification classification)
            {
                ModelId = modelId;
                Profile = profile;
                Classification = classification;
            }

            public string ModelId { get; }
            public ModelProfile Profile { get; }
            public ModelClassification Classification { get; }

            public bool Matches(string? candidate)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                    return false;

                return Profile.Matches(candidate) || string.Equals(ModelId, candidate.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string? GetModelIdFromItem(object? item)
        {
            if (item is ModelListItem modelItem)
                return modelItem.ModelId;

            if (item is string str)
                return string.IsNullOrWhiteSpace(str) ? null : str.Trim();

            return Convert.ToString(item)?.Trim();
        }

        private string? GetSelectedModelId()
        {
            if (modelsComboBox == null)
                return null;

            return GetModelIdFromItem(modelsComboBox.SelectedItem);
        }

        private ModelListItem CreateModelListItem(string modelId)
        {
            var profile = ResolveProfile(modelId);
            var classification = ModelClassifier.Classify(modelId, profile);

            var displayName = string.IsNullOrWhiteSpace(profile.DisplayName)
                ? modelId
                : profile.DisplayName.Trim();

            if (profile.IsUncensored && displayName.IndexOf("Uncensored", StringComparison.OrdinalIgnoreCase) < 0)
                displayName += " (Uncensored)";

            var suggestedText = string.IsNullOrWhiteSpace(classification.IdealUse)
                ? classification.SuggestedUseLabel
                : classification.IdealUse;

            var summary = string.IsNullOrWhiteSpace(suggestedText)
                ? string.Empty
                : CollapseAndTruncate(suggestedText, 120);

            var display = string.IsNullOrWhiteSpace(summary)
                ? displayName
                : $"{displayName} — {summary}";

            return new ModelListItem(modelId, display, profile, classification);
        }

        private enum SearchSeedKind
        {
            None,
            Literal,
            Llm,
            Heuristic
        }

        private static string CollapseAndTruncate(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var collapsed = CollapseWhitespaceRegex.Replace(value.Trim(), " ");
            if (collapsed.Length <= maxLength)
                return collapsed;

            return collapsed.Substring(0, Math.Max(0, maxLength - 3)).TrimEnd() + "...";
        }

        private static bool LooksTruncated(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            var trimmed = text.TrimEnd();
            if (trimmed.Length < 120)
                return false;

            var terminal = trimmed[^1];
            if (terminal == '.' || terminal == '!' || terminal == '?' || terminal == ']' || terminal == ')' || terminal == '"' || terminal == '”')
                return false;

            if (TruncatedTailRegex.IsMatch(trimmed))
                return true;

            var lastSentenceEnd = trimmed.LastIndexOfAny(new[] { '.', '!', '?', '\n' });
            if (lastSentenceEnd < 0)
                return true;

            var trailing = trimmed.Substring(lastSentenceEnd + 1).Trim();
            return trailing.Length > 0;
        }

        private async Task<string> TryAutoContinueAsync(
            IChatClient chatClient,
            string continuationPrompt,
            int requestedPredict,
            int? requestedContext,
            CancellationToken cancellationToken)
        {
            var continuationHistory = new List<ChatMessage>(_chatHistory.Count + 1);
            continuationHistory.AddRange(_chatHistory);
            continuationHistory.Add(new ChatMessage(ChatRole.User, continuationPrompt));

            var continuationOptions = (_options ?? OllamaOptions.Default()).Clone();
            var targetPredict = requestedPredict;
            if (targetPredict <= 0)
            {
                targetPredict = continuationOptions.NumPredict <= 0
                    ? ContinuationMinPredict
                    : continuationOptions.NumPredict;
            }

            if (targetPredict < ContinuationMinPredict)
                targetPredict = ContinuationMinPredict;

            if (targetPredict > ContinuationMaxPredict)
                targetPredict = ContinuationMaxPredict;

            continuationOptions.NumPredict = targetPredict;

            if (requestedContext.HasValue)
            {
                if (!continuationOptions.NumCtx.HasValue || continuationOptions.NumCtx.Value < requestedContext.Value)
                {
                    continuationOptions.NumCtx = requestedContext.Value;
                }
            }
            else if (continuationOptions.NumCtx.HasValue && continuationOptions.NumCtx.Value < ContinuationMinCtx)
            {
                continuationOptions.NumCtx = ContinuationMinCtx;
            }

            var chatOptions = MakeChatOptions(continuationOptions, out _);
            var builder = new StringBuilder();

            await foreach (var update in chatClient.GetStreamingResponseAsync(continuationHistory, options: chatOptions, cancellationToken: cancellationToken))
            {
                var delta = StreamingUpdateHelper.ExtractText(update);
                if (!string.IsNullOrEmpty(delta))
                    builder.Append(delta);
            }

            return builder.ToString();
        }

        private void UpdateAssistantTranscripts(string finalResponse)
        {
            if (string.IsNullOrEmpty(finalResponse))
                return;

            _session.ReplaceLastAssistant(finalResponse);

            if (_chatHistory.Count > 0 && _chatHistory[^1].Role == ChatRole.Assistant)
                _chatHistory[^1] = new ChatMessage(ChatRole.Assistant, finalResponse);
        }

        #endregion

        

        #region UI helpers

        private void LogBackgroundError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            SetDiagnostics(message);
        }

        private void LogToolMessage(string? toolName, string? content, bool success)
        {
            AppendToolMessageToView(toolName, content, success);

            var canonicalName = string.IsNullOrWhiteSpace(toolName) ? "Tool" : toolName!;
            _session.AppendTool(canonicalName, content ?? string.Empty, success);
        }

        private void DisableUiDuringSend(bool sending)
        {
            if (InvokeRequired) { BeginInvoke(new Action<bool>(DisableUiDuringSend), sending); return; }
            sendPromptBtn.Enabled = !sending;
            if (modelsComboBox != null)
                modelsComboBox.Enabled = !sending && !_autoModelSelectionEnabled;
            if (cancelPromptBtn != null)
                cancelPromptBtn.Enabled = sending;
        }

        private IChatClient GetOrCreateChatClient(string model)
        {
            lock (_chatClientLock)
            {
                if (_cachedChatClient != null && string.Equals(_cachedChatModel, model, StringComparison.Ordinal))
                    return _cachedChatClient;

                DisposeCachedChatClientLocked();

                var client = new OllamaApiClient(OllamaBase, model);
                _cachedChatClient = client;
                _cachedChatModel = model;
                return client;
            }
        }

        private void ResetChatClientForSelection(string? model)
        {
            lock (_chatClientLock)
            {
                if (_cachedChatClient != null && !string.Equals(_cachedChatModel, model, StringComparison.Ordinal))
                    DisposeCachedChatClientLocked();
            }
        }

        private void InvalidateChatClient()
        {
            lock (_chatClientLock)
            {
                DisposeCachedChatClientLocked();
            }
        }

        private void DisposeCachedChatClient()
        {
            lock (_chatClientLock)
            {
                DisposeCachedChatClientLocked();
            }
        }

        private void DisposeCachedChatClientLocked()
        {
            if (_cachedChatClient is IDisposable disposable)
            {
                try { disposable.Dispose(); }
                catch { }
            }

            _cachedChatClient = null;
            _cachedChatModel = null;
        }

        private void ModelsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (modelsComboBox == null)
                return;

            var selected = GetSelectedModelId();
            ResetChatClientForSelection(selected);
            SyncModelTableSelection();

            if (!string.IsNullOrWhiteSpace(selected))
            {
                SetActiveModel(selected);
            }
            else
            {
                BeginWarmupFor(null);
            }

            if (autoModelCheckBox?.Checked != true)
                _lastAutoSelectedModel = null;
            else
                _lastAutoSelectedModel = selected;
        }

        private void autoModelCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (autoModelCheckBox == null)
                return;

            var enabled = autoModelCheckBox.Checked;
            _autoModelSelectionEnabled = enabled;

            SaveAppSettings();

            CancelAutoModelSelection();

            if (modelsComboBox != null)
                modelsComboBox.Enabled = !enabled;

            if (enabled)
            {
                _lastAutoSelectedModel = null;
                _ = ApplyAutoModelSelectionAsync(userPromptTxt?.Text, suppressDiagnostics: false);
            }
            else
            {
                _lastAutoSelectedModel = null;
                SetDiagnostics("Auto-pick disabled. Choose a model manually.");
            }

            SyncModelTableSelection();
        }

        private bool TryEnsureActiveModel(out string model)
        {
            model = GetSelectedModelId() ?? string.Empty;

            if (modelsComboBox != null && modelsComboBox.Items.Count == 0 && _modelRefreshLock.CurrentCount == 1 && IsHandleCreated && !IsDisposed)
                _ = RefreshInstalledModelsAsync(showDiagnostics: true);

            if (modelsComboBox == null)
            {
                if (string.IsNullOrWhiteSpace(model))
                {
                    SetDiagnostics("Model selection is unavailable. Restart the app and try again.");
                    return false;
                }

                return true;
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                var fallbackIndex = FindFirstAvailableModelIndex();
                if (fallbackIndex < 0)
                {
                    SetDiagnostics("No models available. Use 'ollama pull <model>' and reload.");
                    return false;
                }

                modelsComboBox.SelectedIndex = fallbackIndex;
                var fallback = GetSelectedModelId() ?? string.Empty;
                ResetChatClientForSelection(fallback);
                if (!string.IsNullOrWhiteSpace(fallback))
                    SetActiveModel(fallback);
                model = fallback;
                SetDiagnostics($"Model list refreshed; switched to '{fallback}'.");
                return true;
            }

            var currentSelection = model;
            var selectionStillValid = modelsComboBox.Items
                .Cast<object>()
                .Select(GetModelIdFromItem)
                .Any(name => string.Equals(name, currentSelection, StringComparison.OrdinalIgnoreCase));

            if (selectionStillValid)
                return true;

            var replaced = TrySelectReplacementModel(currentSelection, out var replacement);
            if (!replaced)
                return false;

            model = replacement;
            return true;
        }

        private int FindFirstAvailableModelIndex()
        {
            if (modelsComboBox == null)
                return -1;

            for (var i = 0; i < modelsComboBox.Items.Count; i++)
            {
                var candidate = GetModelIdFromItem(modelsComboBox.Items[i]);
                if (!string.IsNullOrWhiteSpace(candidate))
                    return i;
            }

            return -1;
        }

        private bool TrySelectReplacementModel(string originalSelection, out string replacement)
        {
            replacement = string.Empty;

            if (modelsComboBox == null)
                return false;

            var replacementIndex = FindFirstAvailableModelIndex();
            if (replacementIndex < 0)
            {
                SetDiagnostics("Previously selected model is no longer available, and no alternatives were found.");
                return false;
            }

            modelsComboBox.SelectedIndex = replacementIndex;
            replacement = GetSelectedModelId() ?? string.Empty;
            ResetChatClientForSelection(replacement);
            if (!string.IsNullOrWhiteSpace(replacement))
                SetActiveModel(replacement);
            SetDiagnostics($"Model '{originalSelection}' is no longer available; switched to '{replacement}'.");
            return true;
        }

        private void SetActiveModel(string model)
        {
            if (activeModelLabel.InvokeRequired) { activeModelLabel.BeginInvoke(new Action<string>(SetActiveModel), model); return; }

            var profile = ResolveProfile(model);

            var displayLabel = string.IsNullOrWhiteSpace(profile.DisplayName)
                ? model
                : (string.Equals(profile.DisplayName, model, StringComparison.OrdinalIgnoreCase)
                    ? profile.DisplayName
                    : $"{profile.DisplayName} ({model})");

            var suffix = profile.IsUncensored ? " (Uncensored)" : string.Empty;
            activeModelLabel.Text = $"Active Model: {displayLabel}{suffix}";

            BeginWarmupFor(model);
        }

        private static string GetCompactModelName(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                return string.Empty;

            var trimmed = model.Trim();
            var lastSlash = trimmed.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < trimmed.Length - 1)
                trimmed = trimmed.Substring(lastSlash + 1);

            return trimmed;
        }

        private void BeginWarmupFor(string? model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                _warmupManager.CancelActiveWarmup();
                UpdateWarmupIndicator(string.Empty, WarmupState.Hidden);
                return;
            }

            var aliases = BuildWarmupAliases(model);
            _warmupManager.RequestWarmup(model, aliases);
        }

        private IEnumerable<string> BuildWarmupAliases(string model)
        {
            var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                model.Trim()
            };

            var profile = FindProfileForName(model);
            if (profile != null)
            {
                aliases.Add(profile.ModelId);
                if (!string.IsNullOrWhiteSpace(profile.DisplayName))
                    aliases.Add(profile.DisplayName);
            }

            return aliases;
        }

        private void WarmupManager_StateChanged(object? sender, WarmupStateChangedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<WarmupStateChangedEventArgs>(WarmupManager_StateChanged), sender, e);
                return;
            }

            var compactName = string.IsNullOrWhiteSpace(e.Model) ? string.Empty : GetCompactModelName(e.Model);

            switch (e.State)
            {
                case WarmupState.Hidden:
                    UpdateWarmupIndicator(string.Empty, WarmupState.Hidden);
                    break;
                case WarmupState.Warming:
                    UpdateWarmupIndicator(string.IsNullOrWhiteSpace(compactName) ? "Warming up model" : $"Warming up {compactName}", WarmupState.Warming);
                    break;
                case WarmupState.Ready:
                    UpdateWarmupIndicator(string.IsNullOrWhiteSpace(compactName) ? "Model ready" : $"{compactName} ready", WarmupState.Ready);
                    break;
                case WarmupState.Failed:
                    UpdateWarmupIndicator(string.IsNullOrWhiteSpace(compactName) ? "Warmup failed" : $"{compactName} warmup failed", WarmupState.Failed);
                    if (e.Error != null)
                        SetDiagnostics($"Warmup failed for '{e.Model}': {e.Error.Message}");
                    break;
            }
        }

        private static async Task WarmModelAsync(string model, CancellationToken token)
        {
            var payload = new
            {
                model,
                prompt = "ping",
                stream = false,
                keep_alive = "5m",
                options = new
                {
                    temperature = 0.0,
                    num_predict = 4
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await Http.PostAsync("api/generate", content, token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            _ = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private void UpdateWarmupIndicator(string? message, WarmupState state)
        {
            if (warmupStatusLabel == null)
                return;

            if (warmupStatusLabel.InvokeRequired)
            {
                warmupStatusLabel.BeginInvoke(new Action<string?, WarmupState>(UpdateWarmupIndicator), message, state);
                return;
            }

            _warmupState = state;

            switch (state)
            {
                case WarmupState.Hidden:
                    _warmupAnimationTimer.Stop();
                    warmupStatusLabel.Visible = false;
                    warmupStatusLabel.Text = string.Empty;
                    _warmupBaseText = null;
                    _warmupEllipsisPhase = 0;
                    break;
                case WarmupState.Warming:
                    warmupStatusLabel.Visible = true;
                    warmupStatusLabel.ForeColor = Color.SteelBlue;
                    _warmupBaseText = string.IsNullOrWhiteSpace(message) ? "Warming up model" : message.Trim();
                    _warmupEllipsisPhase = 0;
                    warmupStatusLabel.Text = _warmupBaseText;
                    _warmupAnimationTimer.Start();
                    break;
                case WarmupState.Ready:
                    _warmupAnimationTimer.Stop();
                    warmupStatusLabel.Visible = true;
                    warmupStatusLabel.ForeColor = Color.FromArgb(34, 139, 34);
                    warmupStatusLabel.Text = string.IsNullOrWhiteSpace(message) ? "Model ready" : message;
                    _warmupBaseText = null;
                    _warmupEllipsisPhase = 0;
                    break;
                case WarmupState.Failed:
                    _warmupAnimationTimer.Stop();
                    warmupStatusLabel.Visible = true;
                    warmupStatusLabel.ForeColor = Color.Firebrick;
                    warmupStatusLabel.Text = string.IsNullOrWhiteSpace(message) ? "Warmup failed" : message;
                    _warmupBaseText = null;
                    _warmupEllipsisPhase = 0;
                    break;
            }
        }

        private void WarmupAnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_warmupState != WarmupState.Warming || warmupStatusLabel == null || string.IsNullOrEmpty(_warmupBaseText))
                return;

            _warmupEllipsisPhase = (_warmupEllipsisPhase + 1) % 4;
            var dots = new string('.', _warmupEllipsisPhase);
            warmupStatusLabel.Text = _warmupBaseText + dots;
        }

        private void TrimChatHistory()
        {
            if (_chatHistory.Count == 0)
                return;

            var systemPrompt = _options.SystemPrompt;
            var systemIndex = -1;

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                for (var i = 0; i < _chatHistory.Count; i++)
                {
                    var message = _chatHistory[i];
                    if (message.Role != ChatRole.System)
                        continue;

                    if (!string.Equals(message.Text, systemPrompt, StringComparison.Ordinal))
                        continue;

                    if (systemIndex == -1)
                    {
                        systemIndex = i;
                    }
                    else
                    {
                        _chatHistory.RemoveAt(i);
                        i--;
                    }
                }

                if (systemIndex > 0)
                {
                    var systemMessage = _chatHistory[systemIndex];
                    _chatHistory.RemoveAt(systemIndex);
                    _chatHistory.Insert(0, systemMessage);
                    systemIndex = 0;
                }
            }

            var keep = new bool[_chatHistory.Count];
            if (systemIndex >= 0 && systemIndex < keep.Length)
                keep[systemIndex] = true;

            var limit = Math.Max(1, _historyTurnLimit);
            var userCount = 0;
            var retaining = true;

            for (var i = _chatHistory.Count - 1; i >= 0; i--)
            {
                if (keep[i])
                    continue;

                if (!retaining)
                    continue;

                keep[i] = true;

                if (_chatHistory[i].Role == ChatRole.User)
                {
                    userCount++;
                    if (userCount >= limit)
                        retaining = false;
                }
            }

            var allKept = true;
            for (var i = 0; i < keep.Length; i++)
            {
                if (!keep[i])
                {
                    allKept = false;
                    break;
                }
            }

            if (allKept)
                return;

            var keptCount = 0;
            for (var i = 0; i < keep.Length; i++)
            {
                if (keep[i])
                    keptCount++;
            }

            var trimmed = new List<ChatMessage>(keptCount);
            for (var i = 0; i < _chatHistory.Count; i++)
            {
                if (keep[i])
                    trimmed.Add(_chatHistory[i]);
            }

            _chatHistory.Clear();
            _chatHistory.AddRange(trimmed);
        }

        private void SetDiagnostics(string text, bool isTrace = false)
        {
            if (diagnosticsTextBox == null) return;

            if (diagnosticsTextBox.InvokeRequired)
            {
                diagnosticsTextBox.BeginInvoke(new Action<string, bool>(SetDiagnostics), text, isTrace);
                return;
            }

            var sanitized = text?.TrimEnd() ?? string.Empty;
            if (sanitized.Length == 0)
            {
                diagnosticsTextBox.Text = string.Empty;
                diagnosticsTextBox.SelectionStart = 0;
                diagnosticsTextBox.ScrollToCaret();
                UpdateDiagnosticsContextMenuState();
                return;
            }

            var entry = new DiagnosticsEntry(DateTime.Now, sanitized, isTrace);
            _diagnosticsEntries.Add(entry);

            UpdateDiagnosticsDisplay(entry);
        }

        private void UpdateDiagnosticsDisplay(DiagnosticsEntry? latest)
        {
            if (diagnosticsTextBox == null)
                return;

            DiagnosticsEntry? entryToShow = null;

            if (_enableWebTrace)
            {
                if (latest.HasValue)
                {
                    entryToShow = latest;
                }
                else if (_diagnosticsEntries.TryGetLatest(out var newest))
                {
                    entryToShow = newest;
                }
            }
            else
            {
                if (latest.HasValue && !latest.Value.IsTrace)
                {
                    entryToShow = latest;
                }
                else
                {
                    for (var i = _diagnosticsEntries.Count - 1; i >= 0; i--)
                    {
                        if (!_diagnosticsEntries[i].IsTrace)
                        {
                            entryToShow = _diagnosticsEntries[i];
                            break;
                        }
                    }
                }
            }

            diagnosticsTextBox.Text = entryToShow.HasValue ? FormatDiagnosticsEntry(entryToShow.Value) : string.Empty;
            diagnosticsTextBox.SelectionStart = diagnosticsTextBox.TextLength;
            diagnosticsTextBox.ScrollToCaret();

            UpdateDiagnosticsContextMenuState();
        }

        private static string FormatDiagnosticsEntry(DiagnosticsEntry entry)
            => $"[{entry.Timestamp:HH:mm:ss}] {entry.Text}";

        private void RefreshDiagnosticsDisplay()
        {
            if (diagnosticsTextBox == null)
                return;

            if (diagnosticsTextBox.InvokeRequired)
            {
                diagnosticsTextBox.BeginInvoke(new Action(RefreshDiagnosticsDisplay));
                return;
            }

            UpdateDiagnosticsDisplay(null);
        }

        private void InitializeDiagnosticsUi()
        {
            if (copyTraceToolStripMenuItem != null)
            {
                copyTraceToolStripMenuItem.Click -= CopyTraceToolStripMenuItem_Click;
                copyTraceToolStripMenuItem.Click += CopyTraceToolStripMenuItem_Click;
            }

            if (openUrlToolStripMenuItem != null)
            {
                openUrlToolStripMenuItem.Click -= OpenUrlToolStripMenuItem_Click;
                openUrlToolStripMenuItem.Click += OpenUrlToolStripMenuItem_Click;
            }

            if (diagnosticsContextMenu != null)
            {
                diagnosticsContextMenu.Opening -= DiagnosticsContextMenu_Opening;
                diagnosticsContextMenu.Opening += DiagnosticsContextMenu_Opening;
            }
            RefreshDiagnosticsDisplay();
        }

        private void UpdateDiagnosticsContextMenuState()
        {
            if (copyTraceToolStripMenuItem != null)
                copyTraceToolStripMenuItem.Enabled = _diagnosticsEntries.Count > 0;

            if (openUrlToolStripMenuItem != null)
                openUrlToolStripMenuItem.Enabled = TryGetLatestDiagnosticsUrl(out _);
        }

        private bool TryGetLatestDiagnosticsUrl(out string url)
        {
            for (var i = _diagnosticsEntries.Count - 1; i >= 0; i--)
            {
                var entry = _diagnosticsEntries[i];

                if (!_enableWebTrace && entry.IsTrace)
                    continue;

                var candidate = entry.Text;
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                var match = DiagnosticsUrlRegex.Match(candidate);
                if (!match.Success)
                    continue;

                var candidateUrl = match.Value.TrimEnd('.', ',', ';', ')');
                if (Uri.IsWellFormedUriString(candidateUrl, UriKind.Absolute))
                {
                    url = candidateUrl;
                    return true;
                }
            }

            url = string.Empty;
            return false;
        }

        private void CopyTraceToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_diagnosticsEntries.Count == 0)
                return;

            var builder = new StringBuilder();
            foreach (var entry in _diagnosticsEntries)
            {
                if (!_enableWebTrace && entry.IsTrace)
                    continue;

                if (builder.Length > 0)
                    builder.AppendLine();

                builder.Append(FormatDiagnosticsEntry(entry));
            }

            if (builder.Length == 0)
            {
                SetDiagnostics("No diagnostics available to copy.");
                return;
            }

            try
            {
                Clipboard.SetText(builder.ToString());
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Copy failed: {ex.Message}");
            }
        }

        private void OpenUrlToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!TryGetLatestDiagnosticsUrl(out var url) || string.IsNullOrWhiteSpace(url))
            {
                SetDiagnostics("No URL found in diagnostics to open.");
                return;
            }

            try
            {
                var info = new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                };
                Process.Start(info);
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Failed to open URL: {ex.Message}");
            }
        }

        private void DiagnosticsContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            UpdateDiagnosticsContextMenuState();

            var copyEnabled = copyTraceToolStripMenuItem?.Enabled == true;
            var openEnabled = openUrlToolStripMenuItem?.Enabled == true;
            if (!copyEnabled && !openEnabled)
                e.Cancel = true;
        }

    // Keeps diagnostics bounded by storing entries in a fixed-capacity ring.
    private sealed class DiagnosticsBuffer : IEnumerable<DiagnosticsEntry>
        {
            private readonly DiagnosticsEntry[] _buffer;
            private int _start;
            private int _count;

            public DiagnosticsBuffer(int capacity)
            {
                if (capacity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(capacity));

                _buffer = new DiagnosticsEntry[capacity];
            }

            public int Count => _count;

            public DiagnosticsEntry this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)_count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    var bufferIndex = (_start + index) % _buffer.Length;
                    return _buffer[bufferIndex];
                }
            }

            public void Add(DiagnosticsEntry entry)
            {
                var insertIndex = (_start + _count) % _buffer.Length;
                _buffer[insertIndex] = entry;

                if (_count == _buffer.Length)
                {
                    _start = (_start + 1) % _buffer.Length;
                }
                else
                {
                    _count++;
                }
            }

            public bool TryGetLatest(out DiagnosticsEntry entry)
            {
                if (_count == 0)
                {
                    entry = default;
                    return false;
                }

                var index = (_start + _count - 1) % _buffer.Length;
                entry = _buffer[index];
                return true;
            }

            public IEnumerator<DiagnosticsEntry> GetEnumerator()
            {
                for (var i = 0; i < _count; i++)
                {
                    var index = (_start + i) % _buffer.Length;
                    yield return _buffer[index];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct DiagnosticsEntry
        {
            public DiagnosticsEntry(DateTime timestamp, string text, bool isTrace)
            {
                Timestamp = timestamp;
                Text = text;
                IsTrace = isTrace;
            }

            public DateTime Timestamp { get; }
            public string Text { get; }
            public bool IsTrace { get; }
        }

        private void CancelPromptBtn_Click(object? sender, EventArgs e)
        {
            CancelCurrentRequest();
            DisableUiDuringSend(false);
        }

        private void CancelCurrentRequest()
        {
            if (_cts == null || _cts.IsCancellationRequested) return;
            _cts.Cancel();
            SetDiagnostics("Cancel requested...");
        }

        private void CleanupAfterSend(bool sendCompleted)
        {
            _sendInProgress = false;
            _cts?.Dispose();
            _cts = null;
            RefreshSessionList();
            PersistSessions();
            DisableUiDuringSend(false);
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return string.IsNullOrWhiteSpace(name) ? "session" : name.Trim();
        }

        #endregion


        private async void Form1_Load(object sender, EventArgs e)
        {
            Control? host = aiOutputTxt?.Parent ?? splitContainer1?.Panel2;
            if (host == null)
                return;

            var legacyBounds = aiOutputTxt?.Bounds ?? new Rectangle(Point.Empty, host.ClientSize);
            var legacyDock = aiOutputTxt?.Dock ?? DockStyle.Fill;
            var legacyAnchor = aiOutputTxt?.Anchor ?? (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right);
            var legacyMargin = aiOutputTxt?.Margin ?? Padding.Empty;

            markdownView = new ThemedMarkdownView
            {
                Name = "markdownView",
                Bounds = legacyBounds,
                Dock = legacyDock == DockStyle.None ? DockStyle.Fill : legacyDock,
                Anchor = legacyAnchor,
                Margin = legacyMargin
            };

            host.Controls.Add(markdownView);

            if (aiOutputTxt != null)
            {
                var targetIndex = Math.Max(0, host.Controls.GetChildIndex(aiOutputTxt));
                host.Controls.SetChildIndex(markdownView, targetIndex);
                markdownView.BringToFront();
                aiOutputTxt.Visible = false;
            }
            else
            {
                markdownView.Dock = DockStyle.Fill;
            }

            if (splitContainer1 != null)
            {
                splitContainer1.SplitterWidth = 6;
                splitContainer1.Panel2MinSize = 100;
            }

            await LoadAndApplyAppSettingsAsync().ConfigureAwait(true);
            await InitializeSessionUiAsync().ConfigureAwait(true);

            try
            {
                await RefreshInstalledModelsAsync(showDiagnostics: true).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Model refresh failed: {ex.Message}");
            }
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            if (!_pendingInitialTranscriptRefresh)
                return;

            _pendingInitialTranscriptRefresh = false;

            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                var current = GetCurrentSession();
                if (current != null)
                    RenderSessionTranscript(current);
            }));
        }

    }

    #region JSON DTOs for /api/tags

    internal sealed class ListTagsResponse
    {
        public List<TagModel>? models { get; set; }
    }

    internal sealed class TagModel
    {
        public string? name { get; set; }
    }

    #endregion

    #region Options + Session

    internal sealed class DebouncedSaver : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly Func<CancellationToken, Task> _callback;
        private readonly Action<Exception> _onError;
        private readonly object _gate = new object();
        private CancellationTokenSource? _cts;
        private Task? _pending;

        public DebouncedSaver(TimeSpan delay, Func<CancellationToken, Task> callback, Action<Exception> onError)
        {
            _delay = delay;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _onError = onError ?? (_ => { });
        }

        public void Schedule()
        {
            CancellationTokenSource? previousCts = null;
            lock (_gate)
            {
                previousCts = _cts;
                var cts = new CancellationTokenSource();
                _cts = cts;
                _pending = DelayAndInvokeAsync(cts);
            }

            if (previousCts != null)
            {
                try { previousCts.Cancel(); }
                catch { }
                finally { previousCts.Dispose(); }
            }
        }

        public async Task FlushAsync()
        {
            CancellationTokenSource? activeCts;
            Task? pending;
            lock (_gate)
            {
                activeCts = _cts;
                pending = _pending;
                _cts = null;
                _pending = null;
            }

            if (activeCts != null)
            {
                try { activeCts.Cancel(); }
                catch { }
                finally { activeCts.Dispose(); }
            }

            if (pending != null)
            {
                try { await pending.ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }

            try
            {
                await _callback(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _onError(ex);
            }
        }

        public void Dispose()
        {
            CancellationTokenSource? cts;
            Task? pending;
            lock (_gate)
            {
                cts = _cts;
                pending = _pending;
                _cts = null;
                _pending = null;
            }

            if (cts != null)
            {
                try { cts.Cancel(); }
                catch { }
                finally { cts.Dispose(); }
            }

            if (pending != null)
            {
                try { pending.Wait(); }
                catch { }
            }
        }

        private async Task DelayAndInvokeAsync(CancellationTokenSource cts)
        {
            try
            {
                await Task.Delay(_delay, cts.Token).ConfigureAwait(false);
                await _callback(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected when rescheduled.
            }
            catch (Exception ex)
            {
                _onError(ex);
            }
            finally
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_cts, cts))
                    {
                        _cts = null;
                        _pending = null;
                    }
                }

                cts.Dispose();
            }
        }
    }

    internal sealed class SessionManager
    {
        private SessionRecord _current = SessionRecord.CreateDefault();

        public SessionRecord Current => _current;

        public void Load(SessionRecord record)
        {
            _current = record ?? SessionRecord.CreateDefault();
        }

        public void AppendUser(string text, string? display) => _current.AppendUser(text, display);

    public void AppendAssistant(string text) => _current.AppendAssistant(text);
    public void AppendSystem(string text) => _current.AppendSystem(text);
    public void ReplaceLastAssistant(string text) => _current.ReplaceLastAssistant(text);

        public void AppendTool(string toolName, string content, bool success) => _current.AppendTool(toolName, content, success);

        public string GetTranscript() => _current.BuildTranscript();
    }

    internal sealed class AppSettings
    {
    public string? Personality { get; set; }
    public bool EnableWebTrace { get; set; }
    public bool AutoModelSelectionEnabled { get; set; }
    public int MaxContinuationAttempts { get; set; } = 2;
        public string? AssistantInstructions { get; set; }
        public OllamaOptions? Options { get; set; }
        public int HistoryTurnLimit { get; set; } = 12;
        public OllamaOptions? LastAdvancedOptions { get; set; }
    }

    internal sealed class SessionRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = GenerateDefaultTitle();
        public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;
        public List<SessionMessage> Messages { get; set; } = new List<SessionMessage>();

        public static SessionRecord CreateDefault() => Create(null);

        public static SessionRecord Create(string? title)
        {
            var record = new SessionRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                Title = string.IsNullOrWhiteSpace(title) ? GenerateDefaultTitle() : title.Trim(),
                LastUpdated = DateTimeOffset.UtcNow,
                Messages = new List<SessionMessage>()
            };
            return record;
        }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Id))
                Id = Guid.NewGuid().ToString("N");

            Title = string.IsNullOrWhiteSpace(Title) ? GenerateDefaultTitle() : Title.Trim();

            if (LastUpdated == default)
                LastUpdated = DateTimeOffset.UtcNow;

            Messages ??= new List<SessionMessage>();

            var normalized = new List<SessionMessage>(Messages.Count);
            foreach (var message in Messages)
            {
                if (message == null) continue;
                message.Normalize();
                normalized.Add(message);
            }

            Messages = normalized;
        }

        public void AppendUser(string text, string? displayContent = null)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            Messages.Add(new SessionMessage
            {
                Role = SessionMessage.Roles.User,
                Content = text,
                DisplayContent = displayContent ?? text
            });

            Touch();
        }

        public void AppendAssistant(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var last = Messages.Count > 0 ? Messages[^1] : null;
            if (last != null && last.IsAssistant)
            {
                last.Content = (last.Content ?? string.Empty) + text;
                last.DisplayContent = (last.DisplayContent ?? string.Empty) + text;
            }
            else
            {
                Messages.Add(new SessionMessage
                {
                    Role = SessionMessage.Roles.Assistant,
                    Content = text,
                    DisplayContent = text
                });
            }

            Touch();
        }

        public void AppendSystem(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            Messages.Add(new SessionMessage
            {
                Role = SessionMessage.Roles.System,
                Content = text,
                DisplayContent = text
            });

            Touch();
        }

        public void ReplaceLastAssistant(string text)
        {
            if (Messages == null || Messages.Count == 0)
                return;

            for (var i = Messages.Count - 1; i >= 0; i--)
            {
                var message = Messages[i];
                if (message == null || !message.IsAssistant)
                    continue;

                message.Content = text ?? string.Empty;
                message.DisplayContent = text ?? string.Empty;
                Touch();
                return;
            }
        }

        public void AppendTool(string toolName, string content, bool success)
        {
            var effectiveContent = content ?? string.Empty;

            Messages.Add(new SessionMessage
            {
                Role = SessionMessage.Roles.Tool,
                Content = effectiveContent,
                ToolName = toolName,
                ToolSuccess = success,
                DisplayContent = effectiveContent
            });

            Touch();
        }

        public void ClearMessages()
        {
            Messages.Clear();
            Touch();
        }

        public List<ChatMessage> ToChatHistory()
        {
            var history = new List<ChatMessage>(Messages.Count);
            foreach (var message in Messages)
            {
                if (message.IsTool)
                {
                    var body = message.Content ?? string.Empty;
                    var text = SessionMessage.FormatToolSystemMessage(message.ToolName, message.ToolSuccess, body);
                    history.Add(new ChatMessage(ChatRole.System, text));
                }
                else
                {
                    var role = SessionMessage.ToChatRole(message.Role);
                    history.Add(new ChatMessage(role, message.Content ?? string.Empty));
                }
            }
            return history;
        }

        public string BuildTranscript() => BuildTranscript(Messages);

        public static string BuildTranscript(IEnumerable<SessionMessage> messages)
            => BuildMarkdownTranscript(messages);

        public static string BuildMarkdownTranscript(IEnumerable<SessionMessage> messages)
        {
            var sb = new StringBuilder();
            foreach (var message in messages)
            {
                if (message == null)
                    continue;

                sb.AppendLine(SessionMessage.MarkdownHeaderFor(message));

                var body = SessionMessage.MarkdownBodyFor(message);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    sb.AppendLine();
                    sb.AppendLine(body);
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        public SessionRecord Clone()
        {
            var clone = new SessionRecord
            {
                Id = Id,
                Title = Title,
                LastUpdated = LastUpdated,
                Messages = Messages.Select(m => m.Clone()).ToList()
            };
            return clone;
        }

        public void Touch()
        {
            LastUpdated = DateTimeOffset.UtcNow;
        }

        private static string GenerateDefaultTitle() => $"Session {DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}";
    }

    internal sealed class SessionMessage
    {
        internal static class Roles
        {
            public const string User = "user";
            public const string Assistant = "assistant";
            public const string System = "system";
            public const string Tool = "tool";
        }

    public string Role { get; set; } = Roles.User;
    public string Content { get; set; } = string.Empty;
    public string? ToolName { get; set; }
    public bool ToolSuccess { get; set; } = true;
    public string? DisplayContent { get; set; }

        public bool IsAssistant => string.Equals(Role, Roles.Assistant, StringComparison.OrdinalIgnoreCase);
        public bool IsTool => string.Equals(Role, Roles.Tool, StringComparison.OrdinalIgnoreCase);

        public SessionMessage Clone() => new SessionMessage
        {
            Role = Role,
            Content = Content,
            ToolName = ToolName,
            ToolSuccess = ToolSuccess,
            DisplayContent = DisplayContent
        };

        public void Normalize()
        {
            Role = string.IsNullOrWhiteSpace(Role) ? Roles.User : Role.Trim().ToLowerInvariant();
            Content ??= string.Empty;
            ToolName = string.IsNullOrWhiteSpace(ToolName) ? null : ToolName.Trim();
            if (string.IsNullOrEmpty(DisplayContent))
                DisplayContent = Content;
        }

        public string GetDisplayContent()
        {
            if (!string.IsNullOrEmpty(DisplayContent))
                return DisplayContent!;

            return Content ?? string.Empty;
        }

        public static ChatRole ToChatRole(string role)
        {
            var normalized = role?.Trim().ToLowerInvariant();
            return normalized switch
            {
                Roles.Assistant => ChatRole.Assistant,
                Roles.System => ChatRole.System,
                Roles.Tool => ChatRole.System,
                _ => ChatRole.User
            };
        }

        public static string HeaderFor(SessionMessage message)
        {
            var normalized = message?.Role?.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case Roles.Assistant:
                    return "=== Assistant ===";
                case Roles.System:
                    return "=== System ===";
                case Roles.Tool:
                    var displayName = string.IsNullOrWhiteSpace(message?.ToolName) ? "External" : message!.ToolName!;
                    var suffix = message != null && !message.ToolSuccess ? " (error)" : string.Empty;
                    return $"=== Tool: {displayName}{suffix} ===";
                default:
                    return "=== User ===";
            }
        }

        public static string MarkdownHeaderFor(SessionMessage message)
        {
            if (message == null)
                return "### User";

            if (message.IsTool)
            {
                var name = string.IsNullOrWhiteSpace(message.ToolName) ? "Tool" : message.ToolName.Trim();
                var status = message.ToolSuccess ? "output" : "error";
                return $"### Tool: {name} ({status})";
            }

            var normalized = message.Role?.Trim().ToLowerInvariant();
            return normalized switch
            {
                Roles.Assistant => "### Assistant",
                Roles.System => "### System",
                _ => "### User"
            };
        }

        public static string MarkdownBodyFor(SessionMessage message)
        {
            if (message == null)
                return string.Empty;

            var body = message.GetDisplayContent() ?? string.Empty;
            body = body.Replace("\r\n", "\n").TrimEnd();

            if (string.IsNullOrWhiteSpace(body))
                return "_(no content)_";

            return body;
        }

        public static string FormatToolSystemMessage(string? toolName, bool success, string content)
        {
            var label = string.IsNullOrWhiteSpace(toolName) ? "External" : toolName.Trim();
            var status = success ? "output" : "error";
            var body = string.IsNullOrEmpty(content) ? string.Empty : content;
            return $"Tool {label} {status}:{Environment.NewLine}{body}";
        }
    }

    #endregion

    #region Simple input dialog

    internal sealed class SimpleInputDialog : Form
    {
        private readonly TextBox _tb = new TextBox { Dock = DockStyle.Top };
        private readonly Button _ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Right, Width = 80 };
        private readonly Button _cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Dock = DockStyle.Right, Width = 80 };
        public string Result => _tb.Text;

        public SimpleInputDialog(string prompt, string initial)
        {
            Text = "Input";
            Width = 420; Height = 140; StartPosition = FormStartPosition.CenterParent;
            var lbl = new Label { Text = prompt, Dock = DockStyle.Top, Padding = new Padding(8) };
            _tb.Text = initial ?? "";
            var panel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            panel.Controls.Add(_ok);
            panel.Controls.Add(_cancel);
            Controls.Add(_tb);
            Controls.Add(lbl);
            Controls.Add(panel);
            AcceptButton = _ok; CancelButton = _cancel;
        }
    }

    #endregion
}
//DONE
