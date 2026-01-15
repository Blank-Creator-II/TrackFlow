using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using TrackFlow.Utils;
using TrackFlow.Service;
using TrackFlow.Models;
using System.Net;

namespace TrackFlow.Forms;
public partial class ExpensesPage : UserControl
{
    // made this a field so Reload() can repopulate it
    private FlowLayoutPanel? historyFlow;

    // search debounce timer and cache
    private readonly System.Windows.Forms.Timer _searchDebounceTimer;
    private const int SearchDebounceMs = 300;
    private List<Expense> _cachedExpenses = new List<Expense>();

    // keep a reference to the search control so timer handler can read it
    private MaterialTextBox2? _searchBar;

    public ExpensesPage()
    {
        this.Dock = DockStyle.Fill;

        // create debounce timer
        _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = SearchDebounceMs };
        _searchDebounceTimer.Tick += (s, e) =>
        {
            _searchDebounceTimer.Stop();
            PerformSearchNow();
        };

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        // Main layout:
        var layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(8)
        };
        // Columns:
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        // Rows:
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f)); // search bar
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // body
        this.Controls.Add(layout);

        // Panels:
        var topBarPanel = new Panel { Dock = DockStyle.Fill };

        var historyPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };

        // historyFlow is now a field
        historyFlow = new FlowLayoutPanel
        {
            Location = new Point(0, 0),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // enable double-buffering to reduce flicker (non-public property)
        try
        {
            typeof(FlowLayoutPanel).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(historyFlow, true);
        }
        catch
        {
            // If reflection fails for some reason, continue without crash; it's a best-effort optimization.
        }

        historyPanel.Controls.Add(historyFlow);

        var insightPanel = new Panel { Dock = DockStyle.Fill };

        // Add Panels:
        layout.Controls.Add(topBarPanel, 0, 0);
        layout.SetColumnSpan(topBarPanel, 2);

        layout.Controls.Add(historyPanel, 0, 1);
        layout.Controls.Add(insightPanel, 1, 1);

        // --------- History (initial population via Reload) ---------
        Reload(); // initial populate

        // keep resize behaviour
        historyPanel.Resize += (s, e) =>
        {
            // ensure flow width tracks panel (account for scrollbar)
            if (historyFlow != null)
                historyFlow.Width = historyPanel.ClientSize.Width + 25;
        };

        historyFlow.Resize += (s, e) =>
        {
            foreach (Control c in historyFlow.Controls)
            {
                c.Width = Math.Max(0, historyFlow.ClientSize.Width - historyFlow.Padding.Horizontal);
            }
        };

        // --------- Top Bar ---------
        topBarPanel.Controls.Add(CreateTopBar(topBarPanel));

        // --------- Summery ---------
        var test_data = new Dictionary<string, double>
        {
            ["Food"] = 500.75,
            ["Rent"] = 1000.25,
            ["Utilities"] = 500.5
        };
        insightPanel.Controls.Add(CreateSummery(test_data));
    }

    // Reloads the expenses list UI from the data service.
    // Call this whenever data changes (add / delete / update).
    public void Reload(List<Expense> list_of_expenses = null!)
    {
        // clear existing
        if (historyFlow == null) return;

        // If caller did not provide a list, refresh cache from service
        if (list_of_expenses is null)
        {
            try
            {
                _cachedExpenses = ExpenseService.LoadExpense() ?? new List<Expense>();
            }
            catch (Exception ex)
            {
                // If loading fails, avoid crashing the UI
                MessageBox.Show($"Failed to load expenses: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                _cachedExpenses = new List<Expense>();
            }

            list_of_expenses = _cachedExpenses;
        }

        // Use layout suspension + double-buffering to minimize flicker
        historyFlow.SuspendLayout();
        try
        {
            historyFlow.Controls.Clear();

            foreach (Expense expense in list_of_expenses)
            {
                var card = CreateExpenseCard(historyFlow, expense);

                // attach right-click context menu for deletion
                var ctx = new ContextMenuStrip();
                var deleteItem = new ToolStripMenuItem("Delete");
                deleteItem.Click += (s, e) =>
                {
                    // confirm deletion
                    var res = MessageBox.Show($"Delete expense {expense.Id.PID}? This cannot be undone.", "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        try
                        {
                            bool ok = ExpenseService.DeleteExpense(expense.Id);
                            if (ok)
                            {
                                Reload();
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete expense (service returned failure).","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting expense: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                        }
                    }
                };
                ctx.Items.Add(deleteItem);

                // Attach context menu to card (card is a Frame that owns its paint/click)
                card.ContextMenuStrip = ctx;

                historyFlow.Controls.Add(card);
            }
        }
        finally
        {
            historyFlow.ResumeLayout();
            historyFlow.Refresh();
            historyFlow.Invalidate();
        }
    }

    private TableLayoutPanel CreateTopBar(Panel master)
    {
        var topBarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 3,
            Padding = new Padding(0)
        };

        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topBarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        topBarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));

        // create search bar and keep as field so debounce timer can read it
        _searchBar = new MaterialTextBox2
        {
            Hint = "Search expenses...",
            Dock = DockStyle.Fill,
            UseTallSize = false
        };

        var search_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Search, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        var add_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Add, 24, MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        var divder = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 4, 2, 0),
            BackColor = MainForm.PrimaryDark
        };

        topBarLayout.Controls.Add(_searchBar, 0, 0);
        topBarLayout.Controls.Add(search_btn, 1, 0);
        topBarLayout.Controls.Add(add_btn, 2, 0);
        topBarLayout.Controls.Add(divder, 0, 1);
        topBarLayout.SetColumnSpan(divder, 3);

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

        // Show AddExpense modally and reload if it returns OK
        add_btn.Click += (s, e) =>
        {
            using var addForm = new AddExpense();
            var dr = addForm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload();
            }
        };

        return topBarLayout;
    }

    // perform search using cached expenses and update UI
    private void PerformSearchNow()
    {
        if (_searchBar == null) return;

        string q = _searchBar.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(q))
        {
            // show full cached list
            Reload(_cachedExpenses);
            return;
        }

        List<Expense> results;
        try
        {
            results = ExpenseService.SearchExpenses(_cachedExpenses, q) ?? new List<Expense>();
        }
        catch (Exception ex)
        {
            // fallback to empty result on error
            MessageBox.Show($"Search failed: {ex.Message}","Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            results = new List<Expense>();
        }

        Reload(results);
    }

    private Frame CreateExpenseCard(FlowLayoutPanel master, Expense expense)
    {
        var card = new Frame
        {
            Title = $"{expense.Category} ━━━ {expense.Date}",
            Subtitle =  $"\nReceiver: {expense.Receiver}: ━━━ Amount: {expense.Amount} {expense.Currency}\n" +
                        $"Bank: {expense.LinkedBank.Name}: ━━━ Type: {expense.Mode}",
            Icon = IconLibrary.GetBitmap(AppIcon.Expense,24,MainForm.PrimaryLight),
            TitleFontSize = 14f,
            SubtitleFontSize = 11f,
            IconSize = new Size(24,24),
            AllowIconUpscale = false,
            Width = Math.Max(0, master.ClientSize.Width - master.Padding.Horizontal),
            Margin = new Padding(0, 0, 0, 10),
            NormalColor = MainForm.PrimaryMid,
            HoverColor = MainForm.PrimaryGrey,
            PressedColor = MainForm.PrimaryAsh,
            HoverDelayMs = 100,
            Cursor = Cursors.Hand
        };

        // clicking shows more detail
        card.Click += (s, e) => {
            using var viewexp = new ViewExpense(expense);
            var dr = viewexp.ShowDialog();
        };

        return card;
    }

    private TableLayoutPanel CreateSummery(Dictionary<string, double> data)
    {
        var summeryPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 2
        };

        summeryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 6f));
        summeryPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        summeryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        summeryPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));
        summeryPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        var seriesCollection = new SeriesCollection();

        foreach (var item in data)
        {
            seriesCollection.Add(new PieSeries
            {
                Title = item.Key,
                Values = new ChartValues<double> { item.Value },
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{1:P0}", chartPoint.SeriesView.Title, chartPoint.Participation)
            });
        }

        var chart = new LiveCharts.WinForms.PieChart
        {
            Dock = DockStyle.Fill,
            Series = seriesCollection,
            LegendLocation = LegendLocation.Right,
            InnerRadius = 80,
            BackColor = System.Drawing.Color.Transparent
        };

        chart.DefaultLegend.Foreground = System.Windows.Media.Brushes.White;

        var Hdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 2, 2, 2),
            BackColor = MainForm.PrimaryDark
        };

        var Vdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 2, 2, 2),
            BackColor = MainForm.PrimaryDark
        };

        var recomendationPanel = new Panel { Dock = DockStyle.Fill };

        summeryPanel.Controls.Add(Vdivider, 0, 0);
        summeryPanel.SetRowSpan(Vdivider, 3);
        summeryPanel.Controls.Add(chart, 1, 0);
        summeryPanel.Controls.Add(Hdivider, 1, 1);
        summeryPanel.Controls.Add(recomendationPanel, 1, 2);

        return summeryPanel;
    }
}

