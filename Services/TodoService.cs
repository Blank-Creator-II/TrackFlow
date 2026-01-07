using System;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
public static class TodoService
{
    public static bool SaveTodo(Todo t)
    {
        var todo_data = new List<string>
        {
            $"[METADATA]",
            $"Id={t.Id.PID}",
            $"Date={t.Date:O}",
            "",
            $"[TODO]"
        };
        
        foreach (Todo.SingleLine line in t.Data)
        {
            todo_data.Add("[LINE]");
            todo_data.Add($"Data={line.Data}");
            todo_data.Add($"State={Convert.ToString(line.State)}");
            if (line.Link is not null) // checks if the line is null since it's an optional feature it might br null
            {
                todo_data.Add($"Link={line.Link.PID}"); 
            }
            else
            {
                todo_data.Add("Link=<null>"); // if the feaure is not used i.e ID is not linked, it will hard code the line <null> so it can be recoverd as null correctly later
            }
            todo_data.Add("[STOP]");
        }
        todo_data.Add("[END]");

        string todo_data_location = Path.Combine(FileHelper.BASE_DIR,"Todo",$"todo_{Guid.NewGuid()}.txt");
        return FileHelper.WriteFile(todo_data_location,todo_data);
    }

    private static Todo _LoadTodo(string[] raw_todo_data)
    {
        bool start_metadata_fetch = false;
        bool start_todo_fetch = false;
        bool start_line_fetch = false;
        bool linked_todo = true;

        Dictionary<string, string> metadata = new Dictionary<string, string>();
        Dictionary<string, string?> line_data = new Dictionary<string, string?>();
        List<Todo.SingleLine> todo_data = new List<Todo.SingleLine>();

        foreach (string line in raw_todo_data)
        {
            if (line == "[METADATA]")
            {
                start_metadata_fetch = true;
                start_todo_fetch = false;
            }
            else if (line == "[TODO]")
            {
                start_metadata_fetch = false;
                start_todo_fetch = true;
            }
            else if (line == "[END]")
            {
                break;
            }
            else if (line == "")
            {
                // Skip
            }
            else
            {
                if (start_metadata_fetch)
                {
                    string[] parts = line.Split("=",2);
                    if (parts.Length != 2) continue;

                    metadata[parts[0]] = parts[1];
                }
                else if (start_todo_fetch)
                {
                    if (line == "[LINE]")
                    {
                        start_line_fetch = true;
                        line_data.Clear();
                    }
                    else if (line == "[STOP]")
                    {
                        start_line_fetch = false;
                        var link_id = new ID
                        {
                            Type = "",
                            Value = 0,
                        };
                        if (line_data["Link"] is not null)
                        {
                            string[] _id = line_data["Link"]!.Split("-",2);
                            link_id.Type = _id[0];
                            link_id.Value = Convert.ToInt32(_id[1]);
                            linked_todo = true;
                        }
                        else
                        {
                            linked_todo = false;
                        }

                        var singleline = new Todo.SingleLine
                        {
                            Data = line_data["Data"]!,
                            State = Convert.ToBoolean(line_data["State"])
                        };
                        if (linked_todo)
                        {
                            singleline.Link = link_id;
                        }

                        todo_data.Add(singleline);
                    }
                    else if (start_line_fetch)
                    {
                        string[] parts = line.Split("=",2);
                        if (parts.Length != 2) continue;

                        line_data[parts[0]] = parts[1] == "<null>" ? null : parts[1]; // if the value of the stored data is hard codded <null> it will recreate in to null data
                    }
                }
            }
        }

        string[] id_parts = metadata["Id"].Split("-",2);
        var build_id = new ID
        {
            Type = id_parts[0],
            Value = Convert.ToInt32(id_parts[1])
        };

        var todo = new Todo
        {
            Id = build_id,
            Date = Convert.ToDateTime(metadata["Date"]),
            Data = todo_data
        };

        return todo;
    }

    public static List<Todo> LoadTodo()
    {
        List<Todo> list_of_todo = new List<Todo>();
        List<string> location_list = FileHelper.FetchData("Todo");

        foreach (string todo_data_path in location_list)
        {
            list_of_todo.Add(_LoadTodo(FileHelper.ReadFile(todo_data_path)));
        }

        return list_of_todo; 
    }
}