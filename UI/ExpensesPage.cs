using System;
using System.Windows.Media;
using MaterialSkin;
using MaterialSkin.Controls;
using LiveCharts;
using LiveCharts.Wpf; // Note: the pie Charts uses WPF-based rendering for WinForms I know another UI for another UI
using LiveCharts.WinForms;
using TrackFlow.Utils;
using TrackFlow.Service;
using TrackFlow.Models;

namespace TrackFlow.Forms;
public class ExpensesPage : UserControl
{
    public ExpensesPage()
    {
        this.Dock = DockStyle.Fill;

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
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50f) // 60% of space
        );
        layout.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 50f) // 40% of space
        );
        // Rows:
        layout.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 56f) // search bar height of 56px
        );
        layout.RowStyles.Add(
            new RowStyle(SizeType.Percent, 100f) // 100% left of space
        );
        this.Controls.Add(layout); // intalize the main layout

        // Panels:
        // search + add panel:
        var topBarPanel = new Panel 
        {
            Dock = DockStyle.Fill
        };

        // history panel:
        var historyPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };
        var historyFlow = new FlowLayoutPanel
        {
            // Do NOT Dock.Fill, we need to control the width.
            Location = new Point(0, 0),
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8),
            // Make it ~25px wider than the container to hide the scrollbar
            // but keep it tall enough to fill the container
            Width = historyPanel.Width + 25, 
            Height = historyPanel.Height,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        historyPanel.Controls.Add(historyFlow); // add the flow panel into main history panel

        // summery panel
        var insightPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        // Add Panels:
        layout.Controls.Add(topBarPanel, 0, 0); // search + add
        layout.SetColumnSpan(topBarPanel, 2);

        layout.Controls.Add(historyPanel, 0, 1); // history of expense

        layout.Controls.Add(insightPanel, 1, 1); // summery and recomendations

        // now we start the heavy lifting:
        // --------- History ---------
        for (int i = 0; i <= 9; i++)
        {
            historyFlow.Controls.Add(CreateExpenseCard(historyFlow)); // a test spawns 10 cards
        }

        historyPanel.Resize += (s, e) => {
            historyFlow.Width = historyPanel.Width + 25;
        };

        historyFlow.Resize += (s, e) =>
        {
            foreach (Control c in historyFlow.Controls)
            {
                c.Width = historyFlow.ClientSize.Width - historyFlow.Padding.Horizontal;
            }
        };
        
        // --------- Top Bar ---------
        topBarPanel.Controls.Add(CreateTopBar(topBarPanel));

        // --------- Summery ---------
        var test_data = new Dictionary<string, double>();
        test_data["Food"] = 500.75;
        test_data["Rent"] = 1000.25;
        test_data["Utilities"] = 500.5;
        insightPanel.Controls.Add(CreateSummery(test_data));
    }

    private TableLayoutPanel CreateTopBar(Panel master)
    {
        var topBarLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2, // 1 for the bar, 2 for divder
            ColumnCount = 3, // 1 for Search, 2 for Buttons
            Padding = new Padding(0)
        };

        // Column 0: Search Bar (Stretches)
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        // Column 1 & 2: Buttons (Fixed size)
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topBarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        // Row 0: Main Bar (search + buttons)
        topBarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        // Row 1: Divder
        topBarLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 6f));

        // 1. Search Bar
        var search_bar = new MaterialTextBox2
        {
            Hint = "Search expenses...",
            Dock = DockStyle.Fill,
            UseTallSize = false // Makes it 36px tall instead of 50px
        };

        // 2. Search Button
        var search_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Search,24,MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0), // stick to the the left of search bar
            Mini = true
        };

        // 3. Add Button
        var add_btn = new MaterialFloatingActionButton
        {
            Icon = IconLibrary.GetBitmap(AppIcon.Add,24,MainForm.PrimaryDark),
            Margin = new Padding(8, 0, 0, 0),
            Mini = true
        };

        // 4. Divder
        var divder = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2,4,2,0),
            BackColor = MainForm.PrimaryDark
        };

        // Add to layout
        topBarLayout.Controls.Add(search_bar, 0, 0);
        topBarLayout.Controls.Add(search_btn, 1, 0);
        topBarLayout.Controls.Add(add_btn, 2, 0);
        topBarLayout.Controls.Add(divder,0,1);
        topBarLayout.SetColumnSpan(divder,3);

        return topBarLayout;
    }

    private Frame CreateExpenseCard(FlowLayoutPanel master)
    {
        var card = new Frame
        {
            Width = master.ClientSize.Width - master.Padding.Horizontal,
            Margin = new Padding(0,0,0,10),
            NormalColor = MainForm.PrimaryMid,
            HoverColor = MainForm.PrimaryGrey,
            PressedColor = MainForm.PrimaryAsh,
            HoverDelayMs = 100
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
                // Shows: Category Name (Percentage%)
                LabelPoint = chartPoint => 
                    string.Format("{1:P0}", chartPoint.SeriesView.Title, chartPoint.Participation)
            });
        }

        var chart = new LiveCharts.WinForms.PieChart
        {
            Dock = DockStyle.Fill,
            Series = seriesCollection,
            LegendLocation = LegendLocation.Right,
            // The magic for Doughnut charts:
            InnerRadius = 80, // Set this higher for a thinner ring
            BackColor = System.Drawing.Color.Transparent 
            
        };

        chart.DefaultLegend.Foreground = System.Windows.Media.Brushes.White;

        var Hdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2,2,2,2),
            BackColor = MainForm.PrimaryDark
        };

        var Vdivider = new MaterialDivider
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2,2,2,2),
            BackColor = MainForm.PrimaryDark
        };

        var recomendationPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        summeryPanel.Controls.Add(Vdivider,0,0);
        summeryPanel.SetRowSpan(Vdivider,3);
        summeryPanel.Controls.Add(chart,1,0);
        summeryPanel.Controls.Add(Hdivider,1,1);
        summeryPanel.Controls.Add(recomendationPanel,1,2);

        return summeryPanel;
    }
}