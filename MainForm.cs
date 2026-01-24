using MaterialSkin;
using MaterialSkin.Controls;
using TrackFlow.Utils;
using TrackFlow.Models;
using System.IO;

namespace TrackFlow.Forms;
public class MainForm : MaterialForm // inherents from MaterialSkin Framework
{
    private readonly MaterialSkinManager skinManager; // creates a variable to store a skin manger for the app
    
    public static Color PrimaryDark = ColorTranslator.FromHtml("#212121"); // Top app bar background, drawer top, header bars
    public static Color PrimaryMid = ColorTranslator.FromHtml("#424242"); // Secondary surfaces, some buttons, sliders
    public static Color PrimaryGrey = ColorTranslator.FromHtml("#5e5c5c"); // Secondary+ surfaces, some buttons, sliders
    public static Color PrimaryAsh = ColorTranslator.FromHtml("#726f6f"); // Secondary++ surfaces, some buttons, sliders
    public static Color PrimaryLight = ColorTranslator.FromHtml("#BDBDBD"); // Cards, raised surfaces, panels
    public static Color PrimaryAccent = ColorTranslator.FromHtml("#87CEEB"); // Action highlights, checkboxes, selected items, FABs
    public static bool PrimaryWhiteShade = true; // text colors

    public MainForm() // a constactor called when the app launches it sets up some needed stuff before the app opens
    {
        skinManager = MaterialSkinManager.Instance; // creates skin manager instance to use for the app
        skinManager.AddFormToManage(this); // the skin manager is going to mange "this" app this refers to the object of this class just like "self"
        skinManager.Theme = MaterialSkinManager.Themes.DARK; // set the whole theme to dark mode
        skinManager.ColorScheme = new ColorScheme( // creates a color scheme aka a theme for the app
            PrimaryDark,
            PrimaryMid, 
            PrimaryLight,
            PrimaryAccent,
            PrimaryWhiteShade ? TextShade.WHITE : TextShade.BLACK
        );

        InitializeWindow(); // calls the actual function that draws or creats the app itself
    }

    // we can use "this" to tell the compiler we are taking about this class aka the Window or app
    // we can leave it which is also fine but riskier
    // it's better if we use "this" so I am going with that when editing main window controls
    private void InitializeWindow()
    {
        this.Icon = new Icon(Path.Combine(FileHelper.BASE_DIR,"Assets","App.ico"));
        this.Text = "TrackFlow"; // the app's title
        this.Height = 720;
        this.Width = 1280;
        MinimumSize = new Size(720, 1280);
        this.StartPosition = FormStartPosition.CenterScreen; // makes the app launch in the center of the screen

        this.Sizable = true; // the app will be resizable
        this.MaximizeBox = true; // creats a maximize button

        InitializeLayout();
    }

    private void InitializeLayout()
    {
        /*
        This Drawer thing is super weird I admit, in python you hard code it
        but this is custom made for us (I used to pray for times like this)
        anyways the drawer and tabs are intalized:
        */
        var sidebar = new MaterialTabControl
        {
            Depth = 0,
            MouseState = MouseState.HOVER,
            Dock = DockStyle.Fill
        };

        // each page will be created in the UI/ folder this is just initalizing and hooking
        var expensePage = new TabPage("Expenses");
        expensePage.Controls.Add(new ExpensesPage()); // the tab will call the class ExpensesPage from the UI/ folder
        var plannerPage = new TabPage("Planner");
        plannerPage.Controls.Add(new PlannerPage()); // the tab will call the class PlannerPage from the UI/ folder

        sidebar.TabPages.Add(expensePage);
        sidebar.TabPages.Add(plannerPage);
        
        this.Controls.Add(sidebar);
        this.DrawerTabControl = sidebar;

        this.DrawerShowIconsWhenHidden = true; // this make the drawer (the sidebar) smaller with only icons
        this.DrawerWidth = 200;
        
        // we attach our icons to the tab controler
        sidebar.ImageList = IconLibrary.CreateImageList(
            [AppIcon.Expense,AppIcon.Planner,],
            24, // the size of the icon
            PrimaryAccent // the color of the icon
        );
        sidebar.TabPages[0].ImageKey = AppIcon.Expense.ToString(); // first tab
        sidebar.TabPages[1].ImageKey = AppIcon.Planner.ToString(); // second tab
    }
}
