using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.WinForms;
using TrackFlow.Utils;
using TrackFlow.Service;
using TrackFlow.Models;

namespace TrackFlow.Forms
{
    public partial class ExpensesPage : UserControl
    {
        // made this a field so Reload() can repopulate it
        private FlowLayoutPanel? historyFlow;

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
        public void Reload()
        {
            // clear existing
            historyFlow!.Controls.Clear();

            // reload from service
            List<Expense> list_of_expenses = ExpenseService.LoadExpense() ?? new List<Expense>();

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
                            // assume ExpenseService.DeleteExpense exists and returns bool
                            bool ok = ExpenseService.DeleteExpense(expense.Id);
                            if (ok)
                            {
                                Reload();
                            }
                            else
                            {
                                MessageBox.Show("Failed to delete expense (service returned failure).");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting expense: {ex.Message}");
                        }
                    }
                };
                ctx.Items.Add(deleteItem);
                card.ContextMenuStrip = ctx;

                historyFlow.Controls.Add(card);
            }

            // ensure widths fix after population
            historyFlow.Invalidate();
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

            var search_bar = new MaterialTextBox2
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

            topBarLayout.Controls.Add(search_bar, 0, 0);
            topBarLayout.Controls.Add(search_btn, 1, 0);
            topBarLayout.Controls.Add(add_btn, 2, 0);
            topBarLayout.Controls.Add(divder, 0, 1);
            topBarLayout.SetColumnSpan(divder, 3);

            // Event hooking:
            search_bar.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                    SearchForExpense();
            };
            search_btn.Click += (s, e) => SearchForExpense();

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

        private Frame CreateExpenseCard(FlowLayoutPanel master, Expense expense)
        {
            var card = new Frame
            {
                Width = Math.Max(0, master.ClientSize.Width - master.Padding.Horizontal),
                Margin = new Padding(0, 0, 0, 10),
                NormalColor = MainForm.PrimaryMid,
                HoverColor = MainForm.PrimaryGrey,
                PressedColor = MainForm.PrimaryAsh,
                HoverDelayMs = 100
            };

            // left as click to view (same as before)
            card.Click += (s, e) => new ViewExpense(expense).Show();

            // Optionally show a simple summary inside the card (non-invasive).
            // test only!

            var tl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(8) };
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
            tl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            tl.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tl.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            var lblTitle = new Label
            {
                Text = $"{expense.Category ?? "Expense"} — {expense.Receiver}",
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.Transparent
            };
            var lblAmount = new Label
            {
                Text = expense.Amount.ToString("N2"),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.Transparent,
                Font = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold)
            };
            var lblDate = new Label
            {
                Text = expense.Date.ToString("yyyy-MM-dd"),
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.LightGray,
                BackColor = System.Drawing.Color.Transparent,
                Font = new Font(FontFamily.GenericSansSerif, 9f)
            };
            var lblMode = new Label
            {
                Text = expense.Mode,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = System.Drawing.Color.LightGray,
                BackColor = System.Drawing.Color.Transparent,
                Font = new Font(FontFamily.GenericSansSerif, 9f)
            };

            tl.Controls.Add(lblTitle, 0, 0);
            tl.Controls.Add(lblAmount, 1, 0);
            tl.Controls.Add(lblDate, 0, 1);
            tl.Controls.Add(lblMode, 1, 1);

            //card.Controls.Add(tl);
            //card.BringToFront();
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
}
