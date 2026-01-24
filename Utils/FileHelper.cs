using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TrackFlow.Utils;
public static class FileHelper
{
    public static readonly string BASE_DIR = AppContext.BaseDirectory; // This is a relative path to the main path

    public static string ToOneLine(string data)
    {
        if (data is null) return string.Empty;

        // escape any existing backslashes so we can restore them later
        // replace any newline variant with the literal sequence "\n"
        return data
            .Replace("\\", "\\\\")              // backslash -> double-backslash
            .Replace("\r\n", "\\n")            // CRLF -> literal \n
            .Replace("\r", "\\n")              // CR -> literal \n
            .Replace("\n", "\\n");             // LF -> literal \n
    }

    public static string ToMultiLine(string stored)
    {
        if (stored is null) return string.Empty;

        // in reverse literal \n -> real newline, then \\ -> \
        // do newline replacement before un-escaping backslashes
        return stored
            .Replace("\\n", Environment.NewLine)
            .Replace("\\\\", "\\");
    }

    public static void CheckDataDirectory()
    {
        // Ensures that the data directory is always created even if there is nothing stored
        var dir = new Dictionary<string,string>();
        dir["Expense"] = Path.Combine(BASE_DIR,"Data","Expense");
        dir["Note"] = Path.Combine(BASE_DIR,"Data","Note");
        dir["Reminder"] = Path.Combine(BASE_DIR,"Data","Reminder");
        dir["Todo"] = Path.Combine(BASE_DIR,"Data","Todo");
        dir["Theme"] = Path.Combine(BASE_DIR,"Data","Theme");

        if (!Directory.Exists(dir["Expense"])){Directory.CreateDirectory(dir["Expense"]);}

        if (!Directory.Exists(dir["Note"])){Directory.CreateDirectory(dir["Note"]);}

        if (!Directory.Exists(dir["Reminder"])){Directory.CreateDirectory(dir["Reminder"]);}

        if (!Directory.Exists(dir["Todo"])){Directory.CreateDirectory(dir["Todo"]);}

        if (!Directory.Exists(dir["Theme"])){Directory.CreateDirectory(dir["Theme"]);}
    }

    public static string[] ReadFile(string path)
    {
        if (File.Exists(path)) // It checks if the file you are  quering existis and gives you the raw text
        {
            try
            {    
                string[] data = File.ReadAllLines(path); // the raw text is stored here
                return data;
            }
            catch (Exception er)
            {
                Console.WriteLine($"[unable to read from the file, maybe the file is corrupted?]\n{er}");
                return Array.Empty<string>(); // returns an empty string array
            }
        }
        else
        {
            Console.WriteLine($"There is no such a thing as {path}");
            return Array.Empty<string>(); // returns an empty string array
        }   
    }

    public static bool WriteFile(string path, List<string> data)
    {
        try
        {
            // Ensure the folder exists
            string? folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            File.WriteAllLines(path, data); // write the file
            return true;
        }
        catch (Exception er)
        {
            Console.WriteLine($"[Failed to write into the file.]\n{er}");
            return false;  // returns a false boolean for safty check
        }
    }

    public static List<string> FetchData(string type)
    {
        List<string> list_of_stored_data = new List<string>();

        string pattern = $@"^{type.ToLower()}_.*\.txt$"; // a pattern which starts with the type like "expense_" and ends with ".txt"
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        string CapitalizeFirst(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }

        string stored_data_location = Path.Combine(BASE_DIR,"Data",CapitalizeFirst(type));

        foreach (string stored_data in Directory.EnumerateFiles(stored_data_location))
        {
            string fileName = Path.GetFileName(stored_data); // fetch the name of the given data
            if (regex.IsMatch(fileName)) // this method will ensure that we are only reading the correct given data files
            {   
                list_of_stored_data.Add(stored_data); // stores the correct data location for the given type
            }
        }

        return list_of_stored_data; // it will finnaly return a list of location based on the given data to the caller
    }
}