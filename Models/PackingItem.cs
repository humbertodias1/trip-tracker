using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class PackingItem
{
    public int Id { get; set; }
    public int TripId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [StringLength(80)]
    public string Category { get; set; } = "General";

    public bool IsPacked { get; set; }

    [StringLength(100)]
    public string? AssignedTo { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
}

