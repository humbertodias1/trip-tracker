using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class DocumentLink
{
    public int Id { get; set; }
    public int TripId { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Url]
    [StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    public Trip? Trip { get; set; }
}
