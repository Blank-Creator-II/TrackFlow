using System.Text;
using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
// A simple tester for services, it calls every storage service (read & write) with some features:
class Tester
{
    private static readonly List<string> _log = new List<string>();

    public static void Start(string[] args)
    {
        Log("=== TrackFlow Service Tester ===\n");

        Dictionary<string, string> test_data = new Dictionary<string, string>();

        test_data["reminder"] = TestReminder();
        test_data["todo"]     = TestTodo();
        test_data["expense"]  = TestExpense();
        test_data["note"]     = TestNote();

        Log("\n=== All tests finished ===\n");

	    bool log_choice = args != null && Array.Exists(args, a => a == "-Ls");
        if (log_choice)
        {
            SaveLog();
        }

        bool delete_choice = args != null && Array.Exists(args, a => a == "-c");
        if (delete_choice)
        {
            CleanData(test_data);
        }
    }

    // ----------------- Reminder -----------------

    static string TestReminder()
    {
        Log(">> Testing ReminderService");

        var reminder = new Reminder
        {
            Id = IDGenerator.GenID("Reminder"),
            SavedDate = DateTime.Now,
            ReminderDate = DateTime.Now.AddHours(1),
            State = true,
            ReminderNote = "Read the new released mangas"
        };

        var saved = ReminderService.SaveReminder(reminder);
        Log($"Saved: {saved.s}");
        Log($"Path : {saved.f}\n");

        var loaded = ReminderService.LoadReminder();
        Log($"Loaded Reminders: {loaded.Count}");

        foreach (var r in loaded)
        {
            Log($"  Reminder ID : {r.Id.PID}");
            Log($"    Note      : {r.ReminderNote}");
            Log($"    State     : {r.State}");
            Log($"    Saved     : {r.SavedDate}");
            Log($"    Remind At : {r.ReminderDate}\n");
        }

        return saved.f;
    }

    // ----------------- Todo -----------------

    static string TestTodo()
    {
        Log(">> Testing TodoService");

        var todo = new Todo
        {
            Id = IDGenerator.GenID("Todo"),
            Date = DateTime.Now,
            Data = new List<Todo.SingleLine>
            {
                new() { Data = "listen to Ado's new release", State = false },
                new() { Data = "finish watching anime", State = true },
                new() { Data = "check if Marine is still no 1", State = false, Link = IDGenerator.GenID("Reminder") },
                new() { Data = "buy fast food", State = true },
                new() { Data = "catch up on streams", State = false }
            }
        };

        var saved = TodoService.SaveTodo(todo);
        Log($"Saved: {saved.s}");
        Log($"Path : {saved.f}\n");

        var loaded = TodoService.LoadTodo();
        Log($"Loaded Todos: {loaded.Count}");

        foreach (var t in loaded)
        {
            Log($"  Todo ID : {t.Id.PID}");
            Log($"  Date    : {t.Date}");

            foreach (var line in t.Data)
            {
                Log($"    - [{(line.State ? "X" : " ")}] {line.Data}" +
                    (line.Link != null ? $" (Link: {line.Link.PID})" : ""));
            }

            Log("");
        }

        return saved.f;
    }

    // ----------------- Expense -----------------

    static string TestExpense()
    {
        Log(">> Testing ExpenseService");

        var expense = new Expense
        {
            Id = IDGenerator.GenID("Expense"),
            Amount = 400.0,
            Date = DateTime.Now,
            Mode = "Splitted",
            Receiver = "Kroni",
            Category = "Entertainment",
            Currency = "USD",
            AppliedCoupon = new Expense.Coupon
            {
                Code = "CYBERPUNK50",
                Description = "50% off cyberpunk games",
                Store = "Steam",
                ExpirationDate = DateTime.Now.AddMonths(1)
            },
            LinkedBank = new Expense.Bank
            {
                Name = "Bank of America",
                AccountType = "Saving",
                AccountId = "9000127981876",
                Balance = 10000,
                LinkDate = DateTime.Now.AddYears(-1)
            }
        };

        var saved = ExpenseService.SaveExpense(expense);
        Log($"Saved: {saved.s}");
        Log($"Path : {saved.f}\n");

        var loaded = ExpenseService.LoadExpense();
        Log($"Loaded Expenses: {loaded.Count}");

        foreach (var e in loaded)
        {
            Log($"  Expense ID : {e.Id.PID}");
            Log($"    Amount   : {e.Amount} {e.Currency}");
            Log($"    Date     : {e.Date}");
            Log($"    Mode     : {e.Mode}");
            Log($"    Receiver : {e.Receiver}");
            Log($"    Category : {e.Category}");

            if (e.AppliedCoupon != null)
            {
                Log($"    Coupon:");
                Log($"      Code        : {e.AppliedCoupon.Code}");
                Log($"      Description : {e.AppliedCoupon.Description}");
                Log($"      Store       : {e.AppliedCoupon.Store}");
                Log($"      Expires     : {e.AppliedCoupon.ExpirationDate}");
            }

            if (e.LinkedBank != null)
            {
                Log($"    Bank:");
                Log($"      Name      : {e.LinkedBank.Name}");
                Log($"      Type      : {e.LinkedBank.AccountType}");
                Log($"      AccountId : {e.LinkedBank.AccountId}");
                Log($"      Balance   : {e.LinkedBank.Balance}");
                Log($"      Linked At : {e.LinkedBank.LinkDate}");
            }

            Log("");
        }

        return saved.f;
    }

    // ----------------- Note -----------------

    static string TestNote()
    {
        Log(">> Testing NoteService");

        var note = new Note
        {
            Id = IDGenerator.GenID("Note"),
            Date = DateTime.Now,
            Data = new List<string>
            {
                "I really don't know why I am writing real things",
                "but the backend works",
                "and that makes me happy"
            }
        };

        var saved = NoteService.SaveNote(note);
        Log($"Saved: {saved.s}");
        Log($"Path : {saved.f}\n");

        var loaded = NoteService.LoadNote();
        Log($"Loaded Notes: {loaded.Count}");

        foreach (var n in loaded)
        {
            Log($"  Note ID : {n.Id.PID}");
            Log($"  Date    : {n.Date}");
            foreach (var line in n.Data)
            {
                Log($"    {line}");
            }
            Log("");
        }

        return saved.f;
    }

    // ----------------- Utilities -----------------

    static void Log(string message)
    {
        Console.WriteLine(message);
        _log.Add(message);
    }

    static void SaveLog()
    {
        string path = Path.Combine(FileHelper.BASE_DIR, $"test_log-{DateTime.Now:yyyy_MM_dd_HH_mm_ss_ffff}.txt");
        FileHelper.WriteFile(path, _log);
        Console.WriteLine($"\n>> Test log saved to: {path}");
    }

    static void CleanData(Dictionary<string, string> test_data)
    {
        Console.WriteLine("\n=== Deleting test data ===\n");
        foreach (string data in test_data.Values)
        {
            if (File.Exists(data))
            {
                File.Delete(data);
                Console.WriteLine($"Deleted test data: {data}");
            }
        }
        Console.WriteLine("\nWARNING: ID_DATA can not be delted since it stores actual ID postion.");
        Console.WriteLine("\n=== Finished Deleting Data ===\n");
    }
}
