namespace TripTracker.Models;

public enum ItineraryItemType
{
    Flight,
    Lodging,
    Transport,
    Activity,
    Food,
    Other
}

public enum ItineraryStatus
{
    Idea,
    Planned,
    Booked,
    Done,
    Canceled
}

public enum ExpenseCategory
{
    Lodging,
    Food,
    Transport,
    Activities,
    Shopping,
    Other
}
