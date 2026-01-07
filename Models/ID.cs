using System;
using System.IO.Pipes;

namespace TrackFlow.Models;
public class ID
{
    public required string Type {get; set;} // the type of ID you manualy! set eg: Expense or Note ID
    public required int Value {get; set;} // the generated value of the ID by IDGenerator.GenID() function
    public string PID {get;} // this is a parsed ID (PID) use this when storing data; the above are used for search function only! 
    public ID()
    {
        PID = $"{Type}-{Value}"; // PID data is a string "Expense-8988" with the type and value stored together this will be automatically made by the constracter
    }
}