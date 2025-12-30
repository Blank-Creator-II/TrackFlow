using System;
using System.IO;
using System.Text.RegularExpressions;
using TrackFlow.Utils;
using TrackFlow.Models;

namespace TrackFlow.Service;
public class FileStorageService
{
    public static bool SaveExpense(Expense e)
    {
        var expense_data = new List<string> // format the data in the correct way
        {
            "[EXPENSE]",
            $"Id={e.Id}",
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

        string expense_data_location = Path.Combine(FileHelper.BASE_DIR,"Data","Expense",$"expense_{DateTime.Now:yyyyMMdd_HHmmss}.txt"); // creates a sanitized unique data file to store at
        return FileHelper.WriteFile(expense_data_location,expense_data); // when this function is called to store data it returns a bool to show if it was successfull opreation or not
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
            else if (line == "[END]" || string.IsNullOrWhiteSpace(line))
            {
                // skip
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
        var expense = new Expense
        {
            Id = expenseData["Id"],
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

        string pattern = @"^expense_.*\.txt$"; // a pattern which starts with "expense_", ends with ".txt"
        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        string expense_data_location = Path.Combine(FileHelper.BASE_DIR,"Data","Expense");

        foreach (string expense_data in Directory.GetFiles(expense_data_location))
        {
            string fileName = Path.GetFileName(expense_data); // fetch the name of the expense data
            if (regex.IsMatch(fileName)) // this method will ensure that we are only reading the correct expense data files
            {   
                // It will call the internall _LoadExpense class and then store the returned Expense class instance in a list of the Expense format
                list_of_expenses.Add(_LoadExpense(FileHelper.ReadFile(expense_data)));
            }
        }

        return list_of_expenses; // it will finnaly return the list of expenses to the caller
    }
}