using System;

namespace TrackFlow.Models;
public class Todo 
{
    public required ID Id {get; set;} // stores id for search and linking purposes
    public required DateTime Date {get; set;} // the creation date of the to-do list
    public class SingleLine // this is each to-do line since every list has more than one stuff it needs a class
    {
        public required string Data {get; set;} // the actual todo line the data like "DO your homework" type of data
        public bool State = false; // is that line have been done or not? for the GUI this is needed it starts as false... obviously you are not gona make a to-do list that you have already done right?
        public ID? Link {get; set;} // (it's optional!) each todo line can be linked to a reminder so you will be reminded to do that, it requires ID of type Reminder
    }
    public required List<SingleLine> Data {get; set;} // this will house the whole list of todo lines with there respective link if they have it, the actual line or data and the state of that data (checked or not)
}