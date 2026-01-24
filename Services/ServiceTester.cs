using System.IO;
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

        Dictionary<string, ID> test_data = new Dictionary<string, ID>();

        test_data["reminder"] = TestReminder();
        TestReminderSearch();
        TestReminderCleanup(test_data);
        test_data["todo"]     = TestTodo();
        TestTodoSearch();
        TestTodoCleanup(test_data);
        test_data["expense"]  = TestExpense();
        TestExpenseSearch();
        TestExpenseCleanup(test_data);
        test_data["note"]     = TestNote();
        TestNoteSearch();
        TestNoteCleanup(test_data);

        Log("\n=== All tests finished ===\n");

	    bool log_choice = args != null && Array.Exists(args, a => a == "-Ls");
        if (log_choice)
        {
            SaveLog();
        }
    }

    // ----------------- Reminder -----------------

    static ID TestReminder()
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

    static void TestReminderSearch()
    {
        Log(">> Testing ReminderService Search");

        var reminders = ReminderService.LoadReminder();

        var byState = ReminderService.SearchReminders(reminders, true);
        Log($"Search State=true : {byState.Count}");

        var byNote = ReminderService.SearchReminders(reminders, "manga");
        Log($"Search 'manga'    : {byNote.Count}\n");
    }

    static void TestReminderCleanup(Dictionary<string, ID> test_data)
    {
        var reminders = ReminderService.LoadReminder();
        if (reminders.Count > 0)
        {
            var res = ReminderService.DeleteReminder(test_data["reminder"]);
            Log($"Cleanup Reminder: {res}");
        }
    }

    // ----------------- Todo -----------------

    static ID TestTodo()
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

    static void TestTodoSearch()
    {
        Log(">> Testing TodoService Search");

        var todos = TodoService.LoadTodo();

        var byText = TodoService.SearchTodos(todos, "anime");
        Log($"Search 'anime' : {byText.Count}");

        var byState = TodoService.SearchTodos(todos, false);
        Log($"Search State=false : {byState.Count}\n");
    }

    static void TestTodoCleanup(Dictionary<string, ID> test_data)
    {
        var todos = TodoService.LoadTodo();
        if (todos.Count > 0)
        {
            var res = TodoService.DeleteTodo(test_data["todo"]);
            Log($"Cleanup Todo: {res}");
        }
    }

    // ----------------- Expense -----------------

    static ID TestExpense()
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

    static void TestExpenseSearch()
    {
        Log(">> Testing ExpenseService Search");

        var expenses = ExpenseService.LoadExpense();

        var byReceiver = ExpenseService.SearchExpenses(expenses, "kroni");
        Log($"Search Receiver 'kroni' : {byReceiver.Count}");

        var byBank = ExpenseService.SearchExpenses(expenses, "bank of america");
        Log($"Search Bank             : {byBank.Count}\n");
    }

    static void TestExpenseCleanup(Dictionary<string, ID> test_data)
    {
        var expenses = ExpenseService.LoadExpense();
        if (expenses.Count > 0)
        {
            var res = ExpenseService.DeleteExpense(test_data["expense"]);
            Log($"Cleanup Expense: {res}");
        }
    }

    // ----------------- Note -----------------

    static ID TestNote()
    {
        Log(">> Testing NoteService");

        var note = new Note
        {
            Id = IDGenerator.GenID("Note"),
            Date = DateTime.Now,
            Title = "Note Test By Test Service",
            Data = FileHelper.ToOneLine( 
                "I really don't know why I am writing real things\n" +
                "but the backend works\n" +
                "and that makes me happy")
            
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

    static void TestNoteSearch()
    {
        Log(">> Testing NoteService Search");

        var notes = NoteService.LoadNote();

        var byText = NoteService.SearchNotes(notes, "backend");
        Log($"Search 'backend' : {byText.Count}");

        var byDate = NoteService.SearchNotes(notes, DateTime.Now.Date);
        Log($"Search Date      : {byDate.Count}\n");
    }

    static void TestNoteCleanup(Dictionary<string, ID> test_data)
    {
        var notes = NoteService.LoadNote();
        if (notes.Count > 0)
        {
            var res = NoteService.DeleteNote(test_data["note"]);
            Log($"Cleanup Note: {res}");
        }
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
}
