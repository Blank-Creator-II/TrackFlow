using System;
using System.Drawing;
using System.Windows.Forms;
using TrackFlow.Models;
using MaterialSkin.Controls;
using TrackFlow.Forms;

namespace TrackFlow.Controls;

public class MaterialCalendar : UserControl
{
    private TableLayoutPanel? main;
    private Label? lblMonth;
    private TableLayoutPanel? grid;

    private DateTime currentMonth = DateTime.Today;

    public event Action<DateTime>? DaySelected;

    public MaterialCalendar()
    {
        Dock = DockStyle.Fill;
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
        BuildGrid();
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

        var prev = new MaterialButton { Icon = IconLibrary.GetBitmap(AppIcon.ArrowBack,24,MainForm.PrimaryLight), Dock = DockStyle.Fill };
        var next = new MaterialButton { Icon = IconLibrary.GetBitmap(AppIcon.ArrowForward,24,MainForm.PrimaryLight), Dock = DockStyle.Fill };
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
        days.SetColumnSpan(Tdivider,13);

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
                }, i-1, 1);
            }
            else
            {
                days.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
                days.Controls.Add(new Label
                {
                    Text = names[counter],
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                }, i-1, 1);
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
        days.SetColumnSpan(Bdivider,13);

        main!.Controls.Add(days, 0, 1);
    }

    private void BuildGrid()
    {
        grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 6,
            ColumnCount = 7
        };

        for (int i = 0; i < 7; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));
        for (int i = 0; i < 6; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));

        main!.Controls.Add(grid, 0, 2);
    }

    private void RenderMonth(DateTime month)
    {
        grid!.Controls.Clear();
        lblMonth!.Text = month.ToString("MMMM yyyy");

        DateTime first = new(month.Year, month.Month, 1);
        int offset = ((int)first.DayOfWeek + 6) % 7;
        DateTime start = first.AddDays(-offset);

        for (int i = 0; i < 42; i++)
        {
            DateTime day = start.AddDays(i);
            var cell = CreateDayCell(day);
            grid.Controls.Add(cell, i % 7, i / 7);
        }
    }

    private Control CreateDayCell(DateTime date)
    {
        var panel = new Frame
        {
            Title = date.Day.ToString(),
            Dock = DockStyle.Fill,
            Margin = new Padding(1),
            HoverColor = MainForm.PrimaryAsh,
            PressedColor = MainForm.PrimaryLight,
            HoverDelayMs = 100,
            Cursor = Cursors.Hand
        };
        panel.Click += (s, e) => DaySelected?.Invoke(date);

        if (date.Date == DateTime.Today)
        {
            panel.NormalColor = MainForm.PrimaryAccent;
        }
        else if (date.Month != currentMonth.Month)
        {
            panel.NormalColor = MainForm.PrimaryMid;           
        }
        else
        {
            panel.NormalColor = MainForm.PrimaryGrey;
        }

        return panel;
    }

    private void ChangeMonth(int delta)
    {
        currentMonth = currentMonth.AddMonths(delta);
        RenderMonth(currentMonth);
    }
}
