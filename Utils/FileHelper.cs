using System;
using System.IO;

namespace TrackFlow.Utils;
public class FileHelper
{
    public static readonly string BASE_DIR = AppContext.BaseDirectory; // This is a relative path to the main path

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
}