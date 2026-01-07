using System;

namespace TrackFlow.Models;
public class Note
{
    public required ID Id {get; set;} // stores the id for search purposes
    public required DateTime Date {get; set;} // creation date of the note
    public required List<string> Data {get; set;} // the data or note itself
}