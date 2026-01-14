using System;

namespace TrackFlow.Models;
public class Expense
{
    public required ID Id {get; set;} // stores the transaction history ID for search purposes
    public required double Amount {get; set;} // stores the expesnse ammount
    public required DateTime Date {get; set;} // stores the date of the transaction
    public required string Mode {get; set;} // stores the mode of the transaction (splited, individual, discounted)
    public required string Receiver {get; set;} // stores the reciver info
    public required string Category {get; set;} // used for ui sorting when seeing transaction history eg: (travel, grocery, medicine)
    public required string Currency {get; set;} // what currency the transaction ocurs in (Dollar, Yen...) it's the name not the symbol! "$..." 
    public class Coupon // this variable has it's own variables, can be accessed by creating it's own instance like AppliedCoupon = new Expense.Coupon
    {
        public string? Code {get; set;} // the coupon's code "FREEBOOK10" or stuff like that
        public string? Description {get; set;} // the coupon's description eg: "get 10% discount on books"
        public string? Store {get; set;} // the coupon's work area, where it is applied to, this is linked to Receiver var 
        public DateTime? ExpirationDate {get; set;} // the coupon's expiration date
    }
    public Coupon? AppliedCoupon {get; set;} // this is where the above Coupon class is stored on you refer this to get the inside vars Expense.AppliedCoupon.Code
    public class Bank //  this var has it's own vars, can be accessed as the same as the coupon way
    {
        public required string Name {get; set;} // the linked bank name eg: (CBE...)
        public required string AccountType {get; set;} // the bank account type (Checking, Saving)
        public required string AccountId {get; set;} // the account ID can be a name "test_acc_100" or numbers "10000657329"
        public required double Balance {get; set;} // the account's money amount 
        public required DateTime LinkDate {get; set;} // the date of link between the app and the bank
    }
    public required Bank LinkedBank {get; set;} // this is also the same as the coupon you refere to this when you need the inside vars Expense.LinkedBank.Name
}


/*
[Example Code]
- every instance of this "Expense" class means 1 transaction.
- when intalizing just use the "var" type rather than saying Expense e = new Expense() it will be
  var e = new Expense() this doesn't mean it's dynamic but since the right part "new Expense()" is clear it understands
- the whole class can be intalized as follows:


var expense = new Expense
{
    Id = IDGenerator.GenID("Expense") // generate a new ID using the utility helper IDGenerator
    Amount = 50.0,
    Date = DateTime.Now,
    Mode = "Discounted",
    Receiver = "Book Store",
    Category = "Education",
    Currency = "USD",

    AppliedCoupon = new Expense.Coupon
    {
        Code = "FREEBOOK10",
        Description = "10% off books",
        Store = "Book Store",
        ExpirationDate = DateTime.Now.AddDays(30)
    },

    LinkedBank = new Expense.Bank
    {
        Name = "Test Bank",
        AccountType = "Saving",
        AccountId = "test_acc_100",
        Balance = 500.00,
        LinkDate = DateTime.Now
    }
};
*/ 