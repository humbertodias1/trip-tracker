using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class ItineraryItem
{
    public int Id { get; set; }
    public int TripId { get; set; }

    [Required]
    public ItineraryItemType Type { get; set; } = ItineraryItemType.Other;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.DateTime)]
    public DateTime StartDateTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? EndDateTime { get; set; }

    [StringLength(200)]
    public string? LocationName { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Url]
    [StringLength(300)]
    public string? MapLink { get; set; }

    [StringLength(100)]
    public string? ConfirmationNumber { get; set; }

    public ItineraryStatus Status { get; set; } = ItineraryStatus.Planned;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
}

