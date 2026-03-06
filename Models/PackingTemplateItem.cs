using System.ComponentModel.DataAnnotations;

namespace TripTracker.Models;

public class PackingTemplateItem
{
    public int Id { get; set; }
    public int PackingTemplateId { get; set; }

    [Required]
    [StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [StringLength(80)]
    public string Category { get; set; } = "General";

    public PackingTemplate? PackingTemplate { get; set; }
}

