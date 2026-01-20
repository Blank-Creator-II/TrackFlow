using System;
using System.IO;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
public static class NoteService
{
    public static (bool s, ID f) SaveNote(Note n)
    {
        var note_data = new List<string> // coverts the Note object into storable data
        {
            "[METADATA]",
            $"Id={n.Id.PID}",
            $"Date={n.Date:O}",
            $"Title={n.Title}",
            "",
            "[NOTE]" // the real note data will be added through loop since we don't know how much it's written manual constraction is impossible
        };

        foreach (string line in n.Data)
        {
            note_data.Add(line); // each line of written note will be added after the [NOTE] marker
        }
        note_data.Add("[END]"); // after all the lines of the note is added a closer tag [END] s used to help when reconstracting fro data

        string note_data_location = Path.Combine(FileHelper.BASE_DIR,"Data","Note",$"note_{Guid.NewGuid()}.txt"); // creates a sanitized unique data file to store at just like expense and others
        return (FileHelper.WriteFile(note_data_location,note_data),n.Id); // returns bool usefull for GUI nothing else
    }

    // everything below is more or less the same style as the expense service read that to understand if you don't get it... pray I don't know
    private static Note _LoadNote(string[] raw_note_data)
    {
        bool start_metadata_fetch = false;
        bool start_note_fetch = false;

        Dictionary<string, string> metadata = new Dictionary<string, string>();
        List<string> note_data = new List<string>();

        foreach (string line in raw_note_data)
        {
            if (line == "[METADATA]")
            {
                start_metadata_fetch = true;
                start_note_fetch = false;
            }   
            else if (line == "[NOTE]")
            {
                start_metadata_fetch = false;
                start_note_fetch = true;
            }
            else if (line == "[END]")
            {
                break;
            }
            else
            {
                if (start_metadata_fetch)
                {
                    string[] parts = line.Split("=",2);
                    if (parts.Length != 2) continue;

                    metadata[parts[0]] = parts[1];
                }
                else if (start_note_fetch)
                {
                    note_data.Add(line);
                }
            }
        }

        string[] id_parts = metadata["Id"].Split("-",2);
        var build_id = new ID
        {
            Type = id_parts[0],
            Value = Convert.ToInt32(id_parts[1])
        };

        var note = new Note
        {
            Id = build_id,
            Date = Convert.ToDateTime(metadata["Date"]),
            Title = metadata["Title"],
            Data = note_data
        };

        return note;
    }

    public static List<Note> LoadNote()
    {
        List<Note> list_of_note = new List<Note>();
        List<string> location_list = FileHelper.FetchData("Note");

        foreach (string note_data_path in location_list)
        {
            list_of_note.Add(_LoadNote(FileHelper.ReadFile(note_data_path)));
        }

        return list_of_note;
    }

    public static bool DeleteNote(ID id)
    {
        List<string> noteFiles = FileHelper.FetchData("Note");

        foreach (string path in noteFiles)
        {
            string[] rawData = FileHelper.ReadFile(path);

            foreach (string line in rawData)
            {
                if (line.StartsWith("Id="))
                {
                    if (line.Substring(3) == id.PID)
                    {
                        File.Delete(path);
                        return true;
                    }
                    break;
                }
            }
        }

        return false;
    }

    public static List<Note> SearchNotes(List<Note> notes, object searchValue)
    {
        if (searchValue == null)
            return new List<Note>();

        string query = searchValue.ToString()!.ToLowerInvariant();
        var rankedResults = new List<(Note note, int score)>();

        foreach (Note n in notes)
        {
            int score = 0;

            void Match(string? value, int exact, int partial)
            {
                if (string.IsNullOrEmpty(value)) return;

                string v = value.ToLowerInvariant();
                if (v == query) score += exact;
                else if (v.Contains(query)) score += partial;
            }

            // Metadata
            Match(n.Id.PID, 100, 50);
            Match(n.Date.ToString("O"), 90, 45);
            Match(n.Title,90,45);

            // Note content
            foreach (string line in n.Data)
            {
                Match(line, 40, 20);
            }

            if (score > 0)
                rankedResults.Add((n, score));
        }

        return rankedResults
            .OrderByDescending(r => r.score)
            .Select(r => r.note)
            .ToList();
    }
}