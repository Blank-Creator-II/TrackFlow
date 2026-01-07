using System;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Utils;
public static class IDGenerator
{
    private static string IDStorage = Path.Combine(FileHelper.BASE_DIR,"Data","ID_DATA.txt"); // this data is only used for generating ids that's why I made it private

    public static ID GenID(string type) // You call this to get a unique ID for that instance only!
    {
        Dictionary<string, int> ids = LoadIds(); // loads the stored IDs list to generate new one like memory

        int nextId;
        if (ids.ContainsKey(type)) // if this type of ID exist just create the ID itself not the type
        {
            nextId = ids[type] + 1;
            ids[type] = nextId;
        }
        else // if this type is new then the ID type will be added to the memory data for future ID generation
        {
            nextId = 1;
            ids.Add(type, nextId);
        }

        SaveIds(ids); // it stores it back to it's memory by calling another function

        var Id = new ID // it creates a new ID object to be used by the caller
        {
            Type = type,
            Value = nextId
        };

        return Id; // it returns the newly genretd ID object back to the caller of this function
    }

    private static Dictionary<string, int> LoadIds() // this are private classes you don't need explanation since you won't use them :)
    {
        Dictionary<string, int> ids = new Dictionary<string, int>();

        if (!File.Exists(IDStorage))
            return ids;

        string[] lines = FileHelper.ReadFile(IDStorage);
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length == 2)
            {
                ids[parts[0]] = Convert.ToInt32(parts[1]);
            }
        }

        return ids;
    }

    private static void SaveIds(Dictionary<string, int> ids) // same as here this is private I am not explaning, I am tired
    {
        List<string> lines = new List<string>();

        foreach (var pair in ids)
        {
            lines.Add(pair.Key + "=" + pair.Value);
        }

        FileHelper.WriteFile(IDStorage, lines);
    }
}
