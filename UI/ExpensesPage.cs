using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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

    // a reference to the recomendation panel
    private Panel? recomendationPanel;

    // search debounce timer and cache
    private readonly System.Windows.Forms.Timer _searchDebounceTimer;
    private const int SearchDebounceMs = 300;
    private List<Expense> _cachedExpenses = new List<Expense>();

    // keep a reference to the search control so timer handler can read it
    private MaterialTextBox2? _searchBar;

    // summary chart (so we can update it later)
    private LiveCharts.WinForms.PieChart? _summaryChart;

    // category colors (hex). Add/adjust categories here.
    private readonly Dictionary<string, string> _categoryColors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Entertainment"] = "#9B27FF",
        ["Grocery"]       = "#2ECC71",
        ["Medicine"]      = "#FF3B30",
        ["Other"]         = "#7F8C8D",
        ["Shopping"]      = "#FF2D95",
        ["Travel"]        = "#00B3FF",
        ["Utilities"]     = "#FFB000"
    };

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
            // best-effort
        }

        historyPanel.Controls.Add(historyFlow);

        var insightPanel = new Panel { Dock = DockStyle.Fill };

        // Add Panels:
        layout.Controls.Add(topBarPanel, 0, 0);
        layout.SetColumnSpan(topBarPanel, 2);

        layout.Controls.Add(historyPanel, 0, 1);
        layout.Controls.Add(insightPanel, 1, 1);

        // --------- History (initial population via Reload) ---------
        Reload(true); // initial populate

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
        // Create the summary panel and keep the chart instance (so we can UpdateSummary later)
        var summaryPanel = CreateSummery(BuildSummaryFromExpenses(ExpenseService.LoadExpense()));
        insightPanel.Controls.Add(summaryPanel);
    }

    // Reloads the expenses list UI from the data service.
    // Call this whenever data changes (add / delete / update).
    public void Reload( bool true_reload, bool searching = false, List<Expense> list_of_expenses = null!)
    {
        // clear existing
        if (historyFlow == null) return;

        // If caller did not provide a list or was asked to truly reload, refresh cache from service
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
                                Reload(true);
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

        // Update the summary chart from the cached data (aggregate by category)
        try
        {
            if (true_reload && !searching)
            {
            var summary = BuildSummaryFromExpenses(_cachedExpenses);
            UpdateSummary(summary);

            // create recomendations based on the expense data:
            recomendationPanel!.Controls.Clear();
            var recs = BuildRecommendationsPanel(summary, maxRecommendations: 4);
            recomendationPanel.Controls.Add(recs);
            }
        }
        catch
        {
            // silent fail if summary update breaks; so it doesn't block the UI
        }
    }

    private TableLayoutPanel CreateTopBar(Panel master)
    {
        var topBarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 4,
            Padding = new Padding(0)
        };

        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
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

        var reload_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Reload, 24, MainForm.PrimaryDark),
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
        topBarLayout.Controls.Add(reload_btn, 2, 0);
        topBarLayout.Controls.Add(add_btn, 3, 0);
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

        // reload the UI from the expense service
        reload_btn.Click += (s, e) =>
        {
            Reload(true);
        };

        // Show AddExpense modally and reload if it returns OK
        add_btn.Click += (s, e) =>
        {
            using var addForm = new AddExpense();
            var dr = addForm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                Reload(true);
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
            Reload(false,true,_cachedExpenses);
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

        Reload(true,true,results);
    }

    private Frame CreateExpenseCard(FlowLayoutPanel master, Expense expense)
    {
        var card = new Frame
        {
            Title = $"{expense.Category} ━━━ {expense.Date}",
            Subtitle =  $"\nReceiver: {expense.Receiver}: ━━━ Amount: {expense.Amount} {expense.Currency}\n" +
                        $"Bank: {expense.LinkedBank.Name}: ━━━ Type: {expense.Mode}",
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

        // set icon with the correct color
        if (expense.Category == "Entertainment"){card.Icon = IconLibrary.GetBitmap(AppIcon.Entertainment,24,ColorTranslator.FromHtml(_categoryColors["Entertainment"]));}
        else if (expense.Category == "Grocery"){card.Icon = IconLibrary.GetBitmap(AppIcon.Grocery,24,ColorTranslator.FromHtml(_categoryColors["Grocery"]));}
        else if (expense.Category == "Medicine"){card.Icon = IconLibrary.GetBitmap(AppIcon.Medicine,24,ColorTranslator.FromHtml(_categoryColors["Medicine"]));}
        else if (expense.Category == "Other"){card.Icon = IconLibrary.GetBitmap(AppIcon.Other,24,ColorTranslator.FromHtml(_categoryColors["Other"]));}
        else if (expense.Category == "Shopping"){card.Icon = IconLibrary.GetBitmap(AppIcon.Shopping,24,ColorTranslator.FromHtml(_categoryColors["Shopping"]));}
        else if (expense.Category == "Travel"){card.Icon = IconLibrary.GetBitmap(AppIcon.Travel,24,ColorTranslator.FromHtml(_categoryColors["Travel"]));}
        else if (expense.Category == "Utilities"){card.Icon = IconLibrary.GetBitmap(AppIcon.Utilities,24,ColorTranslator.FromHtml(_categoryColors["Utilities"]));}

        // clicking shows more detail
        card.Click += (s, e) => {
            using var viewexp = new ViewExpense(expense);
            var dr = viewexp.ShowDialog();
        };

        return card;
    }

    /// Create the summary panel (pie chart). The created PieChart is stored in _summaryChart so UpdateSummary(...) can modify it later.
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

        // Build initial series collection with colors
        var seriesCollection = BuildSeriesCollection(data);

        // Create the chart and store it so we can update later
        _summaryChart = new LiveCharts.WinForms.PieChart
        {
            Dock = DockStyle.Fill,
            Series = seriesCollection,
            LegendLocation = LegendLocation.Right,
            InnerRadius = 80,
            BackColor = System.Drawing.Color.Transparent
        };

        _summaryChart.DefaultLegend.Foreground = System.Windows.Media.Brushes.White;

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

        recomendationPanel = new Panel { Dock = DockStyle.Fill };

        summeryPanel.Controls.Add(Vdivider, 0, 0);
        summeryPanel.SetRowSpan(Vdivider, 3);
        summeryPanel.Controls.Add(_summaryChart, 1, 0);
        summeryPanel.Controls.Add(Hdivider, 1, 1);
        summeryPanel.Controls.Add(recomendationPanel, 1, 2);

        // Inital Recomendation
        recomendationPanel.Controls.Add(BuildRecommendationsPanel(BuildSummaryFromExpenses(ExpenseService.LoadExpense()),4));

        return summeryPanel;
    }

    // this will build a SeriesCollection from a data dictionary and apply per-category colors.
    private SeriesCollection BuildSeriesCollection(Dictionary<string, double> data)
    {
        var sc = new SeriesCollection();

        if (data == null || data.Count == 0)
            return sc;

        double total = data.Values.Sum();

        foreach (var item in data)
        {
            // get color hex or fallback to a default (material accent)
            string hex = _categoryColors.TryGetValue(item.Key, out var h) ? h : "#9E9E9E";
            
            // convert hex to System.Windows.Media.Color safely
            System.Windows.Media.Color mediaColor;
            try
            {
                // allow short/long forms and keep alpha full
                mediaColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            }
            catch
            {
                mediaColor = System.Windows.Media.Color.FromRgb(158, 158, 158);
            }

            var brush = new System.Windows.Media.SolidColorBrush(mediaColor);
            brush.Freeze(); // freeze for perf (WPF brush)

            double percent = total > 0
                ? (item.Value / total) * 100
                : 0;

            var ps = new PieSeries
            {
                Title = $"{percent:0}% - {item.Key}", // shows percent and then the category like "16% - Entertainment" 
                Values = new ChartValues<double> { item.Value },
                DataLabels = true,
                Fill = brush,
                LabelPoint = chartPoint => $"" //$"{chartPoint.Participation:P0}"
            };

            sc.Add(ps);
        }

        return sc;
    }

    // Update the summary chart's series from new data.
    public void UpdateSummary(Dictionary<string, double> data)
    {
        if (_summaryChart == null) return;

        try
        {
            var newSeries = BuildSeriesCollection(data);
            // Replace whole Series collection — LiveCharts will animate/refresh.
            _summaryChart.Series = newSeries;
            _summaryChart.Refresh();
        }
        catch (Exception ex)
        {
            // don't throw the UI into flames if updating fails
            MessageBox.Show($"Failed to update summary: {ex.Message}");
        }
    }

    // Aggregate the expenses by category to produce the data dictionary used by the pie chart.
    private Dictionary<string, double> BuildSummaryFromExpenses(IEnumerable<Expense> expenses)
    {
        var d = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in expenses)
        {
            var cat = string.IsNullOrWhiteSpace(e.Category) ? "Other" : e.Category;
            if (!d.TryGetValue(cat, out var cur)) cur = 0.0;
            d[cat] = cur + e.Amount;
        }

        Console.WriteLine("==============================");
        int count = 1;
        foreach (string key in d.Keys)
        {
            Console.WriteLine($"[{count}]");
            Console.WriteLine($"{key}: {d[key]}");
            Console.WriteLine("--------------------------");
            count++;
        }
        Console.WriteLine("==============================");

        return d;
    }
}
