using TripTracker.Models;

namespace TripTracker.ViewModels;

public class TripDashboardViewModel
{
    public required Trip Trip { get; set; }
    public required IReadOnlyList<ItineraryDayGroupViewModel> ItineraryDays { get; set; }
    public ItineraryItem? NextItem { get; set; }
    public ItineraryItemType? TypeFilter { get; set; }
    public ItineraryStatus? StatusFilter { get; set; }
    public required IReadOnlyList<Expense> Expenses { get; set; }
    public required IReadOnlyDictionary<ExpenseCategory, decimal> ExpenseTotals { get; set; }
    public decimal ExpenseTotal { get; set; }
    public required IReadOnlyList<PackingItem> PackingItems { get; set; }
    public double PackingCompletionPercentage { get; set; }
    public required IReadOnlyList<PackingTemplate> PackingTemplates { get; set; }
    public required IReadOnlyList<DocumentLink> DocumentLinks { get; set; }
    public string ActiveTab { get; set; } = "itinerary";
}

public class ItineraryDayGroupViewModel
{
    public DateTime Date { get; set; }
    public required IReadOnlyList<ItineraryItem> Items { get; set; }
}
