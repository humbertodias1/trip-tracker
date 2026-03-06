using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class Trip
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string Destinations { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string HomeCurrency { get; set; } = "USD";

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public ICollection<ItineraryItem> ItineraryItems { get; set; } = new List<ItineraryItem>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<PackingItem> PackingItems { get; set; } = new List<PackingItem>();
    public ICollection<DocumentLink> DocumentLinks { get; set; } = new List<DocumentLink>();
}
