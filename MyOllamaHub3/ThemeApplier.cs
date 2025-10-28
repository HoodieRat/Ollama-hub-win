// File: ThemeApplier.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace MyOllamaHub3
{
    // Drop-in theming utility. Call ThemeApplier.Apply(this) after InitializeComponent().
    // This version FORCE-colors TabControl backgrounds/headers (no gray), and themes ListBox/CheckedListBox.
    public static class ThemeApplier
    {
        // Fonts (safe fallbacks)
        private static readonly Font HeaderFont = MakeFont(new[] { "Inter SemiBold", "Segoe UI Semibold", "Segoe UI" }, 10f);
        private static readonly Font BodyFont = MakeFont(new[] { "Inter", "Segoe UI" }, 9.5f);
        private static readonly Font ButtonFont = MakeFont(new[] { "Inter SemiBold", "Segoe UI Semibold", "Segoe UI" }, 10f);
        private static readonly Font DataFont = MakeFont(new[] { "Cascadia Code", "Consolas" }, 9f);

        // Refined, darker, welcoming palette
        private static readonly Color WarmSand = Color.FromArgb(198, 176, 145);   // page/background
        private static readonly Color SaddleTan = Color.FromArgb(170, 139, 99);   // tab header band / strips
        private static readonly Color Cocoa = Color.FromArgb(94, 63, 45);         // active tab / primary button
        private static readonly Color BurnishedCaramel = Color.FromArgb(140, 96, 62); // hover
        private static readonly Color Umber = Color.FromArgb(52, 39, 28);         // primary text
        private static readonly Color SoftTaupe = Color.FromArgb(108, 93, 77);    // secondary text
    private static readonly Color Cream = Color.FromArgb(240, 228, 210);      // light text / inputs
    private static readonly Color ListRowEven = Color.FromArgb(220, 204, 180); // primary list row base
    private static readonly Color ListRowOdd = Color.FromArgb(208, 192, 170);  // alt row base
        private static readonly Color WarmStone = Color.FromArgb(155, 140, 124);  // disabled
        private static readonly Color DeepSlate = Color.FromArgb(64, 90, 120);    // accent/focus
        private static readonly Color MistSlate = Color.FromArgb(166, 186, 205);  // alt rows / light hover
        private static readonly Color BorderUmber = Color.FromArgb(120, 102, 86); // borders

        // Track attached tab painters so we don't double-attach
        private static readonly ConditionalWeakTable<TabControl, TabControlPainter> TabPainters = new ConditionalWeakTable<TabControl, TabControlPainter>();

        public static void Apply(Form form)
        {
            if (form == null) return;

            form.BackColor = WarmSand;
            form.ForeColor = Umber;
            form.Font = BodyFont;

            TryApplyCaptionColors(form, Cocoa, Cream); // Title bar theming on Win11+, safe no-op elsewhere
            ApplyToControls(form.Controls);
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                try
                {
                    if (c is Panel)
                    {
                        var p = (Panel)c;
                        p.BackColor = SaddleTan;
                        p.ForeColor = Umber;
                        p.Font = HeaderFont;
                    }
                    else if (c is GroupBox)
                    {
                        var g = (GroupBox)c;
                        g.BackColor = WarmSand;
                        g.ForeColor = Umber;
                        g.Font = HeaderFont;
                    }
                    else if (c is Label)
                    {
                        var lbl = (Label)c;
                        lbl.ForeColor = Umber;
                        lbl.Font = BodyFont;
                    }
                    else if (c is Button)
                    {
                        var b = (Button)c;
                        b.FlatStyle = FlatStyle.Flat;
                        b.BackColor = Cocoa;
                        b.ForeColor = Cream;
                        b.Font = ButtonFont;
                        b.FlatAppearance.BorderColor = BorderUmber;
                        b.FlatAppearance.MouseOverBackColor = BurnishedCaramel;
                        b.FlatAppearance.MouseDownBackColor = Umber;
                    }
                    else if (c is TextBox)
                    {
                        var tb = (TextBox)c;
                        tb.BackColor = Cream;
                        tb.ForeColor = Umber;
                        tb.Font = BodyFont;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        if (!tb.Enabled) { tb.BackColor = WarmStone; tb.ForeColor = SoftTaupe; }
                    }
                    else if (c is RichTextBox)
                    {
                        var rtb = (RichTextBox)c;
                        rtb.BackColor = Cream;
                        rtb.ForeColor = Umber;
                        rtb.Font = DataFont;
                        rtb.BorderStyle = BorderStyle.FixedSingle;
                    }
                    else if (c is CheckBox)
                    {
                        var cb = (CheckBox)c;
                        cb.ForeColor = Umber;
                        cb.Font = BodyFont;
                        cb.FlatStyle = FlatStyle.Flat;
                        cb.FlatAppearance.BorderColor = BorderUmber;
                        cb.FlatAppearance.CheckedBackColor = MistSlate;
                    }
                    else if (c is RadioButton)
                    {
                        var rb = (RadioButton)c;
                        rb.ForeColor = Umber;
                        rb.Font = BodyFont;
                    }
                    else if (c is ComboBox)
                    {
                        var cbx = (ComboBox)c;
                        // Owner-draw for full theming
                        cbx.DrawMode = DrawMode.OwnerDrawFixed;
                        cbx.DropDownStyle = ComboBoxStyle.DropDownList;
                        cbx.FlatStyle = FlatStyle.Flat;
                        cbx.BackColor = Cream;
                        cbx.ForeColor = Umber;
                        cbx.Font = BodyFont;

                        cbx.DrawItem -= ComboBox_DrawItem;
                        cbx.DrawItem += ComboBox_DrawItem;
                        cbx.MeasureItem -= ComboBox_MeasureItem;
                        cbx.MeasureItem += ComboBox_MeasureItem;
                    }
                    else if (c is ListBox)
                    {
                        StyleListBox((ListBox)c);
                    }
                    else if (c is CheckedListBox)
                    {
                        StyleCheckedListBox((CheckedListBox)c);
                    }
                    else if (c is LinkLabel)
                    {
                        var ll = (LinkLabel)c;
                        ll.LinkColor = DeepSlate;
                        ll.ActiveLinkColor = Color.FromArgb(72, 104, 137);
                        ll.VisitedLinkColor = DeepSlate;
                        ll.Font = BodyFont;
                    }
                    else if (c is TabControl)
                    {
                        var tc = (TabControl)c;

                        // Full-force tab coloring: eliminate system gray via background erasing hook + owner draw
                        tc.Font = HeaderFont;
                        tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                        tc.ItemSize = new Size(100, 24);
                        tc.SizeMode = TabSizeMode.Fixed;
                        tc.Padding = new Point(8, 2);

                        // Color each page and disable OS visual style painting
                        foreach (TabPage page in tc.TabPages)
                        {
                            page.UseVisualStyleBackColor = false; // prevent OS gray
                            page.BackColor = WarmSand;
                            page.ForeColor = Umber;
                            page.Font = BodyFont;
                        }

                        // Owner-draw tab headers
                        tc.DrawItem -= TabControl_DrawItem;
                        tc.DrawItem += TabControl_DrawItem;

                        // Attach painter that intercepts WM_ERASEBKGND to fill header band & page area
                        if (!TabPainters.TryGetValue(tc, out _))
                        {
                            var painter = new TabControlPainter(tc, SaddleTan, WarmSand);
                            TabPainters.Add(tc, painter);
                        }
                    }
                    else if (c is DataGridView dgv)
                    {
                        StyleDataGridView(dgv);
                    }

                    else if (c is MenuStrip)
                    {
                        var ms = (MenuStrip)c;
                        ms.BackColor = SaddleTan;
                        ms.ForeColor = Umber;
                        ms.Font = HeaderFont;
                        ms.Renderer = new ThemedMenuRenderer(SaddleTan, WarmSand, MistSlate, BorderUmber, Cocoa, BurnishedCaramel);
                    }
                    else if (c is ContextMenuStrip)
                    {
                        var cms = (ContextMenuStrip)c;
                        cms.BackColor = SaddleTan;
                        cms.ForeColor = Umber;
                        cms.Font = HeaderFont;
                        cms.Renderer = new ThemedMenuRenderer(SaddleTan, WarmSand, MistSlate, BorderUmber, Cocoa, BurnishedCaramel);
                    }
                    else if (c is StatusStrip)
                    {
                        var ss = (StatusStrip)c;
                        ss.BackColor = SaddleTan;
                        ss.ForeColor = Umber;
                        ss.Font = BodyFont;
                    }
                    else if (c is ProgressBar)
                    {
                        var pb = (ProgressBar)c;
                        pb.ForeColor = DeepSlate;
                        pb.BackColor = BorderUmber;
                    }
                    else if (c is ListView lv)
                    {
                        StyleListView(lv);
                    }

                    if (c.HasChildren) ApplyToControls(c.Controls);
                }
                catch
                {
                    // keep going on individual control issues
                }
            }
        }

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
        private const int LVS_EX_DOUBLEBUFFER = 0x00010000;
        private const int LVS_EX_FULLROWSELECT = 0x00000020;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void StyleListView(ListView lv)
        {
            if (lv == null) return;

            // Base theme
            lv.BackColor = ListRowEven;
            lv.ForeColor = Umber;
            lv.Font = BodyFont;
            lv.BorderStyle = BorderStyle.FixedSingle;

            // Behavior
            lv.FullRowSelect = true;
            lv.HideSelection = false;
            lv.GridLines = false;
            lv.HeaderStyle = ColumnHeaderStyle.Clickable;
            lv.HoverSelection = false;
            lv.HotTracking = false;
            lv.UseCompatibleStateImageBehavior = false;

            // Double buffer + full row select extended styles
            TrySetListViewDoubleBuffered(lv, true);
            try
            {
                int ex = LVS_EX_DOUBLEBUFFER | LVS_EX_FULLROWSELECT;
                SendMessage(lv.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, (IntPtr)ex, (IntPtr)ex);
            }
            catch { }

            // De-dupe handlers
            lv.DrawColumnHeader -= ListView_DrawColumnHeader;
            lv.DrawItem -= ListView_DrawItem_BackgroundBySubitem;   // renamed
            lv.DrawSubItem -= ListView_DrawSubItem_Full;            // renamed
            lv.HandleCreated -= ListView_HandleCreated_Reapply;
            lv.SizeChanged -= ListView_InvalidateOnResize;
            lv.MouseMove -= ListView_InvalidateOnMouseMove;         // <— where to put it
            lv.MouseLeave -= ListView_InvalidateOnMouseLeave;

            if (lv.View == View.Details)
            {
                lv.OwnerDraw = true;
                lv.DrawColumnHeader += ListView_DrawColumnHeader;
                lv.DrawItem += ListView_DrawItem_BackgroundBySubitem;   // does not paint bg
                lv.DrawSubItem += ListView_DrawSubItem_Full;            // paints bg + text per cell
            }
            else
            {
                lv.OwnerDraw = false;
            }

            lv.HandleCreated += ListView_HandleCreated_Reapply;
            lv.SizeChanged += ListView_InvalidateOnResize;

            // 🔧 Answer: put the MouseMove/MouseLeave hooks RIGHT HERE
            lv.MouseMove += ListView_InvalidateOnMouseMove;
            lv.MouseLeave += ListView_InvalidateOnMouseLeave;
        }

    private static void ListView_HandleCreated_Reapply(object? sender, EventArgs e)
        {
            var lv = sender as ListView;
            if (lv != null) StyleListView(lv);
        }

    private static void ListView_InvalidateOnResize(object? sender, EventArgs e)
        {
            (sender as ListView)?.Invalidate();
        }
    private static void ListView_InvalidateOnMouseMove(object? sender, MouseEventArgs e)
        {
            (sender as ListView)?.Invalidate();
        }
    private static void ListView_InvalidateOnMouseLeave(object? sender, EventArgs e)
        {
            (sender as ListView)?.Invalidate();
        }

    private static void ListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = false;

            using (var bg = new SolidBrush(Cocoa))
                e.Graphics.FillRectangle(bg, e.Bounds);

            var header = e.Header;
            var headerAlign = header?.TextAlign ?? HorizontalAlignment.Left;

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= headerAlign == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter
                   : headerAlign == HorizontalAlignment.Right ? TextFormatFlags.Right
                   : TextFormatFlags.Left;

            TextRenderer.DrawText(
                e.Graphics,
                header?.Text ?? string.Empty,
                HeaderFont,
                new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height),
                Cream,
                flags
            );

            using (var pen = new Pen(BorderUmber))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        // Do NOT paint background here; we’ll do it in DrawSubItem for EVERY cell.
        // This avoids hover/OS hot overlays nuking subitem text.
    private static void ListView_DrawItem_BackgroundBySubitem(object? sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = false;

            // Focus rectangle only (background handled per-subitem)
            var lv = sender as ListView;
            if (lv != null && lv.Focused && e.Item.Selected)
                e.DrawFocusRectangle();
        }

    private static void ListView_DrawSubItem_Full(object? sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawDefault = false;

            var lv = sender as ListView;
            var item = e.Item;
            if (item == null)
                return;

            var subItem = e.SubItem;
            bool selected = item.Selected;

            // Compute the row back color here so each cell repaints fully and consistently.
            Color rowBack = selected ? MistSlate
                                     : (e.ItemIndex % 2 == 1 ? ListRowOdd : ListRowEven);

            // Fill THIS CELL’s background – this is the key to stability on hover.
            using (var bg = new SolidBrush(rowBack))
                e.Graphics.FillRectangle(bg, e.Bounds);

            // Optional row separator
            using (var pen = new Pen(BorderUmber))
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

            // Checkbox in first column if CheckBoxes enabled
            int leftPad = 8;
            if (lv != null && lv.CheckBoxes && e.ColumnIndex == 0)
            {
                Rectangle box = new Rectangle(e.Bounds.Left + 8, e.Bounds.Top + (e.Bounds.Height - 16) / 2, 16, 16);
                bool isChecked = item.Checked;

                if (Application.RenderWithVisualStyles)
                {
                    var state = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(box.Left, box.Top), state);
                }
                else
                {
                    using (var pen = new Pen(BorderUmber))
                        e.Graphics.DrawRectangle(pen, box);
                    if (isChecked)
                        using (var b = new SolidBrush(DeepSlate))
                            e.Graphics.FillRectangle(b, Rectangle.Inflate(box, -3, -3));
                }

                leftPad = box.Right + 6;
            }

            // Text rect
            int padLeft = (e.ColumnIndex == 0 ? leftPad : e.Bounds.Left + 8);
            Rectangle textRect = new Rectangle(padLeft, e.Bounds.Top, e.Bounds.Right - padLeft - 4, e.Bounds.Height);

            var header = e.Header;
            var headerAlign = header?.TextAlign ?? HorizontalAlignment.Left;

            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            flags |= headerAlign == HorizontalAlignment.Center ? TextFormatFlags.HorizontalCenter
                   : headerAlign == HorizontalAlignment.Right ? TextFormatFlags.Right
                   : TextFormatFlags.Left;

            TextRenderer.DrawText(
                e.Graphics,
                subItem?.Text ?? string.Empty,
                BodyFont,
                textRect,
                Umber,
                flags
            );
        }

        // DoubleBuffered reflection helper
        private static void TrySetListViewDoubleBuffered(ListView lv, bool on)
        {
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(lv, on, null);
            }
            catch { }
        }

        // ---------- DataGridView theming (headers, rows, selection, runtime-added) ----------
        private static void StyleDataGridView(DataGridView dgv)
        {
            if (dgv == null) return;

            // Reduce flicker
            TrySetDoubleBuffered(dgv, true);

            // Grid-level options
            dgv.EnableHeadersVisualStyles = false; // CRITICAL: allow our header colors
            dgv.BackgroundColor = WarmSand;
            dgv.GridColor = BorderUmber;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

            // Selection behavior (feel free to adjust)
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // Columns autosize defaults (optional)
            if (dgv.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None)
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ApplyDgvStyles(dgv);

            // Hook events so columns/rows added later keep the theme
            dgv.ColumnAdded -= Dgv_ColumnAdded_ApplyTheme;
            dgv.ColumnAdded += Dgv_ColumnAdded_ApplyTheme;

            dgv.RowsAdded -= Dgv_RowsAdded_ApplyTheme;
            dgv.RowsAdded += Dgv_RowsAdded_ApplyTheme;

            dgv.DataBindingComplete -= Dgv_DataBindingComplete_ApplyTheme;
            dgv.DataBindingComplete += Dgv_DataBindingComplete_ApplyTheme;
        }

        private static void ApplyDgvStyles(DataGridView dgv)
        {
            // Header (column)
            var colHdr = new DataGridViewCellStyle
            {
                BackColor = Cocoa,
                ForeColor = Cream,
                SelectionBackColor = Cocoa,    // keep header steady on select
                SelectionForeColor = Cream,
                Font = HeaderFont,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                WrapMode = DataGridViewTriState.True
            };
            dgv.ColumnHeadersDefaultCellStyle = colHdr;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            // Header (row)
            var rowHdr = new DataGridViewCellStyle
            {
                BackColor = SaddleTan,
                ForeColor = Umber,
                SelectionBackColor = SaddleTan,
                SelectionForeColor = Umber,
                Font = BodyFont,
                Alignment = DataGridViewContentAlignment.MiddleRight
            };
            dgv.RowHeadersDefaultCellStyle = rowHdr;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // Default cells
            var cell = new DataGridViewCellStyle
            {
                BackColor = Cream,
                ForeColor = Umber,
                SelectionBackColor = MistSlate,
                SelectionForeColor = Umber,
                Font = DataFont,
                WrapMode = DataGridViewTriState.False
            };
            dgv.DefaultCellStyle = cell;

            // Alternating rows
            var alt = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(235, 226, 214), // soft variant of Cream
                ForeColor = Umber,
                SelectionBackColor = MistSlate,
                SelectionForeColor = Umber,
                Font = DataFont
            };
            dgv.AlternatingRowsDefaultCellStyle = alt;

            // Row template (height & font consistency)
            dgv.RowTemplate.DefaultCellStyle = cell;
            dgv.RowTemplate.Height = Math.Max(22, (int)Math.Ceiling(DataFont.GetHeight() + 6));

            // Link columns (if any)
            foreach (DataGridViewColumn col in dgv.Columns)
                StyleColumnByType(col);
        }

        private static void StyleColumnByType(DataGridViewColumn col)
        {
            if (col is DataGridViewLinkColumn linkCol)
            {
                linkCol.LinkColor = DeepSlate;
                linkCol.VisitedLinkColor = DeepSlate;
                linkCol.ActiveLinkColor = Color.FromArgb(72, 104, 137);
                linkCol.TrackVisitedState = false;
                linkCol.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Cream,
                    ForeColor = DeepSlate,
                    SelectionBackColor = MistSlate,
                    SelectionForeColor = Umber,
                    Font = DataFont
                };
            }
            else if (col is DataGridViewCheckBoxColumn chkCol)
            {
                // Keep checkbox readable on both base and selection
                chkCol.DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Cream,
                    ForeColor = Umber,
                    SelectionBackColor = MistSlate,
                    SelectionForeColor = Umber,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }
            else
            {
                // Normal text column
                col.DefaultCellStyle.BackColor = Cream;
                col.DefaultCellStyle.ForeColor = Umber;
                col.DefaultCellStyle.SelectionBackColor = MistSlate;
                col.DefaultCellStyle.SelectionForeColor = Umber;
                col.DefaultCellStyle.Font = DataFont;
            }
        }

        // --- DGV event hooks to keep theme when data/columns change ---
    private static void Dgv_ColumnAdded_ApplyTheme(object? sender, DataGridViewColumnEventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                StyleColumnByType(e.Column);
                // Ensure header style remains intact
                dgv.EnableHeadersVisualStyles = false;
            }
        }

    private static void Dgv_RowsAdded_ApplyTheme(object? sender, DataGridViewRowsAddedEventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                // Reapply row height in case datasource changed it
                dgv.RowTemplate.Height = Math.Max(22, (int)Math.Ceiling(DataFont.GetHeight() + 6));
            }
        }

    private static void Dgv_DataBindingComplete_ApplyTheme(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                ApplyDgvStyles(dgv);
            }
        }

        // Reduce flicker on legacy frameworks via reflection
        private static void TrySetDoubleBuffered(DataGridView dgv, bool on)
        {
            try
            {
                // .DoubleBuffered is protected; reflect it
                var prop = typeof(DataGridView).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(dgv, on, null);
            }
            catch { /* safe no-op */ }
        }

        // ---------- ListBox / CheckedListBox theming ----------

        private static void StyleListBox(ListBox lb)
        {
            lb.BackColor = Cream;
            lb.ForeColor = Umber;
            lb.Font = BodyFont;

            lb.DrawMode = DrawMode.OwnerDrawFixed;
            lb.ItemHeight = Math.Max(18, (int)Math.Ceiling(BodyFont.GetHeight() + 4));
            lb.BorderStyle = BorderStyle.FixedSingle;
            lb.HorizontalScrollbar = false;
            lb.IntegralHeight = false;

            lb.DrawItem -= List_DrawItem;
            lb.DrawItem += List_DrawItem;
        }

        private static void StyleCheckedListBox(CheckedListBox clb)
        {
            clb.BackColor = Cream;
            clb.ForeColor = Umber;
            clb.Font = BodyFont;

            clb.DrawMode = DrawMode.OwnerDrawFixed;
            clb.ItemHeight = Math.Max(18, (int)Math.Ceiling(BodyFont.GetHeight() + 4));
            clb.BorderStyle = BorderStyle.FixedSingle;
            clb.CheckOnClick = true;
            clb.IntegralHeight = false;

            clb.DrawItem -= List_DrawItem;
            clb.DrawItem += List_DrawItem;
        }

    private static void List_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                e.DrawBackground();
                return;
            }

            var ctrl = sender as Control;
            var clbSender = sender as CheckedListBox;
            Font listFont = ctrl?.Font ?? BodyFont;
            bool listEnabled = ctrl?.Enabled ?? true;

            // background shades
            Color baseBack = ListRowEven;
            Color altBack = ListRowOdd;
            Color selBack = MistSlate;
            Color disBack = WarmStone;

            // text colors
            Color baseText = Umber;
            Color disText = SoftTaupe;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool disabled = !listEnabled;

            Rectangle bounds = e.Bounds;
            Color fill = disabled ? disBack : (selected ? selBack : ((e.Index % 2 == 1) ? altBack : baseBack));

            using (var bg = new SolidBrush(fill))
                e.Graphics.FillRectangle(bg, bounds);

            // Checkbox for CheckedListBox
            int textLeft = bounds.Left + 6;
            if (clbSender != null)
            {
                bool isChecked = clbSender.GetItemChecked(e.Index);

                var glyphBounds = new Rectangle(bounds.Left + 6, bounds.Top + (bounds.Height - 16) / 2, 16, 16);

                if (Application.RenderWithVisualStyles)
                {
                    var state = isChecked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                    CheckBoxRenderer.DrawCheckBox(e.Graphics, new Point(glyphBounds.Left, glyphBounds.Top), state);
                }
                else
                {
                    using (var pen = new Pen(BorderUmber))
                        e.Graphics.DrawRectangle(pen, glyphBounds);
                    if (isChecked)
                        using (var b = new SolidBrush(DeepSlate))
                            e.Graphics.FillRectangle(b, Rectangle.Inflate(glyphBounds, -3, -3));
                }

                textLeft = glyphBounds.Right + 6;
            }

            // Item text
            string text = string.Empty;
            var asLB = sender as ListBox;
            var asCLB = clbSender;

            if (asLB != null)
                text = asLB.GetItemText(asLB.Items[e.Index]) ?? string.Empty;
            else if (asCLB != null)
                text = asCLB.GetItemText(asCLB.Items[e.Index]) ?? string.Empty;

            var textRect = new Rectangle(textLeft, bounds.Top, bounds.Width - (textLeft - bounds.Left) - 6, bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                text,
                listFont,
                textRect,
                disabled ? disText : baseText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                e.DrawFocusRectangle();

            using (var pen = new Pen(BorderUmber))
                e.Graphics.DrawRectangle(pen, Rectangle.Inflate(bounds, -1, -1));
        }

        // --- ComboBox owner draw ---
    private static void ComboBox_MeasureItem(object? sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.ItemHeight = Math.Max(18, (int)Math.Ceiling(BodyFont.GetHeight() + 6));
        }

    private static void ComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var cbx = sender as ComboBox;
            if (cbx == null) return;

            e.DrawBackground();
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (var bg = new SolidBrush(selected ? MistSlate : Cream))
                e.Graphics.FillRectangle(bg, e.Bounds);

            TextRenderer.DrawText(
                e.Graphics,
                cbx.GetItemText(cbx.Items[e.Index]),
                cbx.Font,
                e.Bounds,
                Umber,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter
            );

            e.DrawFocusRectangle();
        }

        // --- TabControl owner draw (headers only; background handled by TabControlPainter) ---
    private static void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab = sender as TabControl;
            if (tab == null) return;

            bool selected = (e.Index == tab.SelectedIndex);
            Rectangle rect = e.Bounds;

            using (Brush fill = new SolidBrush(selected ? Cocoa : SaddleTan))
                e.Graphics.FillRectangle(fill, rect);

            TextRenderer.DrawText(
                e.Graphics,
                tab.TabPages[e.Index].Text,
                HeaderFont,
                rect,
                selected ? Cream : Umber,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );

            if (!selected)
                using (var pen = new Pen(BorderUmber))
                    e.Graphics.DrawLine(pen, rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom - 1);

            e.DrawFocusRectangle();
        }

        // --- Menu theming ---
        private class ThemedMenuRenderer : ToolStripProfessionalRenderer
        {
            public ThemedMenuRenderer(Color bar, Color drop, Color hover, Color border, Color pressedStart, Color pressedEnd)
                : base(new ThemedMenuColors(bar, drop, hover, border, pressedStart, pressedEnd)) { }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Umber;
                base.OnRenderItemText(e);
            }
        }

        private class ThemedMenuColors : ProfessionalColorTable
        {
            private readonly Color bar, drop, hover, border, pressedStart, pressedEnd;

            public ThemedMenuColors(Color bar, Color drop, Color hover, Color border, Color pressedStart, Color pressedEnd)
            {
                this.bar = bar; this.drop = drop; this.hover = hover; this.border = border; this.pressedStart = pressedStart; this.pressedEnd = pressedEnd;
            }

            public override Color ToolStripGradientBegin => bar;
            public override Color ToolStripGradientMiddle => bar;
            public override Color ToolStripGradientEnd => bar;

            public override Color ToolStripDropDownBackground => drop;
            public override Color MenuBorder => border;
            public override Color MenuItemBorder => border;

            public override Color MenuItemSelected => hover;
            public override Color MenuItemSelectedGradientBegin => hover;
            public override Color MenuItemSelectedGradientEnd => hover;

            public override Color MenuItemPressedGradientBegin => pressedStart;
            public override Color MenuItemPressedGradientEnd => pressedEnd;

            public override Color ImageMarginGradientBegin => drop;
            public override Color ImageMarginGradientMiddle => drop;
            public override Color ImageMarginGradientEnd => drop;
        }

        // --- Helpers ---
        private static Font MakeFont(string[] families, float size)
        {
            foreach (var f in families)
            {
                try { return new Font(f, size, FontStyle.Regular, GraphicsUnit.Point); }
                catch { /* try next */ }
            }
            return new Font("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Point);
        }

        // Windows 11 Title Bar Coloring (safe no-op elsewhere)
        private static void TryApplyCaptionColors(Form form, Color captionColor, Color textColor)
        {
            try
            {
                // If your target framework doesn't support OperatingSystem.IsWindows(), this still won't throw.
                if (!OperatingSystem.IsWindows()) return;

                IntPtr hwnd = form.Handle;
                const uint DWMWA_CAPTION_COLOR = 35;
                const uint DWMWA_TEXT_COLOR = 36;
                const uint DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

                int cap = ColorToBgra(captionColor);
                int text = ColorToBgra(textColor);
                int dark = 1;

                DwmSetWindowAttribute(hwnd, DWMWA_CAPTION_COLOR, ref cap, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_TEXT_COLOR, ref text, sizeof(int));
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
            }
            catch
            {
                // ignore if unsupported
            }
        }

        private static int ColorToBgra(Color c) => c.B | (c.G << 8) | (c.R << 16) | (c.A << 24);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, uint dwAttribute, ref int pvAttribute, int cbAttribute);

        // =====================================================================
        // INTERNAL: TabControlPainter — attaches to a TabControl without replacing it,
        // intercepts WM_ERASEBKGND to fully paint the header band + page area.
        // =====================================================================
        private sealed class TabControlPainter : NativeWindow, IDisposable
        {
            private const int WM_ERASEBKGND = 0x0014;
            private const int WM_THEMECHANGED = 0x031A;
            private readonly TabControl _tc;
            private readonly Color _headerBandColor;
            private readonly Color _pageColor;

            public TabControlPainter(TabControl tc, Color headerBandColor, Color pageColor)
            {
                _tc = tc ?? throw new ArgumentNullException(nameof(tc));
                _headerBandColor = headerBandColor;
                _pageColor = pageColor;

                if (_tc.IsHandleCreated) AssignHandle(_tc.Handle);
                _tc.HandleCreated += OnHandleCreated;
                _tc.HandleDestroyed += OnHandleDestroyed;
                _tc.Disposed += OnDisposed;
            }

            private void OnHandleCreated(object? sender, EventArgs e)
            {
                AssignHandle(_tc.Handle);
            }

            private void OnHandleDestroyed(object? sender, EventArgs e)
            {
                ReleaseHandle();
            }

            private void OnDisposed(object? sender, EventArgs e)
            {
                Dispose();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_ERASEBKGND && m.WParam != IntPtr.Zero)
                {
                    try
                    {
                        using (var g = Graphics.FromHdc(m.WParam))
                        {
                            // Header band: entire client area behind tabs
                            using (var band = new SolidBrush(_headerBandColor))
                                g.FillRectangle(band, _tc.ClientRectangle);

                            // Page area: inside DisplayRectangle
                            using (var page = new SolidBrush(_pageColor))
                                g.FillRectangle(page, _tc.DisplayRectangle);
                        }
                        m.Result = (IntPtr)1; // indicate we've erased background
                        return;
                    }
                    catch
                    {
                        // fall through to default if anything goes wrong
                    }
                }
                else if (m.Msg == WM_THEMECHANGED)
                {
                    _tc.Invalidate(true);
                }

                base.WndProc(ref m);
            }

            public void Dispose()
            {
                try
                {
                    _tc.HandleCreated -= OnHandleCreated;
                    _tc.HandleDestroyed -= OnHandleDestroyed;
                    _tc.Disposed -= OnDisposed;
                }
                catch { }
                try { ReleaseHandle(); } catch { }
            }
        }
    }
}
