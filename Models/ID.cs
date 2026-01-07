using System;
using System.IO.Pipes;

namespace TrackFlow.Models;
public class ID
{
    public required string Type {get; set;} // the type of ID you manualy! set eg: Expense or Note ID
    public required int Value {get; set;} // the generated value of the ID by IDGenerator.GenID() function
    /* this is a parsed ID (PID) use this when storing data; the above are used for search function only!
     PID works by geting the Type and Value of the ID class then storing only when PID is called 
     so constuctor intalizing first proplems will be solved since it works only when it's called
    */
    public string PID => $"{Type}-{Value}"; 
}