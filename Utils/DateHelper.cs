using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackFlow.Utils;
// a simple helper to sort any list by a DateTime key selector.
public static class DateHelper
{
    // returns a new list in the requested order, newest first or earliest first.
    public static List<T> SortByDate<T>(List<T> items, Func<T, DateTime> dateSelector, bool newest_first = true)
    {
        if (items == null) {return new List<T>();}

        return newest_first ? items.OrderByDescending(dateSelector).ToList() : items.OrderBy(dateSelector).ToList();
    }

    // Filter by exact calendar day (ignores time-of-day)
    public static List<T> FilterByDate<T>(List<T> items, Func<T, DateTime?> dateSelector, DateTime targetDate)
    {
        if (items == null) return new List<T>();
        var d = targetDate.Date;
        return items.Where(x => 
        {
            var dt = dateSelector(x);
            return dt.HasValue && dt.Value.Date == d;
        }).ToList();
    }

    // Filter by inclusive date/time range
    public static List<T> FilterByRange<T>(List<T> items, Func<T, DateTime?> dateSelector, DateTime startInclusive, DateTime endInclusive)
    {
        if (items == null) return new List<T>();
        return items.Where(x =>
        {
            var dt = dateSelector(x);
            return dt.HasValue && dt.Value >= startInclusive && dt.Value <= endInclusive;
        }).ToList();
    }

    // Convenience: last N days (including today)
    public static List<T> FilterLastNDays<T>(List<T> items, Func<T, DateTime?> dateSelector, int days)
    {
        var end = DateTime.Now;
        var start = end.Date.AddDays(-Math.Max(0, days - 1));
        return FilterByRange(items, dateSelector, start, end);
    }
}


/* this is how you use them:

var expenses = ExpenseService.LoadExpense(); 
var newest_first_list_of_data = DateSorter.SortByDate(expenses, e => e.Date, newest_first: true);

var expenses = ExpenseService.LoadExpense();
var list_of_data_on_give_day = DateFilter.FilterByDate(expenses, e => e.Date, selected_date);

var expenses = ExpenseService.LoadExpense();
var list_of_data_between_given_dates = DateFilter.FilterByRange(expenses, e => e.Date, start_date, end_date);

var expenses = ExpenseService.LoadExpense();
var list_of_data_for_the_past_N_dates = DateFilter.FilterLastNDays(expenses, e => e.Date, number_of_days);

*/