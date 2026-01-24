using System;
using System.Globalization;
using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Service;
using TrackFlow.Models;
using TrackFlow.Utils;
using System.Collections;
using System.Reflection;

namespace TrackFlow.Forms;
public partial class PlannerPage : UserControl
{
    private TableLayoutPanel? layout;
    private TableLayoutPanel? sidebody;
    private TableLayoutPanel? mainbody;
    private MaterialCalendar calendar;
    private Panel? dayList;
    private MaterialTextBox? _searchBar;
    private MaterialFloatingActionButton? switch_btn;
    private string? current_body_view;
    private const int SearchDebounceMs = 300;
    private class _cache
    {
       public List<Note>? _cachedNotes;
       public List<Reminder>? _cachedReminders;
       public List<Todo>? _cachedTodos;
    }
    private _cache _cacheCollection = new _cache
    {
        _cachedNotes = new List<Note>(),
        _cachedReminders = new List<Reminder>(),
        _cachedTodos = new List<Todo>()
    };
    private List<Reminder>? _cache_on_N_day_reminders; // I know it's long but it's better cuz I will forget
    private List<Todo>? _cache_on_N_day_todos; // the same thing
    private readonly System.Windows.Forms.Timer _searchDebounceTimer;
    private FlowLayoutPanel? holderFlow;

    public PlannerPage()
    {
        this.Dock = DockStyle.Fill;

        _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = SearchDebounceMs };
        _searchDebounceTimer.Tick += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            PerformSearchNow();
        };

        CreateCacheCollection();

        calendar = new MaterialCalendar(this);

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(8)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f)); // sidebody area => calendar + top reminders
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6f)); // vertical divider  
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f)); // mainbody area => search + (notes / reminders / todo)

        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        this.Controls.Add(layout);

        // sidebody area
        sidebody = new TableLayoutPanel
        { 
            Dock = DockStyle.Fill, 
            ColumnCount = 1,
            RowCount = 3,
        };
        sidebody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        sidebody.RowStyles.Add(new RowStyle(SizeType.Percent, 60f)); // calendar
        sidebody.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f)); // horizontal divider
        sidebody.RowStyles.Add(new RowStyle(SizeType.Percent, 40f)); // day specific todo or reminder
        layout.Controls.Add(sidebody,0,0);

        // vertical divider
        var Vdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 2, 2, 2),
            BackColor = MainForm.PrimaryDark
        };
        layout.Controls.Add(Vdivider,1,0);

        // mainbody area
        mainbody = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
        };
        mainbody.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        mainbody.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f)); // search area
        mainbody.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f)); // horizontal divider
        mainbody.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // notes / reminders / todo
        layout.Controls.Add(mainbody,2,0);

        InitializeSideBody();
        InitializeMainBody();
    }

    private void InitializeSideBody()
    {
        // attach calendar
        sidebody!.Controls.Add(calendar,0,0);
        calendar.DaySelected += date =>
        {
            dayList!.Controls.Clear();
            dayList.Controls.Add(BuildDayList());
        };

        // horizontal divider
        var Hdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 2, 2, 2),
            BackColor = MainForm.PrimaryDark
        };
        sidebody.Controls.Add(Hdivider,0,1);

        // day specific todo or reminder
        dayList = new Panel{ Dock = DockStyle.Fill};
        dayList.Controls.Add(BuildDayList());
        sidebody.Controls.Add(dayList,0,2);
    }

    private void InitializeMainBody()
    {
        // search area
        var searchArea = new Panel { Dock = DockStyle.Fill };
        mainbody!.Controls.Add(searchArea,0,0);
        searchArea.Controls.Add(CreateTopBar(searchArea));

        // horizontal divider
        var Hdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 2, 2, 2),
            BackColor = MainForm.PrimaryDark
        };
        mainbody.Controls.Add(Hdivider,0,1);

        // notes / reminders / todo
        var holderPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };

        // true holder of either notes or reminders or todo
        holderFlow = new FlowLayoutPanel
        {
            Location = new Point(0, 0),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        try
        {
            typeof(FlowLayoutPanel).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(holderFlow, true);
        }
        catch
        {
            // idk throw it or smth
        }
        holderPanel.Controls.Add(holderFlow);
        mainbody.Controls.Add(holderPanel,0,2);

        holderPanel.Resize += (s, e) =>
        {
            if (holderFlow != null)
                holderFlow.Width = holderPanel.ClientSize.Width + 25;
        };

        holderFlow.Resize += (s, e) =>
        {
            foreach (Control c in holderFlow.Controls)
            {
                c.Width = Math.Max(0, holderFlow.ClientSize.Width - 25);
            }
        };

        // Initial Population:
        Reload_Body(_cacheCollection._cachedNotes!);
    }

    private void Reload(string type)
    {
        CreateCacheOnNDay(calendar.SelectedDate,true);

        if (type == "Note") {Reload_Body(_cacheCollection._cachedNotes!);}
        else if (type == "Reminder") {Reload_Body(_cache_on_N_day_reminders!);}
        else if (type == "Todo") {Reload_Body(_cache_on_N_day_todos!);}
        else {Reload(current_body_view!);}
    
        calendar.RefreshUI(calendar.SelectedDate);
        dayList!.Controls.Clear();
        dayList.Controls.Add(BuildDayList());
    }

    private void SetFallback()
    {
        // a fallback message when there is no saved data
        holderFlow!.Controls.Clear();
        var messgae = new MaterialLabel
        {
            Text = "No data have been saved\nTry to add a note, todo or reminder",
            FontType = MaterialSkinManager.fontType.H5,
            AutoSize = false,
            Width = holderFlow!.ClientSize.Width - 26,
            Height = holderFlow!.ClientSize.Height,
            Margin = new Padding(4),
            TextAlign = ContentAlignment.MiddleCenter
        };

        holderFlow.Controls.Add(messgae);

        holderFlow.SizeChanged += (s, e) => {messgae.Width = holderFlow.ClientSize.Width - 25; messgae.Height = holderFlow.ClientSize.Height;};
    }

    private void Reload_Body<T>(List<T> list)
    {
        if (holderFlow == null) return;

        holderFlow.SuspendLayout();
        try
        {
            if (list.Count == 0)
            {
                SetFallback();
                return;
            }

            holderFlow.Controls.Clear();

            if (typeof(T) == typeof(Note))
            {
                current_body_view = "Note";
                switch_btn!.Enabled = false;

                foreach (var note in list.Cast<Note>())
                {
                    var card = CreateNote(holderFlow,note);

                    // attach right-click context menu for deletion
                    var ctx = new ContextMenuStrip();
                    var deleteItem = new ToolStripMenuItem("Delete");
                    deleteItem.Image = IconLibrary.GetBitmap(AppIcon.Delete,20,MainForm.PrimaryLight);
                    deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black; // text color
                    deleteItem.BackColor = MainForm.PrimaryMid; // bg color
                    deleteItem.Paint += (s, e) => 
                    {
                        deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                        deleteItem.BackColor = MainForm.PrimaryMid;
                    };
                    deleteItem.Click += (s, e) =>
                    {
                        // confirm deletion
                        var res = MessageBox.Show($"Delete Note {note.Id.PID}? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                bool ok = NoteService.DeleteNote(note.Id);
                                if (ok)
                                {
                                    Reload("Note");
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete Note (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting Note: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            }
                        }
                    };
                    ctx.Items.Add(deleteItem);

                    // Attach context menu to card (card is a Frame that owns its paint/click)
                    card.ContextMenuStrip = ctx;

                    holderFlow.Controls.Add(card);
                }
            }
            else if (typeof(T) == typeof(Reminder))
            {
                current_body_view = "Reminder";
                switch_btn!.Enabled = true;
                foreach (var reminder in list.Cast<Reminder>())
                {
                    var card = CreateReminder(holderFlow,reminder);

                    // attach right-click context menu for deletion
                    var ctx = new ContextMenuStrip();
                    var toggleItem = new ToolStripMenuItem(reminder.State ? "Disable" : "Enable");
                    toggleItem.Image = IconLibrary.GetBitmap(reminder.State ? AppIcon.Disable : AppIcon.Enable,20,MainForm.PrimaryLight);
                    toggleItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black; // text color
                    toggleItem.BackColor = MainForm.PrimaryMid; // bg color
                    toggleItem.Paint += (s, e) => 
                    {
                        toggleItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                        toggleItem.BackColor = MainForm.PrimaryMid;
                    };
                    toggleItem.Click += (s, e) =>
                    {
                        reminder.State = !reminder.State;
                        bool ok = ReminderService.DeleteReminder(reminder.Id);
                        if (ok)
                        {
                            retry:
                            (bool b, ID i) = ReminderService.SaveReminder(reminder);
                            if (!b)
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
                        Reload_Body(list);
                    };

                    var deleteItem = new ToolStripMenuItem("Delete");
                    deleteItem.Image = IconLibrary.GetBitmap(AppIcon.Delete,20,MainForm.PrimaryLight);
                    deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black; // text color
                    deleteItem.BackColor = MainForm.PrimaryMid; // bg color
                    deleteItem.Paint += (s, e) => 
                    {
                        deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                        deleteItem.BackColor = MainForm.PrimaryMid;
                    };
                    deleteItem.Click += (s, e) =>
                    {
                        // confirm deletion
                        var res = MessageBox.Show($"Delete Reminder {reminder.Id.PID}? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                bool ok = ReminderService.DeleteReminder(reminder.Id);
                                if (ok)
                                {
                                    Reload("Reminder");
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete Reminder (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting Reminder: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            }
                        }
                    };
                    ctx.Items.Add(deleteItem);
                    ctx.Items.Add(toggleItem);

                    // Attach context menu to card (card is a Frame that owns its paint/click)
                    card.ContextMenuStrip = ctx;

                    holderFlow.Controls.Add(card);
                }
            }
            else if (typeof(T) == typeof(Todo))
            {
                current_body_view = "Todo";
                switch_btn!.Enabled = true;
                foreach (var todo in list.Cast<Todo>())
                {
                    var card = CreateTodo(holderFlow,todo);

                    // attach right-click context menu for deletion
                    var ctx = new ContextMenuStrip();
                    var deleteItem = new ToolStripMenuItem("Delete");
                    deleteItem.Image = IconLibrary.GetBitmap(AppIcon.Delete,20,MainForm.PrimaryLight);
                    deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black; // text color
                    deleteItem.BackColor = MainForm.PrimaryMid; // bg color
                    deleteItem.Paint += (s, e) => 
                    {
                        deleteItem.ForeColor = MainForm.PrimaryWhiteShade ? Color.White : Color.Black;
                        deleteItem.BackColor = MainForm.PrimaryMid;
                    };
                    deleteItem.Click += (s, e) =>
                    {
                        // confirm deletion
                        var res = MessageBox.Show($"Delete Todo {todo.Id.PID}? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (res == DialogResult.Yes)
                        {
                            try
                            {
                                bool ok = TodoService.DeleteTodo(todo.Id);
                                if (ok)
                                {
                                    Reload("Todo");
                                }
                                else
                                {
                                    MessageBox.Show("Failed to delete Todo (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error deleting Todo: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            }
                        }
                    };
                    ctx.Items.Add(deleteItem);

                    // Attach context menu to card (card is a Frame that owns its paint/click)
                    card.ContextMenuStrip = ctx;

                    holderFlow.Controls.Add(card);
                }
            }
        }
        finally
        {
            holderFlow.ResumeLayout();
            holderFlow.Invalidate();
        }
    }
    
    private TableLayoutPanel CreateTopBar(Panel master)
    {
        var topBarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 1,
            ColumnCount = 6,
            Padding = new Padding(0)
        };

        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f)); // search bar 0
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // search button 1
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // switch to notes button 2
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // add note 3 
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // add reminder 4 
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize)); // add todo 5
        topBarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        // create search bar and keep as field so debounce timer can read it
        _searchBar = new MaterialTextBox
        {
            Hint = "Search Notes...",
            Dock = DockStyle.Fill,
            UseTallSize = false
        };

        var search_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Search, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        switch_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.SwitchNote, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        var add_note_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.AddNote, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        var add_reminder_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.AddReminder, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        var add_todo_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.AddTodo, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        topBarLayout.Controls.Add(_searchBar, 0, 0);
        topBarLayout.Controls.Add(search_btn, 1, 0);
        topBarLayout.Controls.Add(switch_btn, 2, 0);
        topBarLayout.Controls.Add(add_note_btn, 3, 0);
        topBarLayout.Controls.Add(add_reminder_btn, 4, 0);
        topBarLayout.Controls.Add(add_todo_btn, 5, 0);

        // Event hooking:
        // Debounced typing: restart timer on text change, run search only after idle period
        _searchBar.TextChanged += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        };

        // Enter triggers immediate search
        _searchBar.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                _searchDebounceTimer.Stop();
                PerformSearchNow();
            }
        };

        // search button immediate
        search_btn.Click += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            PerformSearchNow();
        };

        switch_btn.Click += (s, e) =>
        {
            CreateCacheCollection(true);
            Reload_Body(_cacheCollection._cachedNotes!);
            _searchBar.ResetText();
        };

        add_note_btn.Click += (s, e) =>
        {
            using var addnote = new AddNote();
            var dr = addnote.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Note");
            }
        };

        add_reminder_btn.Click += (s, e) =>
        {
            using var addreminder = new AddReminder();
            var dr = addreminder.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Reminder_");
            }
        };

        add_todo_btn.Click += (s, e) =>
        {
            using var addtodo = new AddTodo(this);
            var dr = addtodo.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Todo");
            }
        };

        return topBarLayout;
    }

    private void PerformSearchNow()
    {
        CreateCacheOnNDay(calendar.SelectedDate,true);
        if (_searchBar == null) return;

        string q = _searchBar.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(q))
        {
            if (current_body_view == "Note"){Reload_Body(_cacheCollection._cachedNotes!);}
            else if (current_body_view == "Reminder"){Reload_Body(_cache_on_N_day_reminders!);}
            else if (current_body_view == "Todo"){Reload_Body(_cache_on_N_day_todos!);}
            return;
        }

        List<Note> note_results = new List<Note>();
        List<Reminder> reminder_results = new List<Reminder>();
        List<Todo> todo_results = new List<Todo>();
        try
        {
            if (current_body_view == "Note")
            {
                note_results = NoteService.SearchNotes(_cacheCollection._cachedNotes!, q);

            }
            else if (current_body_view == "Reminder")
            {
                reminder_results = ReminderService.SearchReminders(_cache_on_N_day_reminders!, q);
            }
            else if (current_body_view == "Todo")
            {
                todo_results = TodoService.SearchTodos(_cache_on_N_day_todos!, q);
            }
        }
        catch (Exception ex)
        {
            
            MessageBox.Show($"Search failed: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }
        if ((note_results.Count == 0 && current_body_view == "Note") || 
            (reminder_results.Count == 0 && current_body_view == "Reminder") ||
            (todo_results.Count == 0 && current_body_view == "Todo"))
        {
            holderFlow!.Controls.Clear();
            var messgae = new MaterialLabel
            {
                Text = "No Result For The Searched Value",
                FontType = MaterialSkinManager.fontType.H5,
                AutoSize = false,
                Width = holderFlow!.ClientSize.Width - 26,
                Height = holderFlow!.ClientSize.Height / 2,
                Margin = new Padding(4),
                TextAlign = ContentAlignment.MiddleCenter
            };

            holderFlow.Controls.Add(messgae);

            holderFlow.SizeChanged += (s, e) => {messgae.Width = holderFlow.ClientSize.Width - 25; messgae.Height = holderFlow.ClientSize.Height;};
        }
        else
        {
            if (note_results.Count != 0){Reload_Body(note_results);}
            else if (reminder_results.Count != 0){Reload_Body(reminder_results);}
            else if (todo_results.Count != 0){Reload_Body(todo_results);}
        }
    }

    private Frame CreateNote(FlowLayoutPanel master, Note note)
    {
        var card = new Frame
        {
            Title = note.Title,
            Subtitle = note.Date.ToString(),
            TitleFontSize = 14f,
            SubtitleFontSize = 11f,
            VSubtitleAlignment = StringAlignment.Center,
            IconSize = new Size(24,24),
            Icon = IconLibrary.GetBitmap(AppIcon.Note,24,MainForm.PrimaryLight),
            AllowIconUpscale = false,
            Width = Math.Max(0, master.ClientSize.Width - 25),
            Margin = new Padding(0, 0, 0, 10),
            NormalColor = MainForm.PrimaryMid,
            HoverColor = MainForm.PrimaryGrey,
            PressedColor = MainForm.PrimaryAsh,
            HoverDelayMs = 100,
            Cursor = Cursors.Hand
        };

        card.Click += (s, e) => {
            using var viewnote = new ViewNote(note);
            var dr = viewnote.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Note");
            }
        };

        return card;
    }

    private Frame CreateReminder(FlowLayoutPanel master, Reminder reminder)
    {
        var card = new Frame
        {
            Subtitle = $"{reminder.ReminderDate}" + ((reminder.ReminderNote is not null) ? $"\n{reminder.ReminderNote}" : string.Empty),
            SubtitleFontSize = 14f,
            SubtitleFontStyle = reminder.State ? FontStyle.Regular : FontStyle.Strikeout,
            VSubtitleAlignment = StringAlignment.Center,
            IconSize = new Size(24,24),
            AllowIconUpscale = false,
            Width = Math.Max(0, master.ClientSize.Width - 25),
            Margin = new Padding(0, 0, 0, 10),
            NormalColor = MainForm.PrimaryMid,
            HoverColor = MainForm.PrimaryGrey,
            PressedColor = MainForm.PrimaryAsh,
            HoverDelayMs = 100,
            Cursor = Cursors.Hand
        };

        int comapred_value = DateTime.Compare(reminder.ReminderDate,DateTime.Today);

        if (comapred_value < 0){card.Icon = IconLibrary.GetBitmap(AppIcon.Late,24,Color.Red);} // reminder have passed!
        else if (comapred_value == 0){card.Icon = IconLibrary.GetBitmap(AppIcon.Late,24,Color.Yellow);} // today is the reminder
        else if (comapred_value > 0){card.Icon = IconLibrary.GetBitmap(AppIcon.Late,24,Color.LimeGreen);} // reminder is later date 

        card.Click += (s, e) => {
            using var viewReminder = new ViewReminder(reminder);
            var dr = viewReminder.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Reminder");
            }
        };

        return card;
    }

    private Frame CreateTodo(FlowLayoutPanel master, Todo todo)
    {
        var card = new Frame
        {
            Subtitle = $"{todo.Id.PID}\n\n{todo.Date}",
            VSubtitleAlignment = StringAlignment.Center,
            SubtitleFontSize = 14,
            IconSize = new Size(24,24),
            AllowIconUpscale = false,
            Icon = IconLibrary.GetBitmap(AppIcon.Todo,24,MainForm.PrimaryLight),
            Width = Math.Max(0, master.ClientSize.Width - 25),
            Margin = new Padding(0, 0, 0, 10),
            NormalColor = MainForm.PrimaryMid,
            HoverColor = MainForm.PrimaryGrey,
            PressedColor = MainForm.PrimaryAsh,
            HoverDelayMs = 100,
            Cursor = Cursors.Hand
        };

        card.Click += (s, e) => {
            using var viewtodo = new ViewTodo(todo,this);
            var dr = viewtodo.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload("Todo");
            }
        };

        return card;
    }

    private void CreateCacheCollection(bool force_creation = false)
    {
        if (_cacheCollection._cachedNotes!.Count == 0 || force_creation)
        {
            _cacheCollection._cachedNotes = NoteService.LoadNote();
        }

        if (_cacheCollection._cachedReminders!.Count == 0 || force_creation)
        {
            _cacheCollection._cachedReminders = ReminderService.LoadReminder();
        }

        if (_cacheCollection._cachedTodos!.Count == 0 || force_creation)
        {
            _cacheCollection._cachedTodos = TodoService.LoadTodo();
        } 
    }

    private void CreateCacheOnNDay(DateTime N, bool force_creation = false)
    {
        CreateCacheCollection(force_creation);
    
        if (_cache_on_N_day_reminders == null || force_creation || _cache_on_N_day_reminders.Count == 0)
        {
            _cache_on_N_day_reminders = DateHelper.FilterByDate(
                _cacheCollection._cachedReminders!,
                r => r.ReminderDate,
                N
            );
        }

        if (_cache_on_N_day_todos == null || force_creation || _cache_on_N_day_todos.Count == 0)
        {
            _cache_on_N_day_todos = DateHelper.FilterByDate(
                _cacheCollection._cachedTodos!,
                t => t.Date,
                N
            );
        }
    }
}
