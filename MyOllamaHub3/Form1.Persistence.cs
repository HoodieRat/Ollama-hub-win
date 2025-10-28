using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyOllamaHub3.Models;

namespace MyOllamaHub3
{
    public partial class Form1
    {
        private static JsonSerializerOptions JsonOpts()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        private async Task LoadAndApplyAppSettingsAsync()
        {
            AppSettings? settings = null;
            string? diagnostics = null;

            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var options = JsonOpts();
                        settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                    }
                }
            }
            catch (Exception ex)
            {
                diagnostics = $"Settings load failed: {ex.Message}";
            }

            if (IsDisposed)
                return;

            void Apply()
            {
                _suppressSettingsSave = true;
                try
                {
                    ApplySettingsToState(settings);
                    InitializeAssistantSettings();
                    InitializeGenerationPresetSelector();

                    try
                    {
                        var snapshot = CreateCurrentSettingsSnapshot();
                        var canonical = SerializeSettings(snapshot);
                        lock (_settingsWriteLock)
                        {
                            _lastSavedSettingsJson = canonical;
                            _pendingSettingsJson = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogBackgroundError($"Settings snapshot failed: {ex.Message}");
                    }
                }
                finally
                {
                    _suppressSettingsSave = false;
                }

                if (!string.IsNullOrWhiteSpace(diagnostics))
                    SetDiagnostics(diagnostics);
            }

            if (InvokeRequired)
                Invoke((Action)Apply);
            else
                Apply();
        }

        private void ApplySettingsToState(AppSettings? settings)
        {
            if (settings == null)
            {
                _selectedPersonality = DefaultPersonality;
                _enableWebTrace = false;
                _autoModelSelectionEnabled = false;
                _historyTurnLimit = DefaultHistoryTurnLimit;
                _maxContinuationAttempts = DefaultContinuationAttempts;
                _options = OllamaOptions.Default();
                _tunerSystemPrompt = null;
                _lastAdvancedOptions = null;
                if (richTextBox1 != null)
                    richTextBox1.Text = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(settings.Personality))
                _selectedPersonality = settings.Personality;
            else
                _selectedPersonality = DefaultPersonality;

            _enableWebTrace = settings.EnableWebTrace;
            _autoModelSelectionEnabled = settings.AutoModelSelectionEnabled;

            if (richTextBox1 != null)
                richTextBox1.Text = string.IsNullOrWhiteSpace(settings.AssistantInstructions)
                    ? string.Empty
                    : settings.AssistantInstructions.Trim();

            _historyTurnLimit = settings.HistoryTurnLimit > 0
                ? settings.HistoryTurnLimit
                : DefaultHistoryTurnLimit;

            var attempts = settings.MaxContinuationAttempts;
            if (attempts < 0)
                attempts = DefaultContinuationAttempts;
            _maxContinuationAttempts = attempts;

            if (settings.Options != null)
            {
                _options = settings.Options.Clone();
                _tunerSystemPrompt = _options.SystemPrompt;

                if (!string.IsNullOrWhiteSpace(_tunerSystemPrompt))
                {
                    var trimmed = _tunerSystemPrompt.Trim();

                    foreach (var persona in _personalityPresets.Values)
                    {
                        if (trimmed.EndsWith(persona, StringComparison.Ordinal))
                        {
                            trimmed = trimmed.Substring(0, trimmed.Length - persona.Length).TrimEnd();
                            break;
                        }
                    }

                    _tunerSystemPrompt = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
                    _options.SystemPrompt = _tunerSystemPrompt;
                }
            }
            else
            {
                _options = OllamaOptions.Default();
                _tunerSystemPrompt = null;
            }

            _lastAdvancedOptions = settings.LastAdvancedOptions?.Clone();
            UpdateAdvancedPresetSnapshot(_options);
        }

        private AppSettings CreateCurrentSettingsSnapshot()
        {
            var snapshot = new AppSettings
            {
                Personality = string.IsNullOrWhiteSpace(_selectedPersonality) ? DefaultPersonality : _selectedPersonality,
                EnableWebTrace = _enableWebTrace,
                AutoModelSelectionEnabled = autoModelCheckBox?.Checked ?? _autoModelSelectionEnabled,
                MaxContinuationAttempts = _maxContinuationAttempts,
                AssistantInstructions = string.IsNullOrWhiteSpace(richTextBox1?.Text) ? null : richTextBox1.Text.Trim(),
                Options = (_options ?? OllamaOptions.Default()).Clone(),
                HistoryTurnLimit = _historyTurnLimit > 0 ? _historyTurnLimit : DefaultHistoryTurnLimit,
                LastAdvancedOptions = _lastAdvancedOptions?.Clone()
            };

            if (snapshot.Options != null)
                snapshot.Options.SystemPrompt = _tunerSystemPrompt;

            return snapshot;
        }

        private static string SerializeSettings(AppSettings snapshot)
        {
            var options = JsonOpts();
            options.WriteIndented = true;
            return JsonSerializer.Serialize(snapshot, options);
        }

        private void SaveAppSettings(bool force = false)
        {
            if (!force && _suppressSettingsSave) return;

            string json;
            try
            {
                var snapshot = CreateCurrentSettingsSnapshot();
                json = SerializeSettings(snapshot);
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Settings serialization failed: {ex.Message}");
                return;
            }

            var alreadyPending = false;
            var shouldSkip = false;

            lock (_settingsWriteLock)
            {
                if (string.Equals(_pendingSettingsJson, json, StringComparison.Ordinal))
                {
                    alreadyPending = true;
                }
                else if (string.Equals(_lastSavedSettingsJson, json, StringComparison.Ordinal))
                {
                    _pendingSettingsJson = null;
                    shouldSkip = true;
                }
                else
                {
                    _pendingSettingsJson = json;
                    alreadyPending = false;
                }
            }

            if (shouldSkip)
            {
                if (force)
                    _settingsSaveDebouncer.FlushAsync().GetAwaiter().GetResult();
                return;
            }

            if (force)
            {
                _settingsSaveDebouncer.FlushAsync().GetAwaiter().GetResult();
            }
            else if (!alreadyPending)
            {
                _settingsSaveDebouncer.Schedule();
            }
        }

        private async Task SaveSettingsJsonAsync(CancellationToken token)
        {
            string? json;
            lock (_settingsWriteLock)
            {
                json = _pendingSettingsJson;
                _pendingSettingsJson = null;
            }

            if (string.IsNullOrWhiteSpace(json))
                return;

            await WriteToFileWithRetryAsync(_settingsPath, json, token).ConfigureAwait(false);

            lock (_settingsWriteLock)
            {
                _lastSavedSettingsJson = json;
            }
        }

        private async Task SaveSessionsJsonAsync(CancellationToken token)
        {
            string? json;
            lock (_sessionsWriteLock)
            {
                json = _pendingSessionsJson;
                _pendingSessionsJson = null;
            }

            if (string.IsNullOrWhiteSpace(json))
                return;

            await WriteToFileWithRetryAsync(_sessionStorePath, json, token).ConfigureAwait(false);
        }

        private static async Task WriteToFileWithRetryAsync(string path, string content, CancellationToken token)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            const int maxAttempts = 3;
            var delay = TimeSpan.FromMilliseconds(150);
            var tempPath = path + ".tmp";

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    await WriteTempAndSwapAsync(path, tempPath, content, token).ConfigureAwait(false);
                    return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch when (attempt < maxAttempts)
                {
                    await Task.Delay(delay, token).ConfigureAwait(false);
                }
            }

            await WriteTempAndSwapAsync(path, tempPath, content, token).ConfigureAwait(false);
        }

        private static async Task WriteTempAndSwapAsync(string targetPath, string tempPath, string content, CancellationToken token)
        {
            try
            {
                await File.WriteAllTextAsync(tempPath, content, token).ConfigureAwait(false);
                File.Move(tempPath, targetPath, true);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch
                {
                }
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            CancelAutoModelSelection();
            _warmupManager.Dispose();
            SaveAppSettings(force: true);
            PersistSessions();
            _sessionSaveDebouncer.FlushAsync().GetAwaiter().GetResult();
            DisposeCachedChatClient();
        }

        private void newSessionToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_sendInProgress)
            {
                SetDiagnostics("Finish or cancel the current request before starting a new session.");
                return;
            }

            var newRecord = SessionRecord.CreateDefault();
            _sessions.Add(newRecord);

            ApplySessionRecord(newRecord);
            RefreshSessionList();
            PersistSessions();

            SetDiagnostics($"New session '{newRecord.Title}' started.");
        }

        private void exportSessionToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedSessions();
            if (selected.Count == 0)
            {
                var current = GetCurrentSession();
                if (current != null)
                {
                    selected.Add(current);
                }
                else
                {
                    SetDiagnostics("No sessions available to export.");
                    return;
                }
            }

            if (selected.Count == 1)
            {
                var record = selected[0];
                using var sfd = new SaveFileDialog
                {
                    Filter = "Markdown Files (*.md)|*.md|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    FileName = $"{SanitizeFileName(record.Title)}.md"
                };

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, record.BuildTranscript());
                        SetDiagnostics($"Exported to: {sfd.FileName}");
                    }
                    catch (Exception ex)
                    {
                        SetDiagnostics($"Export failed: {ex.Message}");
                    }
                }

                return;
            }

            using var fbd = new FolderBrowserDialog
            {
                Description = $"Select an export folder for {selected.Count} sessions",
                ShowNewFolderButton = true
            };

            if (fbd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                return;

            var destination = fbd.SelectedPath;
            var failures = new List<string>();

            foreach (var record in selected)
            {
                var fileName = SanitizeFileName(record.Title);
                var targetPath = EnsureUniqueFilePath(destination, fileName, ".md");

                try
                {
                    File.WriteAllText(targetPath, record.BuildTranscript());
                }
                catch (Exception ex)
                {
                    failures.Add($"{record.Title}: {ex.Message}");
                }
            }

            if (failures.Count == 0)
            {
                SetDiagnostics($"Exported {selected.Count} sessions to {destination}.");
            }
            else
            {
                SetDiagnostics($"Export completed with errors: {string.Join("; ", failures)}");
            }
        }

        private void renameSessionToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedSessions();
            if (selected.Count == 0)
            {
                var current = GetCurrentSession();
                if (current != null)
                    selected.Add(current);
            }

            if (selected.Count == 0)
            {
                SetDiagnostics("Select a session to rename.");
                return;
            }

            if (selected.Count > 1)
            {
                SetDiagnostics("Select a single session to rename.");
                return;
            }

            var target = selected[0];

            using var dlg = new SimpleInputDialog("Enter a new session name:", target.Title);
            if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.Result))
            {
                var newName = dlg.Result.Trim();
                target.Title = newName;
                target.Touch();

                if (_currentSessionId == target.Id)
                    UpdateWindowTitle(target.Title);

                RefreshSessionList();
                PersistSessions();

                SetDiagnostics($"Session renamed to '{target.Title}'.");
            }
        }

        private void deleteSessionToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            var selected = GetSelectedSessions();
            if (selected.Count == 0)
            {
                var current = GetCurrentSession();
                if (current != null)
                    selected.Add(current);
            }

            if (selected.Count == 0)
            {
                SetDiagnostics("Select session(s) to delete.");
                return;
            }

            if (_sendInProgress)
            {
                SetDiagnostics("Finish or cancel the current request before deleting a session.");
                return;
            }

            var prompt = selected.Count == 1
                ? $"Delete session '{selected[0].Title}'? This cannot be undone."
                : $"Delete {selected.Count} sessions? This cannot be undone.";

            if (MessageBox.Show(this, prompt, "Delete Sessions",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            var currentId = _currentSessionId;
            var deletedCurrent = selected.Any(s => s.Id == currentId);

            foreach (var record in selected)
            {
                _sessions.Remove(record);
            }

            if (_sessions.Count == 0)
            {
                var fallback = SessionRecord.CreateDefault();
                _sessions.Add(fallback);
                deletedCurrent = true;
            }

            if (deletedCurrent)
            {
                var next = _sessions
                    .OrderByDescending(s => s.LastUpdated)
                    .FirstOrDefault();

                if (next != null)
                {
                    ApplySessionRecord(next);
                }
            }

            RefreshSessionList();
            PersistSessions();

            var message = selected.Count == 1
                ? $"Session '{selected[0].Title}' deleted."
                : $"{selected.Count} sessions deleted.";

            if (deletedCurrent && GetCurrentSession() is { } replacement)
            {
                message += $" Switched to '{replacement.Title}'.";
            }

            SetDiagnostics(message);
        }

        private void sessionsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateSessionMenuState();

            if (_suppressSessionSelection) return;
            var selected = GetSelectedSessions();
            if (selected.Count != 1) return;

            var record = selected[0];
            if (_currentSessionId == record.Id) return;

            if (_sendInProgress)
            {
                _suppressSessionSelection = true;
                try
                {
                    foreach (ListViewItem item in sessionsListView.Items)
                    {
                        if (item.Tag is SessionRecord sr && sr.Id == _currentSessionId)
                        {
                            item.Selected = true;
                            sessionsListView.FocusedItem = item;
                            item.EnsureVisible();
                            break;
                        }
                    }
                }
                finally
                {
                    _suppressSessionSelection = false;
                }

                SetDiagnostics("A response is still streaming. Cancel it before switching sessions.");
                return;
            }

            ApplySessionRecord(record);
            SetDiagnostics($"Loaded session '{record.Title}'.");
        }

        private async Task InitializeSessionUiAsync()
        {
            List<SessionRecord> loaded = new();
            string? diagnostics = null;

            try
            {
                loaded = await LoadSessionsSnapshotAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                diagnostics = $"Session load failed: {ex.Message}";
            }

            if (IsDisposed)
                return;

            void Apply()
            {
                _sessions.Clear();
                foreach (var record in loaded)
                {
                    if (record == null)
                        continue;
                    record.Normalize();
                    _sessions.Add(record);
                }

                ConfigureSessionsTabLayout();

                var hadExistingSessions = _sessions.Count > 0;
                var startupSession = SessionRecord.CreateDefault();
                _sessions.Add(startupSession);

                ApplySessionRecord(startupSession);
                RefreshSessionList();

                if (!hadExistingSessions)
                    PersistSessions();

                if (!string.IsNullOrWhiteSpace(diagnostics))
                    SetDiagnostics(diagnostics);
            }

            if (InvokeRequired)
                Invoke((Action)Apply);
            else
                Apply();
        }

        private void ConfigureSessionsTabLayout()
        {
            if (sessionsTabPage == null || sessionsListView == null)
                return;

            sessionsTabPage.Padding = new Padding(12);
            sessionsListView.Dock = DockStyle.Fill;
            sessionsListView.Margin = Padding.Empty;

            sessionsListView.Resize -= SessionsListView_Resize;
            sessionsListView.Resize += SessionsListView_Resize;

            UpdateSessionsListColumnWidths();
        }

        private void SessionsListView_Resize(object? sender, EventArgs e)
            => UpdateSessionsListColumnWidths();

        private void UpdateSessionsListColumnWidths()
        {
            if (sessionsListView == null || sessionsListView.Columns.Count < 2)
                return;

            var clientWidth = Math.Max(0, sessionsListView.ClientSize.Width);
            if (clientWidth <= 0)
                return;

            var titleWidth = Math.Max(140, (int)Math.Round(clientWidth * 0.6));
            var updatedWidth = Math.Max(120, clientWidth - titleWidth - 1);

            if (updatedWidth < 120)
            {
                updatedWidth = 120;
                titleWidth = Math.Max(100, clientWidth - updatedWidth - 1);
            }

            sessionsListView.BeginUpdate();
            try
            {
                sessionsListView.Columns[0].Width = titleWidth;
                sessionsListView.Columns[1].Width = updatedWidth;

                var totalWidth = sessionsListView.Columns.Cast<ColumnHeader>().Sum(ch => ch.Width);
                var remainder = clientWidth - totalWidth;
                if (remainder != 0)
                {
                    sessionsListView.Columns[1].Width = Math.Max(80, sessionsListView.Columns[1].Width + remainder);
                }
            }
            finally
            {
                sessionsListView.EndUpdate();
            }
        }

        private async Task<List<SessionRecord>> LoadSessionsSnapshotAsync()
        {
            var results = new List<SessionRecord>();

            var directory = Path.GetDirectoryName(_sessionStorePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(_sessionStorePath))
                return results;

            var json = await File.ReadAllTextAsync(_sessionStorePath).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
                return results;

            var loaded = JsonSerializer.Deserialize<List<SessionRecord>>(json, JsonOpts());
            if (loaded == null)
                return results;

            foreach (var record in loaded)
            {
                if (record == null)
                    continue;

                if (!ShouldPersistSession(record))
                    continue;

                results.Add(record);
            }

            return results;
        }

        private void RenderSessionTranscript(SessionRecord record)
        {
            if (markdownView == null)
                return;

            markdownView.Clear();

            if (record?.Messages == null || record.Messages.Count == 0)
            {
                markdownView.AddSystemMessage("_No messages yet._");
                markdownView.EndAssistantMessage();
                return;
            }

            foreach (var message in record.Messages)
                AppendMessageToView(message);

            markdownView.EndAssistantMessage();
        }

        private void AppendMessageToView(SessionMessage? message)
        {
            if (markdownView == null || message == null)
                return;

            if (message.IsTool)
                return;

            var role = message.Role?.Trim().ToLowerInvariant();
            if (string.Equals(role, SessionMessage.Roles.System, StringComparison.OrdinalIgnoreCase))
                return;

            var display = message.GetDisplayContent();
            if (string.Equals(role, SessionMessage.Roles.Assistant, StringComparison.OrdinalIgnoreCase))
            {
                markdownView.AddAssistantMessage(FormatAssistantForDisplay(display));
            }
            else
            {
                AppendUserMessageToView(display);
            }
        }

        private void AppendUserMessageToView(string? content)
        {
            if (markdownView == null)
                return;

            markdownView.AddUserMessage(FormatUserForDisplay(content));
        }

        private void AppendSystemMessageToView(string? content)
        {
            if (markdownView == null)
                return;

            markdownView.AddSystemMessage(FormatSystemForDisplay(content));
        }

        private void AppendToolMessageToView(string? toolName, string? content, bool success)
        {
            if (markdownView == null)
                return;

            markdownView.AddSystemMessage(FormatToolForDisplay(toolName, content, success));
        }

        private static string FormatUserForDisplay(string? content)
        {
            var text = content ?? string.Empty;
            return string.IsNullOrWhiteSpace(text) ? "_(no message)_" : text.TrimEnd();
        }

        private static string FormatAssistantForDisplay(string? content)
        {
            var text = content ?? string.Empty;
            return string.IsNullOrWhiteSpace(text) ? "_(no response)_" : text.TrimEnd();
        }

        private static string FormatSystemForDisplay(string? content)
        {
            var text = content ?? string.Empty;
            var body = string.IsNullOrWhiteSpace(text) ? "_(no details)_" : text.TrimEnd();
            return "**System**\n\n" + body;
        }

        private static string FormatToolForDisplay(string? toolName, string? content, bool success)
        {
            var label = string.IsNullOrWhiteSpace(toolName) ? "Tool" : toolName.Trim();
            var status = success ? "output" : "error";
            var header = $"**{label} ({status})**";
            var body = string.IsNullOrWhiteSpace(content) ? string.Empty : content!.TrimEnd();
            return string.IsNullOrEmpty(body) ? header : header + "\n\n" + body;
        }

        private void ApplySessionRecord(SessionRecord record)
        {
            if (record == null) return;

            record.Normalize();
            _session.Load(record);
            _currentSessionId = record.Id;

            _chatHistory.Clear();
            _chatHistory.AddRange(record.ToChatHistory());

            aiOutputTxt.Text = record.BuildTranscript();
            userPromptTxt.Clear();

            RenderSessionTranscript(record);

            UpdateWindowTitle(record.Title);
            EnsureSystemMessage();
            TrimChatHistory();
        }

        private void RefreshSessionList()
        {
            if (sessionsListView == null) return;

            if (sessionsListView.InvokeRequired)
            {
                sessionsListView.Invoke(new Action(RefreshSessionList));
                return;
            }

            _suppressSessionSelection = true;
            sessionsListView.BeginUpdate();
            try
            {
                sessionsListView.Items.Clear();

                var ordered = _sessions
                    .OrderByDescending(s => s.LastUpdated)
                    .ThenBy(s => s.Title, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                foreach (var session in ordered)
                {
                    var item = new ListViewItem(session.Title)
                    {
                        Tag = session
                    };

                    var updatedText = session.LastUpdated == default
                        ? string.Empty
                        : session.LastUpdated.ToLocalTime().ToString("g");

                    item.SubItems.Add(updatedText);
                    if (_currentSessionId != null && session.Id == _currentSessionId)
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }

                    sessionsListView.Items.Add(item);
                }

                if (_currentSessionId != null)
                {
                    var selectedItem = sessionsListView.Items
                        .Cast<ListViewItem>()
                        .FirstOrDefault(i => i.Tag is SessionRecord sr && sr.Id == _currentSessionId);
                    if (selectedItem != null)
                    {
                        selectedItem.Selected = true;
                        sessionsListView.FocusedItem = selectedItem;
                        selectedItem.EnsureVisible();
                    }
                }
            }
            finally
            {
                sessionsListView.EndUpdate();
                _suppressSessionSelection = false;
                UpdateSessionsListColumnWidths();
                UpdateSessionMenuState();
            }
        }

        private void PersistSessions()
        {
            var snapshot = _sessions
                .Where(ShouldPersistSession)
                .Select(record => record)
                .ToList();

            string json;
            try
            {
                var options = JsonOpts();
                options.WriteIndented = true;
                json = JsonSerializer.Serialize(snapshot, options);
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Session serialization failed: {ex.Message}");
                return;
            }

            lock (_sessionsWriteLock)
            {
                _pendingSessionsJson = json;
            }

            _sessionSaveDebouncer.Schedule();
        }

        private SessionRecord? GetCurrentSession()
        {
            if (string.IsNullOrEmpty(_currentSessionId)) return null;
            return _sessions.FirstOrDefault(s => s.Id == _currentSessionId);
        }

        private List<SessionRecord> GetSelectedSessions()
        {
            var result = new List<SessionRecord>();

            if (sessionsListView?.SelectedItems == null)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (ListViewItem item in sessionsListView.SelectedItems)
            {
                if (item.Tag is SessionRecord record && seen.Add(record.Id))
                {
                    result.Add(record);
                }
            }

            return result;
        }

        private static bool ShouldPersistSession(SessionRecord? record)
        {
            if (record?.Messages == null || record.Messages.Count == 0)
                return false;

            foreach (var message in record.Messages)
            {
                if (message == null)
                    continue;

                if (string.Equals(message.Role, SessionMessage.Roles.User, StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(message.Content))
                {
                    return true;
                }

                if (message.IsAssistant || message.IsTool)
                    return true;
            }

            return false;
        }

        private void UpdateSessionMenuState()
        {
            var selectedCount = sessionsListView?.SelectedItems?.Count ?? 0;
            var hasCurrent = !string.IsNullOrEmpty(_currentSessionId);

            if (renameSessionToolStripMenuItem != null)
                renameSessionToolStripMenuItem.Enabled = selectedCount == 1 || (selectedCount == 0 && hasCurrent);

            if (deleteSessionToolStripMenuItem != null)
                deleteSessionToolStripMenuItem.Enabled = selectedCount > 0 || hasCurrent;

            if (exportSessionToolStripMenuItem != null)
            {
                var hasSelection = selectedCount > 0 || _sessions.Count > 0;
                exportSessionToolStripMenuItem.Enabled = hasSelection;
            }
        }

        private static string EnsureUniqueFilePath(string directory, string baseName, string extension)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Directory must be provided.", nameof(directory));

            var safeBase = string.IsNullOrWhiteSpace(baseName) ? "session" : baseName.Trim();
            var fileName = safeBase + extension;
            var candidate = Path.Combine(directory, fileName);
            var index = 1;

            while (File.Exists(candidate))
            {
                fileName = $"{safeBase} ({index}){extension}";
                candidate = Path.Combine(directory, fileName);
                index++;
            }

            return candidate;
        }

        private void UpdateWindowTitle(string? sessionTitle)
        {
            const string appName = "MyOllamaHub3";
            this.Text = string.IsNullOrWhiteSpace(sessionTitle) ? appName : $"{appName} - {sessionTitle}";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

    }
}
