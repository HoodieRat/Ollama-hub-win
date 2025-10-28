//START
// File: CustomTabControl.cs
// Purpose: A production-ready, fully owner-drawn TabControl that FORCE-colors the tab headers
//          and the control background (no more system gray), with a warm, polished palette.
// Usage:
//   1) Add this file to your project.
//   2) Build, then replace your TabControl(s) in the Designer with CustomTabControl
//      (or create programmatically).
//   3) Optional: tweak public properties (colors/fonts/sizing) at design-time or runtime.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyOllamaHub3
{
    [DesignerCategory("Code")]
    public class CustomTabControl : TabControl
    {
        // --- Warm, premium defaults (interior-designer inspired) ---
        private Color _headerBandColor = Color.FromArgb(170, 139, 99);   // Saddle Tan
        private Color _activeTabColor = Color.FromArgb(94, 63, 45);     // Cocoa
        private Color _inactiveTabColor = Color.FromArgb(170, 139, 99);   // Saddle Tan
        private Color _pageBackColor = Color.FromArgb(198, 176, 145);  // Warm Sand
        private Color _activeTextColor = Color.FromArgb(240, 228, 210);  // Cream
        private Color _inactiveTextColor = Color.FromArgb(52, 39, 28);     // Umber
        private Color _bottomBorderColor = Color.FromArgb(120, 102, 86);   // Border Umber

        private Font _headerFont = MakeFont(new[] { "Inter SemiBold", "Segoe UI Semibold", "Segoe UI" }, 10f);

        private Size _itemSize = new Size(100, 24);   // tight height
        private Point _tabPadding = new Point(8, 2);     // minimal side padding

        public CustomTabControl()
        {
            // Hard-force owner draw & custom painting
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            DrawMode = TabDrawMode.OwnerDrawFixed;
            SizeMode = TabSizeMode.Fixed;
            ItemSize = _itemSize;
            Padding = _tabPadding;

            // Color existing pages on creation as well
            ControlAdded += CustomTabControl_ControlAdded;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            // Ensure existing TabPages are themed and OS visual styles are disabled
            foreach (TabPage page in TabPages)
            {
                ForceThemePage(page);
            }
        }

        // ---- Public, designer-visible properties (optional to tweak) ----
        [Category("Appearance")]
        public Color HeaderBandColor { get => _headerBandColor; set { _headerBandColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Color ActiveTabColor { get => _activeTabColor; set { _activeTabColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Color InactiveTabColor { get => _inactiveTabColor; set { _inactiveTabColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Color PageBackColor { get => _pageBackColor; set { _pageBackColor = value; Invalidate(true); } }

        [Category("Appearance")]
        public Color ActiveTextColor { get => _activeTextColor; set { _activeTextColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Color InactiveTextColor { get => _inactiveTextColor; set { _inactiveTextColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Color BottomBorderColor { get => _bottomBorderColor; set { _bottomBorderColor = value; Invalidate(); } }

        [Category("Appearance")]
        public Font HeaderFont { get => _headerFont; set { _headerFont = value ?? Font; Invalidate(); } }

        [Category("Layout")]
        public Size HeaderItemSize { get => _itemSize; set { _itemSize = value; ItemSize = value; Invalidate(); } }

        [Category("Layout")]
        public Point HeaderPadding { get => _tabPadding; set { _tabPadding = value; Padding = value; Invalidate(); } }

        // --- Strongly force the control background colors to avoid OS gray ---
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // We fully paint; do NOT call base.OnPaintBackground to avoid default gray
            using (var band = new SolidBrush(_headerBandColor))
            {
                e.Graphics.FillRectangle(band, ClientRectangle);
            }
            // Fill the page display rectangle with the page color to meet TabPages
            using (var page = new SolidBrush(_pageBackColor))
            {
                e.Graphics.FillRectangle(page, DisplayRectangle);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Allow base to handle internal details, then we overdraw what matters.
            base.OnPaint(e);

            // Ensure the header band is fully covered (some themes may repaint)
            using (var band = new SolidBrush(_headerBandColor))
            {
                e.Graphics.FillRectangle(band, new Rectangle(0, 0, Width, GetHeaderBandHeight()));
            }
            // The DisplayRectangle fill is already handled in OnPaintBackground
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Custom draw each tab header
            if (e.Index < 0 || e.Index >= TabPages.Count)
                return;

            Rectangle rect = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            using (var fill = new SolidBrush(selected ? _activeTabColor : _inactiveTabColor))
            {
                e.Graphics.FillRectangle(fill, rect);
            }

            // Draw text centered
            string text = TabPages[e.Index].Text;
            TextRenderer.DrawText(
                e.Graphics,
                text,
                _headerFont,
                rect,
                selected ? _activeTextColor : _inactiveTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            // Optional bottom border for inactive tabs (subtle depth)
            if (!selected)
            {
                using (var pen = new Pen(_bottomBorderColor))
                {
                    e.Graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                }
            }

            // Focus cues (accessible)
            e.DrawFocusRectangle();

            base.OnDrawItem(e);
        }

        // --- Ensure any new pages added later are themed and do NOT use system visual style ---
        private void CustomTabControl_ControlAdded(object? sender, ControlEventArgs e)
        {
            if (e.Control is TabPage page)
            {
                ForceThemePage(page);
                Invalidate();
            }
        }

        private void ForceThemePage(TabPage page)
        {
            // CRITICAL: prevent OS gray
            page.UseVisualStyleBackColor = false;
            page.BackColor = _pageBackColor;
            page.ForeColor = _inactiveTextColor;
            page.Font = _headerFont ?? Font;
        }

        private int GetHeaderBandHeight()
        {
            // Estimate header band height from ItemSize and padding; keeps fill coverage correct
            // If tabs at top (default), band height equals ItemSize.Height + a small buffer
            return ItemSize.Height + Math.Max(2, Padding.Y * 2);
        }

        // --- Optional: Windows 11 TitleBar theming helper (can be called by the parent Form) ---
        public static void TryApplyCaptionColors(Form form, Color captionColor, Color textColor)
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return;

                IntPtr hwnd = form.Handle;
                const uint DWMWA_CAPTION_COLOR = 35;
                const uint DWMWA_TEXT_COLOR = 36;
                const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

                int cap = ColorToBgra(captionColor);
                int txt = ColorToBgra(textColor);
                int dark = 1;

                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref cap, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref txt, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
            }
            catch
            {
                // Safe no-op on unsupported systems
            }
        }

        private static Font MakeFont(string[] families, float size)
        {
            foreach (var f in families)
            {
                try { return new Font(f, size, FontStyle.Regular, GraphicsUnit.Point); }
                catch { /* try next */ }
            }
            return new Font("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Point);
        }

        private static int ColorToBgra(Color c) => c.B | (c.G << 8) | (c.R << 16) | (c.A << 24);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute, ref int pvAttribute, int cbAttribute);
    }
}
//DONE
