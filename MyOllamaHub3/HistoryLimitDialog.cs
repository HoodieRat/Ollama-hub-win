using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyOllamaHub3
{
    internal sealed class HistoryLimitDialog : Form
    {
        private readonly NumericUpDown _limitUpDown;
        private readonly Button _okButton;
        private readonly Button _cancelButton;

        public int HistoryTurnLimit => (int)_limitUpDown.Value;

        public HistoryLimitDialog(int currentLimit)
        {
            Text = "History Limit";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(320, 140);
            Font = SystemFonts.MessageBoxFont;

            var label = new Label
            {
                AutoSize = true,
                Text = "Maximum user turns to keep in context:",
                Location = new Point(12, 15)
            };

            _limitUpDown = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 40,
                Value = Clamp(currentLimit),
                Location = new Point(15, 50),
                Width = 120
            };

            _okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 90),
                Width = 75,
                Height =32
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(225, 90),
                Width = 75,
                Height = 32
            };

            AcceptButton = _okButton;
            CancelButton = _cancelButton;

            Controls.Add(label);
            Controls.Add(_limitUpDown);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);

            Shown += (_, __) =>
            {
                _limitUpDown.Select(0, _limitUpDown.Value.ToString().Length);
                _limitUpDown.Focus();
            };
            ThemeApplier.Apply(this);
        }

        private static decimal Clamp(int value)
        {
            if (value < 1) return 1;
            if (value > 40) return 40;
            return value;
        }
    }
}
