using System;
using System.Drawing;
using System.Windows.Forms;
using TrackFlow.Models;
using TrackFlow.Utils;
using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Service;
using System.Windows.Shapes;

namespace TrackFlow.Forms;
public partial class PlannerPage : UserControl
{
    private class MaterialCalendar : UserControl
    {
        private PlannerPage master;
        private TableLayoutPanel? main;
        private Label? lblMonth;
        private TableLayoutPanel? grid;

        private DateTime currentMonth = DateTime.Today;

        // Public single source of truth for the selected date
        public DateTime SelectedDate { get; private set; } = DateTime.Today;

        public event Action<DateTime>? DaySelected;

        // Reusable cell pool (42 cells = 6 rows * 7 cols)
        private readonly List<Frame> _dayCells = new List<Frame>(42);

        public MaterialCalendar(PlannerPage _master)
        {
            master = _master;

            Dock = DockStyle.Fill;
            DoubleBuffered = true;            // reduce flicker
            BuildLayout();
            RenderMonth(currentMonth);
        }

        private void BuildLayout()
        {
            main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 60f)); // month
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f)); // days
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // actual date
            this.Controls.Add(main);

            BuildHeader();
            BuildDayNames();
            BuildGrid(); // grid  pre-populates 42 placeholder cells
        }

        private void BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2
            };
            header.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50f)); // back
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // month text
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50f)); // forward

            var Tdivider = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 2, 2, 2),
                BackColor = MainForm.PrimaryDark
            };
            header.Controls.Add(Tdivider, 0, 0);
            header.SetColumnSpan(Tdivider, 3);

            var prev = new MaterialButton { Icon = IconLibrary.GetBitmap(AppIcon.ArrowBack, 24, MainForm.PrimaryLight), Dock = DockStyle.Fill };
            var next = new MaterialButton { Icon = IconLibrary.GetBitmap(AppIcon.ArrowForward, 24, MainForm.PrimaryLight), Dock = DockStyle.Fill };
            lblMonth = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            prev.Click += (s, e) => ChangeMonth(-1);
            next.Click += (s, e) => ChangeMonth(1);

            header.Controls.Add(prev, 0, 1);
            header.Controls.Add(lblMonth, 1, 1);
            header.Controls.Add(next, 2, 1);

            main!.Controls.Add(header, 0, 0);
        }

        private void BuildDayNames()
        {
            var days = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 13
            };

            string[] names = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            days.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));
            days.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            days.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));

            var Tdivider = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 0, 2, 2),
                BackColor = MainForm.PrimaryDark
            };
            days.Controls.Add(Tdivider, 0, 0);
            days.SetColumnSpan(Tdivider, 13);

            int counter = 0;
            for (int i = 1; i <= 13; i++)
            {
                if (i % 2 == 0)
                {
                    days.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6f));
                    days.Controls.Add(new MaterialDivider
                    {
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2, 2, 2, 2),
                        BackColor = MainForm.PrimaryDark
                    }, i - 1, 1);
                }
                else
                {
                    days.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
                    days.Controls.Add(new Label
                    {
                        Text = names[counter],
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    }, i - 1, 1);
                    counter++;
                }
            }

            var Bdivider = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 2, 2, 2),
                BackColor = MainForm.PrimaryDark
            };
            days.Controls.Add(Bdivider, 0, 2);
            days.SetColumnSpan(Bdivider, 13);

            main!.Controls.Add(days, 0, 1);
        }

        private void BuildGrid()
        {
            grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 7,
            };

            for (int i = 0; i < 7; i++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
            for (int i = 0; i < 6; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));

            // Create 42 reusable Frame cells once and add them to the grid.
            for (int i = 0; i < 42; i++)
            {
                var cell = new Frame
                {
                    Title = "", // will be set in RenderMonth
                    TitleFontStyle = FontStyle.Regular,
                    TitleFontSize = 11,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    HoverColor = MainForm.PrimaryAsh,
                    PressedColor = MainForm.PrimaryAsh,
                    HoverDelayMs = 100,
                    Cursor = Cursors.Hand,
                    DateHolder = DateTime.MinValue, // I just made a holder for it in the Frame class, I am lazy for any other solution
                    IconMargin = 0 // just like that a new value... I am lazy leave me alone ＼(*T▽T*)／
                };

                // Keep the click behavior: update selected date and repaint
                cell.Click += (s, e) =>
                {
                    if (cell.DateHolder != SelectedDate)
                    {
                        SelectedDate = cell.DateHolder;
                        DaySelected?.Invoke(cell.DateHolder);

                        // Re-color all cells quickly (they're in memory)
                        foreach (var c in _dayCells)
                            ColorPanel(c);
                    }
                };

                _dayCells.Add(cell);
                grid.Controls.Add(cell, i % 7, i / 7);
            }

            main!.Controls.Add(grid, 0, 2);
        }

        private void RenderMonth(DateTime month)
        {
            // Performance: build caches once for the month instead of per-cell calls
            master.CreateCacheOnNDay(month,true); // ensure caches exist

            // Build fast lookup maps for reminders/todos by date (date-only keys)
            var remMap = new Dictionary<DateTime, int>();
            var todoMap = new Dictionary<DateTime, int>();

            if (master._cacheCollection._cachedReminders is not null)
            {
                foreach (var r in master._cacheCollection._cachedReminders)
                {
                    var d = r.ReminderDate.Date;
                    if (!remMap.TryGetValue(d, out var c)) remMap[d] = 1; else remMap[d] = c + 1;
                }
            }

            if (master._cacheCollection._cachedTodos is not null)
            {
                foreach (var t in master._cacheCollection._cachedTodos)
                {
                    var d = t.Date.Date;
                    if (!todoMap.TryGetValue(d, out var c)) todoMap[d] = 1; else todoMap[d] = c + 1;
                }
            }

            // Prepare grid update
            grid!.SuspendLayout();
            try
            {
                lblMonth!.Text = month.ToString("MMMM yyyy");

                DateTime first = new(month.Year, month.Month, 1);
                int offset = ((int)first.DayOfWeek + 6) % 7;
                DateTime start = first.AddDays(-offset);

                for (int i = 0; i < 42; i++)
                {
                    DateTime day = start.AddDays(i);
                    var panel = _dayCells[i];

                    // update text and date holder
                    panel.Title = day.Day.ToString();
                    panel.DateHolder = day.Date;

                    // set icons via precomputed maps (no expensive filtering)
                    var hasRem = remMap.TryGetValue(day.Date, out var rc) && rc > 0;
                    var hasTodo = todoMap.TryGetValue(day.Date, out var tc) && tc > 0;

                    if (hasRem && hasTodo)
                    {
                        panel.Icon = IconLibrary.GetBitmap(AppIcon.DotDouble, 15);
                    }
                    else if (hasRem)
                    {
                        panel.Icon = IconLibrary.GetBitmap(AppIcon.DotSingle, 15, ColorTranslator.FromHtml("#ff2929"));
                    }
                    else if (hasTodo)
                    {
                        panel.Icon = IconLibrary.GetBitmap(AppIcon.DotSingle, 15, ColorTranslator.FromHtml("#fff700"));
                    }
                    else
                    {
                        panel.Icon = null;
                    }

                    // recolor according to selected / today / month
                    ColorPanel(panel);
                }
            }
            finally
            {
                grid.ResumeLayout();
                grid.Invalidate(); // one repaint for entire grid
            }
        }

        private void ColorPanel(Frame _)
        {
            if (_.DateHolder == SelectedDate)
            {
                _.NormalColor = MainForm.PrimaryLight;
                _.HoverColor = MainForm.PrimaryLight;
                _.PressedColor = MainForm.PrimaryLight;
                _.TitleColor = MainForm.PrimaryWhiteShade ? Color.Black : Color.White;
                _.SubtitleColor = ControlPaint.Light(_.TitleColor);
            }
            else if (_.DateHolder.Date == DateTime.Today)
            {
                _.NormalColor = MainForm.PrimaryAccent;
                _.TitleColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                _.SubtitleColor = ControlPaint.Light(_.TitleColor);
            }
            else if (_.DateHolder.Month == currentMonth.Month)
            {
                _.NormalColor = MainForm.PrimaryGrey;
                _.TitleColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                _.SubtitleColor = ControlPaint.Light(_.TitleColor);
            }
            else
            {
                _.NormalColor = MainForm.PrimaryMid;
                _.TitleColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                _.SubtitleColor = ControlPaint.Light(_.TitleColor);
            }

            // force repaint of that cell only
            _.Invalidate();
        }

        private void ChangeMonth(int delta)
        {

            currentMonth = currentMonth.AddMonths(delta);
            RenderMonth(currentMonth);
        }

        public void RefreshUI(DateTime month) // exposed function to truly update the calendar
        {
            RenderMonth(month);
        }
    }

    public partial class ViewTodo : MaterialForm
    {
        private readonly Todo _todo;
        private readonly PlannerPage _master;

        public event Action<Todo, int, bool>? LineToggled;

        public ViewTodo(Todo todo, PlannerPage master)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _todo = todo;
            _master = master;

            Text = $"Todo — {_todo.Date:yyyy-MM-dd}";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,56f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
            this.Controls.Add(main);

            var save_btn = new MaterialButton
            {
                Dock = DockStyle.Top,
                Height = 56,
                Text = "Save",
                Icon = IconLibrary.GetBitmap(AppIcon.Save,24,MainForm.PrimaryLight)  
            };
            save_btn.Click += (s, e) => SaveTodo();
            main.Controls.Add(save_btn,0,0);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            main.Controls.Add(panel,0,1);

            var content = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Width = panel.ClientSize.Width + 25,
                Height = panel.ClientSize.Height,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8, 0, 8, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(content);

            panel.SizeChanged += (s, e) =>
            {
                content.Width = panel.ClientSize.Width + 25;
                foreach (Control child in content.Controls)
                {
                    if (child is TableLayoutPanel tbl)
                    {
                        tbl.Width = Math.Max(0, content.ClientSize.Width - 25);
                    }
                }
            };

            // Build UI rows for each todo line
            for (int i = 0; i < _todo.Data.Count; i++)
            {
                Todo.SingleLine line = _todo.Data[i];

                var table = new TableLayoutPanel
                {
                    Width = Math.Max(0, content.ClientSize.Width - 25),
                    Height = 60,
                    ColumnCount = 4,
                    RowCount = 3,
                    Padding = new Padding(0, 0, 8, 0),
                    AutoSize = false
                };

                table.Tag = line;

                // add row/col styles
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));

                var top_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(top_divder, 0, 0);
                table.SetColumnSpan(top_divder, 4);

                var left_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(left_divder, 0, 1);

                var checkbox = new MaterialCheckbox
                {
                    Dock = DockStyle.Fill,
                    Checked = line.State
                };
                table.Controls.Add(checkbox, 1, 1);

                void OnContentSizeChanged(Object? s, EventArgs e)
                {
                    table.Width = Math.Max(0, content.ClientSize.Width - 25);
                }

                void OnTableSizeChanged(object? s, EventArgs e)
                {
                    int chkW = checkbox?.ClientSize.Width ?? 36;
                    var label = table.Controls.Count > 0 ? table.GetControlFromPosition(3, 1) as Frame : null;
                    if (label != null)
                    {
                        label.Width = Math.Max(0, table.ClientSize.Width - chkW);
                        label.Height = table.ClientSize.Height;
                    }
                }

                content.SizeChanged += OnContentSizeChanged;
                table.SizeChanged += OnTableSizeChanged;

                checkbox.CheckedChanged += (s, e) =>
                {
                    var tagged = table.Tag as Todo.SingleLine;
                    if (tagged is not null)
                    {
                        int idx = _todo.Data.IndexOf(tagged);
                        if (idx >= 0)
                        {
                            _todo.Data[idx].State = checkbox.Checked;
                            LineToggled?.Invoke(_todo, idx, checkbox.Checked);
                        }
                    }
                };

                var right_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(right_divder, 2, 1);

                var label = new Frame
                {
                    Margin = new Padding(0),
                    AutoSize = false,
                    Width = Math.Max(0, table.ClientSize.Width - checkbox.ClientSize.Width),
                    Height = table.ClientSize.Height,
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryMid,
                    PressedColor = MainForm.PrimaryMid,
                };

                if (line.Link is not null && _master._cacheCollection?._cachedReminders is not null)
                {
                    var result = ReminderService.SearchReminders(_master._cacheCollection._cachedReminders!, line.Link.PID);
                    if (result != null && result.Count > 0)
                    {
                        label.Title = $"Reminder set for: {result[0].ReminderDate}";
                        label.Subtitle = line.Data;
                        label.TitleFontSize = 12f;
                        label.SubtitleFontSize = 12f;

                        using (var g = table.CreateGraphics())
                        {
                            var sz_title = g.MeasureString(label.Title, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Bold, GraphicsUnit.Point), label.Width);
                            var sz_sub = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                            table.Height = (int)Math.Ceiling(sz_sub.Height) + (int)Math.Ceiling(sz_title.Height) + 40;
                            label.Height = table.Height;
                        }
                    }
                    else
                    {
                        label.Subtitle = line.Data;
                        label.SubtitleFontSize = 12f;
                        label.VSubtitleAlignment = StringAlignment.Center;
                        using (var g = table.CreateGraphics())
                        {
                            var sz = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                            table.Height = (int)Math.Ceiling(sz.Height) + 35;
                            label.Height = table.Height;
                        }
                    }
                }
                else
                {
                    label.Subtitle = line.Data;
                    label.SubtitleFontSize = 12f;
                    label.VSubtitleAlignment = StringAlignment.Center;

                    using (var g = table.CreateGraphics())
                    {
                        var sz = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                        table.Height = (int)Math.Ceiling(sz.Height) + 35;
                        label.Height = table.Height;
                    }
                }

                table.Controls.Add(label, 3, 1);

                var bottom_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(bottom_divder, 0, 2);
                table.SetColumnSpan(bottom_divder, 4);

                var ctx = new ContextMenuStrip();
                var deleteItem = new ToolStripMenuItem("Delete");
                deleteItem.Image = IconLibrary.GetBitmap(AppIcon.Delete, 20, MainForm.PrimaryLight);
                deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                deleteItem.BackColor = MainForm.PrimaryMid;
                deleteItem.Paint += (s, e) =>
                {
                    deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                    deleteItem.BackColor = MainForm.PrimaryMid;
                };

                deleteItem.Click += (s, e) =>
                {
                    var res = MessageBox.Show($"Delete Todo line? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res != DialogResult.Yes) return;

                    try
                    {
                        content.SizeChanged -= OnContentSizeChanged;
                        table.SizeChanged -= OnTableSizeChanged;

                        table.ContextMenuStrip = null;

                        
                        if (content.Controls.Contains(table))
                            content.Controls.Remove(table);

                        table.Disposed += (ss, ee) => ctx.Dispose();
                        table.Dispose();

                        var taggedLine = table.Tag as Todo.SingleLine;
                        if (taggedLine is not null)
                        {
                            int idxToRemove = _todo.Data.IndexOf(taggedLine);
                            if (idxToRemove >= 0)
                                _todo.Data.RemoveAt(idxToRemove);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting Todo line: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                ctx.Items.Add(deleteItem);
                table.ContextMenuStrip = ctx;

                content.Controls.Add(table);
            }
        }

        private void SaveTodo()
        {
            // delte the old todo
            bool ok = TodoService.DeleteTodo(_todo.Id); // delete the physical old file
            if (ok)
            {                
                retry:
                (bool s, ID f) = TodoService.SaveTodo(_todo); // create the new file with the updated data
                if (s)
                {
                    // close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                else
                {
                    var m = MessageBox.Show("Failed to recreate Todo (service returned failure).","Error",MessageBoxButtons.RetryCancel,MessageBoxIcon.Warning);
                    if (m == DialogResult.Retry)
                    {
                        goto retry;
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to update Todo (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
    }

    private class ViewReminder : MaterialForm
    {
        private MaterialComboBox? month;
        private MaterialComboBox? day;
        private MaterialComboBox? year;
        private MaterialComboBox? hour;
        private MaterialComboBox? minute;
        private MaterialComboBox? timeOfDay;
        private MaterialTextBox? note;
        private Reminder _reminder;
        public ViewReminder(Reminder reminder)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _reminder = reminder;

            Text = "Update Reminder";
            Size = new Size(550, 300);
            MinimumSize = new Size(550, 300);
            MaximumSize = new Size(550, 300);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 6,
                Padding = new Padding(0)
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,12f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            this.Controls.Add(main);

            var save_btn = new MaterialButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Save,24,MainForm.PrimaryLight),
                Text = "Save Reminder",
                Margin = new Padding(2,2,2,2),
                Dock = DockStyle.Fill
            };
            main.Controls.Add(save_btn,0,0);
            main.SetColumnSpan(save_btn,3);
            save_btn.Click += (s, e) => BtnSave_Click();

            var label = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Reminder Date",
                FontType = MaterialSkinManager.fontType.H6,
                TextAlign = ContentAlignment.MiddleCenter
            };
            main.Controls.Add(label,0,1);

            var Vdivider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Vdivider,1,1);

            var date = new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            main.Controls.Add(date,2,1);
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            month = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            month.Items.AddRange(Enumerable.Range(1, 12).Cast<object>().ToArray());
            date.Controls.Add(month,0,0);

            day = new MaterialComboBox 
            { 
                Hint = "DD", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            day.Items.AddRange(Enumerable.Range(1, 31).Cast<object>().ToArray());
            date.Controls.Add(day,1,0);

            year = new MaterialComboBox 
            { 
                Hint = "YY", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            year.Items.AddRange(Enumerable.Range(0, 100).Cast<object>().ToArray());
            date.Controls.Add(year,2,0);

            var Hdivider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Hdivider,0,2);
            main.SetColumnSpan(Hdivider,3);


            var label2 = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Reminder Time",
                FontType = MaterialSkinManager.fontType.H6,
                TextAlign = ContentAlignment.MiddleCenter
            };
            main.Controls.Add(label2,0,3);

            var Vdivider2 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Vdivider2,1,3);

            var time = new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            main.Controls.Add(time,2,3);
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            hour = new MaterialComboBox 
            { 
                Hint = "HH", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            hour.Items.AddRange(Enumerable.Range(1, 12).Cast<object>().ToArray());
            time.Controls.Add(hour,0,0);

            minute = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            minute.Items.AddRange(Enumerable.Range(0, 60).Cast<object>().ToArray());
            time.Controls.Add(minute,1,0);

            timeOfDay = new MaterialComboBox 
            { 
                Hint = "AM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            timeOfDay.Items.AddRange(new object[] {"AM","PM"});
            time.Controls.Add(timeOfDay,2,0);

            //  -----------------------------------------------------------
            // month & day:
            month.SelectedIndex = Math.Clamp(_reminder.ReminderDate.Month - 1, 0, month.Items.Count - 1);
            day.SelectedIndex = Math.Clamp(_reminder.ReminderDate.Day - 1, 0, day.Items.Count - 1);

            // year:
            int yearIndex = _reminder.ReminderDate.Year - 2000;
            if (yearIndex < 0) yearIndex = 0;
            if (yearIndex >= year.Items.Count) yearIndex = year.Items.Count - 1;
            year.SelectedIndex = yearIndex;

            // first convert 24h to 12h format
            int displayHour = ((_reminder.ReminderDate.Hour + 11) % 12) + 1;
            hour.SelectedIndex = Math.Clamp(displayHour - 1, 0, Math.Max(0, hour.Items.Count - 1));

            // minute combo uses 0..59 items, indices 0..59
            minute.SelectedIndex = Math.Clamp(_reminder.ReminderDate.Minute, 0, Math.Max(0, minute.Items.Count - 1));

            // AM/PM
            timeOfDay.SelectedIndex = (_reminder.ReminderDate.Hour < 12) ? 0 : 1;
            // -----------------------------------------------------------

            var Hdivider2 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Hdivider2,0,4);
            main.SetColumnSpan(Hdivider2,3);

            note = new MaterialTextBox
            {
                Text = _reminder.ReminderNote,
                Dock = DockStyle.Fill,
                Hint = "Add Note",
                UseTallSize = false
            };
            main.Controls.Add(note,0,5);
            main.SetColumnSpan(note,3);
        }
        private bool TryParseDateFromCombos(MaterialComboBox monthCb, MaterialComboBox dayCb, MaterialComboBox yearCb, out DateTime result)
        {
            result = default;

            if (monthCb?.SelectedItem == null || dayCb?.SelectedItem == null || yearCb?.SelectedItem == null)
                return false;

            if (!int.TryParse(monthCb.SelectedItem.ToString(), out int _month) ||
                !int.TryParse(dayCb.SelectedItem.ToString(), out int _day) ||
                !int.TryParse(yearCb.SelectedItem.ToString(), out int _year))
                return false;

            if (_year >= 0 && _year < 100) _year += 2000;

            try
            {
                result = new DateTime(_year, _month, _day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseTimeFromCombos(MaterialComboBox hourCb, MaterialComboBox minuteCb, MaterialComboBox timeOfDayCb, out DateTime result)
        {
            result = default;

            if (hourCb?.SelectedItem == null ||
                minuteCb?.SelectedItem == null ||
                timeOfDayCb?.SelectedItem == null)
                return false;

            if (!int.TryParse(hourCb.SelectedItem.ToString(), out int _hour) ||
                !int.TryParse(minuteCb.SelectedItem.ToString(), out int _minute))
                return false;

            string amPm = timeOfDayCb.SelectedItem.ToString()!;

            if (_hour < 1 || _hour > 12 || _minute < 0 || _minute > 59)
                return false;

            // Convert to 24-hour time
            if (amPm.Equals("AM", StringComparison.OrdinalIgnoreCase))
            {
                if (_hour == 12)
                    _hour = 0; // 12 AM = 00:xx
            }
            else if (amPm.Equals("PM", StringComparison.OrdinalIgnoreCase))
            {
                if (_hour != 12)
                    _hour += 12; // 1–11 PM → 13–23
            }
            else
            {
                return false;
            }

            result = DateTime.Today.AddHours(_hour).AddMinutes(_minute);
            return true;
        }

        private void BtnSave_Click()
        {
            try
            {
                bool hasDate = TryParseDateFromCombos(month!, day!, year!, out DateTime date);
                bool hasTime = TryParseTimeFromCombos(hour!, minute!, timeOfDay!, out DateTime time);

                // Date validation
                if (!hasDate)
                {
                    if (month!.SelectedItem != null ||
                        day!.SelectedItem != null ||
                        year!.SelectedItem != null)
                    {
                        MessageBox.Show("Reminder date is invalid or incomplete.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        month.Focus();
                        return;
                    }

                    MessageBox.Show("Please select a reminder date.", "Missing Date", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    month.Focus();
                    return;
                }

                // Time validation
                if (!hasTime)
                {
                    if (hour!.SelectedItem != null ||
                        minute!.SelectedItem != null ||
                        timeOfDay!.SelectedItem != null)
                    {
                        MessageBox.Show("Reminder time is invalid or incomplete.", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        hour.Focus();
                        return;
                    }

                    MessageBox.Show("Please select a reminder time.", "Missing Time", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    hour.Focus();
                    return;
                }

                // Combine date + time
                var reminderDateTime = date.Date.Add(time.TimeOfDay);

                // build reminder
                var r = new Reminder
                {
                    Id = _reminder.Id, // old ID
                    SavedDate = _reminder.SavedDate, // old save date
                    ReminderDate = reminderDateTime, // updated date 
                    ReminderNote = string.IsNullOrWhiteSpace(note!.Text) ? null : note.Text // upated note
                };

                SaveReminder(r);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating reminder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveReminder(Reminder r)
        {
           // delte the old note
            bool ok = ReminderService.DeleteReminder(r.Id);
            if (ok)
            {
                retry:
                (bool s, ID f) = ReminderService.SaveReminder(r);
                if (s)
                {
                    // close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                else
                {
                    var m = MessageBox.Show("Failed to recreate Reminder (service returned failure).","Error",MessageBoxButtons.RetryCancel,MessageBoxIcon.Warning);
                    if (m == DialogResult.Retry)
                    {
                        goto retry;
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to update Reminder (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
    }

    private class ViewNote : MaterialForm
    {
        private Note _note;
        private TableLayoutPanel main;
        private MaterialTextBox title;
        private MaterialMultiLineTextBox textbox;
        
        public ViewNote(Note note)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _note = note;

            Text = _note.Title;
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            
            main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
            this.Controls.Add(main);

            title = new MaterialTextBox
            {
                Margin = new Padding(8,8,8,0),
                Hint = "Title...",
                Text = _note.Title,
                Dock = DockStyle.Fill,
                UseTallSize = false
            };
            main.Controls.Add(title,0,0);

            var save_btn = new MaterialButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Save,24,MainForm.PrimaryLight),
                Dock = DockStyle.Fill,
                Text = "Save"
            };
            main.Controls.Add(save_btn,1,0);

            var divider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };            
            main.Controls.Add(divider,0,1);
            main.SetColumnSpan(divider,2);

            textbox = new MaterialMultiLineTextBox
            {
                Hint = "Note...",
                Dock = DockStyle.Fill,
                Text = FileHelper.ToMultiLine(_note.Data)  
            };
            main.Controls.Add(textbox,0,2);
            main.SetColumnSpan(textbox,2);

            save_btn.Click += (s, e) => SaveNote();
        }

        private void SaveNote()
        {
            // delte the old note
            bool ok = NoteService.DeleteNote(_note.Id);
            if (ok)
            {
                // build the new note:
                var note = new Note
                {
                    Id = _note.Id, // use the old note ID
                    Date = _note.Date, // old date
                    Title = "",
                    Data = ""
                };
                note.Title = string.IsNullOrWhiteSpace(title.Text) ? note.Id.PID : title.Text;
                note.Data = FileHelper.ToOneLine(textbox.Text);
                
                retry:
                (bool s, ID f) = NoteService.SaveNote(note);
                if (s)
                {
                    // close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
                else
                {
                    var m = MessageBox.Show("Failed to recreate Note (service returned failure).","Error",MessageBoxButtons.RetryCancel,MessageBoxIcon.Warning);
                    if (m == DialogResult.Retry)
                    {
                        goto retry;
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to update Note (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            }
        }
    }

    private class AddNote : MaterialForm
    {
        private TableLayoutPanel main;
        private MaterialTextBox title;
        private MaterialMultiLineTextBox textbox;
        
        public AddNote()
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            Text = "Add Note";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
            this.Controls.Add(main);

            title = new MaterialTextBox
            {
                Margin = new Padding(8,8,8,0),
                Hint = "Add Title...",
                Dock = DockStyle.Fill,
                UseTallSize = false
            };
            main.Controls.Add(title,0,0);

            var save_btn = new MaterialButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Save,24,MainForm.PrimaryLight),
                Dock = DockStyle.Fill,
                Text = "Save"
            };
            main.Controls.Add(save_btn,1,0);

            var divider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };            
            main.Controls.Add(divider,0,1);
            main.SetColumnSpan(divider,2);

            textbox = new MaterialMultiLineTextBox
            {
                Hint = "Add Note...",
                Dock = DockStyle.Fill  
            };
            main.Controls.Add(textbox,0,2);
            main.SetColumnSpan(textbox,2);

            save_btn.Click += (s, e) => SaveNote();
        }

        private void SaveNote()
        {
            // build note:
            var note = new Note
            {
                Id = IDGenerator.GenID("Note"),
                Date = DateTime.Today,
                Title = "",
                Data = ""
            };
            note.Title = string.IsNullOrWhiteSpace(title.Text) ? note.Id.PID : title.Text;
            note.Data = FileHelper.ToOneLine(textbox.Text);
            
            (bool s, ID f) = NoteService.SaveNote(note);
            if (s)
            {
                // close this form
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
        }
    }

    private class AddReminder : MaterialForm
    {
        private MaterialComboBox? month;
        private MaterialComboBox? day;
        private MaterialComboBox? year;
        private MaterialComboBox? hour;
        private MaterialComboBox? minute;
        private MaterialComboBox? timeOfDay;
        private MaterialTextBox? note;
        public AddReminder()
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            Text = "Add Reminder";
            Size = new Size(550, 300);
            MinimumSize = new Size(550, 300);
            MaximumSize = new Size(550, 300);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 6,
                Padding = new Padding(0)
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,12f));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,50f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,50f));
            this.Controls.Add(main);

            var save_btn = new MaterialButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Save,24,MainForm.PrimaryLight),
                Text = "Save Reminder",
                Margin = new Padding(2,2,2,2),
                Dock = DockStyle.Fill
            };
            main.Controls.Add(save_btn,0,0);
            main.SetColumnSpan(save_btn,3);
            save_btn.Click += (s, e) => BtnSave_Click();

            var label = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Reminder Date",
                FontType = MaterialSkinManager.fontType.H6,
                TextAlign = ContentAlignment.MiddleCenter
            };
            main.Controls.Add(label,0,1);

            var Vdivider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Vdivider,1,1);

            var date = new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            main.Controls.Add(date,2,1);
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            date.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            month = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            month.Items.AddRange(Enumerable.Range(1, 12).Cast<object>().ToArray());
            date.Controls.Add(month,0,0);

            day = new MaterialComboBox 
            { 
                Hint = "DD", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            day.Items.AddRange(Enumerable.Range(1, 31).Cast<object>().ToArray());
            date.Controls.Add(day,1,0);

            year = new MaterialComboBox 
            { 
                Hint = "YY", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            year.Items.AddRange(Enumerable.Range(0, 100).Cast<object>().ToArray());
            date.Controls.Add(year,2,0);

            var Hdivider = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Hdivider,0,2);
            main.SetColumnSpan(Hdivider,3);


            var label2 = new MaterialLabel
            {
                Dock = DockStyle.Fill,
                Text = "Reminder Time",
                FontType = MaterialSkinManager.fontType.H6,
                TextAlign = ContentAlignment.MiddleCenter
            };
            main.Controls.Add(label2,0,3);

            var Vdivider2 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Vdivider2,1,3);

            var time = new TableLayoutPanel 
            {
                Margin = new Padding(2), 
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            main.Controls.Add(time,2,3);
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            time.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            hour = new MaterialComboBox 
            { 
                Hint = "HH", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            hour.Items.AddRange(Enumerable.Range(1, 12).Cast<object>().ToArray());
            time.Controls.Add(hour,0,0);

            minute = new MaterialComboBox 
            { 
                Hint = "MM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            minute.Items.AddRange(Enumerable.Range(0, 60).Cast<object>().ToArray());
            time.Controls.Add(minute,1,0);

            timeOfDay = new MaterialComboBox 
            { 
                Hint = "AM", 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                Margin = new Padding(2), 
                Dock = DockStyle.Fill 
            };
            timeOfDay.Items.AddRange(new object[] {"AM","PM"});
            time.Controls.Add(timeOfDay,2,0);

            //  -----------------------------------------------------------
            var dt = DateTime.Now; 
            // month & day:
            month.SelectedIndex = Math.Clamp(dt.Month - 1, 0, month.Items.Count - 1);
            day.SelectedIndex = Math.Clamp(dt.Day - 1, 0, day.Items.Count - 1);

            // year:
            int yearIndex = dt.Year - 2000;
            if (yearIndex < 0) yearIndex = 0;
            if (yearIndex >= year.Items.Count) yearIndex = year.Items.Count - 1;
            year.SelectedIndex = yearIndex;

            // first convert 24h to 12h format
            int displayHour = ((dt.Hour + 11) % 12) + 1;
            hour.SelectedIndex = Math.Clamp(displayHour - 1, 0, Math.Max(0, hour.Items.Count - 1));

            // minute combo uses 0..59 items, indices 0..59
            minute.SelectedIndex = Math.Clamp(dt.Minute, 0, Math.Max(0, minute.Items.Count - 1));

            // AM/PM
            timeOfDay.SelectedIndex = (dt.Hour < 12) ? 0 : 1;
            // -----------------------------------------------------------

            var Hdivider2 = new MaterialDivider 
            { 
                Dock = DockStyle.Fill, 
                Margin = new Padding(2), 
                BackColor = MainForm.PrimaryDark 
            };
            main.Controls.Add(Hdivider2,0,4);
            main.SetColumnSpan(Hdivider2,3);

            note = new MaterialTextBox
            {
                Dock = DockStyle.Fill,
                Hint = "Add Note",
                UseTallSize = false
            };
            main.Controls.Add(note,0,5);
            main.SetColumnSpan(note,3);
        }
        private bool TryParseDateFromCombos(MaterialComboBox monthCb, MaterialComboBox dayCb, MaterialComboBox yearCb, out DateTime result)
        {
            result = default;

            if (monthCb?.SelectedItem == null || dayCb?.SelectedItem == null || yearCb?.SelectedItem == null)
                return false;

            if (!int.TryParse(monthCb.SelectedItem.ToString(), out int _month) ||
                !int.TryParse(dayCb.SelectedItem.ToString(), out int _day) ||
                !int.TryParse(yearCb.SelectedItem.ToString(), out int _year))
                return false;

            if (_year >= 0 && _year < 100) _year += 2000;

            try
            {
                result = new DateTime(_year, _month, _day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseTimeFromCombos(MaterialComboBox hourCb, MaterialComboBox minuteCb, MaterialComboBox timeOfDayCb, out DateTime result)
        {
            result = default;

            if (hourCb?.SelectedItem == null ||
                minuteCb?.SelectedItem == null ||
                timeOfDayCb?.SelectedItem == null)
                return false;

            if (!int.TryParse(hourCb.SelectedItem.ToString(), out int _hour) ||
                !int.TryParse(minuteCb.SelectedItem.ToString(), out int _minute))
                return false;

            string amPm = timeOfDayCb.SelectedItem.ToString()!;

            if (_hour < 1 || _hour > 12 || _minute < 0 || _minute > 59)
                return false;

            // Convert to 24-hour time
            if (amPm.Equals("AM", StringComparison.OrdinalIgnoreCase))
            {
                if (_hour == 12)
                    _hour = 0; // 12 AM = 00:xx
            }
            else if (amPm.Equals("PM", StringComparison.OrdinalIgnoreCase))
            {
                if (_hour != 12)
                    _hour += 12; // 1–11 PM → 13–23
            }
            else
            {
                return false;
            }

            result = DateTime.Today.AddHours(_hour).AddMinutes(_minute);
            return true;
        }

        private void BtnSave_Click()
        {
            try
            {
                bool hasDate = TryParseDateFromCombos(month!, day!, year!, out DateTime date);
                bool hasTime = TryParseTimeFromCombos(hour!, minute!, timeOfDay!, out DateTime time);

                // Date validation
                if (!hasDate)
                {
                    if (month!.SelectedItem != null ||
                        day!.SelectedItem != null ||
                        year!.SelectedItem != null)
                    {
                        MessageBox.Show("Reminder date is invalid or incomplete.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        month.Focus();
                        return;
                    }

                    MessageBox.Show("Please select a reminder date.", "Missing Date", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    month.Focus();
                    return;
                }

                // Time validation
                if (!hasTime)
                {
                    if (hour!.SelectedItem != null ||
                        minute!.SelectedItem != null ||
                        timeOfDay!.SelectedItem != null)
                    {
                        MessageBox.Show("Reminder time is invalid or incomplete.", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                        hour.Focus();
                        return;
                    }

                    MessageBox.Show("Please select a reminder time.", "Missing Time", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    hour.Focus();
                    return;
                }

                // Combine date + time
                var reminderDateTime = date.Date.Add(time.TimeOfDay);

                // build reminder
                var r = new Reminder
                {
                    Id = IDGenerator.GenID("Reminder"),
                    SavedDate = DateTime.Today,
                    ReminderDate = reminderDateTime,
                    ReminderNote = string.IsNullOrWhiteSpace(note!.Text) ? null : note.Text
                };

                // save reminder
                (bool s, ID i) = ReminderService.SaveReminder(r);
                if (s)
                {
                    // close this form
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating reminder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }

    private class AddTodo : MaterialForm
    {
        private PlannerPage _master;
        private TableLayoutPanel main;
        private Panel? panel;
        private Todo built_todo;
        private List<Todo.SingleLine> _lines = new List<Todo.SingleLine>();

        public AddTodo(PlannerPage master)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _master = master;

            Text = "Add Todo";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(0)
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.Controls.Add(main);

            // build todo
            built_todo = new Todo
            {
                Id = IDGenerator.GenID("Todo"),
                Date = DateTime.Today,
                Data = _lines
            };

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // ensure internal list is empty at start
            _lines = new List<Todo.SingleLine>();

            var todo_adder = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                ColumnCount = 3,
                Padding = new Padding(2, 2, 2, 0)
            };
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // todo text
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // add text
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // todo saver

            var todo_text = new MaterialTextBox
            {
                Hint = "Add Todo...",
                Dock = DockStyle.Fill,
                UseTallSize = false
            };
            todo_adder.Controls.Add(todo_text, 0, 0);

            var add_todo = new MaterialButton
            {
                Text = "Add",
                Icon = IconLibrary.GetBitmap(AppIcon.Add, 24, MainForm.PrimaryDark),
                Margin = new Padding(8, 0, 0, 0),
            };
            todo_adder.Controls.Add(add_todo, 1, 0);

            var save_todo = new MaterialButton
            {
                Text = "Save",
                Icon = IconLibrary.GetBitmap(AppIcon.Save, 24, MainForm.PrimaryDark),
                Margin = new Padding(8, 0, 0, 0),
            };
            todo_adder.Controls.Add(save_todo, 2, 0);
            save_todo.Click += (s, e) => SaveTodo();

            add_todo.Click += (s, e) =>
            {
                string _text = todo_text.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(_text))
                {
                    MessageBox.Show("Todo text can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    todo_text.Focus();
                }
                else
                {
                    todo_text.ResetText();
                    _lines.Add(new Todo.SingleLine{Data = _text});
                    built_todo.Data = _lines;

                    BuildPreview(built_todo);
                }
            };

            main.Controls.Add(todo_adder, 0, 0);

            var header_divder = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 0, 2, 2),
                BackColor = MainForm.PrimaryDark
            };
            main.Controls.Add(header_divder, 0, 1);

            panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            main.Controls.Add(panel, 0, 2);
        }

        private void BuildPreview(Todo _todo)
        {
            panel!.Controls.Clear();

            var content = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Width = panel.ClientSize.Width + 25,
                Height = panel.ClientSize.Height,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8, 0, 8, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            panel.Controls.Add(content);

            panel.SizeChanged += (s, e) =>
            {
                content.Width = panel.ClientSize.Width + 25;
            };

            for (int i = 0; i < _todo.Data.Count; i++)
            {
                Todo.SingleLine line = _todo.Data[i];
                var table = new TableLayoutPanel
                {
                    Width = Math.Max(0, content.ClientSize.Width - 25),
                    Height = 60,
                    ColumnCount = 4,
                    RowCount = 3,
                    Padding = new Padding(0, 0, 8, 0)
                };

                table.Tag = line;

                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4f));
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));

                var top_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(top_divder, 0, 0);
                table.SetColumnSpan(top_divder, 4);

                var left_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(left_divder, 0, 1);

                var checkbox = new MaterialCheckbox
                {
                    Dock = DockStyle.Fill,
                    Checked = line.State
                };
                table.Controls.Add(checkbox, 1, 1);

                EventHandler? onContentSizeChanged = null;
                EventHandler? onTableSizeChanged = null;

                onContentSizeChanged = (s, e) => { table.Width = Math.Max(0, content.ClientSize.Width - 25); };
                onTableSizeChanged = (s, e) =>
                {
                    int chkW = checkbox?.ClientSize.Width ?? 36;
                    var lbl = table.GetControlFromPosition(3, 1) as Frame;
                    if (lbl != null)
                    {
                        lbl.Width = Math.Max(0, table.ClientSize.Width - chkW);
                        lbl.Height = table.ClientSize.Height;
                    }
                };

                content.SizeChanged += onContentSizeChanged;
                table.SizeChanged += onTableSizeChanged;

                checkbox.CheckedChanged += (s, e) =>
                {
                    var tagged = table.Tag as Todo.SingleLine;
                    if (tagged is not null)
                    {
                        int idx = _lines.IndexOf(tagged);
                        if (idx >= 0)
                        {
                            _lines[idx].State = checkbox.Checked;
                        }
                    }
                };

                var right_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(right_divder, 2, 1);

                var label = new Frame
                {
                    Margin = new Padding(0),
                    AutoSize = false,
                    Width = Math.Max(0, table.ClientSize.Width - checkbox.ClientSize.Width),
                    Height = table.ClientSize.Height,
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryMid,
                    PressedColor = MainForm.PrimaryMid,
                };

                if (line.Link is not null && _master._cacheCollection?._cachedReminders is not null)
                {
                    var result = ReminderService.SearchReminders(_master._cacheCollection._cachedReminders!, line.Link.PID);
                    if (result != null && result.Count > 0)
                    {
                        label.Title = $"Reminder set for: {result[0].ReminderDate}";
                        label.Subtitle = line.Data;
                        label.TitleFontSize = 12f;
                        label.SubtitleFontSize = 12f;

                        using (var g = table.CreateGraphics())
                        {
                            var sz_title = g.MeasureString(label.Title, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Bold, GraphicsUnit.Point), label.Width);
                            var sz_sub = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                            table.Height = (int)Math.Ceiling(sz_sub.Height) + (int)Math.Ceiling(sz_title.Height) + 40;
                            label.Height = table.Height;
                        }
                    }
                    else
                    {
                        label.Subtitle = line.Data;
                        label.SubtitleFontSize = 12f;
                        label.VSubtitleAlignment = StringAlignment.Center;
                        using (var g = table.CreateGraphics())
                        {
                            var sz = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                            table.Height = (int)Math.Ceiling(sz.Height) + 35;
                            label.Height = table.Height;
                        }
                    }
                }
                else
                {
                    label.Subtitle = line.Data;
                    label.SubtitleFontSize = 12f;
                    label.VSubtitleAlignment = StringAlignment.Center;
                    using (var g = table.CreateGraphics())
                    {
                        var sz = g.MeasureString(label.Subtitle, new Font(SystemFonts.DefaultFont.FontFamily.Name, 12f, FontStyle.Regular, GraphicsUnit.Point), label.Width);
                        table.Height = (int)Math.Ceiling(sz.Height) + 35;
                        label.Height = table.Height;
                    }
                }

                table.Controls.Add(label, 3, 1);

                var bottom_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(bottom_divder, 0, 2);
                table.SetColumnSpan(bottom_divder, 4);

                var ctx = new ContextMenuStrip();
                var deleteItem = new ToolStripMenuItem("Delete");
                deleteItem.Image = IconLibrary.GetBitmap(AppIcon.Delete, 20, MainForm.PrimaryLight);
                deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                deleteItem.BackColor = MainForm.PrimaryMid;
                deleteItem.Paint += (s, e) =>
                {
                    deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                    deleteItem.BackColor = MainForm.PrimaryMid;
                };

                deleteItem.Click += (s, e) =>
                {
                    var res = MessageBox.Show($"Delete Todo line? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res != DialogResult.Yes) return;

                    try
                    {
                        if (onContentSizeChanged != null) content.SizeChanged -= onContentSizeChanged;
                        if (onTableSizeChanged != null) table.SizeChanged -= onTableSizeChanged;

                        table.ContextMenuStrip = null;

                        if (content.Controls.Contains(table))
                            content.Controls.Remove(table);

                        table.Disposed += (ss, ee) => ctx.Dispose();

                        table.Dispose();

                        var taggedLine = table.Tag as Todo.SingleLine;
                        if (taggedLine is not null)
                        {
                            int idxToRemove = _lines.IndexOf(taggedLine);
                            if (idxToRemove >= 0)
                                _lines.RemoveAt(idxToRemove);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting Todo line: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                ctx.Items.Add(deleteItem);
                table.ContextMenuStrip = ctx;

                content.Controls.Add(table);
            }
        }

        private void SaveTodo()
        {
            (bool s, ID f) = TodoService.SaveTodo(built_todo);
            if (s)
            {
                // close this form
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }
        }
    }


    private FlowLayoutPanel BuildDayList()
    {
        CreateCacheOnNDay(calendar.SelectedDate,true);
        int reminders_count = _cache_on_N_day_reminders!.Count();
        int todos_count = _cache_on_N_day_todos!.Count();

        var _panel = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0,6,6,6),
            Dock = DockStyle.Fill
        };

        if (reminders_count != 0 || todos_count != 0)
        {
            if (reminders_count != 0) // if reminders exist on the selected day
            {
                var reminder_card = new Frame
                {
                    Title = $"{reminders_count}" + (reminders_count > 1 ? " Reminders exist" : " Reminder exist"),
                    TitleFontSize = 14f,
                    TitleFontStyle = FontStyle.Regular,
                    IconSize = new Size(24,24),
                    AllowIconUpscale = false,
                    Icon = IconLibrary.GetBitmap(AppIcon.Late,24,MainForm.PrimaryLight),

                    AutoSize = false,
                    Width = Math.Max(0, _panel.ClientSize.Width - 8),
                    Height = 50,
                    Margin = new Padding(4),
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryGrey,
                    PressedColor = MainForm.PrimaryAsh,
                    HoverDelayMs = 100,
                    Cursor = Cursors.Hand
                };

                _panel.Controls.Add(reminder_card);
                _panel.SizeChanged += (s, e) => {reminder_card.Width = _panel.ClientSize.Width - 8;};

                reminder_card.Click += (s, e) =>
                {
                    Reload_Body(_cache_on_N_day_reminders!);
                };
            }
            if (todos_count != 0) // if todos exist on the selected day
            {
                var todo_card = new Frame
                {
                    Title = $"{todos_count} Todo exist",
                    TitleFontSize = 14f,
                    TitleFontStyle = FontStyle.Regular,
                    IconSize = new Size(24,24),
                    AllowIconUpscale = false,
                    Icon = IconLibrary.GetBitmap(AppIcon.Todo,24,MainForm.PrimaryLight),

                    AutoSize = false,
                    Width = Math.Max(0, _panel.ClientSize.Width - 8),
                    Height = 50,
                    Margin = new Padding(4),
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryGrey,
                    PressedColor = MainForm.PrimaryAsh,
                    HoverDelayMs = 100,
                    Cursor = Cursors.Hand
                };

                _panel.Controls.Add(todo_card);
                _panel.SizeChanged += (s, e) => {todo_card.Width = _panel.ClientSize.Width - 8;};

                todo_card.Click += (s, e) =>
                {
                    Reload_Body(_cache_on_N_day_todos!);
                };   
            }
        }
        else // if no saved reminder or todo is found
        {
            var fallback = new MaterialLabel
            {
                Text = $"No Reminder or Todo is saved\nOn {calendar.SelectedDate.Date:MM/dd/yyyy}",
                FontType = MaterialSkin.MaterialSkinManager.fontType.H6,
                AutoSize = false,
                Width = (int)Math.Round(2.7 * _panel.ClientSize.Width - 8),
                Height = 0,
                Margin = new Padding(0, 4, 3, 4),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _panel.Controls.Add(fallback);
            _panel.SizeChanged += (s, e) => {fallback.Width = _panel.ClientSize.Width - 8 - 5; fallback.Height = _panel.ClientSize.Height;};
        }

        return _panel;
    }
}