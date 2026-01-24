using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using TrackFlow.Forms;
using TrackFlow.Utils;

// This is hell to explain so ask some one else ¯\_(ツ)_/¯ 
namespace TrackFlow.Models;
public enum AppIcon
{
    // Sidebar
    Home,
    Expense,
    Planner,
    Tools,
    Theme,

    // Expense
    Entertainment, // #9B27FF
    Grocery, // #2ECC71
    Medicine, // #FF3B30
    Other, // #7F8C8D
    Shopping, // #FF2D95
    Travel, // #00B3FF
    Utilities, // #FFB000

    // Planner
    AddNote,
    AddReminder,
    AddTodo,
    ArrowBack,
    ArrowForward,
    Disable,
    DotSingle,
    DotDouble,
    Enable,
    Late,
    Link,
    Note,
    SwitchNote,
    Todo,

    // Tools
    Calculate,
    CurrencyExchange,
    TimerPause,
    TimerPlay,
    Timer,
    UnitConvert,

    // Misc
    Add,
    Cancel,
    Check,
    Checked,
    Close,
    Copy,
    Export,
    Reload,
    Delete,
    Save,
    Search,
    Star,
    Undo
}

public static class IconLibrary
{
    private static readonly Dictionary<AppIcon, string> IconPaths = new()
    {
        { AppIcon.Home, "Sidebar/home.svg" },
        { AppIcon.Expense, "Sidebar/expense.svg" },
        { AppIcon.Planner, "Sidebar/planner.svg" },
        { AppIcon.Tools, "Sidebar/tools.svg" },
        { AppIcon.Theme, "Sidebar/theme.svg" },

        { AppIcon.Entertainment, "Expenses/entertainment.svg" },
        { AppIcon.Grocery, "Expenses/grocery.svg" },
        { AppIcon.Medicine, "Expenses/medicine.svg" },
        { AppIcon.Other, "Expenses/other.svg" },
        { AppIcon.Shopping, "Expenses/shopping.svg" },
        { AppIcon.Travel, "Expenses/travel.svg" },
        { AppIcon.Utilities, "Expenses/utilities.svg" },

        { AppIcon.AddNote, "Planner/add_note.svg" },
        { AppIcon.AddReminder, "Planner/add_reminder.svg" },
        { AppIcon.AddTodo, "Planner/add_todo.svg" },
        { AppIcon.ArrowBack, "Planner/arrow_back.svg" },
        { AppIcon.ArrowForward, "Planner/arrow_forward.svg" },
        { AppIcon.Disable, "Planner/disable.svg" },
        { AppIcon.DotSingle, "Planner/dot_single.svg" },
        { AppIcon.DotDouble, "Planner/dot_double.svg" },
        { AppIcon.Enable, "Planner/enable.svg" },
        { AppIcon.Late, "Planner/late.svg" },
        { AppIcon.Link, "Planner/link.svg" },
        { AppIcon.Note, "Planner/note.svg" },
        { AppIcon.SwitchNote, "Planner/switch_note.svg" },
        { AppIcon.Todo, "Planner/todo.svg" },

        { AppIcon.Calculate, "Tools/calculate.svg" },
        { AppIcon.CurrencyExchange, "Tools/currency_exchange.svg" },
        { AppIcon.TimerPause, "Tools/timer_pause.svg" },
        { AppIcon.TimerPlay, "Tools/timer_play.svg" },
        { AppIcon.Timer, "Tools/timer.svg" },
        { AppIcon.UnitConvert, "Tools/unit_convert.svg" },

        { AppIcon.Add, "Misc/add.svg" },
        { AppIcon.Cancel, "Misc/cancel.svg" },
        { AppIcon.Check, "Misc/check.svg" },
        { AppIcon.Checked, "Misc/checked.svg" },
        { AppIcon.Close, "Misc/close.svg" },
        { AppIcon.Copy, "Misc/copy.svg" },
        { AppIcon.Delete, "Misc/delete.svg" },
        { AppIcon.Export, "Misc/export.svg" },
        { AppIcon.Reload, "Misc/reload.svg" },
        { AppIcon.Save, "Misc/save.svg" },
        { AppIcon.Search, "Misc/search.svg" },
        { AppIcon.Star, "Misc/star.svg" },
        { AppIcon.Undo, "Misc/undo.svg" },
    };

    private static readonly Dictionary<(AppIcon, int, Color?), Bitmap> Cache = new();

    public static Bitmap GetBitmap(AppIcon icon, int size, Color? color = null)
    {
        var key = (icon, size, color);

        if (Cache.TryGetValue(key, out var bmp))
            return bmp;

        var path = Path.Combine("Assets", "Icons", IconPaths[icon]).Replace('/', Path.DirectorySeparatorChar);
        bmp = IconLoader.Load(path, size, color);

        Cache[key] = bmp;
        return bmp;
    }

    public static ImageList CreateImageList(IEnumerable<AppIcon> icons, int size, Color color)
    {
        var list = new ImageList
        {
            ImageSize = new Size(size, size),
            ColorDepth = ColorDepth.Depth32Bit
        };

        foreach (var icon in icons)
        {
            list.Images.Add(icon.ToString(), GetBitmap(icon, size, color));
        }

        return list;
    }
}
