using System;
using System.Drawing;
using System.Windows.Forms;
using TrackFlow.Models;
using TrackFlow.Utils;
using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Service;

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
            master.CreateCacheCollection(); // ensure caches exist

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
    }

    private class ViewTodo : MaterialForm
    {
        private Todo _todo;
        private PlannerPage _master;

        // optional events for outer code
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
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            var panel =  new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            this.Controls.Add(panel);

            var content = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Width = panel.ClientSize.Width + 25,
                Height = panel.ClientSize.Height,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(8,0,8,0),
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
                    Width = content.ClientSize.Width - 25, 
                    Height = 60, 
                    ColumnCount = 4,
                    RowCount = 3, 
                    Padding = new Padding(0,0,8,0)
                };
                content.SizeChanged += (s, e) => {table.Width = content.ClientSize.Width - 25;};
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
                table.Controls.Add(top_divder,0,0);
                table.SetColumnSpan(top_divder,4);

                var left_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(left_divder,0,1);

                var checkbox = new MaterialCheckbox
                {
                    Dock = DockStyle.Fill,
                    Checked = line.State  
                };
                table.Controls.Add(checkbox,1,1);

                int index = i;
                checkbox.CheckedChanged += (s, e) =>
                {
                    bool newState = checkbox.Checked;
                    // updated todo:
                    _todo.Data[index].State = newState;
                    LineToggled?.Invoke(_todo, index, newState);
                };

                var right_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(right_divder,2,1);

                var label = new Frame
                {
                    Margin = new Padding(0),
                    AutoSize = false,
                    Width = table.ClientSize.Width - checkbox.ClientSize.Width,
                    Height = table.ClientSize.Height,
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryMid,
                    PressedColor = MainForm.PrimaryMid,
                };
                if (line.Link is not null)
                {
                    List<Reminder> result = ReminderService.SearchReminders(_master._cacheCollection._cachedReminders!,line.Link.PID);
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
                table.SizeChanged += (s, e) => {label.Width = table.ClientSize.Width - checkbox.ClientSize.Width; label.Height = table.ClientSize.Height;};
                table.Controls.Add(label,3,1);

                var bottom_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(bottom_divder,0,2);
                table.SetColumnSpan(bottom_divder,4);

                content.Controls.Add(table);
            }
        }
    }

    private class ViewNote : MaterialForm
    {
        private Note _note;
        private PlannerPage _master;
        
        public ViewNote(Note note, PlannerPage master)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _master = master;
            _note = note;

            Text = _note.Title;
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            
            var textbox = new MaterialMultiLineTextBox2
            {
                Dock = DockStyle.Fill  
            };
            this.Controls.Add(textbox);

            foreach (string line in note.Data)
            {
                textbox.Text += FileHelper.ToMultiLine(line);
            }   
        }
    }

    private class AddTodo : MaterialForm
    { 
        private PlannerPage _master;
        private TableLayoutPanel main;
        private Panel? panel;
        public AddTodo(PlannerPage master)
        {
            var mgr = MaterialSkinManager.Instance;
            mgr.AddFormToManage(this);

            _master = master;

            Text = "Add Todo";
            Size = new Size(700, 670);
            MinimumSize = new Size(700, 670);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;

            main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                Padding = new Padding(0)
            };
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,56f));
            main.RowStyles.Add(new RowStyle(SizeType.Absolute,12f));
            main.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
            this.Controls.Add(main);

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            List<Todo.SingleLine> lines = new List<Todo.SingleLine>(); 
            var todo_adder = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 56,
                ColumnCount = 3,
                Padding = new Padding(2,2,2,0)
            };
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // todo text
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // add text
            todo_adder.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // optional link reminder

            var todo_text = new MaterialTextBox
            {
                Hint = "Add Todo...",
                Dock = DockStyle.Fill,
                UseTallSize = false
            };
            todo_adder.Controls.Add(todo_text,0,0);

            var add_todo = new MaterialFloatingActionButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Add, 24, MainForm.PrimaryDark),
                Margin = new Padding(8, 0, 0, 0),
                Mini = true
            };
            todo_adder.Controls.Add(add_todo,1,0);

            var link_todo = new MaterialFloatingActionButton
            {
                Icon = IconLibrary.GetBitmap(AppIcon.Link, 24, MainForm.PrimaryDark),
                Margin = new Padding(8, 0, 0, 0),
                Mini = true
            };
            todo_adder.Controls.Add(link_todo,2,0);

            add_todo.Click += (s, e) =>
            {
                string _text = todo_text.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(_text))
                {
                    MessageBox.Show("Todo text can not be empty","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    todo_text.Focus();
                }
                else
                {
                    todo_text.ResetText();
                    lines.Add(new Todo.SingleLine
                    {
                        Data = _text
                    });

                    // build todo
                    var built_todo = new Todo
                    {
                        Id = IDGenerator.GenID("Todo"),
                        Date = DateTime.Today,
                        Data = lines
                    };

                    BuildPreview(built_todo);
                }
            };

            main.Controls.Add(todo_adder,0,0);
    
            var header_divder = new MaterialDivider
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(2,0,2,2),
                BackColor = MainForm.PrimaryDark
            };
            main.Controls.Add(header_divder,0,1);

            panel =  new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            main.Controls.Add(panel,0,2);
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
                Padding = new Padding(8,0,8,0),
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
                    Width = content.ClientSize.Width - 25, 
                    Height = 60, 
                    ColumnCount = 4,
                    RowCount = 3, 
                    Padding = new Padding(0,0,8,0)
                };
                content.SizeChanged += (s, e) => {table.Width = content.ClientSize.Width - 25;};
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
                table.Controls.Add(top_divder,0,0);
                table.SetColumnSpan(top_divder,4);

                var left_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(left_divder,0,1);

                var checkbox = new MaterialCheckbox
                {
                    Dock = DockStyle.Fill,
                    Checked = line.State  
                };
                table.Controls.Add(checkbox,1,1);

                int index = i;
                checkbox.CheckedChanged += (s, e) =>
                {
                    bool newState = checkbox.Checked;
                    // updated todo:
                    _todo.Data[index].State = newState;
                };

                var right_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(right_divder,2,1);

                var label = new Frame
                {
                    Margin = new Padding(0),
                    AutoSize = false,
                    Width = table.ClientSize.Width - checkbox.ClientSize.Width,
                    Height = table.ClientSize.Height,
                    NormalColor = MainForm.PrimaryMid,
                    HoverColor = MainForm.PrimaryMid,
                    PressedColor = MainForm.PrimaryMid,
                };
                if (line.Link is not null)
                {
                    List<Reminder> result = ReminderService.SearchReminders(_master._cacheCollection._cachedReminders!,line.Link.PID);
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
                table.SizeChanged += (s, e) => {label.Width = table.ClientSize.Width - checkbox.ClientSize.Width; label.Height = table.ClientSize.Height;};
                table.Controls.Add(label,3,1);

                var bottom_divder = new MaterialDivider
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    BackColor = MainForm.PrimaryDark
                };
                table.Controls.Add(bottom_divder,0,2);
                table.SetColumnSpan(bottom_divder,4);

                content.Controls.Add(table);
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