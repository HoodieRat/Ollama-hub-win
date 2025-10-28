using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MyOllamaHub3.Models;

namespace MyOllamaHub3
{
    public partial class Form1
    {
        private void historyLimitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_sendInProgress)
            {
                SetDiagnostics("Finish or cancel the current request before changing the history limit.");
                return;
            }

            var current = _historyTurnLimit > 0 ? _historyTurnLimit : DefaultHistoryTurnLimit;

            try
            {
                using var dialog = new HistoryLimitDialog(current);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                var updated = dialog.HistoryTurnLimit;
                if (updated <= 0)
                    updated = DefaultHistoryTurnLimit;

                if (updated == _historyTurnLimit)
                    return;

                _historyTurnLimit = updated;
                TrimChatHistory();
                SaveAppSettings();
                SetDiagnostics($"History turn limit set to {_historyTurnLimit}.");
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Unable to update history limit: {ex.Message}");
            }
        }

        private void continuationAttemptsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (_sendInProgress)
            {
                SetDiagnostics("Finish or cancel the current request before changing continuation attempts.");
                return;
            }

            const int maxAllowed = 5;
            var current = _maxContinuationAttempts >= 0 ? _maxContinuationAttempts : DefaultContinuationAttempts;
            var initial = Math.Clamp(current, 0, maxAllowed);

            try
            {
                using var dialog = new Form
                {
                    Text = "Continuation Attempts",
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.CenterParent,
                    ClientSize = new Size(320, 150),
                    Font = SystemFonts.MessageBoxFont
                };

                var label = new Label
                {
                    AutoSize = true,
                    Text = "Automatic continuation attempts (0 disables):",
                    Location = new Point(12, 15)
                };

                var numericUpDown = new NumericUpDown
                {
                    Minimum = 0,
                    Maximum = maxAllowed,
                    Value = initial,
                    Location = new Point(15, 55),
                    Width = 120
                };

                var okButton = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new Point(140, 100),
                    Width = 75
                };

                var cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(225, 100),
                    Width = 75
                };

                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                dialog.Controls.Add(label);
                dialog.Controls.Add(numericUpDown);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);

                dialog.Shown += (_, __) =>
                {
                    var text = numericUpDown.Value.ToString(CultureInfo.InvariantCulture);
                    numericUpDown.Select(0, text.Length);
                    numericUpDown.Focus();
                };

                ThemeApplier.Apply(dialog);

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                var updated = (int)numericUpDown.Value;
                if (updated == _maxContinuationAttempts)
                    return;

                _maxContinuationAttempts = updated;
                SaveAppSettings();

                var status = updated == 0
                    ? "Automatic continuation disabled."
                    : $"Maximum continuation attempts set to {_maxContinuationAttempts}.";

                SetDiagnostics(status);
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Unable to update continuation attempts: {ex.Message}");
            }
        }

        private void tuneModelsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            try
            {
                using var tuner = new ModelTuner(_options.Clone())
                {
                    ActiveModelName = modelsComboBox?.SelectedItem as string
                };

                void HandleApply(OllamaOptions applied) => ApplyModelOptions(applied, string.Empty);

                tuner.OptionsApplied += HandleApply;
                try
                {
                    if (tuner.ShowDialog(this) == DialogResult.OK && tuner.ResultOptions != null)
                    {
                        ApplyModelOptions(tuner.ResultOptions, "Model options updated (unsupported options will be ignored by specific models).");
                    }
                }
                finally
                {
                    tuner.OptionsApplied -= HandleApply;
                }
            }
            catch (Exception ex)
            {
                SetDiagnostics($"Tuner error: {ex.Message}");
            }
        }

        private void clearOutputToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (markdownView == null)
                return;

            markdownView.Clear();
            markdownView.AddSystemMessage("_Output cleared. Select a session or send a message to continue._");
        }
    }
}
