using MaterialSkin;
using MaterialSkin.Controls;

namespace TrackFlow.Forms;

public class MainForm : MaterialForm // inherents from MaterialSkin Framework
{
    private readonly MaterialSkinManager skinManager; // creates a variable to store a skin manger for the app
    public MainForm() // a constactor called when the app launches it sets up some needed stuff before the app opens
    {
        skinManager = MaterialSkinManager.Instance; // creates skin manager instance to use for the app
        skinManager.AddFormToManage(this); // the skin manager is going to mange "this" app this refers to the object of this class just like "self"
        skinManager.Theme = MaterialSkinManager.Themes.DARK; // set the whole theme to dark mode
        skinManager.ColorScheme = new ColorScheme( // creates a color scheme aka a theme for the app
            ColorTranslator.FromHtml("#212121"), // PrimaryDark
            ColorTranslator.FromHtml("#424242"), // PrimaryMid
            ColorTranslator.FromHtml("#BDBDBD"), // PrimaryLight
            ColorTranslator.FromHtml("#1ee9c4ff"), // Accent (aqua leaning to green)
            TextShade.WHITE                      // Text color either WHITE or BLACK only!
        );

        InitializeWindow(); // calls the actual function that draws or creats the app itself
    }

    // we can use "this" to tell the compiler we are taking about this class aka the Window or app
    // we can leave it which is also fine but riskier
    // it's better if we use "this" so I am going with that when editing main window controls
    private void InitializeWindow()
    {
        this.Text = "TrackFlow"; // the app's title
        this.Height = 700;
        this.Width = 1250;
        this.StartPosition = FormStartPosition.CenterScreen; // makes the app launch in the center of the screen

        this.Sizable = true; // the app will be resizable
        this.MaximizeBox = true; // creats a maximize button
    }
}
