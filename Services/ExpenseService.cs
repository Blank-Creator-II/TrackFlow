using System;
using System.IO;
using System.Text.RegularExpressions;
using TrackFlow.Utils;
using TrackFlow.Models;

namespace TrackFlow.Service;
public static class ExpenseService // it's a tool it doesn't need instance of the same class for every work
{
    public static (bool s, string f) SaveExpense(Expense e)
    {
        var expense_data = new List<string> // format the data in the correct way
        {
            "[EXPENSE]",
            $"Id={e.Id.PID}",
            $"Amount={e.Amount}",
            $"Date={e.Date:O}",
            $"Mode={e.Mode}",
            $"Receiver={e.Receiver}",
            $"Category={e.Category}",
            $"Currency={e.Currency}",
            "",
            "[COUPON]",
            $"Code={e.AppliedCoupon.Code}",
            $"Description={e.AppliedCoupon.Description}",
            $"Store={e.AppliedCoupon.Store}",
            $"ExpirationDate={e.AppliedCoupon.ExpirationDate:O}",
            "",                 
            "[BANK]",
            $"Name={e.LinkedBank.Name}",
            $"AccountType={e.LinkedBank.AccountType}",
            $"AccountId={e.LinkedBank.AccountId}",
            $"Balance={e.LinkedBank.Balance}",
            $"LinkDate={e.LinkedBank.LinkDate:O}",
            "[END]" 
        };

        string expense_data_location = Path.Combine(FileHelper.BASE_DIR,"Data","Expense",$"expense_{Guid.NewGuid()}.txt"); // creates a sanitized unique data file to store at
        return (FileHelper.WriteFile(expense_data_location,expense_data),expense_data_location); // when this function is called to store data it returns a bool to show if it was successfull opreation or not
    }

    private static Expense _LoadExpense(string[] raw_expense_data)
    {
        bool start_expense_fetch = false;
        bool start_coupon_fetch = false;
        bool start_bank_fetch = false;

        // Temporary storage for keys/values
        Dictionary<string, string> expenseData = new Dictionary<string, string>();
        Dictionary<string, string> couponData = new Dictionary<string, string>();
        Dictionary<string, string> bankData = new Dictionary<string, string>();

        foreach (string line in raw_expense_data)
        {
            if (line == "[EXPENSE]")
            {
                start_expense_fetch = true;
                start_coupon_fetch = false;
                start_bank_fetch = false;
            }
            else if (line == "[COUPON]")
            {
                start_expense_fetch = false;
                start_coupon_fetch = true;
                start_bank_fetch = false;
            }
            else if (line == "[BANK]")
            {
                start_expense_fetch = false;
                start_coupon_fetch = false;
                start_bank_fetch = true;
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
                string[] parts = line.Split('=', 2); // split only on first '='
                if (parts.Length != 2) continue; // skip invalid lines

                string key = parts[0];
                string value = parts[1];

                if (start_expense_fetch)
                    expenseData[key] = value;
                else if (start_coupon_fetch)
                    couponData[key] = value;
                else if (start_bank_fetch)
                    bankData[key] = value;
            }
        }

        // Now it will build the Expense class as given in the example
        // it will first rebuild the ID from the stored PID
        string[] id_parts = expenseData["Id"].Split('-', 2); // splits the PID into type and value
        var build_id = new ID
        {
            Type = id_parts[0],
            Value = Convert.ToInt32(id_parts[1])
        };
        // after the ID is rebuilt the expense will be rebuild from the data
        var expense = new Expense
        {
            Id = build_id,
            Amount = Convert.ToDouble(expenseData["Amount"]),
            Date = Convert.ToDateTime(expenseData["Date"]),
            Mode = expenseData["Mode"],
            Receiver = expenseData["Receiver"],
            Category = expenseData["Category"],
            Currency = expenseData["Currency"],

            AppliedCoupon = new Expense.Coupon
            {
                Code = couponData["Code"],
                Description = couponData["Description"],
                Store = couponData["Store"],
                ExpirationDate = Convert.ToDateTime(couponData["ExpirationDate"])
            },

            LinkedBank = new Expense.Bank
            {
                Name = bankData["Name"],
                AccountType = bankData["AccountType"],
                AccountId = bankData["AccountId"],
                Balance = Convert.ToDouble(bankData["Balance"]),
                LinkDate = Convert.ToDateTime(bankData["LinkDate"])
            }
        };

        return expense; // after formatting it completely it returns a correct instance of Expense class
    }

    public static List<Expense> LoadExpense() // this function will return a list of Expense class, all the expenses saved in the data folder
    {
        List<Expense> list_of_expenses = new List<Expense>();
        List<string> location_list = FileHelper.FetchData("Expense"); // it checks and fetches all the locations of valid stored expense data

        foreach (string expense_data_path in location_list)
        {
            // It will call the internall _LoadExpense class and then store the returned Expense class instance in a list of the Expense format
            list_of_expenses.Add(_LoadExpense(FileHelper.ReadFile(expense_data_path)));
        }

        return list_of_expenses; // it will finnaly return the list of expenses to the caller
    }
}