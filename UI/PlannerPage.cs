using System;
using System.Globalization;
using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Service;
using TrackFlow.Models;
using TrackFlow.Controls;

namespace TrackFlow.Forms;
public class PlannerPage : UserControl
{
    //private TableLayoutPanel? layout;
    public PlannerPage()
    {
        this.Dock = DockStyle.Fill;

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        /*
        layout = new TableLayoutPanel()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(8)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/3)); // reminder 
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/3)); // notes
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100/3)); // todo list

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f)); // search bar
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // body
        this.Controls.Add(layout);

        // main content
        var topBarPanel = new Panel { Dock = DockStyle.Fill };
        */

        // just a test to see how the custom calendar looks:
        var calendar = new MaterialCalendar();
        
        this.Controls.Add(calendar);

        calendar.DaySelected += date =>
        {
            MessageBox.Show(date.ToString());
        };
    }
}
