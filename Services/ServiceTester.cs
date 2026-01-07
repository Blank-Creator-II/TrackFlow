using TrackFlow.Models;
using TrackFlow.Utils;

namespace TrackFlow.Service;
class Tester
{
    public static void Start()
    {
        Console.WriteLine("=== TrackFlow Service Tester ===\n");

        TestReminder();
        TestTodo();
        TestExpense();
        TestNote();

        Console.WriteLine("\n=== All tests finished ===");
    }

    static void TestReminder()
    {
        Console.WriteLine(">> Testing ReminderService");

        var reminder = new Reminder
        {
            Id = IDGenerator.GenID("Reminder"),
            SavedDate = DateTime.Now,
            ReminderDate = DateTime.Now.AddHours(1),
            State = true,
            ReminderNote = "Read the new released mangas"
        };

        bool saved = ReminderService.SaveReminder(reminder);
        Console.WriteLine($"Saved: {saved}");

        var loadedReminders = ReminderService.LoadReminder();
        foreach (var r in loadedReminders)
        {
            Console.WriteLine($"Id={r.Id.PID}, Note={r.ReminderNote}, State={r.State}, Saved={r.SavedDate}, Remind={r.ReminderDate}");
        }

        Console.WriteLine();
    }

    static void TestTodo()
    {
        Console.WriteLine(">> Testing TodoService");

        var todo = new Todo
        {
            Id = IDGenerator.GenID("Todo"),
            Date = DateTime.Now,
            Data = new List<Todo.SingleLine>
            {
                new Todo.SingleLine { Data = "listen to Ado's new realse", State = false },
                new Todo.SingleLine { Data = "finish watching anime", State = true },
		new Todo.SingleLine { Data = "check if Marine is still no 1", State = false, Link=IDGenerator.GenID("Reminder") },
		new Todo.SingleLine { Data = "buy some fastfood at stool for the late night party", State = true, Link=IDGenerator.GenID("Reminder") },
		new Todo.SingleLine { Data = "check raora back from surgery", State = false },
		new Todo.SingleLine { Data = "catch up on some streams", State = false },
		new Todo.SingleLine { Data = "check if Marine is still no 1", State = false, Link=IDGenerator.GenID("Reminder") },
            }
        };

        bool saved = TodoService.SaveTodo(todo);
        Console.WriteLine($"Saved: {saved}");

        var loadedTodos = TodoService.LoadTodo();
        foreach (var t in loadedTodos)
        {
            Console.WriteLine($"Todo Id={t.Id.PID}, Date={t.Date}");
            foreach (var line in t.Data)
            {
                Console.WriteLine($"  - [{line.State}] {line.Data}");
            }
        }

        Console.WriteLine();
    }

    static void TestExpense()
    {
        Console.WriteLine(">> Testing ExpenseService");

        var expense = new Expense
        {
            Id = IDGenerator.GenID("Expense"),
            Amount = 400.0,
            Date = DateTime.Now,
            Mode = "Splited",
            Receiver = "Kroni",
            Category = "Entertainment",
            Currency = "USD",
            AppliedCoupon = new Expense.Coupon
            {
                Code = "CYPERPUNK50",
                Description = "50% off for cyberpunk genre games!",
                Store = "Steam Valve",
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

        bool saved = ExpenseService.SaveExpense(expense);
        Console.WriteLine($"Saved: {saved}");

        var loadedExpenses = ExpenseService.LoadExpense();
        foreach (var e in loadedExpenses)
        {
            Console.WriteLine($"Expense Id={e.Id.PID}, Amount={e.Amount}, Receiver={e.Receiver}, Category={e.Category}");
            Console.WriteLine($"  Coupon={e.AppliedCoupon.Code}, Bank={e.LinkedBank.Name}");
        }

        Console.WriteLine();
    }

    static void TestNote()
    {
        Console.WriteLine(">> Testing NoteService");

        var note = new Note
        {
            Id = IDGenerator.GenID("Note"),
            Date = DateTime.Now,
            Data = new List<string>
            {
                "I really don't know why I am writing real things",
                "just like if I would this app or so...",
                "well even without UI and barebone backend",
		"it's still working so... it's great for real!",
		"",
		"i don't know what to write anymore.",
		"oh yeah Merry christmas or happy new year based on where you live :)"
            }
        };

        bool saved = NoteService.SaveNote(note);
        Console.WriteLine($"Saved: {saved}");

        var loadedNotes = NoteService.LoadNote();
        foreach (var n in loadedNotes)
        {
            Console.WriteLine($"Note Id={n.Id.PID}, Date={n.Date}");
            foreach (var line in n.Data)
            {
                Console.WriteLine($"  {line}");
            }
        }

        Console.WriteLine();
    }
}
